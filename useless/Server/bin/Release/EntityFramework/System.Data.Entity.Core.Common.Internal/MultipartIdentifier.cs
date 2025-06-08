using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Text;

namespace System.Data.Entity.Core.Common.Internal;

internal static class MultipartIdentifier
{
	private enum MPIState
	{
		MPI_Value,
		MPI_ParseNonQuote,
		MPI_LookForSeparator,
		MPI_LookForNextCharOrSeparator,
		MPI_ParseQuote,
		MPI_RightQuote
	}

	private const int MaxParts = 4;

	internal const int ServerIndex = 0;

	internal const int CatalogIndex = 1;

	internal const int SchemaIndex = 2;

	internal const int TableIndex = 3;

	private static void IncrementStringCount(List<string> ary, ref int position)
	{
		position++;
		ary.Add(string.Empty);
	}

	private static bool IsWhitespace(char ch)
	{
		return char.IsWhiteSpace(ch);
	}

	internal static List<string> ParseMultipartIdentifier(string name, string leftQuote, string rightQuote, char separator)
	{
		List<string> list = new List<string>();
		list.Add(null);
		int position = 0;
		MPIState mPIState = MPIState.MPI_Value;
		StringBuilder stringBuilder = new StringBuilder(name.Length);
		StringBuilder stringBuilder2 = null;
		char c = ' ';
		foreach (char c2 in name)
		{
			switch (mPIState)
			{
			case MPIState.MPI_Value:
			{
				if (IsWhitespace(c2))
				{
					break;
				}
				if (c2 == separator)
				{
					list[position] = string.Empty;
					IncrementStringCount(list, ref position);
					break;
				}
				int index;
				if (-1 != (index = leftQuote.IndexOf(c2)))
				{
					c = rightQuote[index];
					stringBuilder.Length = 0;
					mPIState = MPIState.MPI_ParseQuote;
					break;
				}
				if (-1 != rightQuote.IndexOf(c2))
				{
					throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
				}
				stringBuilder.Length = 0;
				stringBuilder.Append(c2);
				mPIState = MPIState.MPI_ParseNonQuote;
				break;
			}
			case MPIState.MPI_ParseNonQuote:
				if (c2 == separator)
				{
					list[position] = stringBuilder.ToString();
					IncrementStringCount(list, ref position);
					mPIState = MPIState.MPI_Value;
					break;
				}
				if (-1 != rightQuote.IndexOf(c2))
				{
					throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
				}
				if (-1 != leftQuote.IndexOf(c2))
				{
					throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
				}
				if (IsWhitespace(c2))
				{
					list[position] = stringBuilder.ToString();
					if (stringBuilder2 == null)
					{
						stringBuilder2 = new StringBuilder();
					}
					stringBuilder2.Length = 0;
					stringBuilder2.Append(c2);
					mPIState = MPIState.MPI_LookForNextCharOrSeparator;
				}
				else
				{
					stringBuilder.Append(c2);
				}
				break;
			case MPIState.MPI_LookForNextCharOrSeparator:
				if (!IsWhitespace(c2))
				{
					if (c2 == separator)
					{
						IncrementStringCount(list, ref position);
						mPIState = MPIState.MPI_Value;
						break;
					}
					stringBuilder.Append((object?)stringBuilder2);
					stringBuilder.Append(c2);
					list[position] = stringBuilder.ToString();
					mPIState = MPIState.MPI_ParseNonQuote;
				}
				else
				{
					stringBuilder2.Append(c2);
				}
				break;
			case MPIState.MPI_ParseQuote:
				if (c2 == c)
				{
					mPIState = MPIState.MPI_RightQuote;
				}
				else
				{
					stringBuilder.Append(c2);
				}
				break;
			case MPIState.MPI_RightQuote:
				if (c2 == c)
				{
					stringBuilder.Append(c2);
					mPIState = MPIState.MPI_ParseQuote;
					break;
				}
				if (c2 == separator)
				{
					list[position] = stringBuilder.ToString();
					IncrementStringCount(list, ref position);
					mPIState = MPIState.MPI_Value;
					break;
				}
				if (!IsWhitespace(c2))
				{
					throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
				}
				list[position] = stringBuilder.ToString();
				mPIState = MPIState.MPI_LookForSeparator;
				break;
			case MPIState.MPI_LookForSeparator:
				if (!IsWhitespace(c2))
				{
					if (c2 != separator)
					{
						throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
					}
					IncrementStringCount(list, ref position);
					mPIState = MPIState.MPI_Value;
				}
				break;
			}
		}
		switch (mPIState)
		{
		case MPIState.MPI_ParseNonQuote:
		case MPIState.MPI_RightQuote:
			list[position] = stringBuilder.ToString();
			break;
		default:
			throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
		case MPIState.MPI_Value:
		case MPIState.MPI_LookForSeparator:
		case MPIState.MPI_LookForNextCharOrSeparator:
			break;
		}
		return list;
	}
}
