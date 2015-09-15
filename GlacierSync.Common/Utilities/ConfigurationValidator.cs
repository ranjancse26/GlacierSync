using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace GlacierSync.Common
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ConfigEntryNameAttribute : Attribute
	{
		public string Name { get; set; }
		public string ValueMissingErrorMessage { get; set; }
		public bool Required { get; set; }

		public ConfigEntryNameAttribute(string name, bool required, string valueMissingMessage = null)
		{
			Name = name;
			Required = required;
			ValueMissingErrorMessage = valueMissingMessage;
		}
	}

	public static class ConfigurationValidator
	{
		/// <summary>
		/// Validates the configuration based on system environment variables, the application config file, and command line arguments
		/// </summary>
		/// <returns>The configuration to run the backup... or throws an error to let you know something is missing.</returns>
		/// <param name="args">Command Line Arguments</param>
		public static BackupConfiguration ValidateConfiguration(string[] args)
		{
			var backupConfiguration = new BackupConfiguration ();
			if(CommandLine.Parser.Default.ParseArguments(args, backupConfiguration))
			{
				// Successfully parsed command line args... still need to look for any missing
			}

			var properties = backupConfiguration.GetType ().GetProperties ();
			foreach (var property in properties)
			{
				var value = property.GetValue (backupConfiguration, null);
				if (value == null)
				{
					var configEntry = (ConfigEntryNameAttribute)property.GetCustomAttributes (false).FirstOrDefault (a => a.GetType () == typeof(ConfigEntryNameAttribute));
					var configValue = GetConfigValue (configEntry.Name, configEntry.Required, configEntry.ValueMissingErrorMessage);
					property.SetValue (backupConfiguration, configValue, null);
				}
			}

			//These are two fields we can safely set to defaults if they are not configured elsewhere.
			if (string.IsNullOrEmpty(backupConfiguration.BackupFilePath))
			{
				backupConfiguration.BackupFilePath = Path.Combine(Path.GetTempPath(),
				                              Path.GetTempFileName()).Replace(".tmp", ".zip");
			}

			if (string.IsNullOrEmpty (backupConfiguration.ArchiveDescription))
			{
				string.Format ("Archive of {0}", backupConfiguration.DirectoryToArchive);
			}

			return backupConfiguration;
		}

		/// <summary>
		/// Gets a config value - environment variables, then looks to application config file, and last command line args. If command line args are present, they will override other values.
		/// </summary>
		/// <returns>The config value.</returns>
		/// <param name="configEntryName">Config entry name.</param>
		/// <param name="required">If set to <c>true</c> required.</param>
		private static string GetConfigValue(string configEntryName, bool required, string errorMessageIfmissing)
		{
			var configValue = Environment.GetEnvironmentVariable (configEntryName);
			if (string.IsNullOrEmpty (configValue))
			{
				configValue = ConfigurationManager.AppSettings [configEntryName];
				if (string.IsNullOrEmpty (configValue) && required)
				{
					throw new ConfigurationErrorsException (errorMessageIfmissing);
				}
			}

			return configValue;
		}
	}
}

