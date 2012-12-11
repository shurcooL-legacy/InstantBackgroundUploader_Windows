using System;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using System.Security.Cryptography;

namespace InstantBackgroundUploader
{
	class UploaderApplicationContext : ApplicationContext
	{
		public const bool releaseMode = false;
		public const string version = "v0.033";

		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.ContextMenu contextMenu;
		private System.Windows.Forms.MenuItem uploadMenuItem;
		private System.Windows.Forms.MenuItem uploadAsPngMenuItem;
		private System.Windows.Forms.MenuItem showMenuItem;
		private System.Windows.Forms.MenuItem exitMenuItem;
		private System.Windows.Forms.Form mainForm;
		private System.Windows.Forms.Timer timerTime;

		private long upTimeSeconds = 0;
		private long usageSeconds = 0;
		private long totalSeconds = 0;

		private bool hasBeenPluggedIn = false;

		private DateTime sleptAt;

		private PowerLineStatus lastPowerLineStatus = SystemInformation.PowerStatus.PowerLineStatus;

		private string imageUrl;

		private Thread fileSizeThread;

		// Get the application configuration file
		Configuration config = null;

		private System.Net.WebClient webClient = new System.Net.WebClient();
		public static string executableLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
		private bool performedUpdate = false;

		public UploaderApplicationContext()
		{
			// TEMPORARY MAINTENANCE: Keep around for a few weeks after 2011/02/3
			// Clean up from version 0.025 and lower, delete old config file
			try { File.Delete(executableLocation + ".config"); } catch (Exception) {}
			try { File.Delete(executableLocation + ".old_version"); } catch (Exception) {}

			InitializeContext();

			Microsoft.Win32.SystemEvents.SessionEnding += new Microsoft.Win32.SessionEndingEventHandler(SystemEvents_SessionEnding);
			Microsoft.Win32.SystemEvents.PowerModeChanged += new Microsoft.Win32.PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);

			string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
			configFileMap.ExeConfigFilename = Path.Combine(localData, "shurcooL/InstantBackgroundUploader/InstantBackgroundUploader.local.config");
			config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

			if (null != config.AppSettings.Settings["Usage Time"])
				usageSeconds = Convert.ToInt64(config.AppSettings.Settings["Usage Time"].Value);
			if (null != config.AppSettings.Settings["Total Time"])
				totalSeconds = Convert.ToInt64(config.AppSettings.Settings["Total Time"].Value);
			if (null != config.AppSettings.Settings["Has Been Plugged In"])
				hasBeenPluggedIn = Convert.ToBoolean(config.AppSettings.Settings["Has Been Plugged In"].Value);
			if (null != config.AppSettings.Settings["Previous BatteryLifePercent"]) {
				double previousBatteryLifePercent = Convert.ToDouble(config.AppSettings.Settings["Previous BatteryLifePercent"].Value);

				if (SystemInformation.PowerStatus.BatteryLifePercent > previousBatteryLifePercent + 0.03)
				{
					hasBeenPluggedIn = true;
				}
			}

			if (SystemInformation.PowerStatus.BatteryLifePercent >= 0.95)
			{
				// Battery is almost fully charged at start up, reset Usage and Total counters
				usageSeconds = 0;
				totalSeconds = 0;
				hasBeenPluggedIn = false;
			}

			// Check if we're inside Dropbox
			/*try {
				string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				TextReader tr = new StreamReader(Path.Combine(appData, "Dropbox/host.db"));
				tr.ReadLine();		// Skip first line
				MessageBox.Show(DecodeFrom64(tr.ReadLine()) + "\n" + executableLocation);
				tr.Close();
			} catch (Exception) {}*/

			// Compute SHA1 hash
			/*try {
				long exeLength = new FileInfo(executableLocation).Length;
				byte[] exeData = File.ReadAllBytes(executableLocation);
				string exeHash = SHA1Encrypt(exeData);
				MessageBox.Show("Length: " + exeLength + "\n" + exeHash);
			} catch (Exception) {}*/

			if (!releaseMode) ShowMainForm();
		}

		private string DecodeFrom64(string encodedData)
		{
			byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
			string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
			return returnValue;
		}

