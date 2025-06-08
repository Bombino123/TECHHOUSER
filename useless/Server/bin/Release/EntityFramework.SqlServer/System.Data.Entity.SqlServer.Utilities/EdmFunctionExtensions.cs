using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.SqlServer.Utilities;

internal static class EdmFunctionExtensions
{
	internal static bool IsCSpace(this EdmFunction function)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		MetadataProperty val = ((IEnumerable<MetadataProperty>)((MetadataItem)function).MetadataProperties).FirstOrDefault((Func<MetadataProperty, bool>)((MetadataProperty p) => p.Name == "DataSpace"));
		if (val != null)
		{
			return (int)(DataSpace)val.Value == 1;
		}
		return false;
	}

	internal static bool IsCanonicalFunction(this EdmFunction function)
	{
		if (function.IsCSpace())
		{
			return ((EdmType)function).NamespaceName == "Edm";
		}
		return false;
	}
}
