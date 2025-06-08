using System;
using System.Collections.Generic;
using System.Linq;

namespace AntdUI.Svg;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Event)]
public class SvgAttributeAttribute : Attribute
{
	public const string SvgNamespace = "http://www.w3.org/2000/svg";

	public const string XLinkPrefix = "xlink";

	public const string XLinkNamespace = "http://www.w3.org/1999/xlink";

	public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";

	public static readonly List<KeyValuePair<string, string>> Namespaces = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("", "http://www.w3.org/2000/svg"),
		new KeyValuePair<string, string>("xlink", "http://www.w3.org/1999/xlink"),
		new KeyValuePair<string, string>("xml", "http://www.w3.org/XML/1998/namespace")
	};

	private bool _inAttrDictionary;

	private string _name;

	private string _namespace;

	public string NamespaceAndName
	{
		get
		{
			if (_namespace == "http://www.w3.org/2000/svg")
			{
				return _name;
			}
			return Namespaces.First<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => x.Value == _namespace).Key + ":" + _name;
		}
	}

	public string Name => _name;

	public string NameSpace => _namespace;

	public bool InAttributeDictionary => _inAttrDictionary;

	public override bool Equals(object obj)
	{
		return Match(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override bool Match(object obj)
	{
		if (!(obj is SvgAttributeAttribute svgAttributeAttribute))
		{
			return false;
		}
		if (svgAttributeAttribute.Name == string.Empty)
		{
			return false;
		}
		return string.Compare(svgAttributeAttribute.Name, Name) == 0;
	}

	internal SvgAttributeAttribute()
	{
		_name = string.Empty;
	}

	internal SvgAttributeAttribute(string name)
	{
		_name = name;
		_namespace = "http://www.w3.org/2000/svg";
	}

	internal SvgAttributeAttribute(string name, bool inAttrDictionary)
	{
		_name = name;
		_namespace = "http://www.w3.org/2000/svg";
		_inAttrDictionary = inAttrDictionary;
	}

	public SvgAttributeAttribute(string name, string nameSpace)
	{
		_name = name;
		_namespace = nameSpace;
	}
}
