namespace PocketControllerServer
{
    partial class MainForm
    {
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Clean up any resources being used.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// Colours mirror Theme.cs; positions inside the cards are refined in MainForm.LayoutCards.
        /// </summary>
        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            picLogo = new PictureBox();
            lblAppName = new Label();
            lblVersion = new Label();
            pnlStatus = new RoundedPanel();
            lblStatusDot = new StatusDot();
            lblStatusText = new Label();
            pnlConnection = new RoundedPanel();
            lblIpCaption = new Label();
            lblIp = new Label();
            btnCopyIp = new PcButton();
            lblPortCaption = new Label();
            pnlPortBox = new RoundedPanel();
            numPort = new NumericUpDown();
            btnToggle = new PcButton();
            lblHint = new Label();
            pnlSpacer = new Panel();
            pnlClients = new RoundedPanel();
            lblClientsHeader = new Label();
            lblClientCount = new Label();
            lblNoClients = new Label();
            lstClients = new ListBox();
            pnlLog = new RoundedPanel();
            lblLogHeader = new Label();
            btnClearLog = new PcButton();
            rtbLog = new RichTextBox();
            tblCards = new TableLayoutPanel();
            pnlFooter = new Panel();
            lblFooter = new Label();
            pnlBody = new Panel();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            pnlStatus.SuspendLayout();
            pnlConnection.SuspendLayout();
            pnlPortBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            pnlClients.SuspendLayout();
            pnlLog.SuspendLayout();
            tblCards.SuspendLayout();
            pnlFooter.SuspendLayout();
            pnlBody.SuspendLayout();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.BackColor = Color.FromArgb(18, 20, 23);
            pnlHeader.Controls.Add(picLogo);
            pnlHeader.Controls.Add(lblAppName);
            pnlHeader.Controls.Add(lblVersion);
            pnlHeader.Controls.Add(pnlStatus);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(16, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(748, 72);
            pnlHeader.TabIndex = 0;
            //
            // picLogo
            //
            picLogo.Location = new Point(0, 18);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(36, 36);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            //
            // lblAppName
            //
            lblAppName.AutoSize = true;
            lblAppName.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblAppName.ForeColor = Color.FromArgb(232, 235, 239);
            lblAppName.Location = new Point(46, 8);
            lblAppName.Name = "lblAppName";
            lblAppName.Size = new Size(186, 28);
            lblAppName.TabIndex = 1;
            lblAppName.Text = "PocketController";
            //
            // lblVersion
            //
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblVersion.ForeColor = Color.FromArgb(196, 248, 42);
            lblVersion.Location = new Point(49, 40);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(90, 12);
            lblVersion.TabIndex = 2;
            lblVersion.Text = "SERVER  ·  v1.2.1";
            //
            // pnlStatus
            //
            pnlStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnlStatus.BorderColor = Color.FromArgb(54, 59, 68);
            pnlStatus.Controls.Add(lblStatusDot);
            pnlStatus.Controls.Add(lblStatusText);
            pnlStatus.CornerRadius = 17;
            pnlStatus.FillColor = Color.FromArgb(27, 30, 35);
            pnlStatus.Location = new Point(616, 19);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Size = new Size(132, 34);
            pnlStatus.TabIndex = 3;
            //
            // lblStatusDot
            //
            lblStatusDot.DotColor = Color.FromArgb(142, 150, 163);
            lblStatusDot.Location = new Point(14, 12);
            lblStatusDot.Name = "lblStatusDot";
            lblStatusDot.Size = new Size(10, 10);
            lblStatusDot.TabIndex = 0;
            //
            // lblStatusText
            //
            lblStatusText.AutoSize = true;
            lblStatusText.BackColor = Color.FromArgb(27, 30, 35);
            lblStatusText.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblStatusText.ForeColor = Color.FromArgb(142, 150, 163);
            lblStatusText.Location = new Point(32, 8);
            lblStatusText.Name = "lblStatusText";
            lblStatusText.Size = new Size(58, 17);
            lblStatusText.TabIndex = 1;
            lblStatusText.Text = "Stopped";
            //
            // pnlConnection
            //
            pnlConnection.Controls.Add(lblIpCaption);
            pnlConnection.Controls.Add(lblIp);
            pnlConnection.Controls.Add(btnCopyIp);
            pnlConnection.Controls.Add(lblPortCaption);
            pnlConnection.Controls.Add(pnlPortBox);
            pnlConnection.Controls.Add(btnToggle);
            pnlConnection.Controls.Add(lblHint);
            pnlConnection.Dock = DockStyle.Top;
            pnlConnection.Location = new Point(0, 0);
            pnlConnection.Name = "pnlConnection";
            pnlConnection.Size = new Size(748, 116);
            pnlConnection.TabIndex = 0;
            //
            // lblIpCaption
            //
            lblIpCaption.AutoSize = true;
            lblIpCaption.BackColor = Color.FromArgb(27, 30, 35);
            lblIpCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblIpCaption.ForeColor = Color.FromArgb(142, 150, 163);
            lblIpCaption.Location = new Point(18, 14);
            lblIpCaption.Name = "lblIpCaption";
            lblIpCaption.Size = new Size(70, 13);
            lblIpCaption.TabIndex = 0;
            lblIpCaption.Text = "PC ADDRESS";
            //
            // lblIp
            //
            lblIp.AutoSize = true;
            lblIp.BackColor = Color.FromArgb(27, 30, 35);
            lblIp.Font = new Font("Cascadia Mono", 17F, FontStyle.Bold);
            lblIp.ForeColor = Color.FromArgb(92, 214, 230);
            lblIp.Location = new Point(15, 30);
            lblIp.Name = "lblIp";
            lblIp.Size = new Size(170, 30);
            lblIp.TabIndex = 1;
            lblIp.Text = "0.0.0.0";
            //
            // btnCopyIp
            //
            btnCopyIp.AccessibleName = "Copy PC address";
            btnCopyIp.ButtonStyle = PcButtonStyle.Ghost;
            btnCopyIp.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnCopyIp.Location = new Point(194, 32);
            btnCopyIp.Name = "btnCopyIp";
            btnCopyIp.Size = new Size(64, 28);
            btnCopyIp.SurroundColor = Color.FromArgb(27, 30, 35);
            btnCopyIp.TabIndex = 2;
            btnCopyIp.Text = "Copy";
            btnCopyIp.Click += btnCopyIp_Click;
            //
            // lblPortCaption
            //
            lblPortCaption.AutoSize = true;
            lblPortCaption.BackColor = Color.FromArgb(27, 30, 35);
            lblPortCaption.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblPortCaption.ForeColor = Color.FromArgb(142, 150, 163);
            lblPortCaption.Location = new Point(478, 14);
            lblPortCaption.Name = "lblPortCaption";
            lblPortCaption.Size = new Size(57, 13);
            lblPortCaption.TabIndex = 3;
            lblPortCaption.Text = "UDP PORT";
            //
            // pnlPortBox
            //
            pnlPortBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnlPortBox.Controls.Add(numPort);
            pnlPortBox.CornerRadius = 10;
            pnlPortBox.FillColor = Color.FromArgb(37, 41, 48);
            pnlPortBox.Location = new Point(478, 30);
            pnlPortBox.Name = "pnlPortBox";
            pnlPortBox.Size = new Size(96, 36);
            pnlPortBox.TabIndex = 4;
            //
            // numPort
            //
            numPort.AccessibleName = "UDP port";
            numPort.BackColor = Color.FromArgb(37, 41, 48);
            numPort.BorderStyle = BorderStyle.None;
            numPort.Font = new Font("Cascadia Mono", 11F);
            numPort.ForeColor = Color.FromArgb(232, 235, 239);
            numPort.Location = new Point(10, 8);
            numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
            numPort.Name = "numPort";
            numPort.Size = new Size(78, 20);
            numPort.TabIndex = 0;
            numPort.Value = new decimal(new int[] { 5555, 0, 0, 0 });
            //
            // btnToggle
            //
            btnToggle.AccessibleName = "Start server";
            btnToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnToggle.ButtonStyle = PcButtonStyle.Primary;
            btnToggle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnToggle.Location = new Point(598, 27);
            btnToggle.Name = "btnToggle";
            btnToggle.Size = new Size(132, 42);
            btnToggle.SurroundColor = Color.FromArgb(27, 30, 35);
            btnToggle.TabIndex = 5;
            btnToggle.Text = "▶  Start";
            btnToggle.Click += btnToggle_Click;
            //
            // lblHint
            //
            lblHint.AutoSize = true;
            lblHint.BackColor = Color.FromArgb(27, 30, 35);
            lblHint.Font = new Font("Segoe UI", 8.5F);
            lblHint.ForeColor = Color.FromArgb(142, 150, 163);
            lblHint.Location = new Point(18, 84);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(470, 15);
            lblHint.TabIndex = 6;
            lblHint.Text = "Phones on the same Wi‑Fi find this PC automatically while it runs. You can also enter the address and port manually.";
            //
            // pnlSpacer
            //
            pnlSpacer.BackColor = Color.FromArgb(18, 20, 23);
            pnlSpacer.Dock = DockStyle.Top;
            pnlSpacer.Location = new Point(0, 116);
            pnlSpacer.Name = "pnlSpacer";
            pnlSpacer.Size = new Size(748, 12);
            pnlSpacer.TabIndex = 1;
            //
            // pnlClients
            //
            pnlClients.Controls.Add(lblClientsHeader);
            pnlClients.Controls.Add(lblClientCount);
            pnlClients.Controls.Add(lblNoClients);
            pnlClients.Controls.Add(lstClients);
            pnlClients.Dock = DockStyle.Fill;
            pnlClients.Location = new Point(0, 0);
            pnlClients.Margin = new Padding(0, 0, 6, 0);
            pnlClients.Name = "pnlClients";
            pnlClients.Size = new Size(244, 330);
            pnlClients.TabIndex = 0;
            //
            // lblClientsHeader
            //
            lblClientsHeader.AutoSize = true;
            lblClientsHeader.BackColor = Color.FromArgb(27, 30, 35);
            lblClientsHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblClientsHeader.ForeColor = Color.FromArgb(92, 214, 230);
            lblClientsHeader.Location = new Point(16, 14);
            lblClientsHeader.Name = "lblClientsHeader";
            lblClientsHeader.Size = new Size(46, 13);
            lblClientsHeader.TabIndex = 0;
            lblClientsHeader.Text = "PHONES";
            //
            // lblClientCount
            //
            lblClientCount.AutoSize = true;
            lblClientCount.BackColor = Color.FromArgb(27, 30, 35);
            lblClientCount.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblClientCount.ForeColor = Color.FromArgb(142, 150, 163);
            lblClientCount.Location = new Point(196, 14);
            lblClientCount.Name = "lblClientCount";
            lblClientCount.Size = new Size(32, 15);
            lblClientCount.TabIndex = 1;
            lblClientCount.Text = "0 / 4";
            //
            // lblNoClients
            //
            lblNoClients.BackColor = Color.FromArgb(27, 30, 35);
            lblNoClients.Font = new Font("Segoe UI", 9F);
            lblNoClients.ForeColor = Color.FromArgb(142, 150, 163);
            lblNoClients.Location = new Point(10, 42);
            lblNoClients.Name = "lblNoClients";
            lblNoClients.Size = new Size(224, 276);
            lblNoClients.TabIndex = 2;
            lblNoClients.Text = "No phones connected yet.\r\nPress Start, then connect\r\nfrom the app.";
            lblNoClients.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lstClients
            //
            lstClients.AccessibleName = "Connected phones";
            lstClients.BackColor = Color.FromArgb(27, 30, 35);
            lstClients.BorderStyle = BorderStyle.None;
            lstClients.DrawMode = DrawMode.OwnerDrawFixed;
            lstClients.Font = new Font("Segoe UI", 9.5F);
            lstClients.ForeColor = Color.FromArgb(232, 235, 239);
            lstClients.IntegralHeight = false;
            lstClients.ItemHeight = 44;
            lstClients.Location = new Point(10, 42);
            lstClients.Name = "lstClients";
            lstClients.Size = new Size(224, 276);
            lstClients.TabIndex = 3;
            lstClients.DrawItem += LstClients_DrawItem;
            //
            // pnlLog
            //
            pnlLog.Controls.Add(lblLogHeader);
            pnlLog.Controls.Add(btnClearLog);
            pnlLog.Controls.Add(rtbLog);
            pnlLog.Dock = DockStyle.Fill;
            pnlLog.Location = new Point(256, 0);
            pnlLog.Margin = new Padding(6, 0, 0, 0);
            pnlLog.Name = "pnlLog";
            pnlLog.Size = new Size(492, 330);
            pnlLog.TabIndex = 1;
            //
            // lblLogHeader
            //
            lblLogHeader.AutoSize = true;
            lblLogHeader.BackColor = Color.FromArgb(27, 30, 35);
            lblLogHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblLogHeader.ForeColor = Color.FromArgb(92, 214, 230);
            lblLogHeader.Location = new Point(16, 14);
            lblLogHeader.Name = "lblLogHeader";
            lblLogHeader.Size = new Size(61, 13);
            lblLogHeader.TabIndex = 0;
            lblLogHeader.Text = "EVENT LOG";
            //
            // btnClearLog
            //
            btnClearLog.AccessibleName = "Clear event log";
            btnClearLog.ButtonStyle = PcButtonStyle.Ghost;
            btnClearLog.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnClearLog.Location = new Point(418, 9);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(60, 26);
            btnClearLog.SurroundColor = Color.FromArgb(27, 30, 35);
            btnClearLog.TabIndex = 1;
            btnClearLog.Text = "Clear";
            btnClearLog.Click += btnClearLog_Click;
            //
            // rtbLog
            //
            rtbLog.AccessibleName = "Event log";
            rtbLog.BackColor = Color.FromArgb(27, 30, 35);
            rtbLog.BorderStyle = BorderStyle.None;
            rtbLog.Font = new Font("Cascadia Mono", 9F);
            rtbLog.ForeColor = Color.FromArgb(232, 235, 239);
            rtbLog.Location = new Point(16, 44);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbLog.Size = new Size(466, 274);
            rtbLog.TabIndex = 2;
            rtbLog.Text = "";
            //
            // tblCards
            //
            tblCards.BackColor = Color.FromArgb(18, 20, 23);
            tblCards.ColumnCount = 2;
            tblCards.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250F));
            tblCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblCards.Controls.Add(pnlClients, 0, 0);
            tblCards.Controls.Add(pnlLog, 1, 0);
            tblCards.Dock = DockStyle.Fill;
            tblCards.Location = new Point(0, 128);
            tblCards.Margin = new Padding(0);
            tblCards.Name = "tblCards";
            tblCards.RowCount = 1;
            tblCards.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblCards.Size = new Size(748, 330);
            tblCards.TabIndex = 2;
            //
            // pnlFooter
            //
            pnlFooter.BackColor = Color.FromArgb(18, 20, 23);
            pnlFooter.Controls.Add(lblFooter);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(16, 510);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(748, 30);
            pnlFooter.TabIndex = 2;
            //
            // lblFooter
            //
            lblFooter.Dock = DockStyle.Fill;
            lblFooter.Font = new Font("Segoe UI", 8F);
            lblFooter.ForeColor = Color.FromArgb(142, 150, 163);
            lblFooter.Location = new Point(0, 0);
            lblFooter.Name = "lblFooter";
            lblFooter.Size = new Size(748, 30);
            lblFooter.TabIndex = 0;
            lblFooter.Text = "UDP  ·  ViGEmBus  ·  Virtual Xbox 360 (XInput)  ·  github.com/axcel-blade/PocketController";
            lblFooter.TextAlign = ContentAlignment.MiddleCenter;
            //
            // pnlBody
            //
            pnlBody.BackColor = Color.FromArgb(18, 20, 23);
            pnlBody.Controls.Add(tblCards);
            pnlBody.Controls.Add(pnlSpacer);
            pnlBody.Controls.Add(pnlConnection);
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.Location = new Point(16, 76);
            pnlBody.Name = "pnlBody";
            pnlBody.Size = new Size(748, 434);
            pnlBody.TabIndex = 1;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(18, 20, 23);
            ClientSize = new Size(780, 540);
            Controls.Add(pnlBody);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            ForeColor = Color.FromArgb(232, 235, 239);
            MinimumSize = new Size(680, 480);
            Name = "MainForm";
            Padding = new Padding(16, 4, 16, 0);
            Text = "PocketController Server";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            pnlStatus.ResumeLayout(false);
            pnlStatus.PerformLayout();
            pnlConnection.ResumeLayout(false);
            pnlConnection.PerformLayout();
            pnlPortBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            pnlClients.ResumeLayout(false);
            pnlClients.PerformLayout();
            pnlLog.ResumeLayout(false);
            pnlLog.PerformLayout();
            tblCards.ResumeLayout(false);
            pnlFooter.ResumeLayout(false);
            pnlBody.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // Header
        private Panel pnlHeader;
        private PictureBox picLogo;
        private Label lblAppName;
        private Label lblVersion;
        private RoundedPanel pnlStatus;
        private StatusDot lblStatusDot;
        private Label lblStatusText;

        // Connection
        private RoundedPanel pnlConnection;
        private Label lblIpCaption;
        private Label lblIp;
        private PcButton btnCopyIp;
        private Label lblPortCaption;
        private RoundedPanel pnlPortBox;
        private NumericUpDown numPort;
        private PcButton btnToggle;
        private Label lblHint;

        // Body
        private Panel pnlBody;
        private Panel pnlSpacer;
        private TableLayoutPanel tblCards;

        // Clients
        private RoundedPanel pnlClients;
        private Label lblClientsHeader;
        private Label lblClientCount;
        private Label lblNoClients;
        private ListBox lstClients;

        // Log
        private RoundedPanel pnlLog;
        private Label lblLogHeader;
        private PcButton btnClearLog;
        private RichTextBox rtbLog;

        // Footer
        private Panel pnlFooter;
        private Label lblFooter;
    }
}
