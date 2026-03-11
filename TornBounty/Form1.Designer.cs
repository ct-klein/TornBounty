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

        ((System.ComponentModel.ISupportInitialize)dgvBounties).BeginInit();
        pnlTop.SuspendLayout();
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

        // pnlTop
        pnlTop.Controls.Add(lblApiKey);
        pnlTop.Controls.Add(txtApiKey);
        pnlTop.Controls.Add(btnFetch);
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Height = 50;

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
        ClientSize = new Size(800, 550);
        Controls.Add(dgvBounties);
        Controls.Add(pnlBottom);
        Controls.Add(pnlTop);
        MinimumSize = new Size(600, 400);
        Text = "Torn Bounty Viewer";
        StartPosition = FormStartPosition.CenterScreen;

        ((System.ComponentModel.ISupportInitialize)dgvBounties).EndInit();
        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
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
}
