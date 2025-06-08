using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core.Objects.DataClasses;

[Serializable]
[DataContract(IsReference = true)]
public abstract class ComplexObject : StructuralObject
{
	private StructuralObject _parent;

	private string _parentPropertyName;

	internal sealed override bool IsChangeTracked
	{
		get
		{
			if (_parent != null)
			{
				return _parent.IsChangeTracked;
			}
			return false;
		}
	}

	internal void AttachToParent(StructuralObject parent, string parentPropertyName)
	{
		if (_parent != null)
		{
			throw new InvalidOperationException(Strings.ComplexObject_ComplexObjectAlreadyAttachedToParent);
		}
		_parent = parent;
		_parentPropertyName = parentPropertyName;
	}

	internal void DetachFromParent()
	{
		_parent = null;
		_parentPropertyName = null;
	}

	protected sealed override void ReportPropertyChanging(string property)
	{
		Check.NotEmpty(property, "property");
		base.ReportPropertyChanging(property);
		ReportComplexPropertyChanging(null, this, property);
	}

	protected sealed override void ReportPropertyChanged(string property)
	{
		Check.NotEmpty(property, "property");
		ReportComplexPropertyChanged(null, this, property);
		base.ReportPropertyChanged(property);
	}

	internal sealed override void ReportComplexPropertyChanging(string entityMemberName, ComplexObject complexObject, string complexMemberName)
	{
		if (_parent != null)
		{
			_parent.ReportComplexPropertyChanging(_parentPropertyName, complexObject, complexMemberName);
		}
	}

	internal sealed override void ReportComplexPropertyChanged(string entityMemberName, ComplexObject complexObject, string complexMemberName)
	{
		if (_parent != null)
		{
			_parent.ReportComplexPropertyChanged(_parentPropertyName, complexObject, complexMemberName);
		}
	}
}
