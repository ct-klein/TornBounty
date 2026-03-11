using TornBounty.Models;
using TornBounty.Services;

namespace TornBounty;

public partial class Form1 : Form
{
    private readonly TornApiService _apiService = new();
    private string? _prevUrl;
    private string? _nextUrl;
    private List<Bounty> _currentBounties = [];
    private CancellationTokenSource? _cts;

    public Form1()
    {
        InitializeComponent();
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
        _cts = new CancellationTokenSource();

        SetLoadingState(true);
        try
        {
            var response = await fetchFunc();
            _currentBounties = response.Bounties;
            _prevUrl = response.Metadata?.Links?.Prev;
            _nextUrl = response.Metadata?.Links?.Next;

            BindGrid(_currentBounties);
            var timestamp = DateTimeOffset.FromUnixTimeSeconds(response.BountiesTimestamp).LocalDateTime.ToString("g");
            lblStatus.Text = $"Records: {_currentBounties.Count}  |  Timestamp: {timestamp}";
        }
        catch (OperationCanceledException)
        {
            // Cancelled, no action needed
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
            SetLoadingState(false);
        }
    }

    private void BindGrid(List<Bounty> bounties)
    {
        var displayData = bounties.Select(b => new
        {
            b.TargetName,
            b.TargetLevel,
            b.Reward,
            ListerName = b.IsAnonymous ? "(Anonymous)" : b.ListerName ?? "",
            b.Reason,
            b.Quantity,
            ValidUntil = DateTimeOffset.FromUnixTimeSeconds(b.ValidUntil).LocalDateTime.ToString("g"),
            b.TargetId,
            b.ListerId
        }).ToList();

        dgvBounties.DataSource = displayData;

        SetColumnHeader("TargetName", "Target");
        SetColumnHeader("TargetLevel", "Level");
        SetColumnHeader("Reward", "Reward ($)");
        SetColumnHeader("ListerName", "Listed By");
        SetColumnHeader("Reason", "Reason");
        SetColumnHeader("Quantity", "Qty");
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
    }

    private void SetColumnHeader(string columnName, string headerText)
    {
        if (dgvBounties.Columns[columnName] is { } col)
            col.HeaderText = headerText;
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
            BindGrid(_currentBounties);
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
            BindGrid(_currentBounties);
        }
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
