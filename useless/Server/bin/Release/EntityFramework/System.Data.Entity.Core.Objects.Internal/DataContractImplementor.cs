using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class DataContractImplementor
{
	internal static readonly ConstructorInfo DataContractAttributeConstructor = typeof(DataContractAttribute).GetDeclaredConstructor();

	internal static readonly PropertyInfo[] DataContractProperties = new PropertyInfo[1] { typeof(DataContractAttribute).GetDeclaredProperty("IsReference") };

	private readonly Type _baseClrType;

	private readonly DataContractAttribute _dataContract;

	internal DataContractImplementor(EntityType ospaceEntityType)
	{
		_baseClrType = ospaceEntityType.ClrType;
		_dataContract = _baseClrType.GetCustomAttributes<DataContractAttribute>(inherit: false).FirstOrDefault();
	}

	internal void Implement(TypeBuilder typeBuilder)
	{
		if (_dataContract != null)
		{
			object[] propertyValues = new object[1] { _dataContract.IsReference };
			CustomAttributeBuilder customAttribute = new CustomAttributeBuilder(DataContractAttributeConstructor, new object[0], DataContractProperties, propertyValues);
			typeBuilder.SetCustomAttribute(customAttribute);
		}
	}
}
