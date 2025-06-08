namespace System.Data.Entity.Core.Objects.DataClasses;

public interface IEntityChangeTracker
{
	EntityState EntityState { get; }

	void EntityMemberChanging(string entityMemberName);

	void EntityMemberChanged(string entityMemberName);

	void EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName);

	void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName);
}
