using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using dnlib.Threading;

namespace dnlib.DotNet;

public class AssemblyResolver : IAssemblyResolver
{
	private sealed class GacInfo
	{
		public readonly int Version;

		public readonly string Path;

		public readonly string Prefix;

		public readonly string[] SubDirs;

		public GacInfo(int version, string prefix, string path, string[] subDirs)
		{
			Version = version;
			Prefix = prefix;
			Path = path;
			SubDirs = subDirs;
		}
	}

	private static readonly ModuleDef nullModule;

	private static readonly string[] assemblyExtensions;

	private static readonly string[] winMDAssemblyExtensions;

	private static readonly List<GacInfo> gacInfos;

	private static readonly string[] extraMonoPaths;

	private static readonly string[] monoVerDirs;

	private ModuleContext defaultModuleContext;

	private readonly Dictionary<ModuleDef, List<string>> moduleSearchPaths = new Dictionary<ModuleDef, List<string>>();

	private readonly Dictionary<string, AssemblyDef> cachedAssemblies = new Dictionary<string, AssemblyDef>(StringComparer.OrdinalIgnoreCase);

	private readonly List<string> preSearchPaths = new List<string>();

	private readonly List<string> postSearchPaths = new List<string>();

	private bool findExactMatch;

	private bool enableFrameworkRedirect;

	private bool enableTypeDefCache = true;

	private bool useGac = true;

	private readonly Lock theLock = Lock.Create();

	public ModuleContext DefaultModuleContext
	{
		get
		{
			return defaultModuleContext;
		}
		set
		{
			defaultModuleContext = value;
		}
	}

	public bool FindExactMatch
	{
		get
		{
			return findExactMatch;
		}
		set
		{
			findExactMatch = value;
		}
	}

	public bool EnableFrameworkRedirect
	{
		get
		{
			return enableFrameworkRedirect;
		}
		set
		{
			enableFrameworkRedirect = value;
		}
	}

	public bool EnableTypeDefCache
	{
		get
		{
			return enableTypeDefCache;
		}
		set
		{
			enableTypeDefCache = value;
		}
	}

	public bool UseGAC
	{
		get
		{
			return useGac;
		}
		set
		{
			useGac = value;
		}
	}

	public IList<string> PreSearchPaths => preSearchPaths;

	public IList<string> PostSearchPaths => postSearchPaths;

	static AssemblyResolver()
	{
		nullModule = new ModuleDefUser();
		assemblyExtensions = new string[2] { ".dll", ".exe" };
		winMDAssemblyExtensions = new string[1] { ".winmd" };
		monoVerDirs = new string[14]
		{
			"4.5", "4.5\\Facades", "4.5-api", "4.5-api\\Facades", "4.0", "4.0-api", "3.5", "3.5-api", "3.0", "3.0-api",
			"2.0", "2.0-api", "1.1", "1.0"
		};
		gacInfos = new List<GacInfo>();
		if ((object)Type.GetType("Mono.Runtime") != null)
		{
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
			List<string> list = new List<string>();
			foreach (string item in FindMonoPrefixes())
			{
				string text = Path.Combine(Path.Combine(Path.Combine(item, "lib"), "mono"), "gac");
				if (dictionary.ContainsKey(text))
				{
					continue;
				}
				dictionary[text] = true;
				if (Directory.Exists(text))
				{
					gacInfos.Add(new GacInfo(-1, "", Path.GetDirectoryName(text), new string[1] { Path.GetFileName(text) }));
				}
				text = Path.GetDirectoryName(text);
				string[] array = monoVerDirs;
				foreach (string obj in array)
				{
					string text2 = text;
					string[] array2 = obj.Split(new char[1] { '\\' });
					foreach (string path in array2)
					{
						text2 = Path.Combine(text2, path);
					}
					if (Directory.Exists(text2))
					{
						list.Add(text2);
					}
				}
			}
			string environmentVariable = Environment.GetEnvironmentVariable("MONO_PATH");
			if (environmentVariable != null)
			{
				string[] array = environmentVariable.Split(new char[1] { Path.PathSeparator });
				for (int i = 0; i < array.Length; i++)
				{
					string text3 = array[i].Trim();
					if (text3 != string.Empty && Directory.Exists(text3))
					{
						list.Add(text3);
					}
				}
			}
			extraMonoPaths = list.ToArray();
			return;
		}
		string environmentVariable2 = Environment.GetEnvironmentVariable("WINDIR");
		if (!string.IsNullOrEmpty(environmentVariable2))
		{
			string path2 = Path.Combine(environmentVariable2, "assembly");
			if (Directory.Exists(path2))
			{
				gacInfos.Add(new GacInfo(2, "", path2, new string[4] { "GAC_32", "GAC_64", "GAC_MSIL", "GAC" }));
			}
			path2 = Path.Combine(Path.Combine(environmentVariable2, "Microsoft.NET"), "assembly");
			if (Directory.Exists(path2))
			{
				gacInfos.Add(new GacInfo(4, "v4.0_", path2, new string[3] { "GAC_32", "GAC_64", "GAC_MSIL" }));
			}
		}
	}

