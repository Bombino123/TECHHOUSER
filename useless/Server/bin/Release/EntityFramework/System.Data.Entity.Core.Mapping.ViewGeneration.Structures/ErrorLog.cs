using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class ErrorLog : InternalBase
{
	internal class Record : InternalBase
	{
		private EdmSchemaError m_mappingError;

		private List<Cell> m_sourceCells;

		private string m_debugMessage;

		internal EdmSchemaError Error => m_mappingError;

		internal Record(ViewGenErrorCode errorCode, string message, IEnumerable<LeftCellWrapper> wrappers, string debugMessage)
		{
			IEnumerable<Cell> inputCellsForWrappers = LeftCellWrapper.GetInputCellsForWrappers(wrappers);
			Init(errorCode, message, inputCellsForWrappers, debugMessage);
		}

		internal Record(ViewGenErrorCode errorCode, string message, Cell sourceCell, string debugMessage)
		{
			Init(errorCode, message, new Cell[1] { sourceCell }, debugMessage);
		}

		internal Record(ViewGenErrorCode errorCode, string message, IEnumerable<Cell> sourceCells, string debugMessage)
		{
			Init(errorCode, message, sourceCells, debugMessage);
		}

		internal Record(EdmSchemaError error)
		{
			m_debugMessage = error.ToString();
			m_mappingError = error;
		}

		private void Init(ViewGenErrorCode errorCode, string message, IEnumerable<Cell> sourceCells, string debugMessage)
		{
			m_sourceCells = new List<Cell>(sourceCells);
			CellLabel cellLabel = m_sourceCells[0].CellLabel;
			string sourceLocation = cellLabel.SourceLocation;
			int startLineNumber = cellLabel.StartLineNumber;
			int startLinePosition = cellLabel.StartLinePosition;
			string message2 = InternalToString(message, debugMessage, m_sourceCells, errorCode, isInvariant: false);
			m_debugMessage = InternalToString(message, debugMessage, m_sourceCells, errorCode, isInvariant: true);
			m_mappingError = new EdmSchemaError(message2, (int)errorCode, EdmSchemaErrorSeverity.Error, sourceLocation, startLineNumber, startLinePosition);
		}

		internal override void ToCompactString(StringBuilder builder)
		{
			builder.Append(m_debugMessage);
		}

		private static void GetUserLinesFromCells(IEnumerable<Cell> sourceCells, StringBuilder lineBuilder, bool isInvariant)
		{
			IOrderedEnumerable<Cell> orderedEnumerable = sourceCells.OrderBy((Cell cell) => cell.CellLabel.StartLineNumber, Comparer<int>.Default);
			bool flag = true;
			foreach (Cell item in orderedEnumerable)
			{
				if (!flag)
				{
					lineBuilder.Append(isInvariant ? EntityRes.GetString("ViewGen_CommaBlank") : ", ");
				}
				flag = false;
				lineBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", new object[1] { item.CellLabel.StartLineNumber });
			}
		}

		private static string InternalToString(string message, string debugMessage, List<Cell> sourceCells, ViewGenErrorCode errorCode, bool isInvariant)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (isInvariant)
			{
				stringBuilder.AppendLine(debugMessage);
				stringBuilder.Append(isInvariant ? "ERROR" : Strings.ViewGen_Error);
				StringUtil.FormatStringBuilder(stringBuilder, " ({0}): ", (int)errorCode);
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			GetUserLinesFromCells(sourceCells, stringBuilder2, isInvariant);
			if (isInvariant)
			{
				if (sourceCells.Count > 1)
				{
					StringUtil.FormatStringBuilder(stringBuilder, "Problem in Mapping Fragments starting at lines {0}: ", stringBuilder2.ToString());
				}
				else
				{
					StringUtil.FormatStringBuilder(stringBuilder, "Problem in Mapping Fragment starting at line {0}: ", stringBuilder2.ToString());
				}
			}
			else if (sourceCells.Count > 1)
			{
				stringBuilder.Append(Strings.ViewGen_ErrorLog2(stringBuilder2.ToString()));
			}
			else
			{
				stringBuilder.Append(Strings.ViewGen_ErrorLog(stringBuilder2.ToString()));
			}
			stringBuilder.AppendLine(message);
			return stringBuilder.ToString();
		}

		internal string ToUserString()
		{
			return m_mappingError.ToString();
		}
	}

	private readonly List<Record> m_log;

	internal int Count => m_log.Count;

	internal IEnumerable<EdmSchemaError> Errors
	{
		get
		{
			foreach (Record item in m_log)
			{
				yield return item.Error;
			}
		}
	}

	internal ErrorLog()
	{
		m_log = new List<Record>();
	}

	internal void AddEntry(Record record)
	{
		m_log.Add(record);
	}

	internal void Merge(ErrorLog log)
	{
		foreach (Record item in log.m_log)
		{
			m_log.Add(item);
		}
	}

	internal void PrintTrace()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ToCompactString(stringBuilder);
		Helpers.StringTraceLine(stringBuilder.ToString());
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		foreach (Record item in m_log)
		{
			item.ToCompactString(builder);
		}
	}

	internal string ToUserString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Record item in m_log)
		{
			string value = item.ToUserString();
			stringBuilder.AppendLine(value);
		}
		return stringBuilder.ToString();
	}
}
