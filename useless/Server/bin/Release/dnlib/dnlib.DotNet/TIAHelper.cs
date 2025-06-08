using System;

namespace dnlib.DotNet;

internal static class TIAHelper
{
	private readonly struct Info : IEquatable<Info>
	{
		public readonly UTF8String Scope;

		public readonly UTF8String Identifier;

		public Info(UTF8String scope, UTF8String identifier)
		{
			Scope = scope;
			Identifier = identifier;
		}

		public bool Equals(Info other)
		{
			if (stricmp(Scope, other.Scope))
			{
				return UTF8String.Equals(Identifier, other.Identifier);
			}
			return false;
		}

		private static bool stricmp(UTF8String a, UTF8String b)
		{
			byte[] array = a?.Data;
			byte[] array2 = b?.Data;
			if (array == array2)
			{
				return true;
			}
			if (array == null || array2 == null)
			{
				return false;
			}
			if (array.Length != array2.Length)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				byte b2 = array[i];
				byte b3 = array2[i];
				if (65 <= b2 && b2 <= 90)
				{
					b2 = (byte)(b2 - 65 + 97);
				}
				if (65 <= b3 && b3 <= 90)
				{
					b3 = (byte)(b3 - 65 + 97);
				}
				if (b2 != b3)
				{
					return false;
				}
			}
			return true;
		}
	}

	private static readonly UTF8String InvokeString = new UTF8String("Invoke");

	private static Info? GetInfo(TypeDef td)
	{
		if (td == null)
		{
			return null;
		}
		if (td.IsWindowsRuntime)
		{
			return null;
		}
		UTF8String scope = null;
		UTF8String uTF8String = null;
		CustomAttribute customAttribute = td.CustomAttributes.Find("System.Runtime.InteropServices.TypeIdentifierAttribute");
		if (customAttribute != null)
		{
			if (customAttribute.ConstructorArguments.Count >= 2)
			{
				if (customAttribute.ConstructorArguments[0].Type.GetElementType() != ElementType.String)
				{
					return null;
				}
				if (customAttribute.ConstructorArguments[1].Type.GetElementType() != ElementType.String)
				{
					return null;
				}
				scope = (customAttribute.ConstructorArguments[0].Value as UTF8String) ?? ((UTF8String)(customAttribute.ConstructorArguments[0].Value as string));
				uTF8String = (customAttribute.ConstructorArguments[1].Value as UTF8String) ?? ((UTF8String)(customAttribute.ConstructorArguments[1].Value as string));
			}
		}
		else
		{
			AssemblyDef assemblyDef = td.Module?.Assembly;
			if (assemblyDef == null)
			{
				return null;
			}
			if (!assemblyDef.CustomAttributes.IsDefined("System.Runtime.InteropServices.ImportedFromTypeLibAttribute") && !assemblyDef.CustomAttributes.IsDefined("System.Runtime.InteropServices.PrimaryInteropAssemblyAttribute"))
			{
				return null;
			}
		}
		if (UTF8String.IsNull(uTF8String))
		{
			CustomAttribute customAttribute2;
			if (td.IsInterface && td.IsImport)
			{
				customAttribute2 = td.CustomAttributes.Find("System.Runtime.InteropServices.GuidAttribute");
			}
			else
			{
				AssemblyDef assemblyDef2 = td.Module?.Assembly;
				if (assemblyDef2 == null)
				{
					return null;
				}
				customAttribute2 = assemblyDef2.CustomAttributes.Find("System.Runtime.InteropServices.GuidAttribute");
			}
			if (customAttribute2 == null)
			{
				return null;
			}
			if (customAttribute2.ConstructorArguments.Count < 1)
			{
				return null;
			}
			if (customAttribute2.ConstructorArguments[0].Type.GetElementType() != ElementType.String)
			{
				return null;
			}
			scope = (customAttribute2.ConstructorArguments[0].Value as UTF8String) ?? ((UTF8String)(customAttribute2.ConstructorArguments[0].Value as string));
			UTF8String @namespace = td.Namespace;
			UTF8String name = td.Name;
			uTF8String = (UTF8String.IsNullOrEmpty(@namespace) ? name : ((!UTF8String.IsNullOrEmpty(name)) ? new UTF8String(Concat(@namespace.Data, 46, name.Data)) : new UTF8String(Concat(@namespace.Data, 46, Array2.Empty<byte>()))));
		}
		return new Info(scope, uTF8String);
	}

	private static byte[] Concat(byte[] a, byte b, byte[] c)
	{
		byte[] array = new byte[a.Length + 1 + c.Length];
		for (int i = 0; i < a.Length; i++)
		{
			array[i] = a[i];
		}
		array[a.Length] = b;
		int num = 0;
		int num2 = a.Length + 1;
		while (num < c.Length)
		{
			array[num2] = c[num];
			num++;
			num2++;
		}
		return array;
	}

	internal static bool IsTypeDefEquivalent(TypeDef td)
	{
		if (GetInfo(td).HasValue)
		{
			return CheckEquivalent(td);
		}
		return false;
	}

	private static bool CheckEquivalent(TypeDef td)
	{
		int num = 0;
		while (td != null && num < 1000)
		{
			if (num != 0 && !GetInfo(td).HasValue)
			{
				return false;
			}
			if (!((!td.IsInterface) ? (td.IsValueType || td.IsDelegate) : (td.IsImport || td.CustomAttributes.IsDefined("System.Runtime.InteropServices.ComEventInterfaceAttribute"))))
			{
				return false;
			}
			if (td.GenericParameters.Count > 0)
			{
				return false;
			}
			TypeDef declaringType = td.DeclaringType;
			if (declaringType == null)
			{
				return td.IsPublic;
			}
			if (!td.IsNestedPublic)
			{
				return false;
			}
			td = declaringType;
			num++;
		}
		return false;
	}

	public static bool Equivalent(TypeDef td1, TypeDef td2)
	{
		Info? info = GetInfo(td1);
		if (!info.HasValue)
		{
			return false;
		}
		Info? info2 = GetInfo(td2);
		if (!info2.HasValue)
		{
			return false;
		}
		if (!CheckEquivalent(td1) || !CheckEquivalent(td2))
		{
			return false;
		}
		if (!info.Value.Equals(info2.Value))
		{
			return false;
		}
		for (int i = 0; i < 1000; i++)
		{
			if (td1.IsInterface)
			{
				if (!td2.IsInterface)
				{
					return false;
				}
			}
			else
			{
				ITypeDefOrRef baseType = td1.BaseType;
				ITypeDefOrRef baseType2 = td2.BaseType;
				if (baseType == null || baseType2 == null)
				{
					return false;
				}
				if (td1.IsDelegate)
				{
					if (!td2.IsDelegate)
					{
						return false;
					}
					if (!DelegateEquals(td1, td2))
					{
						return false;
					}
				}
				else
				{
					if (!td1.IsValueType)
					{
						return false;
					}
					if (td1.IsEnum != td2.IsEnum)
					{
						return false;
					}
					if (!td2.IsValueType)
					{
						return false;
					}
					if (!ValueTypeEquals(td1, td2, td1.IsEnum))
					{
						return false;
					}
				}
			}
			td1 = td1.DeclaringType;
			td2 = td2.DeclaringType;
			if (td1 == null && td2 == null)
			{
				break;
			}
			if (td1 == null || td2 == null)
			{
				return false;
			}
		}
		return true;
	}

	private static bool DelegateEquals(TypeDef td1, TypeDef td2)
	{
		MethodDef methodDef = td1.FindMethod(InvokeString);
		MethodDef methodDef2 = td2.FindMethod(InvokeString);
		if (methodDef == null || methodDef2 == null)
		{
			return false;
		}
		return true;
	}

	private static bool ValueTypeEquals(TypeDef td1, TypeDef td2, bool isEnum)
	{
		if (td1.Methods.Count != 0 || td2.Methods.Count != 0)
		{
			return false;
		}
		return true;
	}
}
