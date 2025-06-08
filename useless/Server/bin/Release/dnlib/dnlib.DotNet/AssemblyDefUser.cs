using System;
using System.Reflection;
using dnlib.Utils;

namespace dnlib.DotNet;

public class AssemblyDefUser : AssemblyDef
{
	public AssemblyDefUser()
		: this(UTF8String.Empty, new Version(0, 0, 0, 0))
	{
	}

	public AssemblyDefUser(UTF8String name)
		: this(name, new Version(0, 0, 0, 0), new PublicKey())
	{
	}

	public AssemblyDefUser(UTF8String name, Version version)
		: this(name, version, new PublicKey())
	{
	}

	public AssemblyDefUser(UTF8String name, Version version, PublicKey publicKey)
		: this(name, version, publicKey, UTF8String.Empty)
	{
	}

	public AssemblyDefUser(UTF8String name, Version version, PublicKey publicKey, UTF8String locale)
	{
		if ((object)name == null)
		{
			throw new ArgumentNullException("name");
		}
		if ((object)locale == null)
		{
			throw new ArgumentNullException("locale");
		}
		modules = new LazyList<ModuleDef>(this);
		base.name = name;
		base.version = version ?? throw new ArgumentNullException("version");
		base.publicKey = publicKey ?? new PublicKey();
		culture = locale;
		attributes = 0;
	}

	public AssemblyDefUser(AssemblyName asmName)
		: this(new AssemblyNameInfo(asmName))
	{
		hashAlgorithm = (AssemblyHashAlgorithm)asmName.HashAlgorithm;
		attributes = (int)asmName.Flags;
	}

	public AssemblyDefUser(IAssembly asmName)
	{
		if (asmName == null)
		{
			throw new ArgumentNullException("asmName");
		}
		modules = new LazyList<ModuleDef>(this);
		name = asmName.Name;
		version = asmName.Version ?? new Version(0, 0, 0, 0);
		publicKey = (asmName.PublicKeyOrToken as PublicKey) ?? new PublicKey();
		culture = asmName.Culture;
		attributes = 0;
		hashAlgorithm = AssemblyHashAlgorithm.SHA1;
	}
}
