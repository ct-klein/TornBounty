using System.ComponentModel;
using System.Diagnostics;
using TornBounty.Models;
using TornBounty.Services;

namespace TornBounty;

public partial class Form1 : Form
{
    private readonly TornApiService _apiService = new();
    private string? _prevUrl;
    private string? _nextUrl;
    private List<Bounty> _currentBounties = [];
    private Dictionary<int, int> _listingCounts = [];
    private long _bountiesTimestamp;
    private CancellationTokenSource? _cts;

    // Player status lookups cost one API call per target, so cache them briefly
    private const int MaxConcurrentStatusRequests = 5;
    private const int TornErrorTooManyRequests = 5;
    private static readonly TimeSpan StatusCacheDuration = TimeSpan.FromSeconds(60);
    private readonly Dictionary<int, (UserStatus Status, DateTime FetchedUtc)> _statusCache = [];
    // Estimates barely change, so they're kept for the session and refreshed whenever the status is
    private readonly Dictionary<int, int?> _statsTierCache = [];
    private string? _statusWarning;

    // The grid is shown before status lookups finish, then refreshed at most this often as they come in
    private static readonly TimeSpan GridRefreshInterval = TimeSpan.FromSeconds(1);
    private string? _gridSummary;
    private string? _statusProgress;
    private DateTime? _rateLimitResumeLocal;

    public Form1()
    {
        InitializeComponent();
        _apiService.RateLimitWaiting += OnRateLimitWaiting;

        var savedKey = ApiKeyStore.Load();
        if (!string.IsNullOrEmpty(savedKey))
        {
            txtApiKey.Text = savedKey;
            lblStatus.Text = "Saved API key loaded. Click Fetch Bounties.";
        }
    }

    private void OnRateLimitWaiting(TimeSpan wait)
    {
        void Update()
        {
            _rateLimitResumeLocal = DateTime.Now + wait;
            // While status lookups run, the pause is shown alongside their progress instead
            if (_statusProgress is null)
                lblStatus.Text = $"Pausing {Math.Ceiling(wait.TotalSeconds)}s to stay under Torn's API rate limit...";
            else
                UpdateStatusLabel();
        }

        if (InvokeRequired)
            BeginInvoke(Update);
        else
            Update();
    }

    private static bool IsStatusFresh(UserStatus status, DateTime fetchedUtc, DateTime nowUtc)
    {
        // A hospital/jail stay with a known end time can't become Okay before then (barring a revive),
        // so trust it until it ends instead of spending another API call
        if (!status.IsOkay && status.Until > 0)
            return DateTimeOffset.FromUnixTimeSeconds(status.Until.Value).UtcDateTime > nowUtc;

        return nowUtc - fetchedUtc <= StatusCacheDuration;
    }

    private void BtnForgetKey_Click(object? sender, EventArgs e)
    {
        ApiKeyStore.Clear();
        txtApiKey.Clear();
        lblStatus.Text = "Saved API key removed.";
    }

    private async void BtnFetch_Click(object sender, EventArgs e)
    {
        await FetchBountiesAsync(() => _apiService.GetBountiesAsync(txtApiKey.Text.Trim(), _cts!.Token));
    }

