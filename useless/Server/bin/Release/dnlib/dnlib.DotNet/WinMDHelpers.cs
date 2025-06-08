using System;
using System.Collections.Generic;

namespace dnlib.DotNet;

public static class WinMDHelpers
{
	private readonly struct ClassName : IEquatable<ClassName>
	{
		public readonly UTF8String Namespace;

		public readonly UTF8String Name;

		public readonly bool IsValueType;

		public ClassName(UTF8String ns, UTF8String name, bool isValueType = false)
		{
			Namespace = ns;
			Name = name;
			IsValueType = isValueType;
		}

		public ClassName(string ns, string name, bool isValueType = false)
		{
			Namespace = ns;
			Name = name;
			IsValueType = isValueType;
		}

		public static bool operator ==(ClassName a, ClassName b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(ClassName a, ClassName b)
		{
			return !a.Equals(b);
		}

		public bool Equals(ClassName other)
		{
			if (UTF8String.Equals(Namespace, other.Namespace))
			{
				return UTF8String.Equals(Name, other.Name);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ClassName))
			{
				return false;
			}
			return Equals((ClassName)obj);
		}

		public override int GetHashCode()
		{
			return UTF8String.GetHashCode(Namespace) ^ UTF8String.GetHashCode(Name);
		}

		public override string ToString()
		{
			return $"{Namespace}.{Name}";
		}
	}

	private sealed class ProjectedClass
	{
		public readonly ClassName WinMDClass;

		public readonly ClassName ClrClass;

		public readonly ClrAssembly ClrAssembly;

		public readonly ClrAssembly ContractAssembly;

		public ProjectedClass(string mdns, string mdname, string clrns, string clrname, ClrAssembly clrAsm, ClrAssembly contractAsm, bool winMDValueType, bool clrValueType)
		{
			WinMDClass = new ClassName(mdns, mdname, winMDValueType);
			ClrClass = new ClassName(clrns, clrname, clrValueType);
			ClrAssembly = clrAsm;
			ContractAssembly = contractAsm;
		}

		public override string ToString()
		{
			return $"{WinMDClass} <-> {ClrClass}, {CreateAssembly(null, ContractAssembly)}";
		}
	}

	private static readonly ProjectedClass[] ProjectedClasses;

	private static readonly Dictionary<ClassName, ProjectedClass> winMDToCLR;

	private static readonly Version contractAsmVersion;

	private static readonly UTF8String mscorlibName;

	private static readonly UTF8String clrAsmName_Mscorlib;

	private static readonly UTF8String clrAsmName_SystemNumericsVectors;

	private static readonly UTF8String clrAsmName_SystemObjectModel;

	private static readonly UTF8String clrAsmName_SystemRuntime;

	private static readonly UTF8String clrAsmName_SystemRuntimeInteropServicesWindowsRuntime;

	private static readonly UTF8String clrAsmName_SystemRuntimeWindowsRuntime;

	private static readonly UTF8String clrAsmName_SystemRuntimeWindowsRuntimeUIXaml;

	private static readonly byte[] contractPublicKeyToken;

	private static readonly byte[] neutralPublicKey;

	private static readonly UTF8String CloseName;

	private static readonly UTF8String DisposeName;

	private static readonly UTF8String IDisposableNamespace;

	private static readonly UTF8String IDisposableName;

