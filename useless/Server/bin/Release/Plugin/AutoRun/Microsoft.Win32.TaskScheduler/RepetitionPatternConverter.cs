using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Win32.TaskScheduler.Properties;

namespace Microsoft.Win32.TaskScheduler;

internal sealed class RepetitionPatternConverter : TypeConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (!(destinationType == typeof(string)))
		{
			return base.CanConvertTo(context, destinationType);
		}
		return true;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		RepetitionPattern repetitionPattern = (RepetitionPattern)value;
		if (destinationType != typeof(string))
		{
			return base.ConvertTo(context, culture, value, destinationType);
		}
		if (repetitionPattern.Interval == TimeSpan.Zero)
		{
			return "";
		}
		string arg = ((repetitionPattern.Duration == TimeSpan.Zero) ? Resources.TriggerDuration0 : string.Format(Resources.TriggerDurationNot0Short, Trigger.GetBestTimeSpanString(repetitionPattern.Duration)));
		return string.Format(Resources.TriggerRepetitionShort, Trigger.GetBestTimeSpanString(repetitionPattern.Interval), arg);
	}
}
