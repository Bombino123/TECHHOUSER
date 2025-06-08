namespace SMBLibrary.Server;

internal class ProcessStateObject
{
	public ushort SubcommandID;

	public uint MaxParameterCount;

	public uint MaxDataCount;

	public uint Timeout;

	public string Name;

	public byte[] TransactionSetup;

	public byte[] TransactionParameters;

	public byte[] TransactionData;

	public int TransactionParametersReceived;

	public int TransactionDataReceived;
}
