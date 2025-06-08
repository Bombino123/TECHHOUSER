using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.SchemaObjectModel;
using System.IO;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal static class MetadataAssemblyHelper
{
	private const string EcmaPublicKey = "b77a5c561934e089";

	private const string MicrosoftPublicKey = "b03f5f7f11d50a3a";

	private static readonly byte[] _ecmaPublicKeyToken = ScalarType.ConvertToByteArray("b77a5c561934e089");

	private static readonly byte[] _msPublicKeyToken = ScalarType.ConvertToByteArray("b03f5f7f11d50a3a");

	private static readonly Memoizer<Assembly, bool> _filterAssemblyCacheByAssembly = new Memoizer<Assembly, bool>(ComputeShouldFilterAssembly, EqualityComparer<Assembly>.Default);

	internal static Assembly SafeLoadReferencedAssembly(AssemblyName assemblyName)
	{
		Assembly result = null;
		try
		{
			result = Assembly.Load(assemblyName);
		}
		catch (FileNotFoundException)
		{
		}
		catch (FileLoadException)
		{
		}
		return result;
	}

	private static bool ComputeShouldFilterAssembly(Assembly assembly)
	{
		return ShouldFilterAssembly(new AssemblyName(assembly.FullName));
	}

	internal static bool ShouldFilterAssembly(Assembly assembly)
	{
		return _filterAssemblyCacheByAssembly.Evaluate(assembly);
	}

	private static bool ShouldFilterAssembly(AssemblyName assemblyName)
	{
		if (!ArePublicKeyTokensEqual(assemblyName.GetPublicKeyToken(), _ecmaPublicKeyToken))
		{
			return ArePublicKeyTokensEqual(assemblyName.GetPublicKeyToken(), _msPublicKeyToken);
		}
		return true;
	}

	private static bool ArePublicKeyTokensEqual(byte[] left, byte[] right)
	{
		if (left.Length != right.Length)
		{
			return false;
		}
		for (int i = 0; i < left.Length; i++)
		{
			if (left[i] != right[i])
			{
				return false;
			}
		}
		return true;
	}

	internal static IEnumerable<Assembly> GetNonSystemReferencedAssemblies(Assembly assembly)
	{
		AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
		foreach (AssemblyName assemblyName in referencedAssemblies)
		{
			if (!ShouldFilterAssembly(assemblyName))
			{
				Assembly assembly2 = SafeLoadReferencedAssembly(assemblyName);
				if (assembly2 != null)
				{
					yield return assembly2;
				}
			}
		}
	}
}
