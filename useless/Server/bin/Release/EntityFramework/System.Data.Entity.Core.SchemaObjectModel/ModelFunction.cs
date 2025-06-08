using System.Data.Entity.Core.Metadata.Edm;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class ModelFunction : Function
{
	private readonly TypeUsageBuilder _typeUsageBuilder;

	public override SchemaType Type => _type;

	internal TypeUsage TypeUsage
	{
		get
		{
			if (_typeUsageBuilder.TypeUsage == null)
			{
				return null;
			}
			if (base.CollectionKind != 0)
			{
				return TypeUsage.Create(new CollectionType(_typeUsageBuilder.TypeUsage));
			}
			return _typeUsageBuilder.TypeUsage;
		}
	}

	public ModelFunction(Schema parentElement)
		: base(parentElement)
	{
		_isComposable = true;
		_typeUsageBuilder = new TypeUsageBuilder(this);
	}

	internal void ValidateAndSetTypeUsage(ScalarType scalar)
	{
		_typeUsageBuilder.ValidateAndSetTypeUsage(scalar, complainOnMissingFacet: false);
	}

	internal void ValidateAndSetTypeUsage(EdmType edmType)
	{
		_typeUsageBuilder.ValidateAndSetTypeUsage(edmType, complainOnMissingFacet: false);
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "DefiningExpression"))
		{
			HandleDefiningExpressionElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "Parameter"))
		{
			HandleParameterElement(reader);
			return true;
		}
		return false;
	}

	protected override void HandleReturnTypeAttribute(XmlReader reader)
	{
		base.HandleReturnTypeAttribute(reader);
		_isComposable = true;
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (_typeUsageBuilder.HandleAttribute(reader))
		{
			return true;
		}
		return false;
	}

	internal override void ResolveTopLevelNames()
	{
		if (base.UnresolvedReturnType != null && base.Schema.ResolveTypeName(this, base.UnresolvedReturnType, out _type) && _type is ScalarType)
		{
			_typeUsageBuilder.ValidateAndSetTypeUsage(_type as ScalarType, complainOnMissingFacet: false);
		}
		foreach (Parameter parameter in base.Parameters)
		{
			parameter.ResolveTopLevelNames();
		}
		if (base.ReturnTypeList != null)
		{
			base.ReturnTypeList[0].ResolveTopLevelNames();
		}
	}

	private void HandleDefiningExpressionElement(XmlReader reader)
	{
		FunctionCommandText functionCommandText = new FunctionCommandText(this);
		functionCommandText.Parse(reader);
		_commandText = functionCommandText;
	}

	internal override void Validate()
	{
		base.Validate();
		ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);
		if (_isRefType)
		{
			ValidationHelper.ValidateRefType(this, _type);
		}
	}
}
