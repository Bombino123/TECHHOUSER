using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public class SocketUtils
{
	public static void SetKeepAlive(Socket socket, TimeSpan timeout)
	{
		SetKeepAlive(socket, enable: true, timeout, TimeSpan.FromSeconds(1.0));
	}

	public static void SetKeepAlive(Socket socket, bool enable, TimeSpan timeout, TimeSpan interval)
	{
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, optionValue: true);
		byte[] array = new byte[12];
		LittleEndianWriter.WriteUInt32(array, 0, Convert.ToUInt32(enable));
		LittleEndianWriter.WriteUInt32(array, 4, (uint)timeout.TotalMilliseconds);
		LittleEndianWriter.WriteUInt32(array, 8, (uint)interval.TotalMilliseconds);
		socket.IOControl(IOControlCode.KeepAliveValues, array, null);
	}

	public static void ReleaseSocket(Socket socket)
	{
		if (socket == null)
		{
			return;
		}
		if (socket.Connected)
		{
			try
			{
				socket.Shutdown(SocketShutdown.Both);
				socket.Disconnect(reuseSocket: false);
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			catch (SocketException)
			{
			}
		}
		socket.Close();
	}
}
