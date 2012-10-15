using System;
using System.Collections.Generic;
using System.Text;

namespace InstantBackgroundUploader
{
	class FileSizeWorker
	{
		private System.Drawing.Image image = null;

		public FileSizeWorker(System.Drawing.Image image)
		{
			this.image = image;
		}

		public void Run(object parameter)
		{
			long jpegSize, pngSize;

			System.IO.Stream fileStream = new System.IO.MemoryStream();
			image.Save(fileStream, System.Drawing.Imaging.ImageFormat.Jpeg);
			jpegSize = fileStream.Length;

			fileStream = new System.IO.MemoryStream();
			image.Save(fileStream, System.Drawing.Imaging.ImageFormat.Png);
			pngSize = fileStream.Length;

			((UploaderApplicationContext)parameter).setFileSizes(jpegSize, pngSize);
		}
	}
}
