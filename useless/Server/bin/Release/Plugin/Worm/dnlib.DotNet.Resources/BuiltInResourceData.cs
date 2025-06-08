using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using dnlib.IO;

namespace dnlib.DotNet.Resources;

[ComVisible(true)]
public sealed class BuiltInResourceData : IResourceData, IFileSection
{
	private readonly ResourceTypeCode code;

	private readonly object data;

	public object Data => data;

	public ResourceTypeCode Code => code;

	public FileOffset StartOffset { get; set; }

	public FileOffset EndOffset { get; set; }

	public BuiltInResourceData(ResourceTypeCode code, object data)
	{
		this.code = code;
		this.data = data;
	}

	public void WriteData(ResourceBinaryWriter writer, IFormatter formatter)
	{
		switch (code)
		{
		case ResourceTypeCode.String:
			writer.Write((string)data);
			break;
		case ResourceTypeCode.Boolean:
			writer.Write((bool)data);
			break;
		case ResourceTypeCode.Char:
			writer.Write((ushort)(char)data);
			break;
		case ResourceTypeCode.Byte:
			writer.Write((byte)data);
			break;
		case ResourceTypeCode.SByte:
			writer.Write((sbyte)data);
			break;
		case ResourceTypeCode.Int16:
			writer.Write((short)data);
			break;
		case ResourceTypeCode.UInt16:
			writer.Write((ushort)data);
			break;
		case ResourceTypeCode.Int32:
			writer.Write((int)data);
			break;
		case ResourceTypeCode.UInt32:
			writer.Write((uint)data);
			break;
		case ResourceTypeCode.Int64:
			writer.Write((long)data);
			break;
		case ResourceTypeCode.UInt64:
			writer.Write((ulong)data);
			break;
		case ResourceTypeCode.Single:
			writer.Write((float)data);
			break;
		case ResourceTypeCode.Double:
			writer.Write((double)data);
			break;
		case ResourceTypeCode.Decimal:
			writer.Write((decimal)data);
			break;
		case ResourceTypeCode.DateTime:
		{
			DateTime dateTime = (DateTime)data;
			if (writer.FormatVersion == 1)
			{
				writer.Write(dateTime.Ticks);
			}
			else
			{
				writer.Write(dateTime.ToBinary());
			}
			break;
		}
		case ResourceTypeCode.TimeSpan:
			writer.Write(((TimeSpan)data).Ticks);
			break;
		case ResourceTypeCode.ByteArray:
		case ResourceTypeCode.Stream:
		{
			if (writer.FormatVersion == 1)
			{
				throw new NotSupportedException($"{code} is not supported in format version 1 resources");
			}
			byte[] array = (byte[])data;
			writer.Write(array.Length);
			writer.Write(array);
			break;
		}
		default:
			throw new InvalidOperationException("Unknown resource type code");
		case ResourceTypeCode.Null:
			break;
		}
	}

	public override string ToString()
	{
		switch (code)
		{
		case ResourceTypeCode.Null:
			return "null";
		case ResourceTypeCode.String:
		case ResourceTypeCode.Boolean:
		case ResourceTypeCode.Char:
		case ResourceTypeCode.Byte:
		case ResourceTypeCode.SByte:
		case ResourceTypeCode.Int16:
		case ResourceTypeCode.UInt16:
		case ResourceTypeCode.Int32:
		case ResourceTypeCode.UInt32:
		case ResourceTypeCode.Int64:
		case ResourceTypeCode.UInt64:
		case ResourceTypeCode.Single:
		case ResourceTypeCode.Double:
		case ResourceTypeCode.Decimal:
		case ResourceTypeCode.DateTime:
		case ResourceTypeCode.TimeSpan:
			return $"{code}: '{data}'";
		case ResourceTypeCode.ByteArray:
		case ResourceTypeCode.Stream:
			if (data is byte[] array)
			{
				return $"{code}: Length: {array.Length}";
			}
			return $"{code}: '{data}'";
		default:
			return $"{code}: '{data}'";
		}
	}
}
