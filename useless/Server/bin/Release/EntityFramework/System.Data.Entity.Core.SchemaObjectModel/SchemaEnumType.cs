using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class SchemaEnumType : SchemaType
{
	private bool _isFlags;

	private string _unresolvedUnderlyingTypeName;

	private SchemaType _underlyingType;

	private readonly IList<SchemaEnumMember> _enumMembers = new List<SchemaEnumMember>();

	public bool IsFlags => _isFlags;

	public SchemaType UnderlyingType => _underlyingType;

	public IEnumerable<SchemaEnumMember> EnumMembers => _enumMembers;

	public SchemaEnumType(Schema parentElement)
		: base(parentElement)
	{
		if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			base.OtherContent.Add(base.Schema.SchemaSource);
		}
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (!base.HandleElement(reader))
		{
			if (!CanHandleElement(reader, "Member"))
			{
				if (CanHandleElement(reader, "ValueAnnotation"))
				{
					SkipElement(reader);
					return true;
				}
				if (CanHandleElement(reader, "TypeAnnotation"))
				{
					SkipElement(reader);
					return true;
				}
				return false;
			}
			HandleMemberElement(reader);
		}
		return true;
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (!base.HandleAttribute(reader))
		{
			if (SchemaElement.CanHandleAttribute(reader, "IsFlags"))
			{
				HandleBoolAttribute(reader, ref _isFlags);
			}
			else
			{
				if (!SchemaElement.CanHandleAttribute(reader, "UnderlyingType"))
				{
					return false;
				}
				Utils.GetDottedName(base.Schema, reader, out _unresolvedUnderlyingTypeName);
			}
		}
		return true;
	}

	private void HandleMemberElement(XmlReader reader)
	{
		SchemaEnumMember schemaEnumMember = new SchemaEnumMember(this);
		schemaEnumMember.Parse(reader);
		if (!schemaEnumMember.Value.HasValue)
		{
			if (_enumMembers.Count == 0)
			{
				schemaEnumMember.Value = 0L;
			}
			else
			{
				long value = _enumMembers[_enumMembers.Count - 1].Value.Value;
				if (value < long.MaxValue)
				{
					schemaEnumMember.Value = value + 1;
				}
				else
				{
					AddError(ErrorCode.CalculatedEnumValueOutOfRange, EdmSchemaErrorSeverity.Error, Strings.CalculatedEnumValueOutOfRange);
					schemaEnumMember.Value = value;
				}
			}
		}
		_enumMembers.Add(schemaEnumMember);
	}

	internal override void ResolveTopLevelNames()
	{
		if (_unresolvedUnderlyingTypeName == null)
		{
			_underlyingType = base.Schema.SchemaManager.SchemaTypes.Single((SchemaType t) => t is ScalarType && ((ScalarType)t).TypeKind == PrimitiveTypeKind.Int32);
		}
		else
		{
			base.Schema.ResolveTypeName(this, _unresolvedUnderlyingTypeName, out _underlyingType);
		}
	}

	internal override void Validate()
	{
		base.Validate();
		ScalarType enumUnderlyingType = UnderlyingType as ScalarType;
		if (enumUnderlyingType == null || !Helper.IsSupportedEnumUnderlyingType(enumUnderlyingType.TypeKind))
		{
			AddError(ErrorCode.InvalidEnumUnderlyingType, EdmSchemaErrorSeverity.Error, Strings.InvalidEnumUnderlyingType);
		}
		else
		{
			foreach (SchemaEnumMember item in _enumMembers.Where((SchemaEnumMember m) => !Helper.IsEnumMemberValueInRange(enumUnderlyingType.TypeKind, m.Value.Value)))
			{
				item.AddError(ErrorCode.EnumMemberValueOutOfItsUnderylingTypeRange, EdmSchemaErrorSeverity.Error, Strings.EnumMemberValueOutOfItsUnderylingTypeRange(item.Value, item.Name, UnderlyingType.Name));
			}
		}
		if ((from o in _enumMembers
			group o by o.Name into g
			where g.Count() > 1
			select g).Any())
		{
			AddError(ErrorCode.DuplicateEnumMember, EdmSchemaErrorSeverity.Error, Strings.DuplicateEnumMember);
		}
	}
}
