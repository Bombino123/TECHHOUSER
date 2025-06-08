using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

public class ComplexType : StructuralType
{
	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.ComplexType;

	public virtual ReadOnlyMetadataCollection<EdmProperty> Properties => new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(base.Members, Helper.IsEdmProperty);

	internal ComplexType(string name, string namespaceName, DataSpace dataSpace)
		: base(name, namespaceName, dataSpace)
	{
	}

	internal ComplexType()
	{
	}

	internal ComplexType(string name)
		: this(name, "Transient", DataSpace.CSpace)
	{
	}

	internal override void ValidateMemberForAdd(EdmMember member)
	{
	}

	public static ComplexType Create(string name, string namespaceName, DataSpace dataSpace, IEnumerable<EdmMember> members, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(namespaceName, "namespaceName");
		Check.NotNull(members, "members");
		ComplexType complexType = new ComplexType(name, namespaceName, dataSpace);
		foreach (EdmMember member in members)
		{
			complexType.AddMember(member);
		}
		if (metadataProperties != null)
		{
			complexType.AddMetadataProperties(metadataProperties);
		}
		complexType.SetReadOnly();
		return complexType;
	}
}
