using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

[Obsolete("ConceptualModel and StoreModel are now available as properties directly on DbModel.")]
public static class DbModelExtensions
{
	[Obsolete("ConceptualModel is now available as a property directly on DbModel.")]
	public static EdmModel GetConceptualModel(this IEdmModelAdapter model)
	{
		Check.NotNull(model, "model");
		return model.ConceptualModel;
	}

	[Obsolete("StoreModel is now available as a property directly on DbModel.")]
	public static EdmModel GetStoreModel(this IEdmModelAdapter model)
	{
		Check.NotNull(model, "model");
		return model.StoreModel;
	}
}
