using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public abstract class TransactionSubcommand
{
	public abstract TransactionSubcommandName SubcommandName { get; }

	public TransactionSubcommand()
	{
	}

	public virtual byte[] GetSetup()
	{
		return new byte[0];
	}

	public virtual byte[] GetParameters()
	{
		return new byte[0];
	}

	public virtual byte[] GetData(bool isUnicode)
	{
		return new byte[0];
	}

	public static TransactionSubcommand GetSubcommandRequest(byte[] setup, byte[] parameters, byte[] data, bool isUnicode)
	{
		if (setup.Length == 4)
		{
			switch ((TransactionSubcommandName)LittleEndianConverter.ToUInt16(setup, 0))
			{
			case TransactionSubcommandName.TRANS_MAILSLOT_WRITE:
				return new TransactionSetNamedPipeStateRequest(setup, parameters);
			case TransactionSubcommandName.TRANS_RAW_READ_NMPIPE:
				return new TransactionRawReadNamedPipeRequest(setup);
			case TransactionSubcommandName.TRANS_QUERY_NMPIPE_STATE:
				return new TransactionQueryNamedPipeStateRequest(setup, parameters);
			case TransactionSubcommandName.TRANS_QUERY_NMPIPE_INFO:
				return new TransactionQueryNamedPipeInfoRequest(setup, parameters);
			case TransactionSubcommandName.TRANS_PEEK_NMPIPE:
				return new TransactionPeekNamedPipeRequest(setup);
			case TransactionSubcommandName.TRANS_TRANSACT_NMPIPE:
				return new TransactionTransactNamedPipeRequest(setup, data);
			case TransactionSubcommandName.TRANS_RAW_WRITE_NMPIPE:
				return new TransactionRawWriteNamedPipeRequest(setup, data);
			case TransactionSubcommandName.TRANS_READ_NMPIPE:
				return new TransactionReadNamedPipeRequest(setup);
			case TransactionSubcommandName.TRANS_WRITE_NMPIPE:
				return new TransactionWriteNamedPipeRequest(setup, data);
			case TransactionSubcommandName.TRANS_WAIT_NMPIPE:
				return new TransactionWaitNamedPipeRequest(setup);
			case TransactionSubcommandName.TRANS_CALL_NMPIPE:
				return new TransactionCallNamedPipeRequest(setup, data);
			}
		}
		throw new InvalidDataException();
	}
}
