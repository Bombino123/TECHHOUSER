using System.Data.Entity.Core.Metadata.Edm;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal abstract class FacetDescriptionElement : SchemaElement
{
	private int? _minValue;

	private int? _maxValue;

	private bool _isConstant;

	private FacetDescription _facetDescription;

	public abstract EdmType FacetType { get; }

	public int? MinValue => _minValue;

	public int? MaxValue => _maxValue;

	public object DefaultValue { get; set; }

	public FacetDescription FacetDescription => _facetDescription;

	public FacetDescriptionElement(TypeElement type, string name)
		: base(type, name)
	{
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
		if (SchemaElement.CanHandleAttribute(reader, "Minimum"))
		{
			HandleMinimumAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Maximum"))
		{
			HandleMaximumAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "DefaultValue"))
		{
			HandleDefaultAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Constant"))
		{
			HandleConstantAttribute(reader);
			return true;
		}
		return false;
	}

	protected void HandleMinimumAttribute(XmlReader reader)
	{
		int field = -1;
		if (HandleIntAttribute(reader, ref field))
		{
			_minValue = field;
		}
	}

	protected void HandleMaximumAttribute(XmlReader reader)
	{
		int field = -1;
		if (HandleIntAttribute(reader, ref field))
		{
			_maxValue = field;
		}
	}

	protected abstract void HandleDefaultAttribute(XmlReader reader);

	protected void HandleConstantAttribute(XmlReader reader)
	{
		bool field = false;
		if (HandleBoolAttribute(reader, ref field))
		{
			_isConstant = field;
		}
	}

	internal void CreateAndValidateFacetDescription(string declaringTypeName)
	{
		_facetDescription = new FacetDescription(Name, FacetType, MinValue, MaxValue, DefaultValue, _isConstant, declaringTypeName);
	}
}
