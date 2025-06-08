using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Objects;

public sealed class ObjectParameter
{
	private readonly string _name;

	private readonly Type _type;

	private readonly Type _mappableType;

	private TypeUsage _effectiveType;

	private object _value;

	public string Name => _name;

	public Type ParameterType => _type;

	public object Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	internal TypeUsage TypeUsage
	{
		get
		{
			return _effectiveType;
		}
		set
		{
			_effectiveType = value;
		}
	}

	internal Type MappableType => _mappableType;

	internal static bool ValidateParameterName(string name)
	{
		return DbCommandTree.IsValidParameterName(name);
	}

	public ObjectParameter(string name, Type type)
	{
		Check.NotNull(name, "name");
		Check.NotNull(type, "type");
		if (!ValidateParameterName(name))
		{
			throw new ArgumentException(Strings.ObjectParameter_InvalidParameterName(name), "name");
		}
		_name = name;
		_type = type;
		_mappableType = TypeSystem.GetNonNullableType(_type);
	}

	public ObjectParameter(string name, object value)
	{
		Check.NotNull(name, "name");
		Check.NotNull(value, "value");
		if (!ValidateParameterName(name))
		{
			throw new ArgumentException(Strings.ObjectParameter_InvalidParameterName(name), "name");
		}
		_name = name;
		_type = value.GetType();
		_value = value;
		_mappableType = TypeSystem.GetNonNullableType(_type);
	}

	private ObjectParameter(ObjectParameter template)
	{
		_name = template._name;
		_type = template._type;
		_mappableType = template._mappableType;
		_effectiveType = template._effectiveType;
		_value = template._value;
	}

	internal ObjectParameter ShallowCopy()
	{
		return new ObjectParameter(this);
	}

	internal bool ValidateParameterType(ClrPerspective perspective)
	{
		if (perspective.TryGetType(_mappableType, out var outTypeUsage) && TypeSemantics.IsScalarType(outTypeUsage))
		{
			return true;
		}
		return false;
	}
}
