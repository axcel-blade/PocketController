namespace PocketConsoleServer
{
    partial class Form1
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

            btnToggle = new Button();
            numPort = new NumericUpDown();
            lblIp = new Label();
            lblPortLabel = new Label();
            lblClients = new Label();
            lstClients = new ListBox();
            lblLog = new Label();
            rtbLog = new RichTextBox();

            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            SuspendLayout();

            // lblIp
            lblIp.AutoSize = true;
            lblIp.Location = new Point(12, 15);
            lblIp.Text = "IP: ...";

            // lblPortLabel
            lblPortLabel.AutoSize = true;
            lblPortLabel.Location = new Point(12, 45);
            lblPortLabel.Text = "Port:";

            // numPort
            numPort.Location = new Point(55, 42);
            numPort.Minimum = 1024;
            numPort.Maximum = 65535;
            numPort.Value = 5555;
            numPort.Width = 80;

            // btnToggle
            btnToggle.Location = new Point(150, 40);
            btnToggle.Size = new Size(80, 28);
            btnToggle.Text = "Start";
            btnToggle.Click += btnToggle_Click;

            // lblClients
            lblClients.AutoSize = true;
            lblClients.Location = new Point(12, 85);
            lblClients.Text = "Connected clients:";

            // lstClients
            lstClients.Location = new Point(12, 105);
            lstClients.Size = new Size(350, 80);

            // lblLog
            lblLog.AutoSize = true;
            lblLog.Location = new Point(12, 200);
            lblLog.Text = "Event log:";

            // rtbLog
            rtbLog.Location = new Point(12, 220);
            rtbLog.Size = new Size(560, 200);
            rtbLog.ReadOnly = true;
            rtbLog.BackColor = SystemColors.Window;
            rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;

            // Form1
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 440);
            Controls.AddRange(new Control[]
            {
                lblIp, lblPortLabel, numPort, btnToggle,
                lblClients, lstClients,
                lblLog, rtbLog
            });
            Text = "PocketConsole Server v1.0.0";
            MinimizeBox = true;

            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Button btnToggle;
        private NumericUpDown numPort;
        private Label lblIp;
        private Label lblPortLabel;
        private Label lblClients;
        private ListBox lstClients;
        private Label lblLog;
        private RichTextBox rtbLog;
    }
}
