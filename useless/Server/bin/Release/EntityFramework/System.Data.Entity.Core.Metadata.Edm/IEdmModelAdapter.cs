namespace System.Data.Entity.Core.Metadata.Edm;

[Obsolete("ConceptualModel and StoreModel are now available as properties directly on DbModel.")]
public interface IEdmModelAdapter
{
	[Obsolete("ConceptualModel is now available as a property directly on DbModel.")]
	EdmModel ConceptualModel { get; }

	[Obsolete("StoreModel is now available as a property directly on DbModel.")]
	EdmModel StoreModel { get; }
}
