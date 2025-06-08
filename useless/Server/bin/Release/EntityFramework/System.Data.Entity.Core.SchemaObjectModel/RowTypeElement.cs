using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class RowTypeElement : ModelFunctionTypeElement
{
	private readonly SchemaElementLookUpTable<RowTypePropertyElement> _properties = new SchemaElementLookUpTable<RowTypePropertyElement>();

	internal SchemaElementLookUpTable<RowTypePropertyElement> Properties => _properties;

	internal RowTypeElement(SchemaElement parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (CanHandleElement(reader, "Property"))
		{
			HandlePropertyElement(reader);
			return true;
		}
		return false;
	}

	protected void HandlePropertyElement(XmlReader reader)
	{
		RowTypePropertyElement rowTypePropertyElement = new RowTypePropertyElement(this);
		rowTypePropertyElement.Parse(reader);
		_properties.Add(rowTypePropertyElement, doNotAddErrorForEmptyName: true, Strings.DuplicateEntityContainerMemberName);
	}

	internal override void ResolveTopLevelNames()
	{
		foreach (RowTypePropertyElement property in _properties)
		{
			property.ResolveTopLevelNames();
		}
	}

	internal override void WriteIdentity(StringBuilder builder)
	{
		builder.Append("Row[");
		bool flag = true;
		foreach (RowTypePropertyElement property in _properties)
		{
			if (flag)
			{
				flag = !flag;
			}
			else
			{
				builder.Append(", ");
			}
			property.WriteIdentity(builder);
		}
		builder.Append("]");
	}

	internal override TypeUsage GetTypeUsage()
	{
		if (_typeUsage == null)
		{
			List<EdmProperty> list = new List<EdmProperty>();
			foreach (RowTypePropertyElement property in _properties)
			{
				EdmProperty edmProperty = new EdmProperty(property.FQName, property.GetTypeUsage());
				edmProperty.AddMetadataProperties(property.OtherContent);
				list.Add(edmProperty);
			}
			RowType rowType = new RowType(list);
			if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
			{
				rowType.DataSpace = DataSpace.CSpace;
			}
			else
			{
				rowType.DataSpace = DataSpace.SSpace;
			}
			rowType.AddMetadataProperties(base.OtherContent);
			_typeUsage = TypeUsage.Create(rowType);
		}
		return _typeUsage;
	}

	internal override bool ResolveNameAndSetTypeUsage(Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		bool result = true;
		if (_typeUsage == null)
		{
			foreach (RowTypePropertyElement property in _properties)
			{
				if (!property.ResolveNameAndSetTypeUsage(convertedItemCache, newGlobalItems))
				{
					result = false;
				}
			}
		}
		return result;
	}

	internal override void Validate()
	{
		foreach (RowTypePropertyElement property in _properties)
		{
			property.Validate();
		}
		if (_properties.Count == 0)
		{
			AddError(ErrorCode.RowTypeWithoutProperty, EdmSchemaErrorSeverity.Error, Strings.RowTypeWithoutProperty);
		}
	}
}
