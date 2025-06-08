using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

[DebuggerDisplay("{Action} Count={SecurityAttributes.Count}")]
public abstract class DeclSecurity : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IHasCustomDebugInformation
{
	protected uint rid;

	protected SecurityAction action;

	protected IList<SecurityAttribute> securityAttributes;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.DeclSecurity, rid);

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

	public int HasCustomAttributeTag => 8;

	public SecurityAction Action
	{
		get
		{
			return action;
		}
		set
		{
			action = value;
		}
	}

	public IList<SecurityAttribute> SecurityAttributes
	{
		get
		{
			if (securityAttributes == null)
			{
				InitializeSecurityAttributes();
			}
			return securityAttributes;
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

	public int HasCustomDebugInformationTag => 8;

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

	public bool HasSecurityAttributes => SecurityAttributes.Count > 0;

	protected virtual void InitializeSecurityAttributes()
	{
		Interlocked.CompareExchange(ref securityAttributes, new List<SecurityAttribute>(), null);
	}

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	public abstract byte[] GetBlob();

	public string GetNet1xXmlString()
	{
		return GetNet1xXmlStringInternal(SecurityAttributes);
	}

	internal static string GetNet1xXmlStringInternal(IList<SecurityAttribute> secAttrs)
	{
		if (secAttrs == null || secAttrs.Count != 1)
		{
			return null;
		}
		SecurityAttribute securityAttribute = secAttrs[0];
		if (securityAttribute == null || securityAttribute.TypeFullName != "System.Security.Permissions.PermissionSetAttribute")
		{
			return null;
		}
		if (securityAttribute.NamedArguments.Count != 1)
		{
			return null;
		}
		CANamedArgument cANamedArgument = securityAttribute.NamedArguments[0];
		if (cANamedArgument == null || !cANamedArgument.IsProperty || cANamedArgument.Name != "XML")
		{
			return null;
		}
		if (cANamedArgument.ArgumentType.GetElementType() != ElementType.String)
		{
			return null;
		}
		CAArgument argument = cANamedArgument.Argument;
		if (argument.Type.GetElementType() != ElementType.String)
		{
			return null;
		}
		if (argument.Value is UTF8String uTF8String)
		{
			return uTF8String;
		}
		if (argument.Value is string result)
		{
			return result;
		}
		return null;
	}
}
