using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Amazon.Glacier;
using Amazon.Glacier.Model;
using Amazon.Runtime;
using Ionic.Zip;

namespace GlacierSync.Console
{
	public class DirectoryZipper
	{
		protected readonly string DirectoryToArchive;
		protected readonly string BackupFilePath;
		protected readonly IFeedback FeedbackProvider;

		public DirectoryZipper (string directoryToArchive, string backupFilePath, IFeedback feedbackProvider)
		{
			DirectoryToArchive = directoryToArchive;
			BackupFilePath = backupFilePath;
			FeedbackProvider = feedbackProvider;
		}

		public void Execute()
		{
			using (var backup = new ZipFile())
			{
				backup.AddDirectory(DirectoryToArchive);

				backup.SaveProgress += WriteZipProgress;

				backup.Save(BackupFilePath);
			}
		}

		void WriteZipProgress(object sender, SaveProgressEventArgs e)
		{
			if (e.CurrentEntry != null)
			{
				if (e.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
				{
					var update = string.Format("\rSaving {0} of {1}",
					                           e.EntriesSaved, e.EntriesTotal);
					FeedbackProvider.WriteFeedbackWithPercent (update, e.EntriesSaved, e.EntriesTotal);
				}
			}
			if (e.EventType == ZipProgressEventType.Saving_Completed)
			{
				FeedbackProvider.WriteEndOperation ("Zipping Complete");
			}
		}
	}
}

