using System;
using Quartz;

namespace GlacierSync.Service
{
	public class BackupJobWrapper : IJob
	{
		#region IJob implementation

		public void Execute (IJobExecutionContext context)
		{
			//TODO: get necessary params from context

			//TODO: Create new backup, execute
		}

		#endregion
	}
}

