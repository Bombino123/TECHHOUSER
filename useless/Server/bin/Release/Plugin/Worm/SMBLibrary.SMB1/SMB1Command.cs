using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public abstract class SMB1Command
{
	protected byte[] SMBParameters;

	protected byte[] SMBData;

	public abstract CommandName CommandName { get; }

	public SMB1Command()
	{
		SMBParameters = new byte[0];
		SMBData = new byte[0];
	}

	public SMB1Command(byte[] buffer, int offset, bool isUnicode)
	{
		byte b = ByteReader.ReadByte(buffer, ref offset);
		if (CommandName == CommandName.SMB_COM_NT_CREATE_ANDX && b == 42)
		{
			b = 50;
		}
		SMBParameters = ByteReader.ReadBytes(buffer, ref offset, b * 2);
		ushort length = LittleEndianReader.ReadUInt16(buffer, ref offset);
		SMBData = ByteReader.ReadBytes(buffer, ref offset, length);
	}

	public virtual byte[] GetBytes(bool isUnicode)
	{
		if (SMBParameters.Length % 2 > 0)
		{
			throw new Exception("SMB_Parameters Length must be a multiple of 2");
		}
		byte[] array = new byte[1 + SMBParameters.Length + 2 + SMBData.Length];
		byte value = (byte)(SMBParameters.Length / 2);
		if (this is NTCreateAndXResponseExtended)
		{
			value = 42;
		}
		ushort value2 = (ushort)SMBData.Length;
		int offset = 0;
		ByteWriter.WriteByte(array, ref offset, value);
		ByteWriter.WriteBytes(array, ref offset, SMBParameters);
		LittleEndianWriter.WriteUInt16(array, ref offset, value2);
		ByteWriter.WriteBytes(array, ref offset, SMBData);
		return array;
	}

	public static SMB1Command ReadCommand(byte[] buffer, int offset, CommandName commandName, SMB1Header header)
	{
		if ((int)(header.Flags & HeaderFlags.Reply) > 0)
		{
			return ReadCommandResponse(buffer, offset, commandName, header.UnicodeFlag);
		}
		return ReadCommandRequest(buffer, offset, commandName, header.UnicodeFlag);
	}

	public static SMB1Command ReadCommandRequest(byte[] buffer, int offset, CommandName commandName, bool isUnicode)
	{
		switch (commandName)
		{
		case CommandName.SMB_COM_CREATE_DIRECTORY:
			return new CreateDirectoryRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_DELETE_DIRECTORY:
			return new DeleteDirectoryRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_CLOSE:
			return new CloseRequest(buffer, offset);
		case CommandName.SMB_COM_FLUSH:
			return new FlushRequest(buffer, offset);
		case CommandName.SMB_COM_DELETE:
			return new DeleteRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_RENAME:
			return new RenameRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_QUERY_INFORMATION:
			return new QueryInformationRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_SET_INFORMATION:
			return new SetInformationRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_READ:
			return new ReadRequest(buffer, offset);
		case CommandName.SMB_COM_WRITE:
			return new WriteRequest(buffer, offset);
		case CommandName.SMB_COM_CHECK_DIRECTORY:
			return new CheckDirectoryRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_WRITE_RAW:
			return new WriteRawRequest(buffer, offset);
		case CommandName.SMB_COM_SET_INFORMATION2:
			return new SetInformation2Request(buffer, offset);
		case CommandName.SMB_COM_LOCKING_ANDX:
			return new LockingAndXRequest(buffer, offset);
		case CommandName.SMB_COM_TRANSACTION:
			return new TransactionRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_TRANSACTION_SECONDARY:
			return new TransactionSecondaryRequest(buffer, offset);
		case CommandName.SMB_COM_ECHO:
			return new EchoRequest(buffer, offset);
		case CommandName.SMB_COM_OPEN_ANDX:
			return new OpenAndXRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_READ_ANDX:
			return new ReadAndXRequest(buffer, offset);
		case CommandName.SMB_COM_WRITE_ANDX:
			return new WriteAndXRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_TRANSACTION2:
			return new Transaction2Request(buffer, offset, isUnicode);
		case CommandName.SMB_COM_TRANSACTION2_SECONDARY:
			return new Transaction2SecondaryRequest(buffer, offset);
		case CommandName.SMB_COM_FIND_CLOSE2:
			return new FindClose2Request(buffer, offset);
		case CommandName.SMB_COM_TREE_DISCONNECT:
			return new TreeDisconnectRequest(buffer, offset);
		case CommandName.SMB_COM_NEGOTIATE:
			return new NegotiateRequest(buffer, offset);
		case CommandName.SMB_COM_SESSION_SETUP_ANDX:
		{
			byte b2 = ByteReader.ReadByte(buffer, offset);
			if (b2 * 2 == 26)
			{
				return new SessionSetupAndXRequest(buffer, offset, isUnicode);
			}
			if (b2 * 2 == 24)
			{
				return new SessionSetupAndXRequestExtended(buffer, offset, isUnicode);
			}
			throw new InvalidDataException();
		}
		case CommandName.SMB_COM_LOGOFF_ANDX:
			return new LogoffAndXRequest(buffer, offset);
		case CommandName.SMB_COM_TREE_CONNECT_ANDX:
			return new TreeConnectAndXRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_NT_TRANSACT:
			return new NTTransactRequest(buffer, offset);
		case CommandName.SMB_COM_NT_TRANSACT_SECONDARY:
			return new NTTransactSecondaryRequest(buffer, offset);
		case CommandName.SMB_COM_NT_CREATE_ANDX:
			return new NTCreateAndXRequest(buffer, offset, isUnicode);
		case CommandName.SMB_COM_NT_CANCEL:
			return new NTCancelRequest(buffer, offset);
		default:
		{
			byte b = (byte)commandName;
			throw new InvalidDataException("Invalid SMB command 0x" + b.ToString("X2"));
		}
		}
	}

	public static SMB1Command ReadCommandResponse(byte[] buffer, int offset, CommandName commandName, bool isUnicode)
	{
		byte b = ByteReader.ReadByte(buffer, offset);
		switch (commandName)
		{
		case CommandName.SMB_COM_CREATE_DIRECTORY:
			return new CreateDirectoryResponse(buffer, offset);
		case CommandName.SMB_COM_DELETE_DIRECTORY:
			return new DeleteDirectoryResponse(buffer, offset);
		case CommandName.SMB_COM_CLOSE:
			return new CloseResponse(buffer, offset);
		case CommandName.SMB_COM_FLUSH:
			return new FlushResponse(buffer, offset);
		case CommandName.SMB_COM_DELETE:
			return new DeleteResponse(buffer, offset);
		case CommandName.SMB_COM_RENAME:
			return new RenameResponse(buffer, offset);
		case CommandName.SMB_COM_QUERY_INFORMATION:
			if (b * 2 == 20)
			{
				return new QueryInformationResponse(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_SET_INFORMATION:
			return new SetInformationResponse(buffer, offset);
		case CommandName.SMB_COM_READ:
			if (b * 2 == 10)
			{
				return new ReadResponse(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_WRITE:
			if (b * 2 == 2)
			{
				return new WriteResponse(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_CHECK_DIRECTORY:
			return new CheckDirectoryResponse(buffer, offset);
		case CommandName.SMB_COM_WRITE_RAW:
			if (b * 2 == 2)
			{
				return new WriteRawInterimResponse(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_WRITE_COMPLETE:
			if (b * 2 == 2)
			{
				return new WriteRawFinalResponse(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_SET_INFORMATION2:
			return new SetInformation2Response(buffer, offset);
		case CommandName.SMB_COM_LOCKING_ANDX:
			if (b * 2 == 4)
			{
				return new LockingAndXResponse(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_TRANSACTION:
			if (b * 2 == 0)
			{
				return new TransactionInterimResponse(buffer, offset);
			}
			return new TransactionResponse(buffer, offset);
		case CommandName.SMB_COM_ECHO:
			if (b * 2 == 2)
			{
				return new EchoResponse(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_OPEN_ANDX:
			if (b * 2 == 30)
			{
				return new OpenAndXResponse(buffer, offset);
			}
			if (b * 2 == 38)
			{
				return new OpenAndXResponseExtended(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_READ_ANDX:
			if (b * 2 == 24)
			{
				return new ReadAndXResponse(buffer, offset, isUnicode);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_WRITE_ANDX:
			if (b * 2 == 12)
			{
				return new WriteAndXResponse(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_TRANSACTION2:
			if (b * 2 == 0)
			{
				return new Transaction2InterimResponse(buffer, offset);
			}
			return new Transaction2Response(buffer, offset);
		case CommandName.SMB_COM_FIND_CLOSE2:
			return new FindClose2Response(buffer, offset);
		case CommandName.SMB_COM_TREE_DISCONNECT:
			return new TreeDisconnectResponse(buffer, offset);
		case CommandName.SMB_COM_NEGOTIATE:
			if (b * 2 == 34)
			{
				if ((LittleEndianConverter.ToUInt32(buffer, offset + 20) & 0x80000000u) != 0)
				{
					return new NegotiateResponseExtended(buffer, offset);
				}
				return new NegotiateResponse(buffer, offset, isUnicode);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_SESSION_SETUP_ANDX:
			if (b * 2 == 6)
			{
				return new SessionSetupAndXResponse(buffer, offset, isUnicode);
			}
			if (b * 2 == 8)
			{
				return new SessionSetupAndXResponseExtended(buffer, offset, isUnicode);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_LOGOFF_ANDX:
			if (b * 2 == 4)
			{
				return new LogoffAndXResponse(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_TREE_CONNECT_ANDX:
			if (b * 2 == 6)
			{
				return new TreeConnectAndXResponse(buffer, offset, isUnicode);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		case CommandName.SMB_COM_NT_TRANSACT:
			if (b * 2 == 0)
			{
				return new NTTransactInterimResponse(buffer, offset);
			}
			return new NTTransactResponse(buffer, offset);
		case CommandName.SMB_COM_NT_CREATE_ANDX:
			if (b * 2 == 68)
			{
				return new NTCreateAndXResponse(buffer, offset);
			}
			if (b * 2 == 100 || b * 2 == 84)
			{
				return new NTCreateAndXResponseExtended(buffer, offset);
			}
			if (b == 0)
			{
				return new ErrorResponse(commandName);
			}
			throw new InvalidDataException();
		default:
		{
			byte b2 = (byte)commandName;
			throw new InvalidDataException("Invalid SMB command 0x" + b2.ToString("X2"));
		}
		}
	}

	public static implicit operator List<SMB1Command>(SMB1Command command)
	{
		return new List<SMB1Command> { command };
	}
}
