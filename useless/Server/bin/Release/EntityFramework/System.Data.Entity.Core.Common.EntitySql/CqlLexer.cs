using System.Collections.Generic;
using System.Data.Entity.Core.Common.EntitySql.AST;
using System.Data.Entity.Resources;
using System.IO;
using System.Text.RegularExpressions;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class CqlLexer
{
	private delegate Token AcceptMethod();

	internal class Token
	{
		private readonly short _tokenId;

		private readonly object _tokenValue;

		internal short TokenId => _tokenId;

		internal object Value => _tokenValue;

		internal Token(short tokenId, Node tokenValue)
		{
			_tokenId = tokenId;
			_tokenValue = tokenValue;
		}

		internal Token(short tokenId, TerminalToken terminal)
		{
			_tokenId = tokenId;
			_tokenValue = terminal;
		}
	}

	internal class TerminalToken
	{
		private readonly string _token;

		private readonly int _iPos;

		internal int IPos => _iPos;

		internal string Token => _token;

		internal TerminalToken(string token, int iPos)
		{
			_token = token;
			_iPos = iPos;
		}
	}

	internal static class yy_translate
	{
		internal static char translate(char c)
		{
			if (char.IsWhiteSpace(c) || char.IsControl(c))
			{
				if (IsNewLine(c))
				{
					return '\n';
				}
				return ' ';
			}
			if (c < '\u007f')
			{
				return c;
			}
			if (char.IsLetter(c) || char.IsSymbol(c) || char.IsNumber(c))
			{
				return 'a';
			}
			return '`';
		}
	}

	private const int YY_BUFFER_SIZE = 512;

	private const int YY_F = -1;

	private const int YY_NO_STATE = -1;

	private const int YY_NOT_ACCEPT = 0;

	private const int YY_START = 1;

	private const int YY_END = 2;

	private const int YY_NO_ANCHOR = 4;

	private readonly AcceptMethod[] accept_dispatch;

	private const int YY_BOL = 128;

	private const int YY_EOF = 129;

	private readonly TextReader yy_reader;

	private int yy_buffer_index;

	private int yy_buffer_read;

	private int yy_buffer_start;

	private int yy_buffer_end;

	private char[] yy_buffer;

	private int yychar;

	private int yyline;

	private bool yy_at_bol;

	private int yy_lexical_state;

	private const int YYINITIAL = 0;

	private static readonly int[] yy_state_dtrans = new int[1];

	private bool yy_last_was_cr;

	private const int YY_E_INTERNAL = 0;

	private const int YY_E_MATCH = 1;

	private static string[] yy_error_string = new string[2] { "Error: Internal error.\n", "Error: Unmatched input.\n" };

	private static readonly int[] yy_acpt = new int[85]
	{
		0, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 2, 4, 4, 4, 4, 4, 0,
		4, 4, 4, 4, 0, 4, 4, 4, 4, 0,
		4, 4, 4, 0, 4, 4, 0, 4, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4
	};

	private static readonly int[] yy_cmap = new int[130]
	{
		11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
		27, 11, 11, 8, 11, 11, 11, 11, 11, 11,
		11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
		11, 11, 12, 33, 28, 11, 11, 39, 36, 10,
		40, 40, 39, 38, 40, 25, 24, 39, 22, 22,
		22, 22, 22, 22, 22, 22, 22, 22, 40, 40,
		34, 32, 35, 40, 29, 5, 2, 30, 13, 15,
		18, 20, 30, 3, 30, 30, 23, 16, 26, 17,
		30, 30, 6, 19, 14, 21, 30, 30, 9, 7,
		30, 1, 11, 40, 11, 31, 11, 5, 2, 30,
		13, 15, 18, 20, 30, 3, 30, 30, 23, 16,
		4, 17, 30, 30, 6, 19, 14, 21, 30, 30,
		9, 7, 30, 40, 37, 40, 11, 11, 0, 41
	};

	private static readonly int[] yy_rmap = new int[85]
	{
		0, 1, 1, 2, 3, 4, 5, 6, 7, 8,
		9, 10, 1, 1, 11, 1, 1, 1, 1, 12,
		13, 1, 14, 14, 15, 16, 17, 1, 18, 10,
		19, 20, 1, 21, 22, 23, 24, 25, 26, 27,
		5, 28, 29, 30, 31, 32, 33, 34, 35, 36,
		37, 38, 39, 40, 41, 42, 43, 44, 45, 46,
		47, 48, 49, 50, 51, 52, 53, 54, 55, 56,
		57, 58, 59, 60, 61, 62, 63, 11, 64, 65,
		66, 67, 68, 11, 69
	};

	private static readonly int[,] yy_nxt = new int[70, 42]
	{
		{
			1, 2, 3, 83, 83, 83, 83, 83, 4, 20,
			19, -1, 4, 84, 64, 83, 83, 83, 71, 83,
			72, 83, 5, 83, 6, 7, 25, 8, 24, 29,
			83, 83, 22, 23, 28, 23, 33, 36, 32, 32,
			27, 1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 76, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, 4, -1,
			-1, -1, 4, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 21, -1, 39, 21, -1, 21, -1,
			-1, 26, 5, 31, 40, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 35, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 41, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 8, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			19, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 24, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 11, 11, 11, 11, 11, 11, -1, 11,
			-1, -1, -1, 11, 11, 11, 11, 11, 11, 11,
			11, 11, 11, 11, -1, -1, 11, -1, -1, -1,
			11, 11, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, 19, 19, 19, 19, 19, 19, 19, 19, 19,
			9, 19, 19, 19, 19, 19, 19, 19, 19, 19,
			19, 19, 19, 19, 19, 19, 19, 19, 19, 19,
			19, 19, 19, 19, 19, 19, 19, 19, 19, 19,
			19, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			38, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 32, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 10, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			19, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, 24, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 21, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 32, -1, -1, 32, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 14, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, 21, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, 32, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, 44, 83,
			45, -1, 44, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 21, -1, 39, 21, -1, 21, -1,
			-1, -1, 35, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 32, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 21, -1, -1, -1, -1, 21, -1,
			-1, -1, 37, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, 38, 38, 38, 38, 38, 38, 38, -1, 38,
			12, 38, 38, 38, 38, 38, 38, 38, 38, 38,
			38, 38, 38, 38, 38, 38, 38, -1, -1, 38,
			38, 38, 38, 38, 38, 38, 38, 38, 38, 38,
			38, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 37, -1, -1, 42, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, 42, -1,
			-1, -1
		},
		{
			-1, 41, 41, 41, 41, 41, 41, 41, 43, 41,
			41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
			41, 41, 41, 41, 41, 41, 41, 13, 41, 41,
			41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
			41, 13
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, 37, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, 13, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, 44, -1,
			45, -1, 44, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, 45, 45, 45, 45, 45, 45, 45, -1, 45,
			15, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			45, 45, 45, 45, 45, 45, 45, -1, -1, 45,
			45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			45, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, 46, -1,
			47, -1, 46, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, 47, 47, 47, 47, 47, 47, 47, -1, 47,
			16, 47, 47, 47, 47, 47, 47, 47, 47, 47,
			47, 47, 47, 47, 47, 47, 47, -1, -1, 47,
			47, 47, 47, 47, 47, 47, 47, 47, 47, 47,
			47, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, 48, -1,
			38, -1, 48, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, 49, -1,
			50, -1, 49, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, 50, 50, 50, 50, 50, 50, 50, -1, 50,
			17, 50, 50, 50, 50, 50, 50, 50, 50, 50,
			50, 50, 50, 50, 50, 50, 50, -1, -1, 50,
			50, 50, 50, 50, 50, 50, 50, 50, 50, 50,
			50, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, 51, -1,
			52, -1, 51, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, 52, 52, 52, 52, 52, 52, 52, -1, 52,
			18, 52, 52, 52, 52, 52, 52, 52, 52, 52,
			52, 52, 52, 52, 52, 52, 52, -1, -1, 52,
			52, 52, 52, 52, 52, 52, 52, 52, 52, 52,
			52, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 30, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, 46, 83,
			47, -1, 46, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 34, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, 48, 83,
			38, -1, 48, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 30,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, 49, 83,
			50, -1, 49, 83, 83, 83, 83, 81, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 54, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, 51, 83,
			52, -1, 51, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 56, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 58, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 60, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 65, 83, 83, 53, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 55, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 57, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 59, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 61, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 62, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 63, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 66, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 67, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 68, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 69, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 70,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 73, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 73, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 79, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 80, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 74, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 82, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 83, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 75, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		},
		{
			-1, -1, 83, 83, 83, 78, 83, 83, -1, 83,
			-1, -1, -1, 83, 83, 83, 83, 83, 83, 83,
			83, 83, 77, 83, -1, -1, 83, -1, -1, -1,
			83, 77, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1
		}
	};

	private static readonly StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;

	private static Dictionary<string, short> _keywords;

	private static HashSet<string> _invalidAliasNames;

	private static HashSet<string> _invalidInlineFunctionNames;

	private static Dictionary<string, short> _operators;

	private static Dictionary<string, short> _punctuators;

	private static HashSet<string> _canonicalFunctionNames;

	private static Regex _reDateTimeValue;

	private static Regex _reTimeValue;

	private static Regex _reDateTimeOffsetValue;

	private const string _datetimeValueRegularExpression = "^[0-9]{4}-[0-9]{1,2}-[0-9]{1,2}([ ])+[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,2}(\\.[0-9]{1,7})?)?$";

	private const string _timeValueRegularExpression = "^[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,2}(\\.[0-9]{1,7})?)?$";

	private const string _datetimeOffsetValueRegularExpression = "^[0-9]{4}-[0-9]{1,2}-[0-9]{1,2}([ ])+[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,2}(\\.[0-9]{1,7})?)?([ ])*[\\+-][0-9]{1,2}:[0-9]{1,2}$";

	private int _iPos;

	private int _lineNumber;

	private ParserOptions _parserOptions;

	private readonly string _query;

	private bool _symbolAsIdentifierState;

	private bool _symbolAsAliasIdentifierState;

	private bool _symbolAsInlineFunctionNameState;

	private static readonly char[] _newLineCharacters = new char[5] { '\n', '\u0085', '\v', '\u2028', '\u2029' };

	internal string YYText => yytext();

	internal int IPos => _iPos;

	private static Dictionary<string, short> InternalKeywordDictionary
	{
		get
		{
			if (_keywords == null)
			{
				_keywords = new Dictionary<string, short>(60, _stringComparer)
				{
					{
						"all",
						CqlParser.ALL
					},
					{
						"and",
						CqlParser.AND
					},
					{
						"anyelement",
						CqlParser.ANYELEMENT
					},
					{
						"apply",
						CqlParser.APPLY
					},
					{
						"as",
						CqlParser.AS
					},
					{
						"asc",
						CqlParser.ASC
					},
					{
						"between",
						CqlParser.BETWEEN
					},
					{
						"by",
						CqlParser.BY
					},
					{
						"case",
						CqlParser.CASE
					},
					{
						"cast",
						CqlParser.CAST
					},
					{
						"collate",
						CqlParser.COLLATE
					},
					{
						"collection",
						CqlParser.COLLECTION
					},
					{
						"createref",
						CqlParser.CREATEREF
					},
					{
						"cross",
						CqlParser.CROSS
					},
					{
						"deref",
						CqlParser.DEREF
					},
					{
						"desc",
						CqlParser.DESC
					},
					{
						"distinct",
						CqlParser.DISTINCT
					},
					{
						"element",
						CqlParser.ELEMENT
					},
					{
						"else",
						CqlParser.ELSE
					},
					{
						"end",
						CqlParser.END
					},
					{
						"escape",
						CqlParser.ESCAPE
					},
					{
						"except",
						CqlParser.EXCEPT
					},
					{
						"exists",
						CqlParser.EXISTS
					},
					{
						"false",
						CqlParser.LITERAL
					},
					{
						"flatten",
						CqlParser.FLATTEN
					},
					{
						"from",
						CqlParser.FROM
					},
					{
						"full",
						CqlParser.FULL
					},
					{
						"function",
						CqlParser.FUNCTION
					},
					{
						"group",
						CqlParser.GROUP
					},
					{
						"grouppartition",
						CqlParser.GROUPPARTITION
					},
					{
						"having",
						CqlParser.HAVING
					},
					{
						"in",
						CqlParser.IN
					},
					{
						"inner",
						CqlParser.INNER
					},
					{
						"intersect",
						CqlParser.INTERSECT
					},
					{
						"is",
						CqlParser.IS
					},
					{
						"join",
						CqlParser.JOIN
					},
					{
						"key",
						CqlParser.KEY
					},
					{
						"left",
						CqlParser.LEFT
					},
					{
						"like",
						CqlParser.LIKE
					},
					{
						"limit",
						CqlParser.LIMIT
					},
					{
						"multiset",
						CqlParser.MULTISET
					},
					{
						"navigate",
						CqlParser.NAVIGATE
					},
					{
						"not",
						CqlParser.NOT
					},
					{
						"null",
						CqlParser.NULL
					},
					{
						"of",
						CqlParser.OF
					},
					{
						"oftype",
						CqlParser.OFTYPE
					},
					{
						"on",
						CqlParser.ON
					},
					{
						"only",
						CqlParser.ONLY
					},
					{
						"or",
						CqlParser.OR
					},
					{
						"order",
						CqlParser.ORDER
					},
					{
						"outer",
						CqlParser.OUTER
					},
					{
						"overlaps",
						CqlParser.OVERLAPS
					},
					{
						"ref",
						CqlParser.REF
					},
					{
						"relationship",
						CqlParser.RELATIONSHIP
					},
					{
						"right",
						CqlParser.RIGHT
					},
					{
						"row",
						CqlParser.ROW
					},
					{
						"select",
						CqlParser.SELECT
					},
					{
						"set",
						CqlParser.SET
					},
					{
						"skip",
						CqlParser.SKIP
					},
					{
						"then",
						CqlParser.THEN
					},
					{
						"top",
						CqlParser.TOP
					},
					{
						"treat",
						CqlParser.TREAT
					},
					{
						"true",
						CqlParser.LITERAL
					},
					{
						"union",
						CqlParser.UNION
					},
					{
						"using",
						CqlParser.USING
					},
					{
						"value",
						CqlParser.VALUE
					},
					{
						"when",
						CqlParser.WHEN
					},
					{
						"where",
						CqlParser.WHERE
					},
					{
						"with",
						CqlParser.WITH
					}
				};
			}
			return _keywords;
		}
	}

	private static HashSet<string> InternalInvalidAliasNames
	{
		get
		{
			if (_invalidAliasNames == null)
			{
				_invalidAliasNames = new HashSet<string>(_stringComparer)
				{
					"all", "and", "apply", "as", "asc", "between", "by", "case", "cast", "collate",
					"createref", "deref", "desc", "distinct", "element", "else", "end", "escape", "except", "exists",
					"flatten", "from", "group", "having", "in", "inner", "intersect", "is", "join", "like",
					"multiset", "navigate", "not", "null", "of", "oftype", "on", "only", "or", "overlaps",
					"ref", "relationship", "select", "set", "then", "treat", "union", "using", "when", "where",
					"with"
				};
			}
			return _invalidAliasNames;
		}
	}

	private static HashSet<string> InternalInvalidInlineFunctionNames
	{
		get
		{
			if (_invalidInlineFunctionNames == null)
			{
				_invalidInlineFunctionNames = new HashSet<string>(_stringComparer) { "anyelement", "element", "function", "grouppartition", "key", "ref", "row", "skip", "top", "value" };
			}
			return _invalidInlineFunctionNames;
		}
	}

	private static Dictionary<string, short> InternalOperatorDictionary
	{
		get
		{
			if (_operators == null)
			{
				_operators = new Dictionary<string, short>(16, _stringComparer)
				{
					{
						"==",
						CqlParser.OP_EQ
					},
					{
						"!=",
						CqlParser.OP_NEQ
					},
					{
						"<>",
						CqlParser.OP_NEQ
					},
					{
						"<",
						CqlParser.OP_LT
					},
					{
						"<=",
						CqlParser.OP_LE
					},
					{
						">",
						CqlParser.OP_GT
					},
					{
						">=",
						CqlParser.OP_GE
					},
					{
						"&&",
						CqlParser.AND
					},
					{
						"||",
						CqlParser.OR
					},
					{
						"!",
						CqlParser.NOT
					},
					{
						"+",
						CqlParser.PLUS
					},
					{
						"-",
						CqlParser.MINUS
					},
					{
						"*",
						CqlParser.STAR
					},
					{
						"/",
						CqlParser.FSLASH
					},
					{
						"%",
						CqlParser.PERCENT
					}
				};
			}
			return _operators;
		}
	}

	private static Dictionary<string, short> InternalPunctuatorDictionary
	{
		get
		{
			if (_punctuators == null)
			{
				_punctuators = new Dictionary<string, short>(16, _stringComparer)
				{
					{
						",",
						CqlParser.COMMA
					},
					{
						":",
						CqlParser.COLON
					},
					{
						".",
						CqlParser.DOT
					},
					{
						"?",
						CqlParser.QMARK
					},
					{
						"(",
						CqlParser.L_PAREN
					},
					{
						")",
						CqlParser.R_PAREN
					},
					{
						"[",
						CqlParser.L_BRACE
					},
					{
						"]",
						CqlParser.R_BRACE
					},
					{
						"{",
						CqlParser.L_CURLY
					},
					{
						"}",
						CqlParser.R_CURLY
					},
					{
						";",
						CqlParser.SCOLON
					},
					{
						"=",
						CqlParser.EQUAL
					}
				};
			}
			return _punctuators;
		}
	}

	private static HashSet<string> InternalCanonicalFunctionNames
	{
		get
		{
			if (_canonicalFunctionNames == null)
			{
				_canonicalFunctionNames = new HashSet<string>(_stringComparer) { "left", "right" };
			}
			return _canonicalFunctionNames;
		}
	}

	internal CqlLexer(TextReader reader)
		: this()
	{
		if (reader == null)
		{
			throw new EntitySqlException(EntityRes.GetString("ParserInputError"));
		}
		yy_reader = reader;
	}

	internal CqlLexer(FileStream instream)
		: this()
	{
		if (instream == null)
		{
			throw new EntitySqlException(EntityRes.GetString("ParserInputError"));
		}
		yy_reader = new StreamReader(instream);
	}

	private CqlLexer()
	{
		yy_buffer = new char[512];
		yy_buffer_read = 0;
		yy_buffer_index = 0;
		yy_buffer_start = 0;
		yy_buffer_end = 0;
		yychar = 0;
		yyline = 0;
		yy_at_bol = true;
		yy_lexical_state = 0;
		accept_dispatch = new AcceptMethod[85]
		{
			null, null, Accept_2, Accept_3, Accept_4, Accept_5, Accept_6, Accept_7, Accept_8, Accept_9,
			Accept_10, Accept_11, Accept_12, Accept_13, Accept_14, Accept_15, Accept_16, Accept_17, Accept_18, null,
			Accept_20, Accept_21, Accept_22, Accept_23, null, Accept_25, Accept_26, Accept_27, Accept_28, null,
			Accept_30, Accept_31, Accept_32, null, Accept_34, Accept_35, null, Accept_37, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, Accept_53, Accept_54, Accept_55, Accept_56, Accept_57, Accept_58, Accept_59,
			Accept_60, Accept_61, Accept_62, Accept_63, Accept_64, Accept_65, Accept_66, Accept_67, Accept_68, Accept_69,
			Accept_70, Accept_71, Accept_72, Accept_73, Accept_74, Accept_75, Accept_76, Accept_77, Accept_78, Accept_79,
			Accept_80, Accept_81, Accept_82, Accept_83, Accept_84
		};
	}

	private Token Accept_2()
	{
		return HandleEscapedIdentifiers();
	}

	private Token Accept_3()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_4()
	{
		AdvanceIPos();
		ResetSymbolAsIdentifierState(significant: false);
		return null;
	}

	private Token Accept_5()
	{
		return NewLiteralToken(YYText, LiteralKind.Number);
	}

	private Token Accept_6()
	{
		return MapPunctuator(YYText);
	}

	private Token Accept_7()
	{
		return MapOperator(YYText);
	}

	private Token Accept_8()
	{
		_lineNumber++;
		AdvanceIPos();
		ResetSymbolAsIdentifierState(significant: false);
		return null;
	}

	private Token Accept_9()
	{
		return NewLiteralToken(YYText, LiteralKind.String);
	}

	private Token Accept_10()
	{
		return MapDoubleQuotedString(YYText);
	}

	private Token Accept_11()
	{
		return NewParameterToken(YYText);
	}

	private Token Accept_12()
	{
		return NewLiteralToken(YYText, LiteralKind.Binary);
	}

	private Token Accept_13()
	{
		_lineNumber++;
		AdvanceIPos();
		ResetSymbolAsIdentifierState(significant: false);
		return null;
	}

	private Token Accept_14()
	{
		return NewLiteralToken(YYText, LiteralKind.Boolean);
	}

	private Token Accept_15()
	{
		return NewLiteralToken(YYText, LiteralKind.Time);
	}

	private Token Accept_16()
	{
		return NewLiteralToken(YYText, LiteralKind.Guid);
	}

	private Token Accept_17()
	{
		return NewLiteralToken(YYText, LiteralKind.DateTime);
	}

	private Token Accept_18()
	{
		return NewLiteralToken(YYText, LiteralKind.DateTimeOffset);
	}

	private Token Accept_20()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_21()
	{
		return NewLiteralToken(YYText, LiteralKind.Number);
	}

	private Token Accept_22()
	{
		return MapPunctuator(YYText);
	}

	private Token Accept_23()
	{
		return MapOperator(YYText);
	}

	private Token Accept_25()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_26()
	{
		return NewLiteralToken(YYText, LiteralKind.Number);
	}

	private Token Accept_27()
	{
		return MapPunctuator(YYText);
	}

	private Token Accept_28()
	{
		return MapOperator(YYText);
	}

	private Token Accept_30()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_31()
	{
		return NewLiteralToken(YYText, LiteralKind.Number);
	}

	private Token Accept_32()
	{
		return MapOperator(YYText);
	}

	private Token Accept_34()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_35()
	{
		return NewLiteralToken(YYText, LiteralKind.Number);
	}

	private Token Accept_37()
	{
		return NewLiteralToken(YYText, LiteralKind.Number);
	}

	private Token Accept_53()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_54()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_55()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_56()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_57()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_58()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_59()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_60()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_61()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_62()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_63()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_64()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_65()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_66()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_67()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_68()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_69()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_70()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_71()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_72()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_73()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_74()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_75()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_76()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_77()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_78()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_79()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_80()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_81()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_82()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_83()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private Token Accept_84()
	{
		return MapIdentifierOrKeyword(YYText);
	}

	private void yybegin(int state)
	{
		yy_lexical_state = state;
	}

	private char yy_advance()
	{
		if (yy_buffer_index < yy_buffer_read)
		{
			return yy_translate.translate(yy_buffer[yy_buffer_index++]);
		}
		if (yy_buffer_start != 0)
		{
			int num = yy_buffer_start;
			int num2 = 0;
			while (num < yy_buffer_read)
			{
				yy_buffer[num2] = yy_buffer[num];
				num++;
				num2++;
			}
			yy_buffer_end -= yy_buffer_start;
			yy_buffer_start = 0;
			yy_buffer_read = num2;
			yy_buffer_index = num2;
			int num3 = yy_reader.Read(yy_buffer, yy_buffer_read, yy_buffer.Length - yy_buffer_read);
			if (num3 <= 0)
			{
				return '\u0081';
			}
			yy_buffer_read += num3;
		}
		while (yy_buffer_index >= yy_buffer_read)
		{
			if (yy_buffer_index >= yy_buffer.Length)
			{
				yy_buffer = yy_double(yy_buffer);
			}
			int num3 = yy_reader.Read(yy_buffer, yy_buffer_read, yy_buffer.Length - yy_buffer_read);
			if (num3 <= 0)
			{
				return '\u0081';
			}
			yy_buffer_read += num3;
		}
		return yy_translate.translate(yy_buffer[yy_buffer_index++]);
	}

	private void yy_move_end()
	{
		if (yy_buffer_end > yy_buffer_start && '\n' == yy_buffer[yy_buffer_end - 1])
		{
			yy_buffer_end--;
		}
		if (yy_buffer_end > yy_buffer_start && '\r' == yy_buffer[yy_buffer_end - 1])
		{
			yy_buffer_end--;
		}
	}

	private void yy_mark_start()
	{
		for (int i = yy_buffer_start; i < yy_buffer_index; i++)
		{
			if (yy_buffer[i] == '\n' && !yy_last_was_cr)
			{
				yyline++;
			}
			if (yy_buffer[i] == '\r')
			{
				yyline++;
				yy_last_was_cr = true;
			}
			else
			{
				yy_last_was_cr = false;
			}
		}
		yychar = yychar + yy_buffer_index - yy_buffer_start;
		yy_buffer_start = yy_buffer_index;
	}

	private void yy_mark_end()
	{
		yy_buffer_end = yy_buffer_index;
	}

	private void yy_to_mark()
	{
		yy_buffer_index = yy_buffer_end;
		yy_at_bol = yy_buffer_end > yy_buffer_start && (yy_buffer[yy_buffer_end - 1] == '\r' || yy_buffer[yy_buffer_end - 1] == '\n');
	}

	internal string yytext()
	{
		return new string(yy_buffer, yy_buffer_start, yy_buffer_end - yy_buffer_start);
	}

	internal int yy_char()
	{
		return yychar;
	}

	private int yylength()
	{
		return yy_buffer_end - yy_buffer_start;
	}

	private char[] yy_double(char[] buf)
	{
		char[] array = new char[2 * buf.Length];
		for (int i = 0; i < buf.Length; i++)
		{
			array[i] = buf[i];
		}
		return array;
	}

	private void yy_error(int code, bool fatal)
	{
		if (fatal)
		{
			throw new EntitySqlException(EntityRes.GetString("ParserFatalError"));
		}
	}

	internal Token yylex()
	{
		int num = 4;
		int num2 = yy_state_dtrans[yy_lexical_state];
		int num3 = -1;
		int num4 = -1;
		bool flag = true;
		yy_mark_start();
		if (yy_acpt[num2] != 0)
		{
			num4 = num2;
			yy_mark_end();
		}
		Token token;
		while (true)
		{
			char c = ((!flag || !yy_at_bol) ? yy_advance() : '\u0080');
			num3 = yy_nxt[yy_rmap[num2], yy_cmap[(uint)c]];
			if ('\u0081' == c && flag)
			{
				return null;
			}
			if (-1 != num3)
			{
				num2 = num3;
				flag = false;
				if (yy_acpt[num2] != 0)
				{
					num4 = num2;
					yy_mark_end();
				}
				continue;
			}
			if (-1 == num4)
			{
				throw new EntitySqlException(EntitySqlException.GetGenericErrorMessage(_query, yychar));
			}
			num = yy_acpt[num4];
			if ((2u & (uint)num) != 0)
			{
				yy_move_end();
			}
			yy_to_mark();
			if (num4 < 0)
			{
				if (num4 < 85)
				{
					yy_error(0, fatal: false);
				}
			}
			else
			{
				AcceptMethod acceptMethod = accept_dispatch[num4];
				if (acceptMethod != null)
				{
					token = acceptMethod();
					if (token != null)
					{
						break;
					}
				}
			}
			flag = true;
			num2 = yy_state_dtrans[yy_lexical_state];
			num3 = -1;
			num4 = -1;
			yy_mark_start();
			if (yy_acpt[num2] != 0)
			{
				num4 = num2;
				yy_mark_end();
			}
		}
		return token;
	}

	internal CqlLexer(string query, ParserOptions parserOptions)
		: this()
	{
		_query = query;
		_parserOptions = parserOptions;
		yy_reader = new StringReader(_query);
	}

	internal static Token NewToken(short tokenId, Node tokenvalue)
	{
		return new Token(tokenId, tokenvalue);
	}

	internal static Token NewToken(short tokenId, TerminalToken termToken)
	{
		return new Token(tokenId, termToken);
	}

	internal int AdvanceIPos()
	{
		_iPos += YYText.Length;
		return _iPos;
	}

	internal static bool IsReservedKeyword(string term)
	{
		return InternalKeywordDictionary.ContainsKey(term);
	}

	internal Token MapIdentifierOrKeyword(string symbol)
	{
		if (IsEscapedIdentifier(symbol, out var identifierToken))
		{
			return identifierToken;
		}
		if (IsKeyword(symbol, out identifierToken))
		{
			return identifierToken;
		}
		return MapUnescapedIdentifier(symbol);
	}

	private bool IsEscapedIdentifier(string symbol, out Token identifierToken)
	{
		if (symbol.Length > 1 && symbol[0] == '[')
		{
			if (symbol[symbol.Length - 1] == ']')
			{
				Identifier identifier = new Identifier(symbol.Substring(1, symbol.Length - 2), isEscaped: true, _query, _iPos);
				identifier.ErrCtx.ErrorContextInfo = "CtxEscapedIdentifier";
				identifierToken = NewToken(CqlParser.ESCAPED_IDENTIFIER, identifier);
				return true;
			}
			string errorDescription = Strings.InvalidEscapedIdentifier(symbol);
			throw EntitySqlException.Create(_query, errorDescription, _iPos, null, loadErrorContextInfoFromResource: false, null);
		}
		identifierToken = null;
		return false;
	}

	private bool IsKeyword(string symbol, out Token terminalToken)
	{
		char lookAheadChar = GetLookAheadChar();
		if (!IsInSymbolAsIdentifierState(lookAheadChar) && !IsCanonicalFunctionCall(symbol, lookAheadChar) && InternalKeywordDictionary.ContainsKey(symbol))
		{
			ResetSymbolAsIdentifierState(significant: true);
			short num = InternalKeywordDictionary[symbol];
			if (num == CqlParser.AS)
			{
				_symbolAsAliasIdentifierState = true;
			}
			else if (num == CqlParser.FUNCTION)
			{
				_symbolAsInlineFunctionNameState = true;
			}
			terminalToken = NewToken(num, new TerminalToken(symbol, _iPos));
			return true;
		}
		terminalToken = null;
		return false;
	}

	private bool IsCanonicalFunctionCall(string symbol, char lookAheadChar)
	{
		if (lookAheadChar == '(')
		{
			return InternalCanonicalFunctionNames.Contains(symbol);
		}
		return false;
	}

	private Token MapUnescapedIdentifier(string symbol)
	{
		bool flag = InternalInvalidAliasNames.Contains(symbol);
		if (_symbolAsInlineFunctionNameState)
		{
			flag |= InternalInvalidInlineFunctionNames.Contains(symbol);
		}
		ResetSymbolAsIdentifierState(significant: true);
		if (flag)
		{
			string errorDescription = Strings.InvalidAliasName(symbol);
			throw EntitySqlException.Create(_query, errorDescription, _iPos, null, loadErrorContextInfoFromResource: false, null);
		}
		Identifier identifier = new Identifier(symbol, isEscaped: false, _query, _iPos);
		identifier.ErrCtx.ErrorContextInfo = "CtxIdentifier";
		return NewToken(CqlParser.IDENTIFIER, identifier);
	}

	private char GetLookAheadChar()
	{
		yy_mark_end();
		char c = yy_advance();
		while (c != '\u0081' && (char.IsWhiteSpace(c) || IsNewLine(c)))
		{
			c = yy_advance();
		}
		yy_to_mark();
		return c;
	}

	private bool IsInSymbolAsIdentifierState(char lookAheadChar)
	{
		if (!_symbolAsIdentifierState && !_symbolAsAliasIdentifierState && !_symbolAsInlineFunctionNameState)
		{
			return lookAheadChar == '.';
		}
		return true;
	}

	private void ResetSymbolAsIdentifierState(bool significant)
	{
		_symbolAsIdentifierState = false;
		if (significant)
		{
			_symbolAsAliasIdentifierState = false;
			_symbolAsInlineFunctionNameState = false;
		}
	}

	internal Token MapOperator(string oper)
	{
		if (InternalOperatorDictionary.ContainsKey(oper))
		{
			return NewToken(InternalOperatorDictionary[oper], new TerminalToken(oper, _iPos));
		}
		string invalidOperatorSymbol = Strings.InvalidOperatorSymbol;
		throw EntitySqlException.Create(_query, invalidOperatorSymbol, _iPos, null, loadErrorContextInfoFromResource: false, null);
	}

	internal Token MapPunctuator(string punct)
	{
		if (InternalPunctuatorDictionary.ContainsKey(punct))
		{
			ResetSymbolAsIdentifierState(significant: true);
			if (punct.Equals(".", StringComparison.OrdinalIgnoreCase))
			{
				_symbolAsIdentifierState = true;
			}
			return NewToken(InternalPunctuatorDictionary[punct], new TerminalToken(punct, _iPos));
		}
		string invalidPunctuatorSymbol = Strings.InvalidPunctuatorSymbol;
		throw EntitySqlException.Create(_query, invalidPunctuatorSymbol, _iPos, null, loadErrorContextInfoFromResource: false, null);
	}

	internal Token MapDoubleQuotedString(string symbol)
	{
		return NewLiteralToken(symbol, LiteralKind.String);
	}

	internal Token NewLiteralToken(string literal, LiteralKind literalKind)
	{
		string text = literal;
		switch (literalKind)
		{
		case LiteralKind.Binary:
			text = GetLiteralSingleQuotePayload(literal);
			if (!IsValidBinaryValue(text))
			{
				string errorDescription4 = Strings.InvalidLiteralFormat("binary", text);
				throw EntitySqlException.Create(_query, errorDescription4, _iPos, null, loadErrorContextInfoFromResource: false, null);
			}
			break;
		case LiteralKind.String:
			if ('N' == literal[0])
			{
				literalKind = LiteralKind.UnicodeString;
			}
			break;
		case LiteralKind.DateTime:
			text = GetLiteralSingleQuotePayload(literal);
			if (!IsValidDateTimeValue(text))
			{
				string errorDescription2 = Strings.InvalidLiteralFormat("datetime", text);
				throw EntitySqlException.Create(_query, errorDescription2, _iPos, null, loadErrorContextInfoFromResource: false, null);
			}
			break;
		case LiteralKind.Time:
			text = GetLiteralSingleQuotePayload(literal);
			if (!IsValidTimeValue(text))
			{
				string errorDescription5 = Strings.InvalidLiteralFormat("time", text);
				throw EntitySqlException.Create(_query, errorDescription5, _iPos, null, loadErrorContextInfoFromResource: false, null);
			}
			break;
		case LiteralKind.DateTimeOffset:
			text = GetLiteralSingleQuotePayload(literal);
			if (!IsValidDateTimeOffsetValue(text))
			{
				string errorDescription3 = Strings.InvalidLiteralFormat("datetimeoffset", text);
				throw EntitySqlException.Create(_query, errorDescription3, _iPos, null, loadErrorContextInfoFromResource: false, null);
			}
			break;
		case LiteralKind.Guid:
			text = GetLiteralSingleQuotePayload(literal);
			if (!IsValidGuidValue(text))
			{
				string errorDescription = Strings.InvalidLiteralFormat("guid", text);
				throw EntitySqlException.Create(_query, errorDescription, _iPos, null, loadErrorContextInfoFromResource: false, null);
			}
			break;
		}
		return NewToken(CqlParser.LITERAL, new Literal(text, literalKind, _query, _iPos));
	}

	internal Token NewParameterToken(string param)
	{
		return NewToken(CqlParser.PARAMETER, new QueryParameter(param, _query, _iPos));
	}

	internal Token HandleEscapedIdentifiers()
	{
		char c = YYText[0];
		while (true)
		{
			switch (c)
			{
			case ']':
				yy_mark_end();
				c = yy_advance();
				if (c != ']')
				{
					yy_to_mark();
					ResetSymbolAsIdentifierState(significant: true);
					return MapIdentifierOrKeyword(YYText.Replace("]]", "]"));
				}
				break;
			case '\u0081':
			{
				string errorDescription = Strings.InvalidEscapedIdentifierUnbalanced(YYText);
				throw EntitySqlException.Create(_query, errorDescription, _iPos, null, loadErrorContextInfoFromResource: false, null);
			}
			}
			c = yy_advance();
		}
	}

	internal static bool IsLetterOrDigitOrUnderscore(string symbol, out bool isIdentifierASCII)
	{
		isIdentifierASCII = true;
		for (int i = 0; i < symbol.Length; i++)
		{
			isIdentifierASCII = isIdentifierASCII && symbol[i] < '\u0080';
			if (!isIdentifierASCII && !IsLetter(symbol[i]) && !IsDigit(symbol[i]) && symbol[i] != '_')
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsLetter(char c)
	{
		if (c < 'A' || c > 'Z')
		{
			if (c >= 'a')
			{
				return c <= 'z';
			}
			return false;
		}
		return true;
	}

	private static bool IsDigit(char c)
	{
		if (c >= '0')
		{
			return c <= '9';
		}
		return false;
	}

	private static bool isHexDigit(char c)
	{
		if (!IsDigit(c) && (c < 'a' || c > 'f'))
		{
			if (c >= 'A')
			{
				return c <= 'F';
			}
			return false;
		}
		return true;
	}

	internal static bool IsNewLine(char c)
	{
		for (int i = 0; i < _newLineCharacters.Length; i++)
		{
			if (c == _newLineCharacters[i])
			{
				return true;
			}
		}
		return false;
	}

	private static string GetLiteralSingleQuotePayload(string literal)
	{
		if (literal.Split(new char[1] { '\'' }).Length != 3 || -1 == literal.IndexOf('\'') || -1 == literal.LastIndexOf('\''))
		{
			throw new EntitySqlException(Strings.MalformedSingleQuotePayload);
		}
		int num = literal.IndexOf('\'');
		string text = literal.Substring(num + 1, literal.Length - (num + 2));
		if (text.Split(new char[1] { '\'' }).Length != 1)
		{
			throw new EntitySqlException(Strings.MalformedSingleQuotePayload);
		}
		return text;
	}

	private static bool IsValidGuidValue(string guidValue)
	{
		int num = 0;
		if (guidValue.Length - 1 - num + 1 != 36)
		{
			return false;
		}
		int num2 = 0;
		bool flag = true;
		while (flag && num2 < 36)
		{
			flag = ((num2 != 8 && num2 != 13 && num2 != 18 && num2 != 23) ? isHexDigit(guidValue[num + num2]) : (guidValue[num + num2] == '-'));
			num2++;
		}
		return flag;
	}

	private static bool IsValidBinaryValue(string binaryValue)
	{
		if (string.IsNullOrEmpty(binaryValue))
		{
			return true;
		}
		int num = 0;
		bool flag;
		for (flag = binaryValue.Length > 0; flag && num < binaryValue.Length; flag = isHexDigit(binaryValue[num++]))
		{
		}
		return flag;
	}

	private static bool IsValidDateTimeValue(string datetimeValue)
	{
		if (_reDateTimeValue == null)
		{
			_reDateTimeValue = new Regex("^[0-9]{4}-[0-9]{1,2}-[0-9]{1,2}([ ])+[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,2}(\\.[0-9]{1,7})?)?$", RegexOptions.Singleline | RegexOptions.CultureInvariant);
		}
		return _reDateTimeValue.IsMatch(datetimeValue);
	}

	private static bool IsValidTimeValue(string timeValue)
	{
		if (_reTimeValue == null)
		{
			_reTimeValue = new Regex("^[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,2}(\\.[0-9]{1,7})?)?$", RegexOptions.Singleline | RegexOptions.CultureInvariant);
		}
		return _reTimeValue.IsMatch(timeValue);
	}

	private static bool IsValidDateTimeOffsetValue(string datetimeOffsetValue)
	{
		if (_reDateTimeOffsetValue == null)
		{
			_reDateTimeOffsetValue = new Regex("^[0-9]{4}-[0-9]{1,2}-[0-9]{1,2}([ ])+[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,2}(\\.[0-9]{1,7})?)?([ ])*[\\+-][0-9]{1,2}:[0-9]{1,2}$", RegexOptions.Singleline | RegexOptions.CultureInvariant);
		}
		return _reDateTimeOffsetValue.IsMatch(datetimeOffsetValue);
	}
}
