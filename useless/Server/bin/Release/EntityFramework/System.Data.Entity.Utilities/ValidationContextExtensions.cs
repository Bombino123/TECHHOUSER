using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Internal;

namespace System.Data.Entity.Utilities;

internal static class ValidationContextExtensions
{
	public static void SetDisplayName(this ValidationContext validationContext, InternalMemberEntry property, DisplayAttribute displayAttribute)
	{
		string text = displayAttribute?.GetName();
		if (property == null)
		{
			Type objectType = ObjectContextTypeCache.GetObjectType(validationContext.ObjectType);
			validationContext.DisplayName = text ?? objectType.Name;
			validationContext.MemberName = null;
		}
		else
		{
			validationContext.DisplayName = text ?? DbHelpers.GetPropertyPath(property);
			validationContext.MemberName = DbHelpers.GetPropertyPath(property);
		}
	}
}
