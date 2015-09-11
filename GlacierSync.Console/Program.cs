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
    class Program
    {
		public static void Main(string[] args)
		{
			// Required Config Values
			var directoryToArchive = ConfigurationManager.AppSettings["DirectoryToArchive"];
			if(string.IsNullOrEmpty(directoryToArchive))
				throw new ConfigurationErrorsException("Please specify the 'DirectoryToArchive' setting in the application configuration file.");

			var vaultName = ConfigurationManager.AppSettings["VaultName"];
			if (string.IsNullOrEmpty(vaultName))
				throw new ConfigurationErrorsException("Please specify the 'VaultName' setting in the application configuration file.");

			var backupFilePath = ConfigurationManager.AppSettings["BackupFilePath"];
			if (string.IsNullOrEmpty(backupFilePath))
			{
				backupFilePath = Path.Combine(Path.GetTempPath(),
				                              Path.GetTempFileName()).Replace(".tmp", ".zip");
			}

			var archiveDescription = ConfigurationManager.AppSettings["ArchiveDescription"];
			archiveDescription = (string.IsNullOrEmpty(archiveDescription))
				? string.Format("Archive of {0}", directoryToArchive)
					: archiveDescription;

			var backup = new BackupToGlacier (directoryToArchive, backupFilePath, vaultName, new ConsoleFeedback ());
			backup.Execute ();
		}
    }
}