using System;
using Topshelf;

namespace GlacierSync.Service
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			HostFactory.Run (configurator => {
			                 configurator.Service<BackupService>(s =>
							 {
								s.ConstructUsing(service => new BackupService());
								s.WhenStarted(service => service.Start());
								s.WhenStopped(service => service.Stop());
							 });
			                 configurator.RunAsPrompt();
			                 configurator.SetDescription("Runs a backup/zip/glacier push on a schedule");
			                 configurator.SetServiceName("GlacierSync.Backup.Service");
			                 configurator.SetDisplayName("GlacierSync Backup Service");
			                 configurator.StartAutomatically();
			             });
		}
	}
}
