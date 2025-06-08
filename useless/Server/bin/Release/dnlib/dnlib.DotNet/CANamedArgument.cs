using System;

namespace dnlib.DotNet;

public sealed class CANamedArgument : ICloneable
{
	private bool isField;

	private TypeSig type;

	private UTF8String name;

	private CAArgument argument;

	public bool IsField
	{
		get
		{
			return isField;
		}
		set
		{
			isField = value;
		}
	}

	public bool IsProperty
	{
		get
		{
			return !isField;
		}
		set
		{
			isField = !value;
		}
	}

	public TypeSig Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	public UTF8String Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public CAArgument Argument
	{
		get
		{
			return argument;
		}
		set
		{
			argument = value;
		}
	}

	public TypeSig ArgumentType
	{
		get
		{
			return argument.Type;
		}
		set
		{
			argument.Type = value;
		}
	}

	public object Value
	{
		get
		{
			return argument.Value;
		}
		set
		{
			argument.Value = value;
		}
	}

	public CANamedArgument()
	{
	}

	public CANamedArgument(bool isField)
	{
		this.isField = isField;
	}

	public CANamedArgument(bool isField, TypeSig type)
	{
		this.isField = isField;
		this.type = type;
	}

	public CANamedArgument(bool isField, TypeSig type, UTF8String name)
	{
		this.isField = isField;
		this.type = type;
		this.name = name;
	}

	public CANamedArgument(bool isField, TypeSig type, UTF8String name, CAArgument argument)
	{
		this.isField = isField;
		this.type = type;
		this.name = name;
		this.argument = argument;
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public CANamedArgument Clone()
	{
		return new CANamedArgument(isField, type, name, argument.Clone());
	}

	public override string ToString()
	{
		return string.Format("({0}) {1} {2} = {3} ({4})", isField ? "field" : "property", type, name, Value ?? "null", ArgumentType);
	}
}
