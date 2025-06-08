using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public abstract class CascadableNavigationPropertyConfiguration
{
	private readonly NavigationPropertyConfiguration _navigationPropertyConfiguration;

	internal NavigationPropertyConfiguration NavigationPropertyConfiguration => _navigationPropertyConfiguration;

	internal CascadableNavigationPropertyConfiguration(NavigationPropertyConfiguration navigationPropertyConfiguration)
	{
		_navigationPropertyConfiguration = navigationPropertyConfiguration;
	}

	public void WillCascadeOnDelete()
	{
		WillCascadeOnDelete(value: true);
	}

	public void WillCascadeOnDelete(bool value)
	{
		_navigationPropertyConfiguration.DeleteAction = (value ? OperationAction.Cascade : OperationAction.None);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
