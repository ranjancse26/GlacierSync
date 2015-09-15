using System;

using CommandLine;
using CommandLine.Text;

namespace GlacierSync.Common
{
	public class BackupConfiguration
	{
		[ConfigEntryName("AWS_ACCESS_KEY_ID", true, "Please specify the 'AWS_ACCESS_KEY_ID' setting in the application configuration file, environment variable, or command line argument.")]
		[Option('k', "awskeyid", Required = false, HelpText = "The AWS Access Key ID")]
		public string AWSAccessKeyId {get; set;}

		[ConfigEntryName("AWS_SECRET_ACCESS_KEY", true, "Please specify the 'AWS_SECRET_ACCESS_KEY' setting in the application configuration file, environment variable, or command line argument.")]
		[Option('s', "awssecretkey", Required = false, HelpText = "The AWS Secret Key")]
		public string AWSSecretAccessKey {get; set;}

		[ConfigEntryName("DIRECTORY", true, "Please specify the 'DIRECTORY' setting in the application configuration file, environment variable, or command line argument.")]
		[Option('d', "directory", Required = false, HelpText = "The directory to archive")]
		public string DirectoryToArchive {get; set;}

		[ConfigEntryName("VAULT_NAME", true, "Please specify the 'VAULT_NAME' setting in the application configuration file.")]
		[Option('v', "vaultname", Required = false, HelpText = "The name of the vault in AWS Glacier")]
		public string VaultName {get; set;}

		[ConfigEntryName("BACKUP_FILE_PATH", false)]
		[Option('b', "backupfilepath", Required = false, HelpText = "The output file (before data is uploaded)")]
		public string BackupFilePath {get; set;}

		[ConfigEntryName("ARCHIVE_DESCRIPTION", false)]
		[Option('a', "archivedescription", Required = false, HelpText = "The description showin in Glacier for your backup")]
		public string ArchiveDescription {get; set;}

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild (this, (HelpText current) => HelpText.DefaultParsingErrorsHandler (this, current));
		}
	}
}

