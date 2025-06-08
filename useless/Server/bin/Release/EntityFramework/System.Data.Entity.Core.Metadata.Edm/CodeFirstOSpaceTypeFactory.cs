using System.Collections.Generic;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class CodeFirstOSpaceTypeFactory : OSpaceTypeFactory
{
	private readonly List<Action> _referenceResolutions = new List<Action>();

	private readonly Dictionary<EdmType, EdmType> _cspaceToOspace = new Dictionary<EdmType, EdmType>();

	private readonly Dictionary<string, EdmType> _loadedTypes = new Dictionary<string, EdmType>();

	public override List<Action> ReferenceResolutions => _referenceResolutions;

	public override Dictionary<EdmType, EdmType> CspaceToOspace => _cspaceToOspace;

	public override Dictionary<string, EdmType> LoadedTypes => _loadedTypes;

	public override void LogLoadMessage(string message, EdmType relatedType)
	{
	}

	public override void LogError(string errorMessage, EdmType relatedType)
	{
		throw new MetadataException(Strings.InvalidSchemaEncountered(errorMessage));
	}

	public override void TrackClosure(Type type)
	{
	}

	public override void AddToTypesInAssembly(EdmType type)
	{
	}
}
