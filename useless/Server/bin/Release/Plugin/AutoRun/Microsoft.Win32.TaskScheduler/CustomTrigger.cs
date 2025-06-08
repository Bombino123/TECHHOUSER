using System;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public sealed class CustomTrigger : Trigger, ITriggerDelay
{
	private readonly NamedValueCollection nvc = new NamedValueCollection();

	private TimeSpan delay = TimeSpan.MinValue;

	private string name = string.Empty;

	public TimeSpan Delay
	{
		get
		{
			return delay;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string Name => name;

	[XmlArray]
	[XmlArrayItem("Property")]
	public NamedValueCollection Properties => nvc;

	internal CustomTrigger([NotNull] ITrigger iTrigger)
		: base(iTrigger)
	{
	}

	public override object Clone()
	{
		throw new InvalidOperationException("CustomTrigger cannot be cloned due to OS restrictions.");
	}

	internal void UpdateFromXml(string xml)
	{
		nvc.Clear();
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlNamespaceManager.AddNamespace("n", "http://schemas.microsoft.com/windows/2004/02/mit/task");
			XmlNode xmlNode = xmlDocument.DocumentElement?.SelectSingleNode("n:Triggers/*[@id='" + base.Id + "']", xmlNamespaceManager);
			if (xmlNode == null)
			{
				XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("WnfStateChangeTrigger");
				if (elementsByTagName.Count == 1)
				{
					xmlNode = elementsByTagName[0];
				}
			}
			if (xmlNode == null)
			{
				return;
			}
			name = xmlNode.LocalName;
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				switch (childNode.LocalName)
				{
				case "Delay":
					delay = Task.StringToTimeSpan(childNode.InnerText);
					break;
				default:
					nvc.Add(childNode.LocalName, childNode.InnerText);
					break;
				case "StartBoundary":
				case "Enabled":
				case "EndBoundary":
				case "ExecutionTimeLimit":
					break;
				}
			}
		}
		catch
		{
		}
	}

	protected override string V2GetTriggerString()
	{
		return Resources.TriggerCustom1;
	}
}
