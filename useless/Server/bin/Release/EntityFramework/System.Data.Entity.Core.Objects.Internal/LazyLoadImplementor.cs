using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Data.Entity.Core.Objects.Internal;

internal class LazyLoadImplementor
{
	private HashSet<EdmMember> _members;

	public IEnumerable<EdmMember> Members => _members;

	public LazyLoadImplementor(EntityType ospaceEntityType)
	{
		CheckType(ospaceEntityType);
	}

	private void CheckType(EntityType ospaceEntityType)
	{
		_members = new HashSet<EdmMember>();
		foreach (EdmMember member in ospaceEntityType.Members)
		{
			PropertyInfo topProperty = ospaceEntityType.ClrType.GetTopProperty(member.Name);
			if (topProperty != null && EntityProxyFactory.CanProxyGetter(topProperty) && LazyLoadBehavior.IsLazyLoadCandidate(ospaceEntityType, member))
			{
				_members.Add(member);
			}
		}
	}

	public bool CanProxyMember(EdmMember member)
	{
		return _members.Contains(member);
	}

	public virtual void Implement(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
	{
		FieldBuilder arg = typeBuilder.DefineField("_entityWrapper", typeof(object), FieldAttributes.Public);
		registerField(arg, arg2: false);
	}

	public bool EmitMember(TypeBuilder typeBuilder, EdmMember member, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, BaseProxyImplementor baseImplementor)
	{
		if (_members.Contains(member))
		{
			MethodInfo methodInfo = baseProperty.Getter();
			MethodAttributes methodAttributes = methodInfo.Attributes & MethodAttributes.MemberAccessMask;
			Type type = typeof(Func<, , >).MakeGenericType(typeBuilder, baseProperty.PropertyType, typeof(bool));
			MethodInfo method = TypeBuilder.GetMethod(type, typeof(Func<, , >).GetOnlyDeclaredMethod("Invoke"));
			FieldBuilder field = typeBuilder.DefineField(GetInterceptorFieldName(baseProperty.Name), type, FieldAttributes.Private | FieldAttributes.Static);
			MethodBuilder methodBuilder = typeBuilder.DefineMethod("get_" + baseProperty.Name, methodAttributes | (MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName), baseProperty.PropertyType, Type.EmptyTypes);
			ILGenerator iLGenerator = methodBuilder.GetILGenerator();
			Label label = iLGenerator.DefineLabel();
			iLGenerator.DeclareLocal(baseProperty.PropertyType);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Call, methodInfo);
			iLGenerator.Emit(OpCodes.Stloc_0);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, field);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldloc_0);
			iLGenerator.Emit(OpCodes.Callvirt, method);
			iLGenerator.Emit(OpCodes.Brtrue_S, label);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Call, methodInfo);
			iLGenerator.Emit(OpCodes.Ret);
			iLGenerator.MarkLabel(label);
			iLGenerator.Emit(OpCodes.Ldloc_0);
			iLGenerator.Emit(OpCodes.Ret);
			propertyBuilder.SetGetMethod(methodBuilder);
			baseImplementor.AddBasePropertyGetter(baseProperty);
			return true;
		}
		return false;
	}

	internal static string GetInterceptorFieldName(string memberName)
	{
		return "ef_proxy_interceptorFor" + memberName;
	}
}
