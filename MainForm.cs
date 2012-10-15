using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InstantBackgroundUploader
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

			versionLabel.Text = UploaderApplicationContext.version;
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			//startCheckBox.Enabled = !System.Diagnostics.Process.GetCurrentProcess().Parent().ProcessName.Equals("devenv");
			startCheckBox.Enabled = UploaderApplicationContext.releaseMode;
			if (startCheckBox.Enabled)
				startCheckBox.Checked = Util.IsAutoStartEnabled("Instant Background Uploader");
		}

		private void startCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!startCheckBox.Enabled) return;

			if (startCheckBox.Checked)
			{
				Util.SetAutoStart("Instant Background Uploader", UploaderApplicationContext.executableLocation);
			}
			else
			{
				Util.UnSetAutoStart("Instant Background Uploader");
			}
		}
    }
}
