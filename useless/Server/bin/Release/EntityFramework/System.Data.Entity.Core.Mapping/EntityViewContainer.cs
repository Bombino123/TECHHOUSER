using System.Collections.Generic;

namespace System.Data.Entity.Core.Mapping;

[Obsolete("The mechanism to provide pre-generated views has changed. Implement a class that derives from System.Data.Entity.Infrastructure.MappingViews.DbMappingViewCache and has a parameterless constructor, then associate it with a type that derives from DbContext or ObjectContext by using System.Data.Entity.Infrastructure.MappingViews.DbMappingViewCacheTypeAttribute.", true)]
public abstract class EntityViewContainer
{
	internal IEnumerable<KeyValuePair<string, string>> ExtentViews
	{
		get
		{
			for (int i = 0; i < ViewCount; i++)
			{
				yield return GetViewAt(i);
			}
		}
	}

	public string EdmEntityContainerName { get; set; }

	public string StoreEntityContainerName { get; set; }

	public string HashOverMappingClosure { get; set; }

	public string HashOverAllExtentViews { get; set; }

	public int ViewCount { get; protected set; }

	protected abstract KeyValuePair<string, string> GetViewAt(int index);
}
