using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

public abstract class ModuleRef : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IMemberRefParent, IFullName, IHasCustomDebugInformation, IResolutionScope, IModule, IScope, IOwnerModule
{
	protected uint rid;

	protected ModuleDef module;

	protected UTF8String name;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.ModuleRef, rid);

	public uint Rid
	{
		get
		{
			return rid;
		}
		set
		{
			rid = value;
		}
	}

	public int HasCustomAttributeTag => 12;

	public int MemberRefParentTag => 2;

	public int ResolutionScopeTag => 1;

	public ScopeType ScopeType => ScopeType.ModuleRef;

	public string ScopeName => FullName;

	public UTF8String Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public CustomAttributeCollection CustomAttributes
	{
		get
		{
			if (customAttributes == null)
			{
				InitializeCustomAttributes();
			}
			return customAttributes;
		}
	}

	public bool HasCustomAttributes => CustomAttributes.Count > 0;

	public int HasCustomDebugInformationTag => 12;

	public bool HasCustomDebugInfos => CustomDebugInfos.Count > 0;

	public IList<PdbCustomDebugInfo> CustomDebugInfos
	{
		get
		{
			if (customDebugInfos == null)
			{
				InitializeCustomDebugInfos();
			}
			return customDebugInfos;
		}
	}

	public ModuleDef Module => module;

	public ModuleDef DefinitionModule
	{
		get
		{
			if (module == null)
			{
				return null;
			}
			UTF8String a = name;
			if (UTF8String.CaseInsensitiveEquals(a, module.Name))
			{
				return module;
			}
			return DefinitionAssembly?.FindModule(a);
		}
	}

	public AssemblyDef DefinitionAssembly => module?.Assembly;

	public string FullName => UTF8String.ToSystemStringOrEmpty(name);

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	public override string ToString()
	{
		return FullName;
	}
}
