using System.Text;

namespace System.Data.Entity.Core.Common.Utils;

internal abstract class InternalBase
{
	internal abstract void ToCompactString(StringBuilder builder);

	internal virtual void ToFullString(StringBuilder builder)
	{
		ToCompactString(builder);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ToCompactString(stringBuilder);
		return stringBuilder.ToString();
	}

	internal virtual string ToFullString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ToFullString(stringBuilder);
		return stringBuilder.ToString();
	}
}
