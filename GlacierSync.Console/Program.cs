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
			var backupConfig = ConfigurationValidator.ValidateConfiguration (args);
			var backup = new BackupToGlacier (backupConfig, new ConsoleFeedback ());
			backup.Execute ();
		}
    }
}