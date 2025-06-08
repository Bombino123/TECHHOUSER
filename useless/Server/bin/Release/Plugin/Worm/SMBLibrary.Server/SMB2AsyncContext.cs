using SMBLibrary.SMB2;

namespace SMBLibrary.Server;

internal class SMB2AsyncContext
{
	public ulong AsyncID;

	public FileID FileID;

	public SMB2ConnectionState Connection;

	public ulong SessionID;

	public uint TreeID;

	public object IORequest;
}
