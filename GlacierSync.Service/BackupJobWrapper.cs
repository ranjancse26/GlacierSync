using System;

using Quartz;

using GlacierSync.Common;
using GlacierSync.Common.Utilities;
using GlacierSync.Common.Jobs;

namespace GlacierSync.Service
{
	public class BackupJobWrapper : IJob
	{
		#region IJob implementation

		public void Execute (IJobExecutionContext context)
		{
			var backupConfig = (BackupConfiguration)context.JobDetail.JobDataMap["backupConfig"];

			var backup = new BackupToGlacier (backupConfig, new NullFeedback ());
			backup.Execute ();
		}

		#endregion
	}
}

