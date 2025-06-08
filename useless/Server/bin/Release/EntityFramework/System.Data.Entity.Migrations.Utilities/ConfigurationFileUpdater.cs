using System.Data.Entity.Utilities;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace System.Data.Entity.Migrations.Utilities;

internal class ConfigurationFileUpdater
{
	private static readonly XNamespace _asm;

	private static readonly XElement _dependentAssemblyElement;

	static ConfigurationFileUpdater()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		_asm = XNamespace.op_Implicit("urn:schemas-microsoft-com:asm.v1");
		AssemblyName name = typeof(ConfigurationFileUpdater).Assembly().GetName();
		_dependentAssemblyElement = new XElement(_asm + "dependentAssembly", new object[2]
		{
			(object)new XElement(_asm + "assemblyIdentity", new object[3]
			{
				(object)new XAttribute(XName.op_Implicit("name"), (object)"EntityFramework"),
				(object)new XAttribute(XName.op_Implicit("culture"), (object)"neutral"),
				(object)new XAttribute(XName.op_Implicit("publicKeyToken"), (object)"b77a5c561934e089")
			}),
			(object)new XElement(_asm + "codeBase", new object[2]
			{
				(object)new XAttribute(XName.op_Implicit("version"), (object)name.Version.ToString()),
				(object)new XAttribute(XName.op_Implicit("href"), (object)name.CodeBase)
			})
		});
	}

	public virtual string Update(string configurationFile)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		int num;
		object obj;
		if (!string.IsNullOrWhiteSpace(configurationFile))
		{
			num = (File.Exists(configurationFile) ? 1 : 0);
			if (num != 0)
			{
				obj = XDocument.Load(configurationFile);
				goto IL_0021;
			}
		}
		else
		{
			num = 0;
		}
		obj = (object)new XDocument();
		goto IL_0021;
		IL_0021:
		XDocument val = (XDocument)obj;
		((XContainer)((XContainer)(object)((XContainer)(object)((XContainer)(object)val).GetOrAddElement(XName.op_Implicit("configuration"))).GetOrAddElement(XName.op_Implicit("runtime"))).GetOrAddElement(_asm + "assemblyBinding")).Add((object)_dependentAssemblyElement);
		string text = Path.GetTempFileName();
		if (num != 0)
		{
			File.Delete(text);
			text = Path.Combine(Path.GetDirectoryName(configurationFile), Path.GetFileName(text));
		}
		val.Save(text);
		return text;
	}
}
