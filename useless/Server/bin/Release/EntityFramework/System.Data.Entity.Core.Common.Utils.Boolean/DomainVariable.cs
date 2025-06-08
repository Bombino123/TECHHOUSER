using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal class DomainVariable<T_Variable, T_Element>
{
	private readonly T_Variable _identifier;

	private readonly Set<T_Element> _domain;

	private readonly int _hashCode;

	private readonly IEqualityComparer<T_Variable> _identifierComparer;

	internal T_Variable Identifier => _identifier;

	internal Set<T_Element> Domain => _domain;

	internal DomainVariable(T_Variable identifier, Set<T_Element> domain, IEqualityComparer<T_Variable> identifierComparer)
	{
		_identifier = identifier;
		_domain = domain.AsReadOnly();
		_identifierComparer = identifierComparer ?? EqualityComparer<T_Variable>.Default;
		int elementsHashCode = _domain.GetElementsHashCode();
		int hashCode = _identifierComparer.GetHashCode(_identifier);
		_hashCode = elementsHashCode ^ hashCode;
	}

	internal DomainVariable(T_Variable identifier, Set<T_Element> domain)
		: this(identifier, domain, (IEqualityComparer<T_Variable>)null)
	{
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is DomainVariable<T_Variable, T_Element> domainVariable))
		{
			return false;
		}
		if (_hashCode != domainVariable._hashCode)
		{
			return false;
		}
		if (_identifierComparer.Equals(_identifier, domainVariable._identifier))
		{
			return _domain.SetEquals(domainVariable._domain);
		}
		return false;
	}

	public override string ToString()
	{
		return StringUtil.FormatInvariant("{0}{{{1}}}", _identifier.ToString(), _domain);
	}
}
