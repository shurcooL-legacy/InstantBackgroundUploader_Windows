using System;
using System.Windows.Forms;

namespace InstantBackgroundUploader
{
	class UploadWorker
	{
		private System.IO.Stream fileStream = null;
		private String fileStreamName;

		public UploadWorker(System.IO.Stream fileStream, String fileStreamName)
		{
			this.fileStream = fileStream;
			this.fileStreamName = fileStreamName;
		}

		public void Run(object parameter)
		{
			MultipartForm form;
            form = new MultipartForm("http://www.imageshack.us/upload_api.php");
            if (fileStreamName.EndsWith(".png"))
			    form.FileContentType = "image/png";
            else
                form.FileContentType = "image/jpeg";
			//form.setField("uploadtype", "on");
            //form.setField("transurl", "");
			//form.setField("email", "");
			//form.setField("MAX_FILE_SIZE", "13145728");
			//form.setField("refer", "");
			//form.setField("brand", "");
			//form.setField("optsize", "320x320");
            form.setField("key", "12DEFKTYa5517607af7de06ec6272205d57a9cf4");
			try {
				form.sendFile(fileStream, fileStreamName);
			} catch (Exception exception) {
				MessageBox.Show("Exception caught:\n" + exception.Message + "\n" + exception.StackTrace, "Instant Background Uploader", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Parse out the Image URL
			String response = form.ResponseText.ToString().ToLower();
/*System.IO.StreamWriter log = System.IO.File.CreateText("C:\\Users\\Dmitri\\Desktop\\IBU.log");
log.Write(response);
log.Close();*/
			String imageUrl = "";
			try
			{
				/*int index = response.IndexOf("forum code");
				int index2 = response.IndexOf("[img]", index);
				imageUrl = response.Substring(index2 + "[img]".Length, response.IndexOf("[/img]", index2 + "[img]".Length) - (index2 + "[img]".Length));*/

                int StartIndex = response.IndexOf("<image_link>") + "<image_link>".Length;
                int EndIndex = response.IndexOf("</image_link>", StartIndex);
                imageUrl = response.Substring(StartIndex, EndIndex - StartIndex);

				if (imageUrl.Length <= 0) throw new Exception();
			} catch (Exception) {
				MessageBox.Show("Couldn't parse image URL out of web response (probably failed to upload for some reason).", "Instant Background Uploader", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		    ((UploaderApplicationContext)parameter).CompletedImageUpload(imageUrl);
		}
	}
}