	static WinMDHelpers()
	{
		ProjectedClasses = new ProjectedClass[50]
		{
			new ProjectedClass("Windows.Foundation.Metadata", "AttributeUsageAttribute", "System", "AttributeUsageAttribute", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.Foundation.Metadata", "AttributeTargets", "System", "AttributeTargets", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI", "Color", "Windows.UI", "Color", ClrAssembly.SystemRuntimeWindowsRuntime, ClrAssembly.SystemRuntimeWindowsRuntime, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation", "DateTime", "System", "DateTimeOffset", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation", "EventHandler`1", "System", "EventHandler`1", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.Foundation", "EventRegistrationToken", "System.Runtime.InteropServices.WindowsRuntime", "EventRegistrationToken", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntimeInteropServicesWindowsRuntime, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation", "HResult", "System", "Exception", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: true, clrValueType: false),
			new ProjectedClass("Windows.Foundation", "IReference`1", "System", "Nullable`1", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: true),
			new ProjectedClass("Windows.Foundation", "Point", "Windows.Foundation", "Point", ClrAssembly.SystemRuntimeWindowsRuntime, ClrAssembly.SystemRuntimeWindowsRuntime, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation", "Rect", "Windows.Foundation", "Rect", ClrAssembly.SystemRuntimeWindowsRuntime, ClrAssembly.SystemRuntimeWindowsRuntime, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation", "Size", "Windows.Foundation", "Size", ClrAssembly.SystemRuntimeWindowsRuntime, ClrAssembly.SystemRuntimeWindowsRuntime, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation", "TimeSpan", "System", "TimeSpan", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation", "Uri", "System", "Uri", ClrAssembly.SystemRuntime, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.Foundation", "IClosable", "System", "IDisposable", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.Foundation.Collections", "IIterable`1", "System.Collections.Generic", "IEnumerable`1", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.Foundation.Collections", "IVector`1", "System.Collections.Generic", "IList`1", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.Foundation.Collections", "IVectorView`1", "System.Collections.Generic", "IReadOnlyList`1", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.Foundation.Collections", "IMap`2", "System.Collections.Generic", "IDictionary`2", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.Foundation.Collections", "IMapView`2", "System.Collections.Generic", "IReadOnlyDictionary`2", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.Foundation.Collections", "IKeyValuePair`2", "System.Collections.Generic", "KeyValuePair`2", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml.Input", "ICommand", "System.Windows.Input", "ICommand", ClrAssembly.SystemObjectModel, ClrAssembly.SystemObjectModel, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.UI.Xaml.Interop", "IBindableIterable", "System.Collections", "IEnumerable", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.UI.Xaml.Interop", "IBindableVector", "System.Collections", "IList", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.UI.Xaml.Interop", "INotifyCollectionChanged", "System.Collections.Specialized", "INotifyCollectionChanged", ClrAssembly.SystemObjectModel, ClrAssembly.SystemObjectModel, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.UI.Xaml.Interop", "NotifyCollectionChangedEventHandler", "System.Collections.Specialized", "NotifyCollectionChangedEventHandler", ClrAssembly.SystemObjectModel, ClrAssembly.SystemObjectModel, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.UI.Xaml.Interop", "NotifyCollectionChangedEventArgs", "System.Collections.Specialized", "NotifyCollectionChangedEventArgs", ClrAssembly.SystemObjectModel, ClrAssembly.SystemObjectModel, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.UI.Xaml.Interop", "NotifyCollectionChangedAction", "System.Collections.Specialized", "NotifyCollectionChangedAction", ClrAssembly.SystemObjectModel, ClrAssembly.SystemObjectModel, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml.Data", "INotifyPropertyChanged", "System.ComponentModel", "INotifyPropertyChanged", ClrAssembly.SystemObjectModel, ClrAssembly.SystemObjectModel, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.UI.Xaml.Data", "PropertyChangedEventHandler", "System.ComponentModel", "PropertyChangedEventHandler", ClrAssembly.SystemObjectModel, ClrAssembly.SystemObjectModel, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.UI.Xaml.Data", "PropertyChangedEventArgs", "System.ComponentModel", "PropertyChangedEventArgs", ClrAssembly.SystemObjectModel, ClrAssembly.SystemObjectModel, winMDValueType: false, clrValueType: false),
			new ProjectedClass("Windows.UI.Xaml", "CornerRadius", "Windows.UI.Xaml", "CornerRadius", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml", "Duration", "Windows.UI.Xaml", "Duration", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml", "DurationType", "Windows.UI.Xaml", "DurationType", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml", "GridLength", "Windows.UI.Xaml", "GridLength", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml", "GridUnitType", "Windows.UI.Xaml", "GridUnitType", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml", "Thickness", "Windows.UI.Xaml", "Thickness", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml.Interop", "TypeName", "System", "Type", ClrAssembly.Mscorlib, ClrAssembly.SystemRuntime, winMDValueType: true, clrValueType: false),
			new ProjectedClass("Windows.UI.Xaml.Controls.Primitives", "GeneratorPosition", "Windows.UI.Xaml.Controls.Primitives", "GeneratorPosition", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml.Media", "Matrix", "Windows.UI.Xaml.Media", "Matrix", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml.Media.Animation", "KeyTime", "Windows.UI.Xaml.Media.Animation", "KeyTime", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml.Media.Animation", "RepeatBehavior", "Windows.UI.Xaml.Media.Animation", "RepeatBehavior", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml.Media.Animation", "RepeatBehaviorType", "Windows.UI.Xaml.Media.Animation", "RepeatBehaviorType", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.UI.Xaml.Media.Media3D", "Matrix3D", "Windows.UI.Xaml.Media.Media3D", "Matrix3D", ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation.Numerics", "Vector2", "System.Numerics", "Vector2", ClrAssembly.SystemNumericsVectors, ClrAssembly.SystemNumericsVectors, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation.Numerics", "Vector3", "System.Numerics", "Vector3", ClrAssembly.SystemNumericsVectors, ClrAssembly.SystemNumericsVectors, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation.Numerics", "Vector4", "System.Numerics", "Vector4", ClrAssembly.SystemNumericsVectors, ClrAssembly.SystemNumericsVectors, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation.Numerics", "Matrix3x2", "System.Numerics", "Matrix3x2", ClrAssembly.SystemNumericsVectors, ClrAssembly.SystemNumericsVectors, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation.Numerics", "Matrix4x4", "System.Numerics", "Matrix4x4", ClrAssembly.SystemNumericsVectors, ClrAssembly.SystemNumericsVectors, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation.Numerics", "Plane", "System.Numerics", "Plane", ClrAssembly.SystemNumericsVectors, ClrAssembly.SystemNumericsVectors, winMDValueType: true, clrValueType: true),
			new ProjectedClass("Windows.Foundation.Numerics", "Quaternion", "System.Numerics", "Quaternion", ClrAssembly.SystemNumericsVectors, ClrAssembly.SystemNumericsVectors, winMDValueType: true, clrValueType: true)
		};
		winMDToCLR = new Dictionary<ClassName, ProjectedClass>();
		contractAsmVersion = new Version(4, 0, 0, 0);
		mscorlibName = new UTF8String("mscorlib");
		clrAsmName_Mscorlib = new UTF8String("mscorlib");
		clrAsmName_SystemNumericsVectors = new UTF8String("System.Numerics.Vectors");
		clrAsmName_SystemObjectModel = new UTF8String("System.ObjectModel");
		clrAsmName_SystemRuntime = new UTF8String("System.Runtime");
		clrAsmName_SystemRuntimeInteropServicesWindowsRuntime = new UTF8String("System.Runtime.InteropServices.WindowsRuntime");
		clrAsmName_SystemRuntimeWindowsRuntime = new UTF8String("System.Runtime.WindowsRuntime");
		clrAsmName_SystemRuntimeWindowsRuntimeUIXaml = new UTF8String("System.Runtime.WindowsRuntime.UI.Xaml");
		contractPublicKeyToken = new byte[8] { 176, 63, 95, 127, 17, 213, 10, 58 };
		neutralPublicKey = new byte[8] { 183, 122, 92, 86, 25, 52, 224, 137 };
		CloseName = new UTF8String("Close");
		DisposeName = new UTF8String("Dispose");
		IDisposableNamespace = new UTF8String("System");
		IDisposableName = new UTF8String("IDisposable");
		ProjectedClass[] projectedClasses = ProjectedClasses;
		foreach (ProjectedClass projectedClass in projectedClasses)
		{
			winMDToCLR.Add(projectedClass.WinMDClass, projectedClass);
		}
	}

	private static AssemblyRef ToCLR(ModuleDef module, ref UTF8String ns, ref UTF8String name)
	{
		if (!winMDToCLR.TryGetValue(new ClassName(ns, name), out var value))
		{
			return null;
		}
		ns = value.ClrClass.Namespace;
		name = value.ClrClass.Name;
		return CreateAssembly(module, value.ContractAssembly);
	}

	private static AssemblyRef CreateAssembly(ModuleDef module, ClrAssembly clrAsm)
	{
		AssemblyRef assemblyRef = module?.CorLibTypes.AssemblyRef;
		AssemblyRefUser assemblyRefUser = new AssemblyRefUser(GetName(clrAsm), contractAsmVersion, new PublicKeyToken(GetPublicKeyToken(clrAsm)), UTF8String.Empty);
		if (assemblyRef != null && assemblyRef.Name == mscorlibName && IsValidMscorlibVersion(assemblyRef.Version))
		{
			assemblyRefUser.Version = assemblyRef.Version;
		}
		if (module is ModuleDefMD moduleDefMD)
		{
			Version version = null;
			foreach (AssemblyRef assemblyRef2 in moduleDefMD.GetAssemblyRefs())
			{
				if (!assemblyRef2.IsContentTypeWindowsRuntime && !(assemblyRef2.Name != assemblyRefUser.Name) && !(assemblyRef2.Culture != assemblyRefUser.Culture) && PublicKeyBase.TokenEquals(assemblyRef2.PublicKeyOrToken, assemblyRefUser.PublicKeyOrToken) && IsValidMscorlibVersion(assemblyRef2.Version) && ((object)version == null || assemblyRef2.Version > version))
				{
					version = assemblyRef2.Version;
				}
			}
			if ((object)version != null)
			{
				assemblyRefUser.Version = version;
			}
		}
		return assemblyRefUser;
	}

	private static bool IsValidMscorlibVersion(Version version)
	{
		if ((object)version != null)
		{
			return (uint)version.Major <= 5u;
		}
		return false;
	}

	private static UTF8String GetName(ClrAssembly clrAsm)
	{
		return clrAsm switch
		{
			ClrAssembly.Mscorlib => clrAsmName_Mscorlib, 
			ClrAssembly.SystemNumericsVectors => clrAsmName_SystemNumericsVectors, 
			ClrAssembly.SystemObjectModel => clrAsmName_SystemObjectModel, 
			ClrAssembly.SystemRuntime => clrAsmName_SystemRuntime, 
			ClrAssembly.SystemRuntimeInteropServicesWindowsRuntime => clrAsmName_SystemRuntimeInteropServicesWindowsRuntime, 
			ClrAssembly.SystemRuntimeWindowsRuntime => clrAsmName_SystemRuntimeWindowsRuntime, 
			ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml => clrAsmName_SystemRuntimeWindowsRuntimeUIXaml, 
			_ => throw new InvalidOperationException(), 
		};
	}

	private static byte[] GetPublicKeyToken(ClrAssembly clrAsm)
	{
		return clrAsm switch
		{
			ClrAssembly.Mscorlib => neutralPublicKey, 
			ClrAssembly.SystemNumericsVectors => contractPublicKeyToken, 
			ClrAssembly.SystemObjectModel => contractPublicKeyToken, 
			ClrAssembly.SystemRuntime => contractPublicKeyToken, 
			ClrAssembly.SystemRuntimeInteropServicesWindowsRuntime => contractPublicKeyToken, 
			ClrAssembly.SystemRuntimeWindowsRuntime => neutralPublicKey, 
			ClrAssembly.SystemRuntimeWindowsRuntimeUIXaml => neutralPublicKey, 
			_ => throw new InvalidOperationException(), 
		};
	}

	public static TypeRef ToCLR(ModuleDef module, TypeDef td)
	{
		bool isClrValueType;
		return ToCLR(module, td, out isClrValueType);
	}

	public static TypeRef ToCLR(ModuleDef module, TypeDef td, out bool isClrValueType)
	{
		isClrValueType = false;
		if (td == null || !td.IsWindowsRuntime)
		{
			return null;
		}
		IAssembly definitionAssembly = td.DefinitionAssembly;
		if (definitionAssembly == null || !definitionAssembly.IsContentTypeWindowsRuntime)
		{
			return null;
		}
		if (!winMDToCLR.TryGetValue(new ClassName(td.Namespace, td.Name), out var value))
		{
			return null;
		}
		isClrValueType = value.ClrClass.IsValueType;
		return new TypeRefUser(module, value.ClrClass.Namespace, value.ClrClass.Name, CreateAssembly(module, value.ContractAssembly));
	}

	public static TypeRef ToCLR(ModuleDef module, TypeRef tr)
	{
		bool isClrValueType;
		return ToCLR(module, tr, out isClrValueType);
	}

	public static TypeRef ToCLR(ModuleDef module, TypeRef tr, out bool isClrValueType)
	{
		isClrValueType = false;
		if (tr == null)
		{
			return null;
		}
		IAssembly definitionAssembly = tr.DefinitionAssembly;
		if (definitionAssembly == null || !definitionAssembly.IsContentTypeWindowsRuntime)
		{
			return null;
		}
		if (tr.DeclaringType != null)
		{
			return null;
		}
		if (!winMDToCLR.TryGetValue(new ClassName(tr.Namespace, tr.Name), out var value))
		{
			return null;
		}
		isClrValueType = value.ClrClass.IsValueType;
		return new TypeRefUser(module, value.ClrClass.Namespace, value.ClrClass.Name, CreateAssembly(module, value.ContractAssembly));
	}

	public static ExportedType ToCLR(ModuleDef module, ExportedType et)
	{
		if (et == null)
		{
			return null;
		}
		IAssembly definitionAssembly = et.DefinitionAssembly;
		if (definitionAssembly == null || !definitionAssembly.IsContentTypeWindowsRuntime)
		{
			return null;
		}
		if (et.DeclaringType != null)
		{
			return null;
		}
		if (!winMDToCLR.TryGetValue(new ClassName(et.TypeNamespace, et.TypeName), out var value))
		{
			return null;
		}
		return new ExportedTypeUser(module, 0u, value.ClrClass.Namespace, value.ClrClass.Name, et.Attributes, CreateAssembly(module, value.ContractAssembly));
	}

	public static TypeSig ToCLR(ModuleDef module, TypeSig ts)
	{
		if (ts == null)
		{
			return null;
		}
		ElementType elementType = ts.ElementType;
		if (elementType != ElementType.Class && elementType != ElementType.ValueType)
		{
			return null;
		}
		ITypeDefOrRef typeDefOrRef = ((ClassOrValueTypeSig)ts).TypeDefOrRef;
		TypeRef typeRef;
		bool isClrValueType;
		if (typeDefOrRef is TypeDef td)
		{
			typeRef = ToCLR(module, td, out isClrValueType);
			if (typeRef == null)
			{
				return null;
			}
		}
		else
		{
			if (!(typeDefOrRef is TypeRef tr))
			{
				return null;
			}
			typeRef = ToCLR(module, tr, out isClrValueType);
			if (typeRef == null)
			{
				return null;
			}
		}
		if (!isClrValueType)
		{
			return new ClassSig(typeRef);
		}
		return new ValueTypeSig(typeRef);
	}

	public static MemberRef ToCLR(ModuleDef module, MemberRef mr)
	{
		if (mr == null)
		{
			return null;
		}
		if (mr.Name != CloseName)
		{
			return null;
		}
		MethodSig methodSig = mr.MethodSig;
		if (methodSig == null)
		{
			return null;
		}
		IMemberRefParent @class = mr.Class;
		IMemberRefParent class2;
		if (@class is TypeRef tr)
		{
			TypeRef typeRef = ToCLR(module, tr);
			if (typeRef == null || !IsIDisposable(typeRef))
			{
				return null;
			}
			class2 = typeRef;
		}
		else
		{
			if (!(@class is TypeSpec typeSpec))
			{
				return null;
			}
			if (!(typeSpec.TypeSig is GenericInstSig genericInstSig) || !(genericInstSig.GenericType is ClassSig))
			{
				return null;
			}
			TypeRef typeRef2 = genericInstSig.GenericType.TypeRef;
			if (typeRef2 == null)
			{
				return null;
			}
			bool isClrValueType;
			TypeRef typeRef3 = ToCLR(module, typeRef2, out isClrValueType);
			if (typeRef3 == null || !IsIDisposable(typeRef3))
			{
				return null;
			}
			class2 = new TypeSpecUser(new GenericInstSig(isClrValueType ? ((ClassOrValueTypeSig)new ValueTypeSig(typeRef3)) : ((ClassOrValueTypeSig)new ClassSig(typeRef3)), genericInstSig.GenericArguments));
		}
		return new MemberRefUser(mr.Module, DisposeName, methodSig, class2);
	}

	private static bool IsIDisposable(TypeRef tr)
	{
		if (tr.Name == IDisposableName)
		{
			return tr.Namespace == IDisposableNamespace;
		}
		return false;
	}

	public static MemberRef ToCLR(ModuleDef module, MethodDef md)
	{
		if (md == null)
		{
			return null;
		}
		if (md.Name != CloseName)
		{
			return null;
		}
		TypeDef declaringType = md.DeclaringType;
		if (declaringType == null)
		{
			return null;
		}
		TypeRef typeRef = ToCLR(module, declaringType);
		if (typeRef == null || !IsIDisposable(typeRef))
		{
			return null;
		}
		return new MemberRefUser(md.Module, DisposeName, md.MethodSig, typeRef);
	}
}