		public static string SHA1Encrypt(byte[] inputData)
		{
			string ret = String.Empty;

			//Setup crypto
			SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
			//Encrypt
			byte[] data = sha1Hasher.ComputeHash(inputData);
			//Convert from byte 2 hex
			for (int i = 0; i < data.Length; i++)
			{
				ret += data[i].ToString("x2").ToLower();
			}
			//Return encoded string
			return ret;
		}

		void SystemEvents_SessionEnding(object sender, Microsoft.Win32.SessionEndingEventArgs e)
		{
			Dispose(true);
		}

		void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
		{
			if (e.Mode == Microsoft.Win32.PowerModes.StatusChange)
			{
				PowerStatus powerStatus = SystemInformation.PowerStatus;

                /*string str = "lastPowerLineStatus: " + lastPowerLineStatus + "\n"
                    "\n" +
                    "powerStatus:\n" +
                    "powerStatus.PowerLineStatus: " + powerStatus.PowerLineStatus + "\n" +
                    "powerStatus.BatteryLifeRemaining: " + powerStatus.BatteryLifeRemaining + "\n" +
                    "powerStatus.BatteryLifePercent: " + powerStatus.BatteryLifePercent + "\n" +
                    "powerStatus.BatteryFullLifetime: " + powerStatus.BatteryFullLifetime + "\n" +
                    "powerStatus.BatteryChargeStatus: " + powerStatus.BatteryChargeStatus + "\n";
				MessageBox.Show(str);*/

				if (powerStatus.PowerLineStatus == PowerLineStatus.Offline && lastPowerLineStatus == PowerLineStatus.Online && powerStatus.BatteryLifePercent >= 0.95)
				{
					// Unplugged power event, battery almost fully charged, reset Usage and Total counters
					usageSeconds = 0;
					totalSeconds = 0;
					hasBeenPluggedIn = false;
				}
				if (powerStatus.PowerLineStatus == PowerLineStatus.Online && lastPowerLineStatus == PowerLineStatus.Offline)
				{
					// Plugged-in power event
					hasBeenPluggedIn = true;
					UpdateLabelTime();
				}
				lastPowerLineStatus = powerStatus.PowerLineStatus;
			}
			else if (e.Mode == Microsoft.Win32.PowerModes.Suspend)
			{
				// TODO: Distinguish between Hibernate and Sleep, and not increase the totalSeconds counter after a Hibernate event
				sleptAt = DateTime.Now;
			}
			else if (e.Mode == Microsoft.Win32.PowerModes.Resume)
			{
				// TODO: Distinguish between Hibernate and Sleep, and not increase the totalSeconds counter after a Hibernate event
				DateTime wokeUpAt = DateTime.Now;
				totalSeconds += (long)(wokeUpAt - sleptAt).TotalSeconds;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			Microsoft.Win32.SystemEvents.PowerModeChanged -= new Microsoft.Win32.PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
			Microsoft.Win32.SystemEvents.SessionEnding -= new Microsoft.Win32.SessionEndingEventHandler(SystemEvents_SessionEnding);

			SaveConfigurationFile();

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void SaveConfigurationFile()
		{
			config.AppSettings.Settings.Clear();

			// Modify an entry in appSettings
			config.AppSettings.Settings.Add("Usage Time", usageSeconds.ToString());
			config.AppSettings.Settings.Add("Total Time", totalSeconds.ToString());
			config.AppSettings.Settings.Add("Has Been Plugged In", hasBeenPluggedIn.ToString());
			config.AppSettings.Settings.Add("Previous BatteryLifePercent", SystemInformation.PowerStatus.BatteryLifePercent.ToString());

			// Save the configuration file
			config.Save(ConfigurationSaveMode.Full);

			// Force a reload of the changed section, this makes the new values available for reading
			ConfigurationManager.RefreshSection("appSettings");
		}

		private void InitializeContext()
		{
			this.components = new System.ComponentModel.Container();
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenu = new System.Windows.Forms.ContextMenu();
			this.uploadMenuItem = new System.Windows.Forms.MenuItem();
			this.uploadAsPngMenuItem = new System.Windows.Forms.MenuItem();
			this.showMenuItem = new System.Windows.Forms.MenuItem();
			this.exitMenuItem = new System.Windows.Forms.MenuItem();
			this.timerTime = new System.Windows.Forms.Timer(this.components);
			// 
			// notifyIcon
			// 
			this.notifyIcon.ContextMenu = this.contextMenu;
			this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
			this.notifyIcon.BalloonTipClicked += new EventHandler(notifyIcon_BalloonTipClicked);
			this.notifyIcon.Icon = new System.Drawing.Icon(typeof(UploaderApplicationContext), "IBU.ico");
			this.notifyIcon.Text = "Instant Background Uploader";
			this.notifyIcon.Visible = true;
			// 
			// contextMenu
			// 
			this.contextMenu.MenuItems.AddRange(new MenuItem[] { uploadMenuItem, uploadAsPngMenuItem, showMenuItem, exitMenuItem });
			this.contextMenu.Popup += new System.EventHandler(this.contextMenu_Popup);
			// 
			// uploadMenuItem
			// 
			this.uploadMenuItem.Index = 0;
			this.uploadMenuItem.Text = "Upload from Clipboard";
			this.uploadMenuItem.DefaultItem = true;
			this.uploadMenuItem.Click += new System.EventHandler(this.uploadMenuItem_Click);
			// 
			// uploadAsPngMenuItem
			// 
			this.uploadAsPngMenuItem.Index = 1;
			this.uploadAsPngMenuItem.Text = "Upload from Clipboard as PNG (lossless quality)";
			this.uploadAsPngMenuItem.Click += new System.EventHandler(this.uploadAsPngMenuItem_Click);
			// 
			// showMenuItem
			// 
			this.showMenuItem.Index = 2;
			this.showMenuItem.Text = "Settings";
			this.showMenuItem.Click += new System.EventHandler(this.showMenuItem_Click);
			// 
			// exitMenuItem
			// 
			this.exitMenuItem.Index = 3;
			this.exitMenuItem.Text = "Exit";
			this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
			//
			// timerTime
			//
			this.timerTime.Enabled = true;
			this.timerTime.Interval = 1000;
			this.timerTime.Tick += new System.EventHandler(this.timerTime_Tick);
		}

		void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
		{
			try {
				System.Diagnostics.Process.Start(imageUrl);
			} catch (Exception) {}
		}

		public void CompletedImageUpload(string imageUrl)
		{
			this.imageUrl = imageUrl;
			/*Clipboard.SetText(imageUrl);
			this.notifyIcon.ShowBalloonTip(3000, "Image Upload Complete", "Image was uploaded successfully,\nthe URL is now in your Clipboard.\n" + imageUrl, ToolTipIcon.Info);*/
		}

		private void notifyIcon_DoubleClick(object sender, EventArgs e)
		{
			ShowMainForm();
		}

		public void setFileSizes(long jpegSize, long pngSize)
		{
			uploadMenuItem.Text = "Upload from Clipboard" + " (" + jpegSize / 1024 + " KB)";
			uploadAsPngMenuItem.Text = "Upload from Clipboard as PNG (lossless quality)" + " (" + pngSize / 1024 + " KB)";
		}

		private void contextMenu_Popup(object sender, EventArgs e)
		{
			if (Clipboard.ContainsImage()) {
				if (null != fileSizeThread) fileSizeThread.Abort();

				uploadMenuItem.Enabled = true;
				//uploadMenuItem.Text = "Upload from Clipboard" + " (" + GetImageSize(System.Drawing.Imaging.ImageFormat.Jpeg) / 1024 + " KB)";
				uploadMenuItem.Text = "Upload from Clipboard" + "          ";

				uploadAsPngMenuItem.Visible = true;
				//uploadAsPngMenuItem.Text = "Upload from Clipboard as PNG (lossless quality)" + " (" + GetImageSize(System.Drawing.Imaging.ImageFormat.Png) / 1024 + " KB)";
				uploadAsPngMenuItem.Text = "Upload from Clipboard as PNG (lossless quality)" + "          ";

				FileSizeWorker fileSizeWorker = new FileSizeWorker(Clipboard.GetImage());
				fileSizeThread = new Thread(new ParameterizedThreadStart(fileSizeWorker.Run));
				fileSizeThread.Start(this);
			} else {
				uploadMenuItem.Enabled = false;
				uploadMenuItem.Text = "(No Image in Clipboard)";

				uploadAsPngMenuItem.Visible = false;
			}
		}

		private long GetImageSize(System.Drawing.Imaging.ImageFormat imageFormat)
		{
			Stream fileStream = new MemoryStream();
			Clipboard.GetImage().Save(fileStream, imageFormat);
			return fileStream.Length;
		}

		private void timerTime_Tick(object sender, EventArgs e)
		{
			++upTimeSeconds;
			++usageSeconds;
			++totalSeconds;

			if (mainForm != null)
				UpdateLabelTime();
		}

		private string SecondsToTime(long seconds)
		{
			if (seconds < 60)
				return seconds.ToString() + " seconds";
			else if (seconds < 60 * 60)
				return (seconds / 60).ToString() + " minutes " + (seconds % 60).ToString() + " seconds";
			else
				return (seconds / (60 * 60)).ToString() + " hours " + ((seconds / 60) % 60).ToString() + " minutes";
		}

		private void UpdateLabelTime()
		{
			((MainForm)mainForm).upTimeLabel.Text = "Up Time: " + SecondsToTime(upTimeSeconds);
			((MainForm)mainForm).usageLabel.Text = "Usage: " + SecondsToTime(usageSeconds);
			((MainForm)mainForm).totalLabel.Text = "Total: " + SecondsToTime(totalSeconds);
			System.Drawing.Size newSize = ((MainForm)mainForm).timeSinceLastFullChargeGroupBox.Size; newSize.Height = !hasBeenPluggedIn ? 56 : 77;
			((MainForm)mainForm).timeSinceLastFullChargeGroupBox.Size = newSize;
		}

		private void UploadFromClipboard(System.Drawing.Imaging.ImageFormat imageFormat, String fileName)
		{
			if (Clipboard.ContainsImage())
			{
				imageUrl = "";

				Stream fileStream = new MemoryStream();
				Clipboard.GetImage().Save(fileStream, imageFormat);

				UploadWorker uploadWorker = new UploadWorker(fileStream, fileName);
				Thread uploadThread = new Thread(new ParameterizedThreadStart(uploadWorker.Run));

				uploadThread.Start(this);
				uploadThread.Join();

				if (imageUrl.Length > 0)
				{
					Clipboard.SetText(imageUrl);
					this.notifyIcon.ShowBalloonTip(3000, "Image Upload Complete", "Image was uploaded successfully,\nthe URL is now in your Clipboard:\n" + imageUrl, ToolTipIcon.Info);

					if (!performedUpdate && releaseMode) try {
						webClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(webClient_DownloadFileCompleted);
						webClient.DownloadFileAsync(new Uri("https://github.com/downloads/shurcooL/InstantBackgroundUploader_Windows/InstantBackgroundUploader.exe"), Path.Combine(Path.GetTempPath(), "InstantBackgroundUploader.exe.update"));
					} catch (Exception) {}
				}
			}
			else
			{
				MessageBox.Show("There is no image in Clipboard to upload.", "Instant Background Uploader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		void webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			try {
				// If there's an error downloading new version, delete any potential update file and return
				if (null != e.Error) {
					try { File.Delete(Path.Combine(Path.GetTempPath(), "InstantBackgroundUploader.exe.update")); } catch (Exception) {}
					return;
				}

				bool updateSameAsExe = Compare(Path.Combine(Path.GetTempPath(), "InstantBackgroundUploader.exe.update"), executableLocation);		// See if update file is different
				if (updateSameAsExe)
				{
					try { File.Delete(Path.Combine(Path.GetTempPath(), "InstantBackgroundUploader.exe.update")); } catch (Exception) {}				// Delete the update file, don't do anything else
				}
				else
				{
					string oldVersionFileName = Path.GetTempFileName();																				// Generate an unused temp filename for old_version
					try { File.Delete(oldVersionFileName); } catch (Exception) {}																	// Delete old_version in Temp Folder
					File.Move(executableLocation, oldVersionFileName);																				// Try to move current exe to old_version in Temp Folder
					if (File.Exists(executableLocation))																							// If copy happened instead of move
					{
						try { File.Delete(oldVersionFileName); } catch (Exception) {}																// Delete the copied file
						try { File.Delete(executableLocation + ".old_version"); } catch (Exception) {}												// Delete any old_version in Current Folder
						File.Move(executableLocation, executableLocation + ".old_version");															// Move current exe to old_version in Current Folder
					}
					File.Move(Path.Combine(Path.GetTempPath(), "InstantBackgroundUploader.exe.update"), executableLocation);						// Move update to current exe
				}

				performedUpdate = true;
			} catch (Exception) {}
			//} catch (Exception exception) {MessageBox.Show("Exception caught in webClient_DownloadFileCompleted():\n" + exception.Message + "\n" + exception.StackTrace, "Instant Background Uploader", MessageBoxButtons.OK, MessageBoxIcon.Error);}
		}

		/// <summary>
		/// Compares two files.
		/// </summary>
		/// <param name="fileName1">fileName of first file</param>
		/// <param name="fileName2">fileName of second file</param>
		/// <returns>true if two files are the same</returns>
		private bool Compare(string fileName1, string fileName2)
		{
			long f1Length = new FileInfo(fileName1).Length;
			long f2Length = new FileInfo(fileName2).Length;

			if (f1Length != f2Length)
				return false;				// Files are different if file size is different
			else
			{
				byte[] f1Data = File.ReadAllBytes(fileName1);
				byte[] f2Data = File.ReadAllBytes(fileName2);
				for (long i = 0; i < f1Length; ++i)
				{
					if (f1Data[i] != f2Data[i])
						return false;		// Or at least one different byte
				}
			}

			return true;					// Otherwise they're the same
		}

		private void uploadMenuItem_Click(object sender, EventArgs e)
		{
			UploadFromClipboard(System.Drawing.Imaging.ImageFormat.Jpeg, "Image.jpg");
		}

		private void uploadAsPngMenuItem_Click(object sender, EventArgs e)
		{
			UploadFromClipboard(System.Drawing.Imaging.ImageFormat.Png, "Image.png");
		}

		private void showMenuItem_Click(object sender, EventArgs e)
		{
			ShowMainForm();
		}

		private void exitMenuItem_Click(object sender, EventArgs e)
		{
			ExitThread();
		}

		private void ShowMainForm()
		{
			if (mainForm == null)
			{
				mainForm = new MainForm();
				UpdateLabelTime();
				mainForm.Show();
				mainForm.Activate();

				((MainForm)mainForm).upTimeLabel.DoubleClick += new System.EventHandler(this.upTimeLabel_DoubleClick);
				((MainForm)mainForm).usageLabel.DoubleClick += new System.EventHandler(this.usageLabel_DoubleClick);
				((MainForm)mainForm).totalLabel.DoubleClick += new System.EventHandler(this.totalLabel_DoubleClick);
                ((MainForm)mainForm).hasBeenPluggedInLabel.DoubleClick += new System.EventHandler(this.hasBeenPluggedInLabel_DoubleClick);
				mainForm.Closed += new EventHandler(mainForm_Closed);
			}
			else
			{
				mainForm.WindowState = FormWindowState.Normal;
				mainForm.Activate();
			}
		}

		private void upTimeLabel_DoubleClick(object sender, EventArgs e)
		{
			if (DialogResult.OK == MessageBox.Show("Are you sure you'd like to reset the Up Time counter?", "Instant Background Uploader", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
			{
				timerTime.Enabled = false;
				upTimeSeconds = 0;
				UpdateLabelTime();
				timerTime.Enabled = true;
			}
		}

		private void usageLabel_DoubleClick(object sender, EventArgs e)
		{
			if (DialogResult.OK == MessageBox.Show("Are you sure you'd like to reset the Usage counter?", "Instant Background Uploader", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
			{
				timerTime.Enabled = false;
				usageSeconds = 0;
				UpdateLabelTime();
				timerTime.Enabled = true;
			}
		}

		private void totalLabel_DoubleClick(object sender, EventArgs e)
		{
			if (DialogResult.OK == MessageBox.Show("Are you sure you'd like to reset the Total counter?", "Instant Background Uploader", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
			{
				timerTime.Enabled = false;
				totalSeconds = 0;
				UpdateLabelTime();
				timerTime.Enabled = true;
			}
		}

        private void hasBeenPluggedInLabel_DoubleClick(object sender, EventArgs e)
        {
            if (DialogResult.OK == MessageBox.Show("Are you sure you'd like to reset that it has been plugged in?", "Instant Background Uploader", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
            {
                timerTime.Enabled = false;
                hasBeenPluggedIn = false;
                UpdateLabelTime();
                timerTime.Enabled = true;
            }
        }

		private void mainForm_Closed(object sender, EventArgs e)
		{
			this.mainForm = null;

			if (!releaseMode) exitMenuItem_Click(null, null);
		}
	}
}
