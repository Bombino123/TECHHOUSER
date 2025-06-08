using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class RelationshipEnd : SchemaElement, IRelationshipEnd
{
	private string _unresolvedType;

	private RelationshipMultiplicity? _multiplicity;

	private List<OnOperation> _operations;

	public SchemaEntityType Type { get; private set; }

	public RelationshipMultiplicity? Multiplicity
	{
		get
		{
			return _multiplicity;
		}
		set
		{
			_multiplicity = value;
		}
	}

	public ICollection<OnOperation> Operations
	{
		get
		{
			if (_operations == null)
			{
				_operations = new List<OnOperation>();
			}
			return _operations;
		}
	}

	internal new IRelationship ParentElement => (IRelationship)base.ParentElement;

	public RelationshipEnd(Relationship relationship)
		: base(relationship)
	{
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		if (Type == null && _unresolvedType != null && base.Schema.ResolveTypeName(this, _unresolvedType, out var type))
		{
			Type = type as SchemaEntityType;
			if (Type == null)
			{
				AddError(ErrorCode.InvalidRelationshipEndType, EdmSchemaErrorSeverity.Error, Strings.InvalidRelationshipEndType(ParentElement.Name, type.FQName));
			}
		}
	}

	internal override void Validate()
	{
		base.Validate();
		if (Multiplicity == RelationshipMultiplicity.Many && Operations.Count != 0)
		{
			AddError(ErrorCode.EndWithManyMultiplicityCannotHaveOperationsSpecified, EdmSchemaErrorSeverity.Error, Strings.EndWithManyMultiplicityCannotHaveOperationsSpecified(Name, ParentElement.FQName));
		}
		if (ParentElement.Constraints.Count == 0 && !Multiplicity.HasValue)
		{
			AddError(ErrorCode.EndWithoutMultiplicity, EdmSchemaErrorSeverity.Error, Strings.EndWithoutMultiplicity(Name, ParentElement.FQName));
		}
	}

	protected override void HandleAttributesComplete()
	{
		if (Name == null && _unresolvedType != null)
		{
			Name = Utils.ExtractTypeName(_unresolvedType);
		}
		base.HandleAttributesComplete();
	}

	protected override bool ProhibitAttribute(string namespaceUri, string localName)
	{
		if (base.ProhibitAttribute(namespaceUri, localName))
		{
			return true;
		}
		if (namespaceUri == null)
		{
			_ = localName == "Name";
			return false;
		}
		return false;
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Multiplicity"))
		{
			HandleMultiplicityAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Role"))
		{
			HandleNameAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Type"))
		{
			HandleTypeAttribute(reader);
			return true;
		}
		return false;
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "OnDelete"))
		{
			HandleOnDeleteElement(reader);
			return true;
		}
		return false;
	}

	private void HandleTypeAttribute(XmlReader reader)
	{
		if (Utils.GetDottedName(base.Schema, reader, out var name))
		{
			_unresolvedType = name;
		}
	}

	private void HandleMultiplicityAttribute(XmlReader reader)
	{
		if (!RelationshipMultiplicityConverter.TryParseMultiplicity(reader.Value, out var multiplicity))
		{
			AddError(ErrorCode.InvalidMultiplicity, EdmSchemaErrorSeverity.Error, reader, Strings.InvalidRelationshipEndMultiplicity(ParentElement.Name, reader.Value));
		}
		_multiplicity = multiplicity;
	}

	private void HandleOnDeleteElement(XmlReader reader)
	{
		HandleOnOperationElement(reader, Operation.Delete);
	}

	private void HandleOnOperationElement(XmlReader reader, Operation operation)
	{
		foreach (OnOperation operation2 in Operations)
		{
			if (operation2.Operation == operation)
			{
				AddError(ErrorCode.InvalidOperation, EdmSchemaErrorSeverity.Error, reader, Strings.DuplicationOperation(reader.Name));
			}
		}
		OnOperation onOperation = new OnOperation(this, operation);
		onOperation.Parse(reader);
		_operations.Add(onOperation);
	}
}
