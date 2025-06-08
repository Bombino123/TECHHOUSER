using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class Relationship : SchemaType, IRelationship
{
	private RelationshipEndCollection _ends;

	private List<ReferentialConstraint> _constraints;

	private bool _isForeignKey;

	public IList<IRelationshipEnd> Ends
	{
		get
		{
			if (_ends == null)
			{
				_ends = new RelationshipEndCollection();
			}
			return _ends;
		}
	}

	public IList<ReferentialConstraint> Constraints
	{
		get
		{
			if (_constraints == null)
			{
				_constraints = new List<ReferentialConstraint>();
			}
			return _constraints;
		}
	}

	public RelationshipKind RelationshipKind { get; private set; }

	public bool IsForeignKey => _isForeignKey;

	public Relationship(Schema parent, RelationshipKind kind)
		: base(parent)
	{
		RelationshipKind = kind;
		if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			_isForeignKey = false;
			base.OtherContent.Add(base.Schema.SchemaSource);
		}
		else if (base.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
		{
			_isForeignKey = true;
		}
	}

	public bool TryGetEnd(string roleName, out IRelationshipEnd end)
	{
		return _ends.TryGetEnd(roleName, out end);
	}

	internal override void Validate()
	{
		base.Validate();
		bool flag = false;
		foreach (RelationshipEnd end in Ends)
		{
			end.Validate();
			if (RelationshipKind == RelationshipKind.Association && end.Operations.Count > 0)
			{
				if (flag)
				{
					end.AddError(ErrorCode.InvalidOperation, EdmSchemaErrorSeverity.Error, Strings.InvalidOperationMultipleEndsInAssociation);
				}
				flag = true;
			}
		}
		if (Constraints.Count == 0)
		{
			if (base.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
			{
				AddError(ErrorCode.MissingConstraintOnRelationshipType, EdmSchemaErrorSeverity.Error, Strings.MissingConstraintOnRelationshipType(FQName));
			}
			return;
		}
		foreach (ReferentialConstraint constraint in Constraints)
		{
			constraint.Validate();
		}
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		foreach (RelationshipEnd end in Ends)
		{
			end.ResolveTopLevelNames();
		}
		foreach (ReferentialConstraint constraint in Constraints)
		{
			constraint.ResolveTopLevelNames();
		}
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "End"))
		{
			HandleEndElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "ReferentialConstraint"))
		{
			HandleConstraintElement(reader);
			return true;
		}
		return false;
	}

	private void HandleEndElement(XmlReader reader)
	{
		RelationshipEnd relationshipEnd = new RelationshipEnd(this);
		relationshipEnd.Parse(reader);
		if (Ends.Count == 2)
		{
			AddError(ErrorCode.InvalidAssociation, EdmSchemaErrorSeverity.Error, Strings.TooManyAssociationEnds(FQName));
		}
		else
		{
			Ends.Add(relationshipEnd);
		}
	}

	private void HandleConstraintElement(XmlReader reader)
	{
		ReferentialConstraint referentialConstraint = new ReferentialConstraint(this);
		referentialConstraint.Parse(reader);
		Constraints.Add(referentialConstraint);
		if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel && base.Schema.SchemaVersion >= 2.0)
		{
			_isForeignKey = true;
		}
	}
}
