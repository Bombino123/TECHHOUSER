namespace System.Data.Entity.Utilities;

internal static class DbModelBuilderVersionExtensions
{
	public static double GetEdmVersion(this DbModelBuilderVersion modelBuilderVersion)
	{
		switch (modelBuilderVersion)
		{
		case DbModelBuilderVersion.V4_1:
		case DbModelBuilderVersion.V5_0_Net4:
			return 2.0;
		case DbModelBuilderVersion.Latest:
		case DbModelBuilderVersion.V5_0:
		case DbModelBuilderVersion.V6_0:
			return 3.0;
		default:
			throw new ArgumentOutOfRangeException("modelBuilderVersion");
		}
	}

	public static bool IsEF6OrHigher(this DbModelBuilderVersion modelBuilderVersion)
	{
		if (modelBuilderVersion < DbModelBuilderVersion.V6_0)
		{
			return modelBuilderVersion == DbModelBuilderVersion.Latest;
		}
		return true;
	}
}
