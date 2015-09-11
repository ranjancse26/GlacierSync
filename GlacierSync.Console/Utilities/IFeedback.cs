using System;

namespace GlacierSync.Console
{
	public interface IFeedback
	{
		void WriteFeedback (string feedback);
		void WriteFeedbackWithPercent (string feedback, int complete, int total);
		void WriteEndOperation(string feedback);
		void WriteErrorFeedback(string feedback);
	}
}

