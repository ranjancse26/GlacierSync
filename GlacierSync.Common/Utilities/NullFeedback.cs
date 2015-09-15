using System;

namespace GlacierSync.Common.Utilities
{
	public class NullFeedback : IFeedback
	{
		#region IFeedback implementation

		public void WriteFeedback (string feedback)
		{
		}

		public void WriteFeedbackWithPercent (string feedback, int complete, int total)
		{
		}

		public void WriteEndOperation (string feedback)
		{
		}

		public void WriteErrorFeedback (string feedback)
		{
		}

		#endregion
	}
}

