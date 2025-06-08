using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Utilities;

internal class AttributeProvider
{
	private readonly ConcurrentDictionary<PropertyInfo, IEnumerable<Attribute>> _discoveredAttributes = new ConcurrentDictionary<PropertyInfo, IEnumerable<Attribute>>();

	public virtual IEnumerable<Attribute> GetAttributes(MemberInfo memberInfo)
	{
		Type type = memberInfo as Type;
		if (type != null)
		{
			return GetAttributes(type);
		}
		return GetAttributes((PropertyInfo)memberInfo);
	}

	public virtual IEnumerable<Attribute> GetAttributes(Type type)
	{
		List<Attribute> attrs = new List<Attribute>(GetTypeDescriptor(type).GetAttributes().Cast<Attribute>());
		foreach (Attribute item in from a in type.GetCustomAttributes<Attribute>(inherit: true)
			where a.GetType().FullName.Equals("System.Data.Services.Common.EntityPropertyMappingAttribute", StringComparison.Ordinal) && !attrs.Contains(a)
			select a)
		{
			attrs.Add(item);
		}
		return attrs;
	}

	public virtual IEnumerable<Attribute> GetAttributes(PropertyInfo propertyInfo)
	{
		return _discoveredAttributes.GetOrAdd(propertyInfo, delegate(PropertyInfo pi)
		{
			PropertyDescriptor propertyDescriptor = GetTypeDescriptor(pi.DeclaringType).GetProperties()[pi.Name];
			IEnumerable<Attribute> enumerable = ((propertyDescriptor != null) ? propertyDescriptor.Attributes.Cast<Attribute>() : pi.GetCustomAttributes<Attribute>(inherit: true));
			ICollection<Attribute> collection = (ICollection<Attribute>)GetAttributes(pi.PropertyType);
			if (collection.Count > 0)
			{
				enumerable = enumerable.Except(collection);
			}
			return enumerable.ToList();
		});
	}

	private static ICustomTypeDescriptor GetTypeDescriptor(Type type)
	{
		return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
	}

	public virtual void ClearCache()
	{
		_discoveredAttributes.Clear();
	}
}
