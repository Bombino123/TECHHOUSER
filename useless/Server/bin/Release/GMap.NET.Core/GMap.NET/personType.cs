using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace GMap.NET;

[Serializable]
[GeneratedCode("xsd", "4.0.30319.1")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "http://www.topografix.com/GPX/1/1")]
public class personType
{
	private string nameField;

	private emailType emailField;

	private linkType linkField;

	public string name
	{
		get
		{
			return nameField;
		}
		set
		{
			nameField = value;
		}
	}

	public emailType email
	{
		get
		{
			return emailField;
		}
		set
		{
			emailField = value;
		}
	}

	public linkType link
	{
		get
		{
			return linkField;
		}
		set
		{
			linkField = value;
		}
	}
}