    private async void BtnPrev_Click(object sender, EventArgs e)
    {
        if (_prevUrl is null)
        {
            MessageBox.Show("No previous page available.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var url = _prevUrl;
        await FetchBountiesAsync(() => _apiService.GetBountiesByUrlAsync(url, txtApiKey.Text.Trim(), _cts!.Token));
    }

    private async void BtnNext_Click(object sender, EventArgs e)
    {
        if (_nextUrl is null)
        {
            MessageBox.Show("No next page available.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var url = _nextUrl;
        await FetchBountiesAsync(() => _apiService.GetBountiesByUrlAsync(url, txtApiKey.Text.Trim(), _cts!.Token));
    }

    private async Task FetchBountiesAsync(Func<Task<BountyResponse>> fetchFunc)
    {
        if (string.IsNullOrWhiteSpace(txtApiKey.Text))
        {
            MessageBox.Show("Please enter your API key.", "API Key Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _cts?.Cancel();
        var cts = _cts = new CancellationTokenSource();
        _statusWarning = null;
        _statusProgress = null;
        _rateLimitResumeLocal = null;

        SetLoadingState(true);
        try
        {
            var response = await fetchFunc();
            // One row per target: keep the highest reward and remember how many listings there were
            var byTarget = response.Bounties.GroupBy(b => b.TargetId).ToList();
            _listingCounts = byTarget.ToDictionary(g => g.Key, g => g.Count());
            _currentBounties = [.. byTarget.Select(g => g.MaxBy(b => b.Reward)!)];
            _prevUrl = response.Metadata?.Links?.Prev;
            _nextUrl = response.Metadata?.Links?.Next;
            _bountiesTimestamp = response.BountiesTimestamp;

            // The key worked, so remember it for next time
            ApiKeyStore.Save(txtApiKey.Text.Trim());

            // Show the page now; statuses and estimates fill in as the lookups complete
            BindGrid();
            SetLoadingState(false);

            await LoadPlayerStatusesAsync(txtApiKey.Text.Trim(), cts.Token);
            BindGrid(keepGridPosition: true);
        }
        catch (OperationCanceledException)
        {
            // Cancelled, no action needed
        }
        catch (TornApiException ex)
        {
            lblStatus.Text = $"API error {ex.Code}: {ex.ApiMessage}";
            MessageBox.Show(ex.Message, "Torn API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"API request failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // A newer fetch may have started (and cancelled this one); leave its loading state alone
            if (cts == _cts)
                SetLoadingState(false);
        }
    }

    private async Task LoadPlayerStatusesAsync(string apiKey, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var targetIds = _currentBounties
            .Select(b => b.TargetId)
            .Distinct()
            .Where(id => !_statusCache.TryGetValue(id, out var cached) || !IsStatusFresh(cached.Status, cached.FetchedUtc, now))
            .ToList();

        if (targetIds.Count == 0)
            return;

        // Linked so hitting the rate limit stops the remaining lookups
        using var rateLimitCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var throttle = new SemaphoreSlim(MaxConcurrentStatusRequests);
        var completed = 0;
        var failed = 0;
        var sinceRefresh = Stopwatch.StartNew();
        _statusProgress = $"Checking player status 0/{targetIds.Count}";
        UpdateStatusLabel();

        async Task LoadOneAsync(int userId)
        {
            await throttle.WaitAsync(rateLimitCts.Token);
            try
            {
                var lookup = await _apiService.GetUserLookupAsync(userId, apiKey, rateLimitCts.Token);
                if (lookup.Profile?.Status is { } status)
                    _statusCache[userId] = (status, DateTime.UtcNow);
                _statsTierCache[userId] = BattleStatsEstimator.EstimateTier(lookup);
            }
            catch (TornApiException ex) when (ex.Code == TornErrorTooManyRequests)
            {
                _statusWarning = "API rate limit reached; some statuses unknown";
                rateLimitCts.Cancel();
            }
            catch (Exception ex) when (ex is TornApiException or HttpRequestException)
            {
                // One bad lookup shouldn't stop the rest; the row just shows no status
                failed++;
            }
            finally
            {
                throttle.Release();
                completed++;

                // Once a newer fetch has started, the grid and label belong to it
                if (!cancellationToken.IsCancellationRequested)
                {
                    _statusProgress = $"Checking player status {completed}/{targetIds.Count}";
                    if (sinceRefresh.Elapsed >= GridRefreshInterval)
                    {
                        BindGrid(keepGridPosition: true);
                        sinceRefresh.Restart();
                    }
                    else
                    {
                        UpdateStatusLabel();
                    }
                }
            }
        }

        try
        {
            await Task.WhenAll(targetIds.Select(LoadOneAsync));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Stopped early due to rate limit; _statusWarning is already set
        }

        _statusProgress = null;
        _rateLimitResumeLocal = null;
        if (_statusWarning is null && failed > 0)
            _statusWarning = $"{failed} status lookup(s) failed";
    }

    // Players whose status couldn't be looked up are hidden too, since they can't be confirmed Okay
    private bool IsHiddenByStatusFilter(Bounty bounty) =>
        chkOnlyOkay.Checked
        && !(_statusCache.TryGetValue(bounty.TargetId, out var cached) && cached.Status.IsOkay);

    // Index 0 is "Any"; index n allows estimate tiers 0..n-1.
    // Players without an estimate are hidden when a limit is set, since they can't be confirmed under it
    private bool IsHiddenByStatsFilter(Bounty bounty) =>
        cboMaxStats.SelectedIndex > 0
        && !(GetStatsTier(bounty.TargetId) is { } tier && tier < cboMaxStats.SelectedIndex);

    private int? GetStatsTier(int targetId) => _statsTierCache.GetValueOrDefault(targetId);

    private void ChkOnlyOkay_CheckedChanged(object? sender, EventArgs e)
    {
        BindGrid();
    }

    private void CboMaxStats_SelectedIndexChanged(object? sender, EventArgs e)
    {
        BindGrid();
    }

    private void LevelFilter_ValueChanged(object? sender, EventArgs e)
    {
        // Keep min <= max by nudging the other control
        if (sender == nudMinLevel && nudMinLevel.Value > nudMaxLevel.Value)
            nudMaxLevel.Value = nudMinLevel.Value;
        else if (sender == nudMaxLevel && nudMaxLevel.Value < nudMinLevel.Value)
            nudMinLevel.Value = nudMaxLevel.Value;

        BindGrid();
    }

    private void BtnResetFilter_Click(object? sender, EventArgs e)
    {
        nudMinLevel.Value = nudMinLevel.Minimum;
        nudMaxLevel.Value = nudMaxLevel.Maximum;
        cboMaxStats.SelectedIndex = 0;
    }

    // keepGridPosition restores the scroll position and selected target after rebinding,
    // so background refreshes don't jump the grid back to the top
    private void BindGrid(bool keepGridPosition = false)
    {
        var firstDisplayedRow = dgvBounties.FirstDisplayedScrollingRowIndex;
        var selectedTargetId = dgvBounties.CurrentRow?.Cells["TargetId"].Value as int?;
        var selectedColumn = dgvBounties.CurrentCell?.ColumnIndex ?? 0;

        var minLevel = (int)nudMinLevel.Value;
        var maxLevel = (int)nudMaxLevel.Value;
        var filtered = _currentBounties
            .Where(b => b.TargetLevel >= minLevel && b.TargetLevel <= maxLevel)
            .ToList();
        var notOkayCount = filtered.RemoveAll(IsHiddenByStatusFilter);
        var overStatsCount = filtered.RemoveAll(IsHiddenByStatsFilter);

        var displayData = filtered.Select(b => new
        {
            b.TargetName,
            b.TargetLevel,
            Status = _statusCache.TryGetValue(b.TargetId, out var cached) ? cached.Status.Description ?? cached.Status.State ?? "" : "",
            EstStats = GetStatsTier(b.TargetId) is { } tier ? BattleStatsEstimator.TierLabels[tier] : "",
            b.Reward,
            ListerName = b.IsAnonymous ? "(Anonymous)" : b.ListerName ?? "",
            b.Reason,
            b.Quantity,
            Listings = _listingCounts.GetValueOrDefault(b.TargetId, 1),
            ValidUntil = DateTimeOffset.FromUnixTimeSeconds(b.ValidUntil).LocalDateTime.ToString("g"),
            b.TargetId,
            b.ListerId
        }).ToList();

        dgvBounties.DataSource = displayData;

        SetColumnHeader("TargetName", "Target");
        SetColumnHeader("TargetLevel", "Level");
        SetColumnHeader("Status", "Status");
        SetColumnHeader("EstStats", "Est. Stats");
        SetColumnHeader("Reward", "Reward ($)");
        SetColumnHeader("ListerName", "Listed By");
        SetColumnHeader("Reason", "Reason");
        SetColumnHeader("Quantity", "Qty");
        SetColumnHeader("Listings", "Listings");
        SetColumnHeader("ValidUntil", "Valid Until");
        SetColumnHeader("TargetId", "Target ID");
        SetColumnHeader("ListerId", "Lister ID");

        if (dgvBounties.Columns["Reward"] is { } rewardCol)
        {
            rewardCol.DefaultCellStyle.Format = "N0";
            rewardCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        if (dgvBounties.Columns["TargetLevel"] is { } levelCol)
            levelCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        if (dgvBounties.Columns["Quantity"] is { } qtyCol)
            qtyCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        dgvBounties.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

        if (keepGridPosition)
            RestoreGridPosition(firstDisplayedRow, selectedTargetId, selectedColumn);

        if (_bountiesTimestamp > 0)
        {
            var timestamp = DateTimeOffset.FromUnixTimeSeconds(_bountiesTimestamp).LocalDateTime.ToString("g");
            var summary = $"Showing {filtered.Count} of {_currentBounties.Count}";
            if (notOkayCount > 0)
                summary += $" ({notOkayCount} not Okay hidden)";
            if (overStatsCount > 0)
                summary += $" ({overStatsCount} over stats limit hidden)";
            _gridSummary = summary + $"  |  Timestamp: {timestamp}";
            UpdateStatusLabel();
        }
    }

    private void RestoreGridPosition(int firstDisplayedRow, int? selectedTargetId, int selectedColumn)
    {
        // Setting CurrentCell scrolls it into view, so restore the selection before the scroll position
        if (selectedTargetId is not null)
        {
            foreach (DataGridViewRow row in dgvBounties.Rows)
            {
                if (row.Cells["TargetId"].Value as int? == selectedTargetId)
                {
                    dgvBounties.CurrentCell = row.Cells[Math.Min(selectedColumn, row.Cells.Count - 1)];
                    break;
                }
            }
        }

        if (firstDisplayedRow >= 0 && firstDisplayedRow < dgvBounties.RowCount)
            dgvBounties.FirstDisplayedScrollingRowIndex = firstDisplayedRow;
    }

    private void UpdateStatusLabel()
    {
        if (_gridSummary is null)
            return;

        var text = _gridSummary;
        if (_statusProgress is not null)
        {
            text += $"  |  {_statusProgress}...";
            if (_rateLimitResumeLocal is { } resume && resume > DateTime.Now)
                text += $" (paused for API rate limit until {resume:T})";
        }
        if (_statusWarning is not null)
            text += $"  |  {_statusWarning}";
        lblStatus.Text = text;
    }

    private void SetColumnHeader(string columnName, string headerText)
    {
        if (dgvBounties.Columns[columnName] is { } col)
            col.HeaderText = headerText;
    }

    private void DgvBounties_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        // RowIndex is -1 when the header is double-clicked
        if (e.RowIndex < 0)
            return;

        if (dgvBounties.Rows[e.RowIndex].Cells["TargetId"].Value is not int targetId)
            return;

        var url = $"https://www.torn.com/page.php?sid=attack&user2ID={targetId}";
        try
        {
            // UseShellExecute opens the URL in the default browser
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            MessageBox.Show($"Could not open browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvBounties_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        var columnName = dgvBounties.Columns[e.ColumnIndex].Name;

        if (columnName == "TargetLevel")
        {
            var tag = dgvBounties.Columns[e.ColumnIndex].Tag as string;
            if (tag == "asc")
            {
                _currentBounties = [.. _currentBounties.OrderByDescending(b => b.TargetLevel)];
                dgvBounties.Columns[e.ColumnIndex].Tag = "desc";
            }
            else
            {
                _currentBounties = [.. _currentBounties.OrderBy(b => b.TargetLevel)];
                dgvBounties.Columns[e.ColumnIndex].Tag = "asc";
            }
            BindGrid();
        }
        else if (columnName == "Reward")
        {
            var tag = dgvBounties.Columns[e.ColumnIndex].Tag as string;
            if (tag == "asc")
            {
                _currentBounties = [.. _currentBounties.OrderByDescending(b => b.Reward)];
                dgvBounties.Columns[e.ColumnIndex].Tag = "desc";
            }
            else
            {
                _currentBounties = [.. _currentBounties.OrderBy(b => b.Reward)];
                dgvBounties.Columns[e.ColumnIndex].Tag = "asc";
            }
            BindGrid();
        }
        else if (columnName == "EstStats")
        {
            // Unknown estimates sort last in both directions
            var tag = dgvBounties.Columns[e.ColumnIndex].Tag as string;
            if (tag == "asc")
            {
                _currentBounties = [.. _currentBounties.OrderByDescending(b => GetStatsTier(b.TargetId) ?? -1)];
                dgvBounties.Columns[e.ColumnIndex].Tag = "desc";
            }
            else
            {
                _currentBounties = [.. _currentBounties.OrderBy(b => GetStatsTier(b.TargetId) ?? int.MaxValue)];
                dgvBounties.Columns[e.ColumnIndex].Tag = "asc";
            }
            BindGrid();
        }
        else if (columnName == "Status")
        {
            // Unknown statuses sort last in both directions
            var tag = dgvBounties.Columns[e.ColumnIndex].Tag as string;
            if (tag == "asc")
            {
                _currentBounties = [.. _currentBounties
                    .OrderBy(b => GetStatusSortKey(b.TargetId).Rank == StatusRankUnknown)
                    .ThenByDescending(b => GetStatusSortKey(b.TargetId))];
                dgvBounties.Columns[e.ColumnIndex].Tag = "desc";
            }
            else
            {
                _currentBounties = [.. _currentBounties.OrderBy(b => GetStatusSortKey(b.TargetId))];
                dgvBounties.Columns[e.ColumnIndex].Tag = "asc";
            }
            BindGrid();
        }
    }

    private const int StatusRankUnknown = 3;

    // Ascending order: Okay first, then timed states (hospital/jail) by soonest release,
    // then untimed states (traveling, abroad, ...) by name, then unknown
    private (int Rank, long Until, string State) GetStatusSortKey(int targetId)
    {
        if (!_statusCache.TryGetValue(targetId, out var cached))
            return (StatusRankUnknown, 0, "");

        var status = cached.Status;
        if (status.IsOkay)
            return (0, 0, "");
        if (status.Until > 0)
            return (1, status.Until.Value, "");
        return (2, 0, status.State ?? "");
    }

    private void SetLoadingState(bool loading)
    {
        btnFetch.Enabled = !loading;
        txtApiKey.Enabled = !loading;
        Cursor = loading ? Cursors.WaitCursor : Cursors.Default;
        if (loading)
        {
            lblStatus.Text = "Loading...";
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _apiService.Dispose();
        base.OnFormClosed(e);
    }
}
