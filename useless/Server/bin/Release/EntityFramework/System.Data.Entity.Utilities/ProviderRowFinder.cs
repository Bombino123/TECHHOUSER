using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Entity.Utilities;

internal class ProviderRowFinder
{
	public virtual DataRow FindRow(Type hintType, Func<DataRow, bool> selector, IEnumerable<DataRow> dataRows)
	{
		AssemblyName assemblyName = ((hintType == null) ? null : new AssemblyName(hintType.Assembly().FullName));
		foreach (DataRow dataRow in dataRows)
		{
			string typeName = (string)dataRow[3];
			AssemblyName rowProviderFactoryAssemblyName = null;
			Type.GetType(typeName, delegate(AssemblyName a)
			{
				rowProviderFactoryAssemblyName = a;
				return (Assembly)null;
			}, (Assembly _, string __, bool ___) => (Type)null);
			if (rowProviderFactoryAssemblyName == null || (!(hintType == null) && !string.Equals(assemblyName.Name, rowProviderFactoryAssemblyName.Name, StringComparison.OrdinalIgnoreCase)))
			{
				continue;
			}
			try
			{
				if (selector(dataRow))
				{
					return dataRow;
				}
			}
			catch (Exception)
			{
			}
		}
		return null;
	}
}
