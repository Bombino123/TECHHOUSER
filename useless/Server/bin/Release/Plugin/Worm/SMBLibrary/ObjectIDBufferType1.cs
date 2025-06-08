using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class ObjectIDBufferType1
{
	public const int Length = 64;

	public Guid ObjectId;

	public Guid BirthVolumeId;

	public Guid BirthObjectId;

	public Guid DomainId;

	public ObjectIDBufferType1()
	{
	}

	public ObjectIDBufferType1(byte[] buffer)
	{
		ObjectId = LittleEndianConverter.ToGuid(buffer, 0);
		BirthVolumeId = LittleEndianConverter.ToGuid(buffer, 16);
		BirthObjectId = LittleEndianConverter.ToGuid(buffer, 32);
		DomainId = LittleEndianConverter.ToGuid(buffer, 48);
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[64];
		LittleEndianWriter.WriteGuid(array, 0, ObjectId);
		LittleEndianWriter.WriteGuid(array, 16, BirthVolumeId);
		LittleEndianWriter.WriteGuid(array, 32, BirthObjectId);
		LittleEndianWriter.WriteGuid(array, 48, DomainId);
		return array;
	}
}
