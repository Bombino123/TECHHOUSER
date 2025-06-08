using System;
using System.Text;

namespace dnlib.DotNet.Writer;

internal sealed class MetadataErrorContext
{
	private sealed class ErrorSource : IDisposable
	{
		private MetadataErrorContext context;

		private readonly ErrorSource originalValue;

		public object Value { get; }

		public ErrorSource(MetadataErrorContext context, object value)
		{
			this.context = context;
			Value = value;
			originalValue = context.source;
		}

		public void Dispose()
		{
			if (context != null)
			{
				context.source = originalValue;
				context = null;
			}
		}
	}

	private ErrorSource source;

	public MetadataEvent Event { get; set; }

	public IDisposable SetSource(object source)
	{
		return this.source = new ErrorSource(this, source);
	}

	public void Append(string errorLevel, ref string message, ref object[] args)
	{
		int num = 1;
		string text = source?.Value as string;
		IMDTokenProvider iMDTokenProvider = source?.Value as IMDTokenProvider;
		if (iMDTokenProvider != null)
		{
			num += 2;
		}
		int num2 = args.Length;
		StringBuilder stringBuilder = new StringBuilder(message);
		object[] array = new object[args.Length + num];
		Array.Copy(args, 0, array, 0, args.Length);
		if (stringBuilder.Length != 0 && stringBuilder[stringBuilder.Length - 1] != '.')
		{
			stringBuilder.Append('.');
		}
		stringBuilder.AppendFormat(" {0} occurred after metadata event {{{1}}}", errorLevel, num2);
		array[num2] = Event;
		if (iMDTokenProvider != null)
		{
			string text2 = ((iMDTokenProvider is TypeDef) ? "type" : ((iMDTokenProvider is FieldDef) ? "field" : ((iMDTokenProvider is MethodDef) ? "method" : ((iMDTokenProvider is EventDef) ? "event" : ((!(iMDTokenProvider is PropertyDef)) ? "???" : "property")))));
			string arg = text2;
			stringBuilder.AppendFormat(" during writing {0} '{{{1}}}' (0x{{{2}:X8}})", arg, num2 + 1, num2 + 2);
			array[num2 + 1] = iMDTokenProvider;
			array[num2 + 2] = iMDTokenProvider.MDToken.Raw;
		}
		else if (text != null)
		{
			stringBuilder.AppendFormat(" during writing {0}", text);
		}
		message = stringBuilder.Append('.').ToString();
		args = array;
	}
}
