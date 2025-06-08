using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Data.Entity.Migrations.Utilities;

public class IndentedTextWriter : TextWriter
{
	public const string DefaultTabString = "    ";

	public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

	private readonly TextWriter _writer;

	private int _indentLevel;

	private bool _tabsPending;

	private readonly string _tabString;

	private readonly List<string> _cachedIndents = new List<string>();

	public override Encoding Encoding => _writer.Encoding;

	public override string NewLine
	{
		get
		{
			return _writer.NewLine;
		}
		set
		{
			_writer.NewLine = value;
		}
	}

	public int Indent
	{
		get
		{
			return _indentLevel;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			_indentLevel = value;
		}
	}

	public TextWriter InnerWriter => _writer;

	public IndentedTextWriter(TextWriter writer)
		: this(writer, "    ")
	{
	}

	public IndentedTextWriter(TextWriter writer, string tabString)
		: base(Culture)
	{
		_writer = writer;
		_tabString = tabString;
		_indentLevel = 0;
		_tabsPending = false;
	}

	public override void Close()
	{
		_writer.Close();
	}

	public override void Flush()
	{
		_writer.Flush();
	}

	protected virtual void OutputTabs()
	{
		if (_tabsPending)
		{
			_writer.Write(CurrentIndentation());
			_tabsPending = false;
		}
	}

	public virtual string CurrentIndentation()
	{
		if (_indentLevel <= 0 || string.IsNullOrEmpty(_tabString))
		{
			return string.Empty;
		}
		if (_indentLevel == 1)
		{
			return _tabString;
		}
		int num = _indentLevel - 2;
		string text = ((num < _cachedIndents.Count) ? _cachedIndents[num] : null);
		if (text == null)
		{
			text = BuildIndent(_indentLevel);
			if (num == _cachedIndents.Count)
			{
				_cachedIndents.Add(text);
			}
			else
			{
				for (int i = _cachedIndents.Count; i <= num; i++)
				{
					_cachedIndents.Add(null);
				}
				_cachedIndents[num] = text;
			}
		}
		return text;
	}

	private string BuildIndent(int numberOfIndents)
	{
		StringBuilder stringBuilder = new StringBuilder(numberOfIndents * _tabString.Length);
		for (int i = 0; i < numberOfIndents; i++)
		{
			stringBuilder.Append(_tabString);
		}
		return stringBuilder.ToString();
	}

	public override void Write(string value)
	{
		OutputTabs();
		_writer.Write(value);
		if (value != null && (value.Equals("\r\n", StringComparison.Ordinal) || value.Equals("\n", StringComparison.Ordinal)))
		{
			_tabsPending = true;
		}
	}

	public override void Write(bool value)
	{
		OutputTabs();
		_writer.Write(value);
	}

	public override void Write(char value)
	{
		OutputTabs();
		_writer.Write(value);
	}

	public override void Write(char[] buffer)
	{
		OutputTabs();
		_writer.Write(buffer);
	}

	public override void Write(char[] buffer, int index, int count)
	{
		OutputTabs();
		_writer.Write(buffer, index, count);
	}

	public override void Write(double value)
	{
		OutputTabs();
		_writer.Write(value);
	}

	public override void Write(float value)
	{
		OutputTabs();
		_writer.Write(value);
	}

	public override void Write(int value)
	{
		OutputTabs();
		_writer.Write(value);
	}

	public override void Write(long value)
	{
		OutputTabs();
		_writer.Write(value);
	}

	public override void Write(object value)
	{
		OutputTabs();
		_writer.Write(value);
	}

	public override void Write(string format, object arg0)
	{
		OutputTabs();
		_writer.Write(format, arg0);
	}

	public override void Write(string format, object arg0, object arg1)
	{
		OutputTabs();
		_writer.Write(format, arg0, arg1);
	}

	public override void Write(string format, params object[] arg)
	{
		OutputTabs();
		_writer.Write(format, arg);
	}

	public void WriteLineNoTabs(string value)
	{
		_writer.WriteLine(value);
	}

	public override void WriteLine(string value)
	{
		OutputTabs();
		_writer.WriteLine(value);
		_tabsPending = true;
	}

	public override void WriteLine()
	{
		OutputTabs();
		_writer.WriteLine();
		_tabsPending = true;
	}

	public override void WriteLine(bool value)
	{
		OutputTabs();
		_writer.WriteLine(value);
		_tabsPending = true;
	}

	public override void WriteLine(char value)
	{
		OutputTabs();
		_writer.WriteLine(value);
		_tabsPending = true;
	}

	public override void WriteLine(char[] buffer)
	{
		OutputTabs();
		_writer.WriteLine(buffer);
		_tabsPending = true;
	}

	public override void WriteLine(char[] buffer, int index, int count)
	{
		OutputTabs();
		_writer.WriteLine(buffer, index, count);
		_tabsPending = true;
	}

	public override void WriteLine(double value)
	{
		OutputTabs();
		_writer.WriteLine(value);
		_tabsPending = true;
	}

	public override void WriteLine(float value)
	{
		OutputTabs();
		_writer.WriteLine(value);
		_tabsPending = true;
	}

	public override void WriteLine(int value)
	{
		OutputTabs();
		_writer.WriteLine(value);
		_tabsPending = true;
	}

	public override void WriteLine(long value)
	{
		OutputTabs();
		_writer.WriteLine(value);
		_tabsPending = true;
	}

	public override void WriteLine(object value)
	{
		OutputTabs();
		_writer.WriteLine(value);
		_tabsPending = true;
	}

	public override void WriteLine(string format, object arg0)
	{
		OutputTabs();
		_writer.WriteLine(format, arg0);
		_tabsPending = true;
	}

	public override void WriteLine(string format, object arg0, object arg1)
	{
		OutputTabs();
		_writer.WriteLine(format, arg0, arg1);
		_tabsPending = true;
	}

	public override void WriteLine(string format, params object[] arg)
	{
		OutputTabs();
		_writer.WriteLine(format, arg);
		_tabsPending = true;
	}

	[CLSCompliant(false)]
	public override void WriteLine(uint value)
	{
		OutputTabs();
		_writer.WriteLine(value);
		_tabsPending = true;
	}
}
