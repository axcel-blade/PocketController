namespace PocketControllerServer
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // --- Controls ---
            pnlHeader      = new Panel();
            lblAppName     = new Label();
            lblVersion     = new Label();
            pnlStatus      = new Panel();
            lblStatusDot   = new Label();
            lblStatusText  = new Label();

            pnlBody        = new Panel();

            pnlConnection  = new Panel();
            lblIpIcon      = new Label();
            lblIp          = new Label();
            lblPortLabel   = new Label();
            numPort        = new NumericUpDown();
            btnToggle      = new Button();

            pnlClients     = new Panel();
            lblClientsHeader = new Label();
            lblClientCount = new Label();
            lstClients     = new ListBox();

            pnlLog         = new Panel();
            lblLogHeader   = new Label();
            btnClearLog    = new Button();
            rtbLog         = new RichTextBox();

            pnlFooter      = new Panel();
            lblFooter      = new Label();

            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            pnlHeader.SuspendLayout();
            pnlStatus.SuspendLayout();
            pnlBody.SuspendLayout();
            pnlConnection.SuspendLayout();
            pnlClients.SuspendLayout();
            pnlLog.SuspendLayout();
            pnlFooter.SuspendLayout();
            SuspendLayout();

            // ── Palette ────────────────────────────────────────────────
            var bg        = Color.FromArgb(15,  15,  23);
            var surface   = Color.FromArgb(24,  24,  37);
            var card      = Color.FromArgb(30,  30,  46);
            var border    = Color.FromArgb(45,  45,  65);
            var accent    = Color.FromArgb(99,  102, 241);  // indigo-500
            var accentHov = Color.FromArgb(79,  82,  221);
            var textPri   = Color.FromArgb(226, 232, 240);
            var textSec   = Color.FromArgb(148, 163, 184);
            var green     = Color.FromArgb(34,  197, 94);
            var red       = Color.FromArgb(239, 68,  68);
            var fontMain  = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            var fontBold  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            var fontSm    = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            var fontH     = new Font("Segoe UI", 11f,  FontStyle.Bold);
            var fontTitle = new Font("Segoe UI", 14f,  FontStyle.Bold);

            // ── Header ─────────────────────────────────────────────────
            pnlHeader.Dock      = DockStyle.Top;
            pnlHeader.Height    = 64;
            pnlHeader.BackColor = surface;
            pnlHeader.Padding   = new Padding(20, 0, 20, 0);

            lblAppName.AutoSize  = true;
            lblAppName.Text      = "PocketController";
            lblAppName.Font      = fontTitle;
            lblAppName.ForeColor = textPri;
            lblAppName.Location  = new Point(20, 14);

            lblVersion.AutoSize  = true;
            lblVersion.Text      = "SERVER  v1.1.1";
            lblVersion.Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            lblVersion.ForeColor = accent;
            lblVersion.Location  = new Point(23, 42);

            // status group (top-right)
            pnlStatus.BackColor = surface;
            pnlStatus.Size      = new Size(140, 40);
            pnlStatus.Anchor    = AnchorStyles.Top | AnchorStyles.Right;
            pnlStatus.Location  = new Point(700 - 160, 12);

            lblStatusDot.AutoSize  = false;
            lblStatusDot.Size      = new Size(12, 12);
            lblStatusDot.Location  = new Point(0, 14);
            lblStatusDot.BackColor = red;
            // circle via Paint
            lblStatusDot.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.Clear(surface);
                using var b = new SolidBrush(lblStatusDot.BackColor);
                e.Graphics.FillEllipse(b, 0, 0, 11, 11);
            };

            lblStatusText.AutoSize  = true;
            lblStatusText.Text      = "Stopped";
            lblStatusText.Font      = fontBold;
            lblStatusText.ForeColor = textSec;
            lblStatusText.Location  = new Point(18, 10);

            pnlStatus.Controls.AddRange(new Control[] { lblStatusDot, lblStatusText });
            pnlHeader.Controls.AddRange(new Control[] { lblAppName, lblVersion, pnlStatus });

            // ── Body ───────────────────────────────────────────────────
            pnlBody.Dock      = DockStyle.Fill;
            pnlBody.BackColor = bg;
            pnlBody.Padding   = new Padding(16, 12, 16, 8);

            // ── Connection card ────────────────────────────────────────
            pnlConnection.BackColor  = card;
            pnlConnection.Dock       = DockStyle.Top;
            pnlConnection.Height     = 60;
            pnlConnection.Padding    = new Padding(16, 0, 16, 0);
            pnlConnection.Margin     = new Padding(0, 0, 0, 12);
            pnlConnection.Paint     += (s, e) => DrawCardBorder(e, card, border);

            lblIpIcon.AutoSize  = false;
            lblIpIcon.Size      = new Size(20, 20);
            lblIpIcon.Location  = new Point(16, 20);
            lblIpIcon.ForeColor = accent;
            lblIpIcon.Font      = new Font("Segoe UI", 12f);
            lblIpIcon.Text      = "⊕";

            lblIp.AutoSize  = true;
            lblIp.Location  = new Point(40, 20);
            lblIp.Text      = "IP: ...";
            lblIp.Font      = fontBold;
            lblIp.ForeColor = textPri;

            lblPortLabel.AutoSize  = true;
            lblPortLabel.Location  = new Point(240, 22);
            lblPortLabel.Text      = "Port";
            lblPortLabel.Font      = fontSm;
            lblPortLabel.ForeColor = textSec;

            numPort.Location        = new Point(278, 17);
            numPort.Size            = new Size(80, 28);
            numPort.Minimum         = 1024;
            numPort.Maximum         = 65535;
            numPort.Value           = 5555;
            numPort.Font            = fontMain;
            numPort.ForeColor       = textPri;
            numPort.BackColor       = surface;
            numPort.BorderStyle     = BorderStyle.FixedSingle;

            btnToggle.Location      = new Point(375, 13);
            btnToggle.Size          = new Size(110, 34);
            btnToggle.Text          = "▶  Start";
            btnToggle.Font          = fontBold;
            btnToggle.ForeColor     = Color.White;
            btnToggle.BackColor     = accent;
            btnToggle.FlatStyle     = FlatStyle.Flat;
            btnToggle.FlatAppearance.BorderSize  = 0;
            btnToggle.FlatAppearance.MouseOverBackColor = accentHov;
            btnToggle.Cursor        = Cursors.Hand;
            btnToggle.Click        += btnToggle_Click;

            pnlConnection.Controls.AddRange(new Control[]
                { lblIpIcon, lblIp, lblPortLabel, numPort, btnToggle });

            // ── Clients card ───────────────────────────────────────────
            pnlClients.BackColor  = card;
            pnlClients.Anchor     = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            pnlClients.Location   = new Point(0, 0);
            pnlClients.Width      = 220;
            pnlClients.Paint     += (s, e) => DrawCardBorder(e, card, border);

            lblClientsHeader.AutoSize  = true;
            lblClientsHeader.Location  = new Point(16, 12);
            lblClientsHeader.Text      = "CLIENTS";
            lblClientsHeader.Font      = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblClientsHeader.ForeColor = textSec;

            lblClientCount.AutoSize  = true;
            lblClientCount.Location  = new Point(16, 30);
            lblClientCount.Text      = "0 / 4 connected";
            lblClientCount.Font      = fontSm;
            lblClientCount.ForeColor = textSec;

            lstClients.Anchor         = AnchorStyles.Top | AnchorStyles.Left |
                                        AnchorStyles.Bottom | AnchorStyles.Right;
            lstClients.Location       = new Point(10, 56);
            lstClients.Size           = new Size(200, 100);   // height adjusted in OnResize
            lstClients.BackColor      = surface;
            lstClients.ForeColor      = textPri;
            lstClients.Font           = fontMain;
            lstClients.BorderStyle    = BorderStyle.None;
            lstClients.DrawMode       = DrawMode.OwnerDrawFixed;
            lstClients.ItemHeight     = 32;
            lstClients.DrawItem      += LstClients_DrawItem;

            pnlClients.Controls.AddRange(new Control[]
                { lblClientsHeader, lblClientCount, lstClients });

            // ── Log card ───────────────────────────────────────────────
            pnlLog.BackColor  = card;
            pnlLog.Anchor     = AnchorStyles.Top | AnchorStyles.Left |
                                 AnchorStyles.Bottom | AnchorStyles.Right;
            pnlLog.Location   = new Point(232, 0);
            pnlLog.Width      = 400;   // adjusted in OnResize
            pnlLog.Paint     += (s, e) => DrawCardBorder(e, card, border);

            lblLogHeader.AutoSize  = true;
            lblLogHeader.Location  = new Point(16, 12);
            lblLogHeader.Text      = "EVENT LOG";
            lblLogHeader.Font      = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblLogHeader.ForeColor = textSec;

            btnClearLog.AutoSize       = false;
            btnClearLog.Size           = new Size(56, 22);
            btnClearLog.Anchor         = AnchorStyles.Top | AnchorStyles.Right;
            btnClearLog.Location       = new Point(330, 10);  // adjusted in OnResize
            btnClearLog.Text           = "Clear";
            btnClearLog.Font           = fontSm;
            btnClearLog.ForeColor      = textSec;
            btnClearLog.BackColor      = surface;
            btnClearLog.FlatStyle      = FlatStyle.Flat;
            btnClearLog.FlatAppearance.BorderColor = border;
            btnClearLog.FlatAppearance.BorderSize  = 1;
            btnClearLog.Cursor         = Cursors.Hand;
            btnClearLog.Click         += (_, _) => rtbLog.Clear();

            rtbLog.Anchor      = AnchorStyles.Top | AnchorStyles.Left |
                                  AnchorStyles.Bottom | AnchorStyles.Right;
            rtbLog.Location    = new Point(10, 44);
            rtbLog.Size        = new Size(380, 200);   // adjusted in OnResize
            rtbLog.BackColor   = surface;
            rtbLog.ForeColor   = textPri;
            rtbLog.Font        = new Font("Cascadia Code", 8.5f, FontStyle.Regular);
            rtbLog.ReadOnly    = true;
            rtbLog.BorderStyle = BorderStyle.None;
            rtbLog.ScrollBars  = RichTextBoxScrollBars.Vertical;

            pnlLog.Controls.AddRange(new Control[]
                { lblLogHeader, btnClearLog, rtbLog });

            // ── Bottom container for the two cards ────────────────────
            pnlCards = new Panel();
            pnlCards.Dock      = DockStyle.Fill;
            pnlCards.BackColor = bg;
            pnlCards.Controls.AddRange(new Control[] { pnlLog, pnlClients });

            pnlSpacer = new Panel();
            pnlSpacer.Dock      = DockStyle.Top;
            pnlSpacer.Height    = 12;
            pnlSpacer.BackColor = bg;

            pnlBody.Controls.AddRange(new Control[]
                { pnlCards, pnlSpacer, pnlConnection });

            // ── Footer ─────────────────────────────────────────────────
            pnlFooter.Dock      = DockStyle.Bottom;
            pnlFooter.Height    = 28;
            pnlFooter.BackColor = surface;

            lblFooter.AutoSize  = false;
            lblFooter.Dock      = DockStyle.Fill;
            lblFooter.Text      = "© 2026 Axcel Blade  •  github.com/axcel-blade  •  UDP  •  ViGEmBus";
            lblFooter.Font      = fontSm;
            lblFooter.ForeColor = textSec;
            lblFooter.TextAlign = ContentAlignment.MiddleCenter;

            pnlFooter.Controls.Add(lblFooter);

            // ── Form ───────────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new Size(700, 500);
            MinimumSize         = new Size(560, 420);
            BackColor           = bg;
            Text                = "PocketController Server";
            Controls.AddRange(new Control[] { pnlBody, pnlFooter, pnlHeader });

            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlStatus.ResumeLayout(false);
            pnlStatus.PerformLayout();
            pnlBody.ResumeLayout(false);
            pnlConnection.ResumeLayout(false);
            pnlConnection.PerformLayout();
            pnlClients.ResumeLayout(false);
            pnlClients.PerformLayout();
            pnlLog.ResumeLayout(false);
            pnlLog.PerformLayout();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private static void DrawCardBorder(PaintEventArgs e, Color bg, Color border)
        {
            using var p = new Pen(border, 1);
            e.Graphics.DrawRectangle(p, 0, 0, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
        }

        private void LstClients_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var surface = Color.FromArgb(24, 24, 37);
            var accent  = Color.FromArgb(99, 102, 241);
            var textPri = Color.FromArgb(226, 232, 240);
            var hover   = Color.FromArgb(38, 38, 56);

            bool selected = (e.State & DrawItemState.Selected) != 0;
            e.Graphics.FillRectangle(new SolidBrush(selected ? hover : surface), e.Bounds);

            // green dot
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(34, 197, 94)),
                e.Bounds.X + 10, e.Bounds.Y + 10, 8, 8);

            // text
            var text = lstClients.Items[e.Index]?.ToString() ?? "";
            TextRenderer.DrawText(e.Graphics, text,
                new Font("Segoe UI", 9f, FontStyle.Regular),
                new Point(e.Bounds.X + 26, e.Bounds.Y + 7),
                textPri);
        }

        // Panels
        private Panel  pnlHeader, pnlStatus, pnlBody, pnlConnection;
        private Panel  pnlClients, pnlLog, pnlFooter, pnlCards, pnlSpacer;

        // Header
        private Label  lblAppName, lblVersion, lblStatusDot, lblStatusText;

        // Connection
        private Label          lblIpIcon, lblIp, lblPortLabel;
        private NumericUpDown  numPort;
        private Button         btnToggle;

        // Clients
        private Label    lblClientsHeader, lblClientCount;
        private ListBox  lstClients;

        // Log
        private Label        lblLogHeader;
        private Button       btnClearLog;
        private RichTextBox  rtbLog;

        // Footer
        private Label lblFooter;
    }
}