	private static string GetCurrentMonoPrefix()
	{
		string text = typeof(object).Module.FullyQualifiedName;
		for (int i = 0; i < 4; i++)
		{
			text = Path.GetDirectoryName(text);
		}
		return text;
	}

	private static IEnumerable<string> FindMonoPrefixes()
	{
		yield return GetCurrentMonoPrefix();
		string environmentVariable = Environment.GetEnvironmentVariable("MONO_GAC_PREFIX");
		if (string.IsNullOrEmpty(environmentVariable))
		{
			yield break;
		}
		string[] array = environmentVariable.Split(new char[1] { Path.PathSeparator });
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (text != string.Empty)
			{
				yield return text;
			}
		}
	}

	public AssemblyResolver()
		: this(null)
	{
	}

	public AssemblyResolver(ModuleContext defaultModuleContext)
	{
		this.defaultModuleContext = defaultModuleContext;
		enableFrameworkRedirect = true;
	}

	public AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule)
	{
		if (assembly == null)
		{
			return null;
		}
		if (EnableFrameworkRedirect && !FindExactMatch)
		{
			FrameworkRedirect.ApplyFrameworkRedirect(ref assembly, sourceModule);
		}
		theLock.EnterWriteLock();
		try
		{
			AssemblyDef assemblyDef = Resolve2(assembly, sourceModule);
			if (assemblyDef == null)
			{
				string text = UTF8String.ToSystemStringOrEmpty(assembly.Name);
				string text2 = text.Trim();
				if (text != text2)
				{
					assembly = new AssemblyNameInfo
					{
						Name = text2,
						Version = assembly.Version,
						PublicKeyOrToken = assembly.PublicKeyOrToken,
						Culture = assembly.Culture
					};
					assemblyDef = Resolve2(assembly, sourceModule);
				}
			}
			if (assemblyDef == null)
			{
				cachedAssemblies[GetAssemblyNameKey(assembly)] = null;
				return null;
			}
			string assemblyNameKey = GetAssemblyNameKey(assemblyDef);
			string assemblyNameKey2 = GetAssemblyNameKey(assembly);
			cachedAssemblies.TryGetValue(assemblyNameKey, out var value);
			cachedAssemblies.TryGetValue(assemblyNameKey2, out var value2);
			if (value != assemblyDef && value2 != assemblyDef && enableTypeDefCache)
			{
				IList<ModuleDef> modules = assemblyDef.Modules;
				int count = modules.Count;
				for (int i = 0; i < count; i++)
				{
					ModuleDef moduleDef = modules[i];
					if (moduleDef != null)
					{
						moduleDef.EnableTypeDefFindCache = true;
					}
				}
			}
			bool flag = false;
			if (!cachedAssemblies.ContainsKey(assemblyNameKey))
			{
				cachedAssemblies.Add(assemblyNameKey, assemblyDef);
				flag = true;
			}
			if (!cachedAssemblies.ContainsKey(assemblyNameKey2))
			{
				cachedAssemblies.Add(assemblyNameKey2, assemblyDef);
				flag = true;
			}
			if (flag || value == assemblyDef || value2 == assemblyDef)
			{
				return assemblyDef;
			}
			assemblyDef.ManifestModule?.Dispose();
			return value ?? value2;
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	public bool AddToCache(ModuleDef module)
	{
		if (module != null)
		{
			return AddToCache(module.Assembly);
		}
		return false;
	}

	public bool AddToCache(AssemblyDef asm)
	{
		if (asm == null)
		{
			return false;
		}
		string assemblyNameKey = GetAssemblyNameKey(asm);
		theLock.EnterWriteLock();
		try
		{
			if (cachedAssemblies.TryGetValue(assemblyNameKey, out var value) && value != null)
			{
				return asm == value;
			}
			cachedAssemblies[assemblyNameKey] = asm;
			return true;
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	public bool Remove(ModuleDef module)
	{
		if (module != null)
		{
			return Remove(module.Assembly);
		}
		return false;
	}

	public bool Remove(AssemblyDef asm)
	{
		if (asm == null)
		{
			return false;
		}
		string assemblyNameKey = GetAssemblyNameKey(asm);
		theLock.EnterWriteLock();
		try
		{
			return cachedAssemblies.Remove(assemblyNameKey);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	public void Clear()
	{
		theLock.EnterWriteLock();
		List<AssemblyDef> list;
		try
		{
			list = new List<AssemblyDef>(cachedAssemblies.Values);
			cachedAssemblies.Clear();
		}
		finally
		{
			theLock.ExitWriteLock();
		}
		foreach (AssemblyDef item in list)
		{
			if (item == null)
			{
				continue;
			}
			foreach (ModuleDef module in item.Modules)
			{
				module.Dispose();
			}
		}
	}

	public IEnumerable<AssemblyDef> GetCachedAssemblies()
	{
		theLock.EnterReadLock();
		try
		{
			return cachedAssemblies.Values.ToArray();
		}
		finally
		{
			theLock.ExitReadLock();
		}
	}

	private static string GetAssemblyNameKey(IAssembly asmName)
	{
		return asmName.FullNameToken;
	}

	private AssemblyDef Resolve2(IAssembly assembly, ModuleDef sourceModule)
	{
		if (cachedAssemblies.TryGetValue(GetAssemblyNameKey(assembly), out var value))
		{
			return value;
		}
		ModuleContext context = defaultModuleContext;
		if (context == null && sourceModule != null)
		{
			context = sourceModule.Context;
		}
		value = FindExactAssembly(assembly, PreFindAssemblies(assembly, sourceModule, matchExactly: true), context) ?? FindExactAssembly(assembly, FindAssemblies(assembly, sourceModule, matchExactly: true), context) ?? FindExactAssembly(assembly, PostFindAssemblies(assembly, sourceModule, matchExactly: true), context);
		if (value != null)
		{
			return value;
		}
		if (!findExactMatch)
		{
			value = FindClosestAssembly(assembly);
			value = FindClosestAssembly(assembly, value, PreFindAssemblies(assembly, sourceModule, matchExactly: false), context);
			value = FindClosestAssembly(assembly, value, FindAssemblies(assembly, sourceModule, matchExactly: false), context);
			value = FindClosestAssembly(assembly, value, PostFindAssemblies(assembly, sourceModule, matchExactly: false), context);
		}
		return value;
	}

	private AssemblyDef FindExactAssembly(IAssembly assembly, IEnumerable<string> paths, ModuleContext moduleContext)
	{
		if (paths == null)
		{
			return null;
		}
		AssemblyNameComparer compareAll = AssemblyNameComparer.CompareAll;
		foreach (string path in paths)
		{
			ModuleDefMD moduleDefMD = null;
			try
			{
				moduleDefMD = ModuleDefMD.Load(path, moduleContext);
				AssemblyDef assembly2 = moduleDefMD.Assembly;
				if (assembly2 != null && compareAll.Equals(assembly, assembly2))
				{
					moduleDefMD = null;
					return assembly2;
				}
			}
			catch
			{
			}
			finally
			{
				moduleDefMD?.Dispose();
			}
		}
		return null;
	}

	private AssemblyDef FindClosestAssembly(IAssembly assembly)
	{
		AssemblyDef assemblyDef = null;
		AssemblyNameComparer compareAll = AssemblyNameComparer.CompareAll;
		foreach (KeyValuePair<string, AssemblyDef> cachedAssembly in cachedAssemblies)
		{
			AssemblyDef value = cachedAssembly.Value;
			if (value != null && compareAll.CompareClosest(assembly, assemblyDef, value) == 1)
			{
				assemblyDef = value;
			}
		}
		return assemblyDef;
	}

	private AssemblyDef FindClosestAssembly(IAssembly assembly, AssemblyDef closest, IEnumerable<string> paths, ModuleContext moduleContext)
	{
		if (paths == null)
		{
			return closest;
		}
		AssemblyNameComparer compareAll = AssemblyNameComparer.CompareAll;
		foreach (string path in paths)
		{
			ModuleDefMD moduleDefMD = null;
			try
			{
				moduleDefMD = ModuleDefMD.Load(path, moduleContext);
				AssemblyDef assembly2 = moduleDefMD.Assembly;
				if (assembly2 != null && compareAll.CompareClosest(assembly, closest, assembly2) == 1)
				{
					if (!IsCached(closest))
					{
						closest?.ManifestModule?.Dispose();
					}
					closest = assembly2;
					moduleDefMD = null;
				}
			}
			catch
			{
			}
			finally
			{
				moduleDefMD?.Dispose();
			}
		}
		return closest;
	}

	private bool IsCached(AssemblyDef asm)
	{
		if (asm == null)
		{
			return false;
		}
		if (cachedAssemblies.TryGetValue(GetAssemblyNameKey(asm), out var value))
		{
			return value == asm;
		}
		return false;
	}

	private IEnumerable<string> FindAssemblies2(IAssembly assembly, IEnumerable<string> paths)
	{
		if (paths == null)
		{
			yield break;
		}
		string asmSimpleName = UTF8String.ToSystemStringOrEmpty(assembly.Name);
		string[] array = (assembly.IsContentTypeWindowsRuntime ? winMDAssemblyExtensions : assemblyExtensions);
		string[] array2 = array;
		foreach (string ext in array2)
		{
			foreach (string path in paths)
			{
				string text;
				try
				{
					text = Path.Combine(path, asmSimpleName + ext);
				}
				catch (ArgumentException)
				{
					yield break;
				}
				if (File.Exists(text))
				{
					yield return text;
				}
			}
		}
	}

	protected virtual IEnumerable<string> PreFindAssemblies(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
	{
		foreach (string item in FindAssemblies2(assembly, preSearchPaths))
		{
			yield return item;
		}
	}

	protected virtual IEnumerable<string> PostFindAssemblies(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
	{
		foreach (string item in FindAssemblies2(assembly, postSearchPaths))
		{
			yield return item;
		}
	}

	protected virtual IEnumerable<string> FindAssemblies(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
	{
		if (assembly.IsContentTypeWindowsRuntime)
		{
			string text;
			try
			{
				text = Path.Combine(Path.Combine(Environment.SystemDirectory, "WinMetadata"), string.Concat(assembly.Name, ".winmd"));
			}
			catch (ArgumentException)
			{
				text = null;
			}
			if (File.Exists(text))
			{
				yield return text;
			}
		}
		else if (UseGAC)
		{
			foreach (string item in FindAssembliesGac(assembly, sourceModule, matchExactly))
			{
				yield return item;
			}
		}
		foreach (string item2 in FindAssembliesModuleSearchPaths(assembly, sourceModule, matchExactly))
		{
			yield return item2;
		}
	}

	private IEnumerable<string> FindAssembliesGac(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
	{
		if (matchExactly)
		{
			return FindAssembliesGacExactly(assembly, sourceModule);
		}
		return FindAssembliesGacAny(assembly, sourceModule);
	}

	private IEnumerable<GacInfo> GetGacInfos(ModuleDef sourceModule)
	{
		int version = ((sourceModule == null) ? int.MinValue : (sourceModule.IsClr40 ? 4 : 2));
		foreach (GacInfo gacInfo in gacInfos)
		{
			if (gacInfo.Version == version)
			{
				yield return gacInfo;
			}
		}
		foreach (GacInfo gacInfo2 in gacInfos)
		{
			if (gacInfo2.Version != version)
			{
				yield return gacInfo2;
			}
		}
	}

	private IEnumerable<string> FindAssembliesGacExactly(IAssembly assembly, ModuleDef sourceModule)
	{
		foreach (GacInfo gacInfo in GetGacInfos(sourceModule))
		{
			foreach (string item in FindAssembliesGacExactly(gacInfo, assembly, sourceModule))
			{
				yield return item;
			}
		}
		if (extraMonoPaths == null)
		{
			yield break;
		}
		foreach (string extraMonoPath in GetExtraMonoPaths(assembly, sourceModule))
		{
			yield return extraMonoPath;
		}
	}

	private static IEnumerable<string> GetExtraMonoPaths(IAssembly assembly, ModuleDef sourceModule)
	{
		if (extraMonoPaths == null)
		{
			yield break;
		}
		string[] array = extraMonoPaths;
		foreach (string path in array)
		{
			string text;
			try
			{
				text = Path.Combine(path, string.Concat(assembly.Name, ".dll"));
			}
			catch (ArgumentException)
			{
				break;
			}
			if (File.Exists(text))
			{
				yield return text;
			}
		}
	}

	private IEnumerable<string> FindAssembliesGacExactly(GacInfo gacInfo, IAssembly assembly, ModuleDef sourceModule)
	{
		PublicKeyToken publicKeyToken = PublicKeyBase.ToPublicKeyToken(assembly.PublicKeyOrToken);
		if (gacInfo == null || publicKeyToken == null)
		{
			yield break;
		}
		string pktString = publicKeyToken.ToString();
		string verString = Utils.CreateVersionWithNoUndefinedValues(assembly.Version).ToString();
		string cultureString = UTF8String.ToSystemStringOrEmpty(assembly.Culture);
		if (cultureString.Equals("neutral", StringComparison.OrdinalIgnoreCase))
		{
			cultureString = string.Empty;
		}
		string asmSimpleName = UTF8String.ToSystemStringOrEmpty(assembly.Name);
		string[] subDirs = gacInfo.SubDirs;
		foreach (string path in subDirs)
		{
			string path2 = Path.Combine(gacInfo.Path, path);
			try
			{
				path2 = Path.Combine(path2, asmSimpleName);
			}
			catch (ArgumentException)
			{
				break;
			}
			path2 = Path.Combine(path2, gacInfo.Prefix + verString + "_" + cultureString + "_" + pktString);
			string text = Path.Combine(path2, asmSimpleName + ".dll");
			if (File.Exists(text))
			{
				yield return text;
			}
		}
	}

	private IEnumerable<string> FindAssembliesGacAny(IAssembly assembly, ModuleDef sourceModule)
	{
		foreach (GacInfo gacInfo in GetGacInfos(sourceModule))
		{
			foreach (string item in FindAssembliesGacAny(gacInfo, assembly, sourceModule))
			{
				yield return item;
			}
		}
		if (extraMonoPaths == null)
		{
			yield break;
		}
		foreach (string extraMonoPath in GetExtraMonoPaths(assembly, sourceModule))
		{
			yield return extraMonoPath;
		}
	}

	private IEnumerable<string> FindAssembliesGacAny(GacInfo gacInfo, IAssembly assembly, ModuleDef sourceModule)
	{
		if (gacInfo == null)
		{
			yield break;
		}
		string asmSimpleName = UTF8String.ToSystemStringOrEmpty(assembly.Name);
		string[] subDirs = gacInfo.SubDirs;
		foreach (string path in subDirs)
		{
			string path2 = Path.Combine(gacInfo.Path, path);
			try
			{
				path2 = Path.Combine(path2, asmSimpleName);
			}
			catch (ArgumentException)
			{
				break;
			}
			foreach (string dir in GetDirs(path2))
			{
				string text = Path.Combine(dir, asmSimpleName + ".dll");
				if (File.Exists(text))
				{
					yield return text;
				}
			}
		}
	}

	private IEnumerable<string> GetDirs(string baseDir)
	{
		if (!Directory.Exists(baseDir))
		{
			return Array2.Empty<string>();
		}
		List<string> list = new List<string>();
		try
		{
			DirectoryInfo[] directories = new DirectoryInfo(baseDir).GetDirectories();
			foreach (DirectoryInfo directoryInfo in directories)
			{
				list.Add(directoryInfo.FullName);
			}
		}
		catch
		{
		}
		return list;
	}

	private IEnumerable<string> FindAssembliesModuleSearchPaths(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
	{
		string asmSimpleName = UTF8String.ToSystemStringOrEmpty(assembly.Name);
		IEnumerable<string> searchPaths = GetSearchPaths(sourceModule);
		string[] array = (assembly.IsContentTypeWindowsRuntime ? winMDAssemblyExtensions : assemblyExtensions);
		string[] array2 = array;
		foreach (string ext in array2)
		{
			foreach (string path in searchPaths)
			{
				for (int i = 0; i < 2; i++)
				{
					string text;
					try
					{
						text = ((i != 0) ? Path.Combine(Path.Combine(path, asmSimpleName), asmSimpleName + ext) : Path.Combine(path, asmSimpleName + ext));
					}
					catch (ArgumentException)
					{
						yield break;
					}
					if (File.Exists(text))
					{
						yield return text;
					}
				}
			}
		}
	}

	private IEnumerable<string> GetSearchPaths(ModuleDef module)
	{
		ModuleDef moduleDef = module;
		if (moduleDef == null)
		{
			moduleDef = nullModule;
		}
		if (moduleSearchPaths.TryGetValue(moduleDef, out var value))
		{
			return value;
		}
		return moduleSearchPaths[moduleDef] = new List<string>(GetModuleSearchPaths(module));
	}

	protected virtual IEnumerable<string> GetModuleSearchPaths(ModuleDef module)
	{
		return GetModulePrivateSearchPaths(module);
	}

	protected IEnumerable<string> GetModulePrivateSearchPaths(ModuleDef module)
	{
		if (module == null)
		{
			return Array2.Empty<string>();
		}
		AssemblyDef assembly = module.Assembly;
		if (assembly == null)
		{
			return Array2.Empty<string>();
		}
		module = assembly.ManifestModule;
		if (module == null)
		{
			return Array2.Empty<string>();
		}
		string text = null;
		try
		{
			string location = module.Location;
			if (location != string.Empty)
			{
				DirectoryInfo parent = Directory.GetParent(location);
				if (parent != null)
				{
					text = parent.FullName;
					string text2 = location + ".config";
					if (File.Exists(text2))
					{
						return GetPrivatePaths(text, text2);
					}
				}
			}
		}
		catch
		{
		}
		if (text != null)
		{
			return new List<string> { text };
		}
		return Array2.Empty<string>();
	}

	private IEnumerable<string> GetPrivatePaths(string baseDir, string configFileName)
	{
		List<string> list = new List<string>();
		try
		{
			string directoryName = Path.GetDirectoryName(Path.GetFullPath(configFileName));
			list.Add(directoryName);
			using FileStream input = new FileStream(configFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(XmlReader.Create(input));
			foreach (object item in xmlDocument.GetElementsByTagName("probing"))
			{
				if (!(item is XmlElement xmlElement))
				{
					continue;
				}
				string attribute = xmlElement.GetAttribute("privatePath");
				if (string.IsNullOrEmpty(attribute))
				{
					continue;
				}
				string[] array = attribute.Split(new char[1] { ';' });
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i].Trim();
					if (text == "")
					{
						continue;
					}
					string fullPath = Path.GetFullPath(Path.Combine(directoryName, text.Replace('\\', Path.DirectorySeparatorChar)));
					if (Directory.Exists(fullPath))
					{
						char directorySeparatorChar = Path.DirectorySeparatorChar;
						if (fullPath.StartsWith(baseDir + directorySeparatorChar))
						{
							list.Add(fullPath);
						}
					}
				}
			}
		}
		catch (ArgumentException)
		{
		}
		catch (IOException)
		{
		}
		catch (XmlException)
		{
		}
		return list;
	}
}
