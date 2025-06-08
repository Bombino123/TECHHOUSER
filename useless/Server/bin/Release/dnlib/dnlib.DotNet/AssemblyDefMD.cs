using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Utils;

namespace dnlib.DotNet;

internal sealed class AssemblyDefMD : AssemblyDef, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	private volatile bool hasInitdTFA;

	private string tfaFramework;

	private Version tfaVersion;

	private string tfaProfile;

	private bool tfaReturnValue;

	private static readonly UTF8String nameSystemRuntimeVersioning = new UTF8String("System.Runtime.Versioning");

	private static readonly UTF8String nameTargetFrameworkAttribute = new UTF8String("TargetFrameworkAttribute");

	public uint OrigRid => origRid;

	protected override void InitializeDeclSecurities()
	{
		RidList declSecurityRidList = readerModule.Metadata.GetDeclSecurityRidList(Table.Assembly, origRid);
		LazyList<DeclSecurity, RidList> value = new LazyList<DeclSecurity, RidList>(declSecurityRidList.Count, declSecurityRidList, (RidList list2, int index) => readerModule.ResolveDeclSecurity(list2[index]));
		Interlocked.CompareExchange(ref declSecurities, value, null);
	}

	protected override void InitializeModules()
	{
		RidList moduleRidList = readerModule.GetModuleRidList();
		LazyList<ModuleDef, RidList> value = new LazyList<ModuleDef, RidList>(moduleRidList.Count + 1, this, moduleRidList, delegate(RidList list2, int index)
		{
			ModuleDef moduleDef = ((index != 0) ? readerModule.ReadModule(list2[index - 1], this) : readerModule);
			if (moduleDef == null)
			{
				moduleDef = new ModuleDefUser("INVALID", Guid.NewGuid());
			}
			moduleDef.Assembly = this;
			return moduleDef;
		});
		Interlocked.CompareExchange(ref modules, value, null);
	}

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.Assembly, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), default(GenericParamContext), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	public override bool TryGetOriginalTargetFrameworkAttribute(out string framework, out Version version, out string profile)
	{
		if (!hasInitdTFA)
		{
			InitializeTargetFrameworkAttribute();
		}
		framework = tfaFramework;
		version = tfaVersion;
		profile = tfaProfile;
		return tfaReturnValue;
	}

	private void InitializeTargetFrameworkAttribute()
	{
		if (hasInitdTFA)
		{
			return;
		}
		RidList customAttributeRidList = readerModule.Metadata.GetCustomAttributeRidList(Table.Assembly, origRid);
		GenericParamContext gpContext = default(GenericParamContext);
		for (int i = 0; i < customAttributeRidList.Count; i++)
		{
			uint num = customAttributeRidList[i];
			if (!readerModule.TablesStream.TryReadCustomAttributeRow(num, out var row))
			{
				continue;
			}
			ICustomAttributeType customAttributeType = readerModule.ResolveCustomAttributeType(row.Type, gpContext);
			if (TryGetName(customAttributeType, out var ns, out var uTF8String) && !(ns != nameSystemRuntimeVersioning) && !(uTF8String != nameTargetFrameworkAttribute))
			{
				CustomAttribute customAttribute = CustomAttributeReader.Read(readerModule, customAttributeType, row.Value, gpContext);
				if (customAttribute != null && customAttribute.ConstructorArguments.Count == 1 && customAttribute.ConstructorArguments[0].Value is UTF8String uTF8String2 && TryCreateTargetFrameworkInfo(uTF8String2, out var framework, out var version, out var profile))
				{
					tfaFramework = framework;
					tfaVersion = version;
					tfaProfile = profile;
					tfaReturnValue = true;
					break;
				}
			}
		}
		hasInitdTFA = true;
	}

	private static bool TryGetName(ICustomAttributeType caType, out UTF8String ns, out UTF8String name)
	{
		ITypeDefOrRef typeDefOrRef = ((!(caType is MemberRef memberRef)) ? (caType as MethodDef)?.DeclaringType : memberRef.DeclaringType);
		if (typeDefOrRef is TypeRef typeRef)
		{
			ns = typeRef.Namespace;
			name = typeRef.Name;
			return true;
		}
		if (typeDefOrRef is TypeDef typeDef)
		{
			ns = typeDef.Namespace;
			name = typeDef.Name;
			return true;
		}
		ns = null;
		name = null;
		return false;
	}

	private static bool TryCreateTargetFrameworkInfo(string attrString, out string framework, out Version version, out string profile)
	{
		framework = null;
		version = null;
		profile = null;
		string[] array = attrString.Split(new char[1] { ',' });
		if (array.Length < 2 || array.Length > 3)
		{
			return false;
		}
		string text = array[0].Trim();
		if (text.Length == 0)
		{
			return false;
		}
		Version version2 = null;
		string text2 = null;
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { '=' });
			if (array2.Length != 2)
			{
				return false;
			}
			string text3 = array2[0].Trim();
			string text4 = array2[1].Trim();
			if (text3.Equals("Version", StringComparison.OrdinalIgnoreCase))
			{
				if (text4.StartsWith("v", StringComparison.OrdinalIgnoreCase))
				{
					text4 = text4.Substring(1);
				}
				if (!TryParse(text4, out version2))
				{
					return false;
				}
				version2 = new Version(version2.Major, version2.Minor, (version2.Build != -1) ? version2.Build : 0, 0);
			}
			else if (text3.Equals("Profile", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(text4))
			{
				text2 = text4;
			}
		}
		if ((object)version2 == null)
		{
			return false;
		}
		framework = text;
		version = version2;
		profile = text2;
		return true;
	}

	private static int ParseInt32(string s)
	{
		if (!int.TryParse(s, out var result))
		{
			return 0;
		}
		return result;
	}

	private static bool TryParse(string s, out Version version)
	{
		Match match = Regex.Match(s, "^(\\d+)\\.(\\d+)$");
		if (match.Groups.Count == 3)
		{
			version = new Version(ParseInt32(match.Groups[1].Value), ParseInt32(match.Groups[2].Value));
			return true;
		}
		match = Regex.Match(s, "^(\\d+)\\.(\\d+)\\.(\\d+)$");
		if (match.Groups.Count == 4)
		{
			version = new Version(ParseInt32(match.Groups[1].Value), ParseInt32(match.Groups[2].Value), ParseInt32(match.Groups[3].Value));
			return true;
		}
		match = Regex.Match(s, "^(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)$");
		if (match.Groups.Count == 5)
		{
			version = new Version(ParseInt32(match.Groups[1].Value), ParseInt32(match.Groups[2].Value), ParseInt32(match.Groups[3].Value), ParseInt32(match.Groups[4].Value));
			return true;
		}
		version = null;
		return false;
	}

	public AssemblyDefMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		if (rid != 1)
		{
			modules = new LazyList<ModuleDef>(this);
		}
		readerModule.TablesStream.TryReadAssemblyRow(origRid, out var row);
		hashAlgorithm = (AssemblyHashAlgorithm)row.HashAlgId;
		version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);
		attributes = (int)row.Flags;
		name = readerModule.StringsStream.ReadNoNull(row.Name);
		culture = readerModule.StringsStream.ReadNoNull(row.Locale);
		publicKey = new PublicKey(readerModule.BlobStream.Read(row.PublicKey));
	}
}
