using System;

namespace GlacierSync.Common.Utilities
{
	public class ConsoleFeedback : IFeedback
	{
		protected int progressUpdateLen = 0;
		protected string progressUpdate = "";

		public ConsoleFeedback ()
		{
		}

		#region IFeedback implementation
		public void WriteFeedback (string feedback)
		{
			System.Console.SetCursorPosition(0, 0);
			progressUpdate = feedback;
			progressUpdateLen = progressUpdate.Length;
			System.Console.Write(progressUpdate);
		}

		public void WriteFeedbackWithPercent(string feedback, int complete, int total)
		{
			System.Console.SetCursorPosition(0, 0);
			progressUpdateLen = progressUpdate.Length;
			progressUpdate = string.Format("\r{0} [{1}]",
			                               feedback,
			                               GetProgressBar(complete, total, 40)).PadRight(progressUpdateLen);
			System.Console.Write(progressUpdate);
		}

		public void WriteEndOperation(string feedback)
		{
			System.Console.WriteLine (feedback);
		}

		public void WriteErrorFeedback(string feedback)
		{
			System.Console.Error.WriteLine (feedback);
		}
		#endregion

		private string GetProgressBar(long current, long total, int len, char symbol = '=')
		{
			if (total == 0)
				return "".PadRight(len);

			var percentComplete = (decimal)current / total;
			var numSymbolsToDraw = (int)Math.Floor(percentComplete * len);
			var symbols = new char[numSymbolsToDraw];
			for (var i = 0; i < numSymbolsToDraw; i++)
			{
				symbols[i] = symbol;
			}
			return new string(symbols).PadRight(len);
		}
	}
}

