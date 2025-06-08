using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class TargetPerspective : Perspective
{
	internal const DataSpace TargetPerspectiveDataSpace = DataSpace.SSpace;

	private readonly ModelPerspective _modelPerspective;

	internal TargetPerspective(MetadataWorkspace metadataWorkspace)
		: base(metadataWorkspace, DataSpace.SSpace)
	{
		_modelPerspective = new ModelPerspective(metadataWorkspace);
	}

	internal override bool TryGetTypeByName(string fullName, bool ignoreCase, out TypeUsage usage)
	{
		Check.NotEmpty(fullName, "fullName");
		EdmType item = null;
		if (base.MetadataWorkspace.TryGetItem<EdmType>(fullName, ignoreCase, base.TargetDataspace, out item))
		{
			usage = TypeUsage.Create(item);
			usage = Helper.GetModelTypeUsage(usage);
			return true;
		}
		return _modelPerspective.TryGetTypeByName(fullName, ignoreCase, out usage);
	}

	internal override bool TryGetEntityContainer(string name, bool ignoreCase, out EntityContainer entityContainer)
	{
		if (!base.TryGetEntityContainer(name, ignoreCase, out entityContainer))
		{
			return _modelPerspective.TryGetEntityContainer(name, ignoreCase, out entityContainer);
		}
		return true;
	}
}
