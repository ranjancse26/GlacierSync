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

			var backup = new BackupToGlacier (GlacierSync.Common.Utilities.ConfigurationValidator.ValidateConfiguration(args), new ConsoleFeedback ());
			backup.Execute ();
		}
    }
}