namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class ParserOptions
{
	internal enum CompilationMode
	{
		NormalMode,
		RestrictedViewGenerationMode,
		UserViewGenerationMode
	}

	internal CompilationMode ParserCompilationMode;

	internal StringComparer NameComparer
	{
		get
		{
			if (!NameComparisonCaseInsensitive)
			{
				return StringComparer.Ordinal;
			}
			return StringComparer.OrdinalIgnoreCase;
		}
	}

	internal bool NameComparisonCaseInsensitive
	{
		get
		{
			if (ParserCompilationMode != CompilationMode.RestrictedViewGenerationMode)
			{
				return true;
			}
			return false;
		}
	}
}
