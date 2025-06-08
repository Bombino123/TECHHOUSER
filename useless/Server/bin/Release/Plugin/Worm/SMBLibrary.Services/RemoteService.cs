using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.Services;

[ComVisible(true)]
public abstract class RemoteService
{
	public abstract Guid InterfaceGuid { get; }

	public abstract string PipeName { get; }

	public abstract byte[] GetResponseBytes(ushort opNum, byte[] requestBytes);
}
