namespace System.Data.Entity.Core.Objects.DataClasses;

public interface IEntityWithChangeTracker
{
	void SetChangeTracker(IEntityChangeTracker changeTracker);
}
