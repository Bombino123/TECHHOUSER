using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core.Objects.DataClasses;

[Serializable]
internal class RelationshipNavigation
{
	private readonly string _relationshipName;

	private readonly string _from;

	private readonly string _to;

	[NonSerialized]
	private RelationshipNavigation _reverse;

	[NonSerialized]
	private NavigationPropertyAccessor _fromAccessor;

	[NonSerialized]
	private NavigationPropertyAccessor _toAccessor;

	[NonSerialized]
	private readonly AssociationType _associationType;

	internal AssociationType AssociationType => _associationType;

	internal string RelationshipName => _relationshipName;

	internal string From => _from;

	internal string To => _to;

	internal NavigationPropertyAccessor ToPropertyAccessor => _toAccessor;

	internal bool IsInitialized
	{
		get
		{
			if (_toAccessor != null)
			{
				return _fromAccessor != null;
			}
			return false;
		}
	}

	internal RelationshipNavigation Reverse
	{
		get
		{
			if (_reverse == null || !_reverse.IsInitialized)
			{
				_reverse = ((_associationType != null) ? new RelationshipNavigation(_associationType, _to, _from, _toAccessor, _fromAccessor) : new RelationshipNavigation(_relationshipName, _to, _from, _toAccessor, _fromAccessor));
			}
			return _reverse;
		}
	}

	internal RelationshipNavigation(string relationshipName, string from, string to, NavigationPropertyAccessor fromAccessor, NavigationPropertyAccessor toAccessor)
	{
		Check.NotEmpty(relationshipName, "relationshipName");
		Check.NotEmpty(from, "from");
		Check.NotEmpty(to, "to");
		_relationshipName = relationshipName;
		_from = from;
		_to = to;
		_fromAccessor = fromAccessor;
		_toAccessor = toAccessor;
	}

	internal RelationshipNavigation(AssociationType associationType, string from, string to, NavigationPropertyAccessor fromAccessor, NavigationPropertyAccessor toAccessor)
	{
		_associationType = associationType;
		_relationshipName = associationType.FullName;
		_from = from;
		_to = to;
		_fromAccessor = fromAccessor;
		_toAccessor = toAccessor;
	}

	internal void InitializeAccessors(NavigationPropertyAccessor fromAccessor, NavigationPropertyAccessor toAccessor)
	{
		_fromAccessor = fromAccessor;
		_toAccessor = toAccessor;
	}

	public override bool Equals(object obj)
	{
		RelationshipNavigation relationshipNavigation = obj as RelationshipNavigation;
		if (this != relationshipNavigation)
		{
			if (this != null && relationshipNavigation != null && RelationshipName == relationshipNavigation.RelationshipName && From == relationshipNavigation.From)
			{
				return To == relationshipNavigation.To;
			}
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return RelationshipName.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "RelationshipNavigation: ({0},{1},{2})", new object[3] { _relationshipName, _from, _to });
	}
}
