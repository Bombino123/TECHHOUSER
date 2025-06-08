using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Data.Entity.Core.Objects.Internal;

internal class BaseProxyImplementor
{
	private readonly List<PropertyInfo> _baseGetters;

	private readonly List<PropertyInfo> _baseSetters;

	internal static readonly MethodInfo StringEquals = typeof(string).GetDeclaredMethod("op_Equality", typeof(string), typeof(string));

	private static readonly ConstructorInfo _invalidOperationConstructor = typeof(InvalidOperationException).GetDeclaredConstructor();

	public List<PropertyInfo> BaseGetters => _baseGetters;

	public List<PropertyInfo> BaseSetters => _baseSetters;

	public BaseProxyImplementor()
	{
		_baseGetters = new List<PropertyInfo>();
		_baseSetters = new List<PropertyInfo>();
	}

	public void AddBasePropertyGetter(PropertyInfo baseProperty)
	{
		_baseGetters.Add(baseProperty);
	}

	public void AddBasePropertySetter(PropertyInfo baseProperty)
	{
		_baseSetters.Add(baseProperty);
	}

	public void Implement(TypeBuilder typeBuilder)
	{
		if (_baseGetters.Count > 0)
		{
			ImplementBaseGetter(typeBuilder);
		}
		if (_baseSetters.Count > 0)
		{
			ImplementBaseSetter(typeBuilder);
		}
	}

	private void ImplementBaseGetter(TypeBuilder typeBuilder)
	{
		ILGenerator iLGenerator = typeBuilder.DefineMethod("GetBasePropertyValue", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(object), new Type[1] { typeof(string) }).GetILGenerator();
		Label[] array = new Label[_baseGetters.Count];
		for (int i = 0; i < _baseGetters.Count; i++)
		{
			array[i] = iLGenerator.DefineLabel();
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Ldstr, _baseGetters[i].Name);
			iLGenerator.Emit(OpCodes.Call, StringEquals);
			iLGenerator.Emit(OpCodes.Brfalse_S, array[i]);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Call, _baseGetters[i].Getter());
			iLGenerator.Emit(OpCodes.Ret);
			iLGenerator.MarkLabel(array[i]);
		}
		iLGenerator.Emit(OpCodes.Newobj, _invalidOperationConstructor);
		iLGenerator.Emit(OpCodes.Throw);
	}

	private void ImplementBaseSetter(TypeBuilder typeBuilder)
	{
		ILGenerator iLGenerator = typeBuilder.DefineMethod("SetBasePropertyValue", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void), new Type[2]
		{
			typeof(string),
			typeof(object)
		}).GetILGenerator();
		Label[] array = new Label[_baseSetters.Count];
		for (int i = 0; i < _baseSetters.Count; i++)
		{
			array[i] = iLGenerator.DefineLabel();
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Ldstr, _baseSetters[i].Name);
			iLGenerator.Emit(OpCodes.Call, StringEquals);
			iLGenerator.Emit(OpCodes.Brfalse_S, array[i]);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldarg_2);
			iLGenerator.Emit(OpCodes.Castclass, _baseSetters[i].PropertyType);
			iLGenerator.Emit(OpCodes.Call, _baseSetters[i].Setter());
			iLGenerator.Emit(OpCodes.Ret);
			iLGenerator.MarkLabel(array[i]);
		}
		iLGenerator.Emit(OpCodes.Newobj, _invalidOperationConstructor);
		iLGenerator.Emit(OpCodes.Throw);
	}
}
