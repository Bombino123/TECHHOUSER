using System;
using System.Reflection;
using System.Text;

namespace dnlib.DotNet;

internal static class ReflectionExtensions
{
	public static void GetTypeNamespaceAndName_TypeDefOrRef(this Type type, out string @namespace, out string name)
	{
		name = Unescape(type.Name) ?? string.Empty;
		if (!type.IsNested)
		{
			@namespace = type.Namespace ?? string.Empty;
			return;
		}
		string text = Unescape(type.DeclaringType.FullName);
		string text2 = Unescape(type.FullName);
		if (text.Length + 1 + name.Length == text2.Length)
		{
			@namespace = string.Empty;
		}
		else
		{
			@namespace = text2.Substring(text.Length + 1, text2.Length - text.Length - 1 - name.Length - 1);
		}
	}

	public static bool IsSZArray(this Type self)
	{
		if ((object)self == null || !self.IsArray)
		{
			return false;
		}
		PropertyInfo property = self.GetType().GetProperty("IsSzArray", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if ((object)property != null)
		{
			return (bool)property.GetValue(self, Array2.Empty<object>());
		}
		return (self.Name ?? string.Empty).EndsWith("[]");
	}

	public static ElementType GetElementType2(this Type a)
	{
		if ((object)a == null)
		{
			return ElementType.End;
		}
		if (a.IsArray)
		{
			if (!a.IsSZArray())
			{
				return ElementType.Array;
			}
			return ElementType.SZArray;
		}
		if (a.IsByRef)
		{
			return ElementType.ByRef;
		}
		if (a.IsPointer)
		{
			return ElementType.Ptr;
		}
		if (a.IsGenericParameter)
		{
			if ((object)a.DeclaringMethod != null)
			{
				return ElementType.MVar;
			}
			return ElementType.Var;
		}
		if (a.IsGenericType && !a.IsGenericTypeDefinition)
		{
			return ElementType.GenericInst;
		}
		if ((object)a == typeof(void))
		{
			return ElementType.Void;
		}
		if ((object)a == typeof(bool))
		{
			return ElementType.Boolean;
		}
		if ((object)a == typeof(char))
		{
			return ElementType.Char;
		}
		if ((object)a == typeof(sbyte))
		{
			return ElementType.I1;
		}
		if ((object)a == typeof(byte))
		{
			return ElementType.U1;
		}
		if ((object)a == typeof(short))
		{
			return ElementType.I2;
		}
		if ((object)a == typeof(ushort))
		{
			return ElementType.U2;
		}
		if ((object)a == typeof(int))
		{
			return ElementType.I4;
		}
		if ((object)a == typeof(uint))
		{
			return ElementType.U4;
		}
		if ((object)a == typeof(long))
		{
			return ElementType.I8;
		}
		if ((object)a == typeof(ulong))
		{
			return ElementType.U8;
		}
		if ((object)a == typeof(float))
		{
			return ElementType.R4;
		}
		if ((object)a == typeof(double))
		{
			return ElementType.R8;
		}
		if ((object)a == typeof(string))
		{
			return ElementType.String;
		}
		if ((object)a == typeof(TypedReference))
		{
			return ElementType.TypedByRef;
		}
		if ((object)a == typeof(IntPtr))
		{
			return ElementType.I;
		}
		if ((object)a == typeof(UIntPtr))
		{
			return ElementType.U;
		}
		if ((object)a == typeof(object))
		{
			return ElementType.Object;
		}
		if (!a.IsValueType)
		{
			return ElementType.Class;
		}
		return ElementType.ValueType;
	}

	public static bool IsGenericButNotGenericTypeDefinition(this Type type)
	{
		if ((object)type != null && !type.IsGenericTypeDefinition)
		{
			return type.IsGenericType;
		}
		return false;
	}

	public static bool IsGenericButNotGenericMethodDefinition(this MethodBase mb)
	{
		if ((object)mb != null && !mb.IsGenericMethodDefinition)
		{
			return mb.IsGenericMethod;
		}
		return false;
	}

	internal static bool MustTreatTypeAsGenericInstType(this Type declaringType, Type t)
	{
		if ((object)declaringType != null && declaringType.IsGenericTypeDefinition)
		{
			return (object)t == declaringType;
		}
		return false;
	}

	public static bool IsTypeDef(this Type type)
	{
		if ((object)type != null && !type.HasElementType)
		{
			if (type.IsGenericType)
			{
				return type.IsGenericTypeDefinition;
			}
			return true;
		}
		return false;
	}

	internal static string Unescape(string name)
	{
		if (string.IsNullOrEmpty(name) || name.IndexOf('\\') < 0)
		{
			return name;
		}
		StringBuilder stringBuilder = new StringBuilder(name.Length);
		for (int i = 0; i < name.Length; i++)
		{
			if (name[i] == '\\' && i < name.Length - 1 && IsReservedTypeNameChar(name[i + 1]))
			{
				stringBuilder.Append(name[++i]);
			}
			else
			{
				stringBuilder.Append(name[i]);
			}
		}
		return stringBuilder.ToString();
	}

	private static bool IsReservedTypeNameChar(char c)
	{
		switch (c)
		{
		case '&':
		case '*':
		case '+':
		case ',':
		case '[':
		case '\\':
		case ']':
			return true;
		default:
			return false;
		}
	}
}
