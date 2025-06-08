using dnlib.Utils;

namespace dnlib.DotNet;

public class TypeDefUser : TypeDef
{
	public TypeDefUser(UTF8String name)
		: this(null, name, null)
	{
	}

	public TypeDefUser(UTF8String @namespace, UTF8String name)
		: this(@namespace, name, null)
	{
	}

	public TypeDefUser(UTF8String name, ITypeDefOrRef baseType)
		: this(null, name, baseType)
	{
	}

	public TypeDefUser(UTF8String @namespace, UTF8String name, ITypeDefOrRef baseType)
	{
		fields = new LazyList<FieldDef>(this);
		methods = new LazyList<MethodDef>(this);
		genericParameters = new LazyList<GenericParam>(this);
		nestedTypes = new LazyList<TypeDef>(this);
		events = new LazyList<EventDef>(this);
		properties = new LazyList<PropertyDef>(this);
		base.@namespace = @namespace;
		base.name = name;
		base.baseType = baseType;
		baseType_isInitialized = true;
	}
}
