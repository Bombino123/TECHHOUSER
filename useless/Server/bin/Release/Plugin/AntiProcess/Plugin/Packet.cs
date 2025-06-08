using Leb128;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			AntiProcess.Block((bool)array[0], (object[])array[1]);
		}
		catch
		{
		}
	}
}
