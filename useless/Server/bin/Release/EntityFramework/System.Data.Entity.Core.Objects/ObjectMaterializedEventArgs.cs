namespace System.Data.Entity.Core.Objects;

public class ObjectMaterializedEventArgs : EventArgs
{
	private readonly object _entity;

	public object Entity => _entity;

	public ObjectMaterializedEventArgs(object entity)
	{
		_entity = entity;
	}
}
