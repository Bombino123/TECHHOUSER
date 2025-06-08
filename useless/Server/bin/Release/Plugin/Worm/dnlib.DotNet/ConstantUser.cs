using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class ConstantUser : Constant
{
	public ConstantUser()
	{
	}

	public ConstantUser(object value)
	{
		type = GetElementType(value);
		base.value = value;
	}

	public ConstantUser(object value, ElementType type)
	{
		base.type = type;
		base.value = value;
	}

	private static ElementType GetElementType(object value)
	{
		if (value == null)
		{
			return ElementType.Class;
		}
		return System.Type.GetTypeCode(value.GetType()) switch
		{
			TypeCode.Boolean => ElementType.Boolean, 
			TypeCode.Char => ElementType.Char, 
			TypeCode.SByte => ElementType.I1, 
			TypeCode.Byte => ElementType.U1, 
			TypeCode.Int16 => ElementType.I2, 
			TypeCode.UInt16 => ElementType.U2, 
			TypeCode.Int32 => ElementType.I4, 
			TypeCode.UInt32 => ElementType.U4, 
			TypeCode.Int64 => ElementType.I8, 
			TypeCode.UInt64 => ElementType.U8, 
			TypeCode.Single => ElementType.R4, 
			TypeCode.Double => ElementType.R8, 
			TypeCode.String => ElementType.String, 
			_ => ElementType.Void, 
		};
	}
}
