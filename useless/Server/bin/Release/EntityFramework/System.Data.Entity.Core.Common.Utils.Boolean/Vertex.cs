using System.Diagnostics;
using System.Globalization;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class Vertex : IEquatable<Vertex>
{
	internal static readonly Vertex One = new Vertex();

	internal static readonly Vertex Zero = new Vertex();

	internal readonly int Variable;

	internal readonly Vertex[] Children;

	private Vertex()
	{
		Variable = int.MaxValue;
		Children = new Vertex[0];
	}

	internal Vertex(int variable, Vertex[] children)
	{
		if (variable >= int.MaxValue)
		{
			throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.BoolExprAssert, 0, "exceeded number of supported variables");
		}
		Variable = variable;
		Children = children;
	}

	[Conditional("DEBUG")]
	private static void AssertConstructorArgumentsValid(int variable, Vertex[] children)
	{
		for (int i = 0; i < children.Length; i++)
		{
			_ = children[i];
		}
	}

	internal bool IsOne()
	{
		return One == this;
	}

	internal bool IsZero()
	{
		return Zero == this;
	}

	internal bool IsSink()
	{
		return Variable == int.MaxValue;
	}

	public bool Equals(Vertex other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		if (IsOne())
		{
			return "_1_";
		}
		if (IsZero())
		{
			return "_0_";
		}
		return string.Format(CultureInfo.InvariantCulture, "<{0}, {1}>", new object[2]
		{
			Variable,
			StringUtil.ToCommaSeparatedString(Children)
		});
	}
}
