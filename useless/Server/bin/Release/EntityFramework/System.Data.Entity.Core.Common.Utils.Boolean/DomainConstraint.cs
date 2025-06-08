namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal class DomainConstraint<T_Variable, T_Element>
{
	private readonly DomainVariable<T_Variable, T_Element> _variable;

	private readonly Set<T_Element> _range;

	private readonly int _hashCode;

	internal DomainVariable<T_Variable, T_Element> Variable => _variable;

	internal Set<T_Element> Range => _range;

	internal DomainConstraint(DomainVariable<T_Variable, T_Element> variable, Set<T_Element> range)
	{
		_variable = variable;
		_range = range.AsReadOnly();
		_hashCode = _variable.GetHashCode() ^ _range.GetElementsHashCode();
	}

	internal DomainConstraint(DomainVariable<T_Variable, T_Element> variable, T_Element element)
		: this(variable, new Set<T_Element>(new T_Element[1] { element }).MakeReadOnly())
	{
	}

	internal DomainConstraint<T_Variable, T_Element> InvertDomainConstraint()
	{
		return new DomainConstraint<T_Variable, T_Element>(_variable, _variable.Domain.Difference(_range).AsReadOnly());
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is DomainConstraint<T_Variable, T_Element> domainConstraint))
		{
			return false;
		}
		if (_hashCode != domainConstraint._hashCode)
		{
			return false;
		}
		if (_range.SetEquals(domainConstraint._range))
		{
			return _variable.Equals(domainConstraint._variable);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public override string ToString()
	{
		return StringUtil.FormatInvariant("{0} in [{1}]", _variable, _range);
	}
}
