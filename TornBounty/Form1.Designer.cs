namespace TornBounty;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // Controls
        var lblApiKey = new Label();
        txtApiKey = new TextBox();
        btnFetch = new Button();
        btnPrev = new Button();
        btnNext = new Button();
        dgvBounties = new DataGridView();
        lblStatus = new Label();
        var pnlTop = new Panel();
        var pnlBottom = new Panel();
        var pnlFilter = new Panel();
        var lblMinLevel = new Label();
        var lblMaxLevel = new Label();
        nudMinLevel = new NumericUpDown();
        nudMaxLevel = new NumericUpDown();
        btnResetFilter = new Button();
        chkOnlyOkay = new CheckBox();
        var lblMaxStats = new Label();
        cboMaxStats = new ComboBox();
        btnForgetKey = new Button();

        ((System.ComponentModel.ISupportInitialize)dgvBounties).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudMinLevel).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudMaxLevel).BeginInit();
        pnlTop.SuspendLayout();
        pnlFilter.SuspendLayout();
        pnlBottom.SuspendLayout();
        SuspendLayout();

        // lblApiKey
        lblApiKey.AutoSize = true;
        lblApiKey.Location = new Point(12, 15);
        lblApiKey.Text = "API Key:";

        // txtApiKey
        txtApiKey.Location = new Point(75, 12);
        txtApiKey.Size = new Size(350, 23);
        txtApiKey.Text = "";
        txtApiKey.UseSystemPasswordChar = true;

        // btnFetch
        btnFetch.Location = new Point(435, 10);
        btnFetch.Size = new Size(100, 28);
        btnFetch.Text = "Fetch Bounties";
        btnFetch.UseVisualStyleBackColor = true;
        btnFetch.Click += BtnFetch_Click;

        // btnForgetKey
        btnForgetKey.Location = new Point(545, 10);
        btnForgetKey.Size = new Size(90, 28);
        btnForgetKey.Text = "Forget Key";
        btnForgetKey.UseVisualStyleBackColor = true;
        btnForgetKey.Click += BtnForgetKey_Click;

        // pnlTop
        pnlTop.Controls.Add(lblApiKey);
        pnlTop.Controls.Add(txtApiKey);
        pnlTop.Controls.Add(btnFetch);
        pnlTop.Controls.Add(btnForgetKey);
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Height = 50;

        // lblMinLevel
        lblMinLevel.AutoSize = false;
        lblMinLevel.Dock = DockStyle.Left;
        lblMinLevel.Width = 65;
        lblMinLevel.TextAlign = ContentAlignment.MiddleLeft;
        lblMinLevel.Text = "Min Level:";

        // nudMinLevel
        nudMinLevel.Dock = DockStyle.Left;
        nudMinLevel.Width = 60;
        nudMinLevel.Minimum = 1;
        nudMinLevel.Maximum = 100;
        nudMinLevel.Value = 1;
        nudMinLevel.ValueChanged += LevelFilter_ValueChanged;

        // lblMaxLevel
        lblMaxLevel.AutoSize = false;
        lblMaxLevel.Dock = DockStyle.Left;
        lblMaxLevel.Width = 85;
        lblMaxLevel.Padding = new Padding(20, 0, 0, 0);
        lblMaxLevel.TextAlign = ContentAlignment.MiddleLeft;
        lblMaxLevel.Text = "Max Level:";

        // nudMaxLevel
        nudMaxLevel.Dock = DockStyle.Left;
        nudMaxLevel.Width = 60;
        nudMaxLevel.Minimum = 1;
        nudMaxLevel.Maximum = 100;
        nudMaxLevel.Value = 100;
        nudMaxLevel.ValueChanged += LevelFilter_ValueChanged;

        // btnResetFilter
        btnResetFilter.Dock = DockStyle.Left;
        btnResetFilter.Width = 100;
        btnResetFilter.Text = "Reset Filter";
        btnResetFilter.UseVisualStyleBackColor = true;
        btnResetFilter.Click += BtnResetFilter_Click;

        // chkOnlyOkay
        chkOnlyOkay.Dock = DockStyle.Left;
        chkOnlyOkay.AutoSize = false;
        chkOnlyOkay.Width = 170;
        chkOnlyOkay.Padding = new Padding(20, 0, 0, 0);
        chkOnlyOkay.Text = "Only show status Okay";
        chkOnlyOkay.Checked = true;
        chkOnlyOkay.CheckedChanged += ChkOnlyOkay_CheckedChanged;

        // lblMaxStats
        lblMaxStats.AutoSize = false;
        lblMaxStats.Dock = DockStyle.Left;
        lblMaxStats.Width = 110;
        lblMaxStats.Padding = new Padding(10, 0, 0, 0);
        lblMaxStats.TextAlign = ContentAlignment.MiddleLeft;
        lblMaxStats.Text = "Max Est. Stats:";

        // cboMaxStats - item 0 is "Any", the rest match BattleStatsEstimator.TierLabels
        cboMaxStats.Dock = DockStyle.Left;
        cboMaxStats.Width = 110;
        cboMaxStats.DropDownStyle = ComboBoxStyle.DropDownList;
        cboMaxStats.Items.Add("Any");
        cboMaxStats.Items.AddRange([.. Services.BattleStatsEstimator.TierLabels]);
        cboMaxStats.SelectedIndex = 0;
        cboMaxStats.SelectedIndexChanged += CboMaxStats_SelectedIndexChanged;

        // pnlFilter - Dock.Left children lay out right-to-left in add order, so add rightmost first
        pnlFilter.Controls.Add(cboMaxStats);
        pnlFilter.Controls.Add(lblMaxStats);
        pnlFilter.Controls.Add(chkOnlyOkay);
        pnlFilter.Controls.Add(btnResetFilter);
        pnlFilter.Controls.Add(new Label { Dock = DockStyle.Left, Width = 20 });
        pnlFilter.Controls.Add(nudMaxLevel);
        pnlFilter.Controls.Add(lblMaxLevel);
        pnlFilter.Controls.Add(nudMinLevel);
        pnlFilter.Controls.Add(lblMinLevel);
        pnlFilter.Dock = DockStyle.Top;
        pnlFilter.Height = 38;
        pnlFilter.Padding = new Padding(12, 6, 0, 6);

        // dgvBounties
        dgvBounties.AllowUserToAddRows = false;
        dgvBounties.AllowUserToDeleteRows = false;
        dgvBounties.AllowUserToOrderColumns = true;
        dgvBounties.ReadOnly = true;
        dgvBounties.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvBounties.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvBounties.RowHeadersVisible = false;
        dgvBounties.Dock = DockStyle.Fill;
        dgvBounties.BackgroundColor = SystemColors.Window;
        dgvBounties.BorderStyle = BorderStyle.Fixed3D;
        dgvBounties.ColumnHeaderMouseClick += DgvBounties_ColumnHeaderMouseClick;
        dgvBounties.CellDoubleClick += DgvBounties_CellDoubleClick;

        // btnPrev
        btnPrev.Dock = DockStyle.Left;
        btnPrev.Size = new Size(100, 45);
        btnPrev.Text = "<< Previous";
        btnPrev.UseVisualStyleBackColor = true;
        btnPrev.Click += BtnPrev_Click;

        // btnNext
        btnNext.Dock = DockStyle.Right;
        btnNext.Size = new Size(100, 45);
        btnNext.Text = "Next >>";
        btnNext.UseVisualStyleBackColor = true;
        btnNext.Click += BtnNext_Click;

        // lblStatus
        lblStatus.Dock = DockStyle.Fill;
        lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        lblStatus.Text = "Enter your API key and click Fetch Bounties.";

        // pnlBottom - add order matters for docking: Fill last
        pnlBottom.Controls.Add(lblStatus);
        pnlBottom.Controls.Add(btnNext);
        pnlBottom.Controls.Add(btnPrev);
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Height = 45;

        // Form1
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 550);
        Controls.Add(dgvBounties);
        Controls.Add(pnlBottom);
        Controls.Add(pnlFilter);
        Controls.Add(pnlTop);
        MinimumSize = new Size(700, 400);
        Text = "Torn Bounty Viewer";
        StartPosition = FormStartPosition.CenterScreen;

        ((System.ComponentModel.ISupportInitialize)dgvBounties).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudMinLevel).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudMaxLevel).EndInit();
        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
        pnlFilter.ResumeLayout(false);
        pnlBottom.ResumeLayout(false);
        pnlBottom.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TextBox txtApiKey;
    private Button btnFetch;
    private Button btnPrev;
    private Button btnNext;
    private DataGridView dgvBounties;
    private Label lblStatus;
    private NumericUpDown nudMinLevel;
    private NumericUpDown nudMaxLevel;
    private Button btnResetFilter;
    private CheckBox chkOnlyOkay;
    private ComboBox cboMaxStats;
    private Button btnForgetKey;
}
