using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class AssemblyRefUser : AssemblyRef
{
	public static AssemblyRefUser CreateMscorlibReferenceCLR10()
	{
		return new AssemblyRefUser("mscorlib", new Version(1, 0, 3300, 0), new PublicKeyToken("b77a5c561934e089"));
	}

	public static AssemblyRefUser CreateMscorlibReferenceCLR11()
	{
		return new AssemblyRefUser("mscorlib", new Version(1, 0, 5000, 0), new PublicKeyToken("b77a5c561934e089"));
	}

	public static AssemblyRefUser CreateMscorlibReferenceCLR20()
	{
		return new AssemblyRefUser("mscorlib", new Version(2, 0, 0, 0), new PublicKeyToken("b77a5c561934e089"));
	}

	public static AssemblyRefUser CreateMscorlibReferenceCLR40()
	{
		return new AssemblyRefUser("mscorlib", new Version(4, 0, 0, 0), new PublicKeyToken("b77a5c561934e089"));
	}

	public AssemblyRefUser()
		: this(UTF8String.Empty)
	{
	}

	public AssemblyRefUser(UTF8String name)
		: this(name, new Version(0, 0, 0, 0))
	{
	}

	public AssemblyRefUser(UTF8String name, Version version)
		: this(name, version, new PublicKey())
	{
	}

	public AssemblyRefUser(UTF8String name, Version version, PublicKeyBase publicKey)
		: this(name, version, publicKey, UTF8String.Empty)
	{
	}

	public AssemblyRefUser(UTF8String name, Version version, PublicKeyBase publicKey, UTF8String locale)
	{
		if ((object)name == null)
		{
			throw new ArgumentNullException("name");
		}
		if ((object)locale == null)
		{
			throw new ArgumentNullException("locale");
		}
		base.name = name;
		base.version = version ?? throw new ArgumentNullException("version");
		publicKeyOrToken = publicKey;
		culture = locale;
		attributes = ((publicKey is PublicKey) ? 1 : 0);
	}

	public AssemblyRefUser(AssemblyName asmName)
		: this(new AssemblyNameInfo(asmName))
	{
		attributes = (int)asmName.Flags;
	}

	public AssemblyRefUser(IAssembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("asmName");
		}
		version = assembly.Version ?? new Version(0, 0, 0, 0);
		publicKeyOrToken = assembly.PublicKeyOrToken;
		name = (UTF8String.IsNullOrEmpty(assembly.Name) ? UTF8String.Empty : assembly.Name);
		culture = assembly.Culture;
		attributes = ((publicKeyOrToken is PublicKey) ? 1 : 0) | (int)assembly.ContentType;
	}
}
