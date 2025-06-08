using System.Globalization;
using System.IO;
using System.Text;

namespace System.Data.SQLite.EF6;

internal class SqlWriter : StringWriter
{
	private int indent = -1;

	private bool atBeginningOfLine = true;

	internal int Indent
	{
		get
		{
			return indent;
		}
		set
		{
			indent = value;
		}
	}

	public SqlWriter(StringBuilder b)
		: base(b, CultureInfo.InvariantCulture)
	{
	}

	public override void Write(string value)
	{
		if (value == "\r\n")
		{
			base.WriteLine();
			atBeginningOfLine = true;
			return;
		}
		if (atBeginningOfLine)
		{
			if (indent > 0)
			{
				base.Write(new string('\t', indent));
			}
			atBeginningOfLine = false;
		}
		base.Write(value);
	}

	public override void WriteLine()
	{
		base.WriteLine();
		atBeginningOfLine = true;
	}
}
