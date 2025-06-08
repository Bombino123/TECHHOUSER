using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class ExtentPair
{
	private readonly EntitySetBase m_left;

	private readonly EntitySetBase m_right;

	internal EntitySetBase Left => m_left;

	internal EntitySetBase Right => m_right;

	public override bool Equals(object obj)
	{
		if (obj is ExtentPair extentPair && extentPair.Left.Equals(Left))
		{
			return extentPair.Right.Equals(Right);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Left.GetHashCode() << 4) ^ Right.GetHashCode();
	}

	internal ExtentPair(EntitySetBase left, EntitySetBase right)
	{
		m_left = left;
		m_right = right;
	}
}
