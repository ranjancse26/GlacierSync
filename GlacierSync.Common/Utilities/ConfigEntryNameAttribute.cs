using System;

namespace GlacierSync.Common.Utilities
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
}

