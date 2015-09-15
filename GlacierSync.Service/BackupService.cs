using System;
using System.Configuration;
using System.IO;

using Quartz;
using Quartz.Impl;

namespace GlacierSync.Service
{
	public class BackupService
	{
		private IScheduler scheduler;

		public BackupService ()
		{
			var backupConfig = GlacierSync.Common.Utilities.ConfigurationValidator.ValidateConfiguration (new string[0]);

			ISchedulerFactory schedulerFactory = new StdSchedulerFactory ();
			scheduler = schedulerFactory.GetScheduler ();

			var crontab = "0 0/1 * * * ?";

			var trigger = TriggerBuilder.Create ()
					.WithIdentity ("cron based trigger", "triggers")
					.StartNow ()
					.WithCronSchedule (crontab)
					.WithDescription ("cron based trigger")
					.Build ();

			var data = new JobDataMap ();
			data.Put ("backupConfig", backupConfig);

			var job = JobBuilder.Create<BackupJobWrapper> ()
					.WithIdentity ("GlacierSync Backup")
					.UsingJobData (data)
					.Build ();

			scheduler.ScheduleJob (job, trigger);
		}

		public bool Start()
		{ 
			scheduler.Start ();
			return true; 
		}
		public bool Stop()
		{
			scheduler.Shutdown ();
			return true;
		}
	}
}

