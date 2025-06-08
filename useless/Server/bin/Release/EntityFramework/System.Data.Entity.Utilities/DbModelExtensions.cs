using System.Data.Entity.Infrastructure;
using System.Xml;
using System.Xml.Linq;

namespace System.Data.Entity.Utilities;

internal static class DbModelExtensions
{
	public static XDocument GetModel(this DbModel model)
	{
		return DbContextExtensions.GetModel(delegate(XmlWriter w)
		{
			EdmxWriter.WriteEdmx(model, w);
		});
	}
}
