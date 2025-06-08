using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.SchemaObjectModel;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;
using System.Security;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class AspProxy
{
	private const string BUILD_MANAGER_TYPE_NAME = "System.Web.Compilation.BuildManager";

	private const string AspNetAssemblyName = "System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	private static readonly byte[] _systemWebPublicKeyToken = ScalarType.ConvertToByteArray("b03f5f7f11d50a3a");

	private Assembly _webAssembly;

	private bool _triedLoadingWebAssembly;

	internal bool IsAspNetEnvironment()
	{
		if (!TryInitializeWebAssembly())
		{
			return false;
		}
		try
		{
			return InternalMapWebPath("~") != null;
		}
		catch (SecurityException)
		{
			return false;
		}
		catch (Exception e)
		{
			if (e.IsCatchableExceptionType())
			{
				return false;
			}
			throw;
		}
	}

	public bool TryInitializeWebAssembly()
	{
		if (_webAssembly != null)
		{
			return true;
		}
		if (_triedLoadingWebAssembly)
		{
			return false;
		}
		_triedLoadingWebAssembly = true;
		if (!IsSystemWebLoaded())
		{
			return false;
		}
		try
		{
			_webAssembly = Assembly.Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
			return _webAssembly != null;
		}
		catch (Exception e)
		{
			if (!e.IsCatchableExceptionType())
			{
				throw;
			}
		}
		return false;
	}

	public static bool IsSystemWebLoaded()
	{
		try
		{
			return AppDomain.CurrentDomain.GetAssemblies().Any((Assembly a) => a.GetName().Name == "System.Web" && a.GetName().GetPublicKeyToken() != null && a.GetName().GetPublicKeyToken().SequenceEqual(_systemWebPublicKeyToken));
		}
		catch
		{
		}
		return false;
	}

	private void InitializeWebAssembly()
	{
		if (!TryInitializeWebAssembly())
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext);
		}
	}

	internal string MapWebPath(string path)
	{
		path = InternalMapWebPath(path);
		if (path == null)
		{
			throw new InvalidOperationException(Strings.InvalidUseOfWebPath("~"));
		}
		return path;
	}

	internal string InternalMapWebPath(string path)
	{
		InitializeWebAssembly();
		try
		{
			return (string)_webAssembly.GetType("System.Web.Hosting.HostingEnvironment", throwOnError: true).GetDeclaredMethod("MapPath", typeof(string)).Invoke(null, new object[1] { path });
		}
		catch (TargetException innerException)
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, innerException);
		}
		catch (ArgumentException innerException2)
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, innerException2);
		}
		catch (TargetInvocationException innerException3)
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, innerException3);
		}
		catch (TargetParameterCountException innerException4)
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, innerException4);
		}
		catch (MethodAccessException innerException5)
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, innerException5);
		}
		catch (MemberAccessException innerException6)
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, innerException6);
		}
		catch (TypeLoadException innerException7)
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, innerException7);
		}
	}

	internal bool HasBuildManagerType()
	{
		Type buildManager;
		return TryGetBuildManagerType(out buildManager);
	}

	private bool TryGetBuildManagerType(out Type buildManager)
	{
		InitializeWebAssembly();
		buildManager = _webAssembly.GetType("System.Web.Compilation.BuildManager", throwOnError: false);
		return buildManager != null;
	}

	internal IEnumerable<Assembly> GetBuildManagerReferencedAssemblies()
	{
		MethodInfo referencedAssembliesMethod = GetReferencedAssembliesMethod();
		if (referencedAssembliesMethod == null)
		{
			return new List<Assembly>();
		}
		ICollection collection = null;
		try
		{
			collection = (ICollection)referencedAssembliesMethod.Invoke(null, null);
			if (collection == null)
			{
				return new List<Assembly>();
			}
			return collection.Cast<Assembly>();
		}
		catch (TargetException innerException)
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, innerException);
		}
		catch (TargetInvocationException innerException2)
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, innerException2);
		}
		catch (MethodAccessException innerException3)
		{
			throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, innerException3);
		}
	}

	internal MethodInfo GetReferencedAssembliesMethod()
	{
		if (!TryGetBuildManagerType(out var buildManager))
		{
			throw new InvalidOperationException(Strings.UnableToFindReflectedType("System.Web.Compilation.BuildManager", "System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
		}
		return buildManager.GetDeclaredMethod("GetReferencedAssemblies");
	}
}
