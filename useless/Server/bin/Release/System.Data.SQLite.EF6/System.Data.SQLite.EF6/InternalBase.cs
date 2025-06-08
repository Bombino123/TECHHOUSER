using System.Runtime;
using System.Text;

namespace System.Data.SQLite.EF6;

internal abstract class InternalBase
{
	[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
	protected InternalBase()
	{
	}

	internal abstract void ToCompactString(StringBuilder builder);

	internal virtual string ToFullString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ToFullString(stringBuilder);
		return stringBuilder.ToString();
	}

	[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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
}
