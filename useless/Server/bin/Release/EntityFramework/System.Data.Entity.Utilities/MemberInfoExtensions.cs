using System.Reflection;

namespace System.Data.Entity.Utilities;

internal static class MemberInfoExtensions
{
	public static object GetValue(this MemberInfo memberInfo)
	{
		PropertyInfo propertyInfo = memberInfo as PropertyInfo;
		if (!(propertyInfo != null))
		{
			return ((FieldInfo)memberInfo).GetValue(null);
		}
		return propertyInfo.GetValue(null, null);
	}
}
