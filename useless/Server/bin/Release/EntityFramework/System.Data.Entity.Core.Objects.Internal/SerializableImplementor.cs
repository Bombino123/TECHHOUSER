using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Security;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class SerializableImplementor
{
	private readonly Type _baseClrType;

	private readonly bool _baseImplementsISerializable;

	private readonly bool _canOverride;

	private readonly MethodInfo _getObjectDataMethod;

	private readonly ConstructorInfo _serializationConstructor;

	internal static readonly MethodInfo GetTypeFromHandleMethod = typeof(Type).GetDeclaredMethod("GetTypeFromHandle", typeof(RuntimeTypeHandle));

	internal static readonly MethodInfo AddValueMethod = typeof(SerializationInfo).GetDeclaredMethod("AddValue", typeof(string), typeof(object), typeof(Type));

	internal static readonly MethodInfo GetValueMethod = typeof(SerializationInfo).GetDeclaredMethod("GetValue", typeof(string), typeof(Type));

	internal bool TypeIsSuitable
	{
		get
		{
			if (_baseImplementsISerializable)
			{
				return _canOverride;
			}
			return true;
		}
	}

	internal bool TypeImplementsISerializable => _baseImplementsISerializable;

	internal SerializableImplementor(EntityType ospaceEntityType)
	{
		_baseClrType = ospaceEntityType.ClrType;
		_baseImplementsISerializable = _baseClrType.IsSerializable() && typeof(ISerializable).IsAssignableFrom(_baseClrType);
		if (!_baseImplementsISerializable)
		{
			return;
		}
		_getObjectDataMethod = _baseClrType.GetInterfaceMap(typeof(ISerializable)).TargetMethods[0];
		if (_getObjectDataMethod.IsVirtual && !_getObjectDataMethod.IsFinal && _getObjectDataMethod.IsPublic)
		{
			_serializationConstructor = _baseClrType.GetDeclaredConstructor((ConstructorInfo c) => c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly, new Type[2]
			{
				typeof(SerializationInfo),
				typeof(StreamingContext)
			}, new Type[2]
			{
				typeof(SerializationInfo),
				typeof(object)
			}, new Type[2]
			{
				typeof(object),
				typeof(StreamingContext)
			}, new Type[2]
			{
				typeof(object),
				typeof(object)
			});
			_canOverride = _serializationConstructor != null;
		}
	}

	internal void Implement(TypeBuilder typeBuilder, IEnumerable<FieldBuilder> serializedFields)
	{
		if (!_baseImplementsISerializable || !_canOverride)
		{
			return;
		}
		Type[] parameterTypes = new Type[2]
		{
			typeof(SerializationInfo),
			typeof(StreamingContext)
		};
		MethodBuilder methodBuilder = typeBuilder.DefineMethod(_getObjectDataMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, null, parameterTypes);
		methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(SecurityCriticalAttribute).GetDeclaredConstructor(), new object[0]));
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		foreach (FieldBuilder serializedField in serializedFields)
		{
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Ldstr, serializedField.Name);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, serializedField);
			iLGenerator.Emit(OpCodes.Ldtoken, serializedField.FieldType);
			iLGenerator.Emit(OpCodes.Call, GetTypeFromHandleMethod);
			iLGenerator.Emit(OpCodes.Callvirt, AddValueMethod);
		}
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_2);
		iLGenerator.Emit(OpCodes.Call, _getObjectDataMethod);
		iLGenerator.Emit(OpCodes.Ret);
		MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
		methodAttributes |= ((!_serializationConstructor.IsPublic) ? MethodAttributes.Private : MethodAttributes.Public);
		ILGenerator iLGenerator2 = typeBuilder.DefineConstructor(methodAttributes, CallingConventions.Standard | CallingConventions.HasThis, parameterTypes).GetILGenerator();
		iLGenerator2.Emit(OpCodes.Ldarg_0);
		iLGenerator2.Emit(OpCodes.Ldarg_1);
		iLGenerator2.Emit(OpCodes.Ldarg_2);
		iLGenerator2.Emit(OpCodes.Call, _serializationConstructor);
		foreach (FieldBuilder serializedField2 in serializedFields)
		{
			iLGenerator2.Emit(OpCodes.Ldarg_0);
			iLGenerator2.Emit(OpCodes.Ldarg_1);
			iLGenerator2.Emit(OpCodes.Ldstr, serializedField2.Name);
			iLGenerator2.Emit(OpCodes.Ldtoken, serializedField2.FieldType);
			iLGenerator2.Emit(OpCodes.Call, GetTypeFromHandleMethod);
			iLGenerator2.Emit(OpCodes.Callvirt, GetValueMethod);
			iLGenerator2.Emit(OpCodes.Castclass, serializedField2.FieldType);
			iLGenerator2.Emit(OpCodes.Stfld, serializedField2);
		}
		iLGenerator2.Emit(OpCodes.Ret);
	}
}
