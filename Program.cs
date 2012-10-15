using System;
using System.Windows.Forms;

namespace InstantBackgroundUploader
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			if (false == UploaderApplicationContext.releaseMode)
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new UploaderApplicationContext());
			}
			else
			{
				bool createdNew = true;
				using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, "shurcooL/InstantBackgroundUploader/" + UploaderApplicationContext.releaseMode, out createdNew))
				{
					if (createdNew)
					{
						Application.EnableVisualStyles();
						Application.SetCompatibleTextRenderingDefault(false);
						Application.Run(new UploaderApplicationContext());
					}
				}
				if (!createdNew)
					MessageBox.Show("Instant Background Uploader is already running.", "Instant Background Uploader", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}
}
