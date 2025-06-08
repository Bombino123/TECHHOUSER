using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace dnlib.DotNet.Emit;

internal static class MethodTableToTypeConverter
{
	private const string METHOD_NAME = "m";

	private static readonly MethodInfo setMethodBodyMethodInfo;

	private static readonly FieldInfo localSignatureFieldInfo;

	private static readonly FieldInfo sigDoneFieldInfo;

	private static readonly FieldInfo currSigFieldInfo;

	private static readonly FieldInfo signatureFieldInfo;

	private static readonly FieldInfo ptrFieldInfo;

	private static readonly Dictionary<IntPtr, Type> addrToType;

	private static ModuleBuilder moduleBuilder;

	private static int numNewTypes;

	private static object lockObj;

	static MethodTableToTypeConverter()
	{
		setMethodBodyMethodInfo = typeof(MethodBuilder).GetMethod("SetMethodBody", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		localSignatureFieldInfo = typeof(ILGenerator).GetField("m_localSignature", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		sigDoneFieldInfo = typeof(SignatureHelper).GetField("m_sigDone", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		currSigFieldInfo = typeof(SignatureHelper).GetField("m_currSig", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		signatureFieldInfo = typeof(SignatureHelper).GetField("m_signature", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		ptrFieldInfo = typeof(RuntimeTypeHandle).GetField("m_ptr", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		addrToType = new Dictionary<IntPtr, Type>();
		lockObj = new object();
		if ((object)ptrFieldInfo == null)
		{
			moduleBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("DynAsm"), AssemblyBuilderAccess.Run).DefineDynamicModule("DynMod");
		}
		if ((object)localSignatureFieldInfo == null)
		{
			localSignatureFieldInfo = Type.GetType("System.Reflection.Emit.RuntimeILGenerator")?.GetField("m_localSignature", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}
	}

	public static Type Convert(IntPtr address)
	{
		lock (lockObj)
		{
			if (addrToType.TryGetValue(address, out var value))
			{
				return value;
			}
			value = GetTypeNET20(address) ?? GetTypeUsingTypeBuilder(address);
			addrToType[address] = value;
			return value;
		}
	}

	private static Type GetTypeUsingTypeBuilder(IntPtr address)
	{
		if ((object)moduleBuilder == null)
		{
			return null;
		}
		TypeBuilder typeBuilder = moduleBuilder.DefineType(GetNextTypeName());
		MethodBuilder mb = typeBuilder.DefineMethod("m", System.Reflection.MethodAttributes.Static, typeof(void), Array2.Empty<Type>());
		try
		{
			if ((object)setMethodBodyMethodInfo != null)
			{
				return GetTypeNET45(typeBuilder, mb, address);
			}
			return GetTypeNET40(typeBuilder, mb, address);
		}
		catch
		{
			moduleBuilder = null;
			return null;
		}
	}

	private static Type GetTypeNET45(TypeBuilder tb, MethodBuilder mb, IntPtr address)
	{
		byte[] array = new byte[1] { 42 };
		int num = 8;
		byte[] localSignature = GetLocalSignature(address);
		setMethodBodyMethodInfo.Invoke(mb, new object[5] { array, num, localSignature, null, null });
		return tb.CreateType().GetMethod("m", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetMethodBody()
			.LocalVariables[0].LocalType;
	}

	private static Type GetTypeNET40(TypeBuilder tb, MethodBuilder mb, IntPtr address)
	{
		ILGenerator iLGenerator = mb.GetILGenerator();
		iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);
		iLGenerator.DeclareLocal(typeof(int));
		byte[] localSignature = GetLocalSignature(address);
		SignatureHelper obj = (SignatureHelper)localSignatureFieldInfo.GetValue(iLGenerator);
		sigDoneFieldInfo.SetValue(obj, true);
		currSigFieldInfo.SetValue(obj, localSignature.Length);
		signatureFieldInfo.SetValue(obj, localSignature);
		return tb.CreateType().GetMethod("m", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetMethodBody()
			.LocalVariables[0].LocalType;
	}

	private static Type GetTypeNET20(IntPtr address)
	{
		if ((object)ptrFieldInfo == null)
		{
			return null;
		}
		object obj = default(RuntimeTypeHandle);
		ptrFieldInfo.SetValue(obj, address);
		return Type.GetTypeFromHandle((RuntimeTypeHandle)obj);
	}

	private static string GetNextTypeName()
	{
		return $"Type{numNewTypes++}";
	}

	private static byte[] GetLocalSignature(IntPtr mtAddr)
	{
		ulong num = (ulong)mtAddr.ToInt64();
		if (IntPtr.Size != 4)
		{
			byte[] obj = new byte[11]
			{
				7, 1, 33, 0, 0, 0, 0, 0, 0, 0,
				0
			};
			obj[3] = (byte)num;
			obj[4] = (byte)(num >> 8);
			obj[5] = (byte)(num >> 16);
			obj[6] = (byte)(num >> 24);
			obj[7] = (byte)(num >> 32);
			obj[8] = (byte)(num >> 40);
			obj[9] = (byte)(num >> 48);
			obj[10] = (byte)(num >> 56);
			return obj;
		}
		byte[] obj2 = new byte[7] { 7, 1, 33, 0, 0, 0, 0 };
		obj2[3] = (byte)num;
		obj2[4] = (byte)(num >> 8);
		obj2[5] = (byte)(num >> 16);
		obj2[6] = (byte)(num >> 24);
		return obj2;
	}
}
