using System.Collections.Generic;
using System.Data.Entity.Core.Objects.ELinq;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Text;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public class RowType : StructuralType
{
	private ReadOnlyMetadataCollection<EdmProperty> _properties;

	private readonly InitializerMetadata _initializerMetadata;

	internal InitializerMetadata InitializerMetadata => _initializerMetadata;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.RowType;

	public virtual ReadOnlyMetadataCollection<EdmProperty> Properties
	{
		get
		{
			if (_properties == null)
			{
				Interlocked.CompareExchange(ref _properties, new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(base.Members, Helper.IsEdmProperty), null);
			}
			return _properties;
		}
	}

	public ReadOnlyMetadataCollection<EdmProperty> DeclaredProperties => GetDeclaredOnlyMembers<EdmProperty>();

	internal RowType()
	{
	}

	internal RowType(IEnumerable<EdmProperty> properties)
		: this(properties, null)
	{
	}

	internal RowType(IEnumerable<EdmProperty> properties, InitializerMetadata initializerMetadata)
		: base(GetRowTypeIdentityFromProperties(CheckProperties(properties), initializerMetadata), "Transient", (DataSpace)(-1))
	{
		if (properties != null)
		{
			foreach (EdmProperty property in properties)
			{
				AddProperty(property);
			}
		}
		_initializerMetadata = initializerMetadata;
		SetReadOnly();
	}

	private void AddProperty(EdmProperty property)
	{
		Check.NotNull(property, "property");
		AddMember(property);
	}

	internal override void ValidateMemberForAdd(EdmMember member)
	{
	}

	private static string GetRowTypeIdentityFromProperties(IEnumerable<EdmProperty> properties, InitializerMetadata initializerMetadata)
	{
		StringBuilder stringBuilder = new StringBuilder("rowtype[");
		if (properties != null)
		{
			int num = 0;
			foreach (EdmProperty property in properties)
			{
				if (num > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append("(");
				stringBuilder.Append(property.Name);
				stringBuilder.Append(",");
				property.TypeUsage.BuildIdentity(stringBuilder);
				stringBuilder.Append(")");
				num++;
			}
		}
		stringBuilder.Append("]");
		if (initializerMetadata != null)
		{
			stringBuilder.Append(",").Append(initializerMetadata.Identity);
		}
		return stringBuilder.ToString();
	}

	private static IEnumerable<EdmProperty> CheckProperties(IEnumerable<EdmProperty> properties)
	{
		if (properties != null)
		{
			int num = 0;
			foreach (EdmProperty property in properties)
			{
				if (property == null)
				{
					throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("properties"));
				}
				num++;
			}
		}
		return properties;
	}

	internal override bool EdmEquals(MetadataItem item)
	{
		if (this == item)
		{
			return true;
		}
		if (item == null || BuiltInTypeKind.RowType != item.BuiltInTypeKind)
		{
			return false;
		}
		RowType rowType = (RowType)item;
		if (base.Members.Count != rowType.Members.Count)
		{
			return false;
		}
		for (int i = 0; i < base.Members.Count; i++)
		{
			EdmMember edmMember = base.Members[i];
			EdmMember edmMember2 = rowType.Members[i];
			if (!edmMember.EdmEquals(edmMember2) || !edmMember.TypeUsage.EdmEquals(edmMember2.TypeUsage))
			{
				return false;
			}
		}
		return true;
	}

	public static RowType Create(IEnumerable<EdmProperty> properties, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotNull(properties, "properties");
		RowType rowType = new RowType(properties);
		if (metadataProperties != null)
		{
			rowType.AddMetadataProperties(metadataProperties);
		}
		rowType.SetReadOnly();
		return rowType;
	}
}
