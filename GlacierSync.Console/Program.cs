using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

using GlacierSync.Common.Jobs;
using GlacierSync.Common.Utilities;

namespace GlacierSync.Console
{
    class Program
    {
		public static void Main(string[] args)
		{
			// Required Environment Variables
			var awsAccessKey = Environment.GetEnvironmentVariable ("AWS_ACCESS_KEY_ID");
			if (string.IsNullOrEmpty (awsAccessKey))
			{
				throw new ConfigurationErrorsException ("Missing environment variable: AWS_ACCESS_KEY_ID");
			}

			var awsSecretKey = Environment.GetEnvironmentVariable ("AWS_SECRET_ACCESS_KEY");
			if (string.IsNullOrEmpty(awsSecretKey))
			{
				throw new ConfigurationErrorsException ("Missing environment variable: AWS_SECRET_ACCESS_KEY");
			}

			// Required Config Values
			//TODO: Move these into command line parameters instead of config values
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

			var backup = new BackupToGlacier (directoryToArchive, backupFilePath, vaultName, archiveDescription, new ConsoleFeedback ());
			backup.Execute ();
		}
    }
}