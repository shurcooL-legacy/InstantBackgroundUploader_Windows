namespace InstantBackgroundUploader
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
		private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.upTimeLabel = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.usageLabel = new System.Windows.Forms.Label();
            this.totalLabel = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.startCheckBox = new System.Windows.Forms.CheckBox();
            this.timeSinceLastFullChargeGroupBox = new System.Windows.Forms.GroupBox();
            this.hasBeenPluggedInLabel = new System.Windows.Forms.Label();
            this.timeSinceLastFullChargeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // upTimeLabel
            // 
            this.upTimeLabel.AutoSize = true;
            this.upTimeLabel.Location = new System.Drawing.Point(12, 42);
            this.upTimeLabel.Name = "upTimeLabel";
            this.upTimeLabel.Size = new System.Drawing.Size(47, 13);
            this.upTimeLabel.TabIndex = 0;
            this.upTimeLabel.Text = "Up Time";
            this.toolTip.SetToolTip(this.upTimeLabel, "Up Time not counting sleep");
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 60000;
            this.toolTip.InitialDelay = 100;
            this.toolTip.ReshowDelay = 0;
            // 
            // usageLabel
            // 
            this.usageLabel.AutoSize = true;
            this.usageLabel.Location = new System.Drawing.Point(6, 16);
            this.usageLabel.Name = "usageLabel";
            this.usageLabel.Size = new System.Drawing.Size(38, 13);
            this.usageLabel.TabIndex = 4;
            this.usageLabel.Text = "Usage";
            this.toolTip.SetToolTip(this.usageLabel, "Total Up Time since last full charge, not counting sleep");
            // 
            // totalLabel
            // 
            this.totalLabel.AutoSize = true;
            this.totalLabel.Location = new System.Drawing.Point(6, 36);
            this.totalLabel.Name = "totalLabel";
            this.totalLabel.Size = new System.Drawing.Size(31, 13);
            this.totalLabel.TabIndex = 5;
            this.totalLabel.Text = "Total";
            this.toolTip.SetToolTip(this.totalLabel, "Total Up Time since last full charge, including sleep");
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(256, 246);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(41, 13);
            this.versionLabel.TabIndex = 1;
            this.versionLabel.Text = "version";
            // 
            // startCheckBox
            // 
            this.startCheckBox.AutoSize = true;
            this.startCheckBox.Location = new System.Drawing.Point(12, 12);
            this.startCheckBox.Name = "startCheckBox";
            this.startCheckBox.Size = new System.Drawing.Size(117, 17);
            this.startCheckBox.TabIndex = 2;
            this.startCheckBox.Text = "Start with Windows";
            this.startCheckBox.UseVisualStyleBackColor = true;
            this.startCheckBox.CheckedChanged += new System.EventHandler(this.startCheckBox_CheckedChanged);
            // 
            // timeSinceLastFullChargeGroupBox
            // 
            this.timeSinceLastFullChargeGroupBox.Controls.Add(this.hasBeenPluggedInLabel);
            this.timeSinceLastFullChargeGroupBox.Controls.Add(this.totalLabel);
            this.timeSinceLastFullChargeGroupBox.Controls.Add(this.usageLabel);
            this.timeSinceLastFullChargeGroupBox.Location = new System.Drawing.Point(12, 64);
            this.timeSinceLastFullChargeGroupBox.Name = "timeSinceLastFullChargeGroupBox";
            this.timeSinceLastFullChargeGroupBox.Size = new System.Drawing.Size(276, 77);
            this.timeSinceLastFullChargeGroupBox.TabIndex = 5;
            this.timeSinceLastFullChargeGroupBox.TabStop = false;
            this.timeSinceLastFullChargeGroupBox.Text = "Time since last full charge";
            // 
            // hasBeenPluggedInLabel
            // 
            this.hasBeenPluggedInLabel.AutoSize = true;
            this.hasBeenPluggedInLabel.Location = new System.Drawing.Point(6, 56);
            this.hasBeenPluggedInLabel.Name = "hasBeenPluggedInLabel";
            this.hasBeenPluggedInLabel.Size = new System.Drawing.Size(268, 13);
            this.hasBeenPluggedInLabel.TabIndex = 6;
            this.hasBeenPluggedInLabel.Text = "Computer has been plugged in since the last full charge";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 268);
            this.Controls.Add(this.timeSinceLastFullChargeGroupBox);
            this.Controls.Add(this.startCheckBox);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.upTimeLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Instant Background Uploader";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.timeSinceLastFullChargeGroupBox.ResumeLayout(false);
            this.timeSinceLastFullChargeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

		public System.Windows.Forms.Label upTimeLabel;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.CheckBox startCheckBox;
        public System.Windows.Forms.Label usageLabel;
        public System.Windows.Forms.Label totalLabel;
        public System.Windows.Forms.Label hasBeenPluggedInLabel;
        public System.Windows.Forms.GroupBox timeSinceLastFullChargeGroupBox;
	}
}
