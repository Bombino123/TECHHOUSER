using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

public abstract class StandAloneSig : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IHasCustomDebugInformation, IContainsGenericParameter
{
	protected uint rid;

	protected CallingConventionSig signature;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.StandAloneSig, rid);

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

	public int HasCustomAttributeTag => 11;

	public CallingConventionSig Signature
	{
		get
		{
			return signature;
		}
		set
		{
			signature = value;
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

	public int HasCustomDebugInformationTag => 11;

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

	public MethodSig MethodSig
	{
		get
		{
			return signature as MethodSig;
		}
		set
		{
			signature = value;
		}
	}

	public LocalSig LocalSig
	{
		get
		{
			return signature as LocalSig;
		}
		set
		{
			signature = value;
		}
	}

	public bool ContainsGenericParameter => TypeHelper.ContainsGenericParameter(this);

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}
}
