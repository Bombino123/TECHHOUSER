namespace SMBLibrary.Server;

internal class SMB1AsyncContext
{
	public ushort UID;

	public ushort TID;

	public uint PID;

	public ushort MID;

	public ushort FileID;

	public SMB1ConnectionState Connection;

	public object IORequest;
}
