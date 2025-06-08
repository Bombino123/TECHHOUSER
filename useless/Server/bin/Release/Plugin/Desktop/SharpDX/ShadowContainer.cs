using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpDX;

internal class ShadowContainer : DisposeBase
{
	private readonly Dictionary<Guid, CppObjectShadow> guidToShadow = new Dictionary<Guid, CppObjectShadow>();

	private static readonly Dictionary<Type, List<Type>> typeToShadowTypes = new Dictionary<Type, List<Type>>();

	private IntPtr guidPtr;

	public IntPtr[] Guids { get; private set; }

	public unsafe void Initialize(ICallbackable callbackable)
	{
		callbackable.Shadow = this;
		Type type = callbackable.GetType();
		List<Type> value;
		lock (typeToShadowTypes)
		{
			if (!typeToShadowTypes.TryGetValue(type, out value))
			{
				Type[] interfaces = SharpDXTypeExtensions.GetTypeInfo(type).GetInterfaces();
				value = new List<Type>();
				value.AddRange(interfaces);
				typeToShadowTypes.Add(type, value);
				Type[] array = interfaces;
				foreach (Type type2 in array)
				{
					if (ShadowAttribute.Get(type2) == null)
					{
						value.Remove(type2);
						continue;
					}
					Type[] interfaces2 = SharpDXTypeExtensions.GetTypeInfo(type2).GetInterfaces();
					foreach (Type item in interfaces2)
					{
						value.Remove(item);
					}
				}
			}
		}
		CppObjectShadow cppObjectShadow = null;
		foreach (Type item2 in value)
		{
			CppObjectShadow cppObjectShadow2 = (CppObjectShadow)Activator.CreateInstance(ShadowAttribute.Get(item2).Type);
			cppObjectShadow2.Initialize(callbackable);
			if (cppObjectShadow == null)
			{
				cppObjectShadow = cppObjectShadow2;
				guidToShadow.Add(ComObjectShadow.IID_IUnknown, cppObjectShadow);
			}
			guidToShadow.Add(Utilities.GetGuidFromType(item2), cppObjectShadow2);
			Type[] array = SharpDXTypeExtensions.GetTypeInfo(item2).GetInterfaces();
			foreach (Type type3 in array)
			{
				if (ShadowAttribute.Get(type3) != null)
				{
					guidToShadow.Add(Utilities.GetGuidFromType(type3), cppObjectShadow2);
				}
			}
		}
		int num = 0;
		foreach (Guid key in guidToShadow.Keys)
		{
			if (key != Utilities.GetGuidFromType(typeof(IInspectable)) && key != Utilities.GetGuidFromType(typeof(IUnknown)))
			{
				num++;
			}
		}
		guidPtr = Marshal.AllocHGlobal(Utilities.SizeOf<Guid>() * num);
		Guids = new IntPtr[num];
		int num2 = 0;
		Guid* ptr = (Guid*)(void*)guidPtr;
		foreach (Guid key2 in guidToShadow.Keys)
		{
			if (!(key2 == Utilities.GetGuidFromType(typeof(IInspectable))) && !(key2 == Utilities.GetGuidFromType(typeof(IUnknown))))
			{
				ptr[num2] = key2;
				Guids[num2] = new IntPtr(ptr + num2);
				num2++;
			}
		}
	}

	internal IntPtr Find(Type type)
	{
		return Find(Utilities.GetGuidFromType(type));
	}

	internal IntPtr Find(Guid guidType)
	{
		return FindShadow(guidType)?.NativePointer ?? IntPtr.Zero;
	}

	internal CppObjectShadow FindShadow(Guid guidType)
	{
		guidToShadow.TryGetValue(guidType, out var value);
		return value;
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		foreach (CppObjectShadow value in guidToShadow.Values)
		{
			value.Dispose();
		}
		guidToShadow.Clear();
		if (guidPtr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(guidPtr);
			guidPtr = IntPtr.Zero;
		}
	}
}
