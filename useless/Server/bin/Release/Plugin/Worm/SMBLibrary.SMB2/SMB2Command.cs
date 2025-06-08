using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public abstract class SMB2Command
{
	public SMB2Header Header;

	public SMB2CommandName CommandName => Header.Command;

	public ulong MessageID => Header.MessageID;

	public int Length => 64 + CommandLength;

	public abstract int CommandLength { get; }

	public SMB2Command(SMB2CommandName commandName)
	{
		Header = new SMB2Header(commandName);
	}

	public SMB2Command(byte[] buffer, int offset)
	{
		Header = new SMB2Header(buffer, offset);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		Header.WriteBytes(buffer, offset);
		WriteCommandBytes(buffer, offset + 64);
	}

	public abstract void WriteCommandBytes(byte[] buffer, int offset);

	public byte[] GetBytes()
	{
		byte[] array = new byte[Length];
		WriteBytes(array, 0);
		return array;
	}

	public static SMB2Command ReadRequest(byte[] buffer, int offset)
	{
		SMB2CommandName sMB2CommandName = (SMB2CommandName)LittleEndianConverter.ToUInt16(buffer, offset + 12);
		switch (sMB2CommandName)
		{
		case SMB2CommandName.Negotiate:
			return new NegotiateRequest(buffer, offset);
		case SMB2CommandName.SessionSetup:
			return new SessionSetupRequest(buffer, offset);
		case SMB2CommandName.Logoff:
			return new LogoffRequest(buffer, offset);
		case SMB2CommandName.TreeConnect:
			return new TreeConnectRequest(buffer, offset);
		case SMB2CommandName.TreeDisconnect:
			return new TreeDisconnectRequest(buffer, offset);
		case SMB2CommandName.Create:
			return new CreateRequest(buffer, offset);
		case SMB2CommandName.Close:
			return new CloseRequest(buffer, offset);
		case SMB2CommandName.Flush:
			return new FlushRequest(buffer, offset);
		case SMB2CommandName.Read:
			return new ReadRequest(buffer, offset);
		case SMB2CommandName.Write:
			return new WriteRequest(buffer, offset);
		case SMB2CommandName.Lock:
			return new LockRequest(buffer, offset);
		case SMB2CommandName.IOCtl:
			return new IOCtlRequest(buffer, offset);
		case SMB2CommandName.Cancel:
			return new CancelRequest(buffer, offset);
		case SMB2CommandName.Echo:
			return new EchoRequest(buffer, offset);
		case SMB2CommandName.QueryDirectory:
			return new QueryDirectoryRequest(buffer, offset);
		case SMB2CommandName.ChangeNotify:
			return new ChangeNotifyRequest(buffer, offset);
		case SMB2CommandName.QueryInfo:
			return new QueryInfoRequest(buffer, offset);
		case SMB2CommandName.SetInfo:
			return new SetInfoRequest(buffer, offset);
		default:
		{
			ushort num = (ushort)sMB2CommandName;
			throw new InvalidDataException("Invalid SMB2 command 0x" + num.ToString("X4"));
		}
		}
	}

	public static List<SMB2Command> ReadRequestChain(byte[] buffer, int offset)
	{
		List<SMB2Command> list = new List<SMB2Command>();
		SMB2Command sMB2Command;
		do
		{
			sMB2Command = ReadRequest(buffer, offset);
			list.Add(sMB2Command);
			offset += (int)sMB2Command.Header.NextCommand;
		}
		while (sMB2Command.Header.NextCommand != 0);
		return list;
	}

	public static byte[] GetCommandChainBytes(List<SMB2Command> commands)
	{
		return GetCommandChainBytes(commands, null, SMB2Dialect.SMB2xx);
	}

	public static byte[] GetCommandChainBytes(List<SMB2Command> commands, byte[] signingKey, SMB2Dialect dialect)
	{
		int num = 0;
		for (int i = 0; i < commands.Count; i++)
		{
			int length = commands[i].Length;
			if (i < commands.Count - 1)
			{
				int num2 = (int)Math.Ceiling((double)length / 8.0) * 8;
				num += num2;
			}
			else
			{
				num += length;
			}
		}
		byte[] array = new byte[num];
		int num3 = 0;
		for (int j = 0; j < commands.Count; j++)
		{
			SMB2Command sMB2Command = commands[j];
			int length2 = sMB2Command.Length;
			int num4;
			if (j < commands.Count - 1)
			{
				num4 = (int)Math.Ceiling((double)length2 / 8.0) * 8;
				sMB2Command.Header.NextCommand = (uint)num4;
			}
			else
			{
				num4 = length2;
			}
			sMB2Command.WriteBytes(array, num3);
			if (sMB2Command.Header.IsSigned && signingKey != null)
			{
				byte[] bytes = SMB2Cryptography.CalculateSignature(signingKey, dialect, array, num3, num4);
				ByteWriter.WriteBytes(array, num3 + 48, bytes, 16);
			}
			num3 += num4;
		}
		return array;
	}

	public static SMB2Command ReadResponse(byte[] buffer, int offset)
	{
		SMB2CommandName sMB2CommandName = (SMB2CommandName)LittleEndianConverter.ToUInt16(buffer, offset + 12);
		ushort num = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		switch (sMB2CommandName)
		{
		case SMB2CommandName.Negotiate:
			return num switch
			{
				65 => new NegotiateResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.SessionSetup:
			if (num == 9)
			{
				NTStatus nTStatus2 = (NTStatus)LittleEndianConverter.ToUInt32(buffer, offset + 8);
				if (nTStatus2 == NTStatus.STATUS_SUCCESS || nTStatus2 == NTStatus.STATUS_MORE_PROCESSING_REQUIRED)
				{
					return new SessionSetupResponse(buffer, offset);
				}
				return new ErrorResponse(buffer, offset);
			}
			throw new InvalidDataException();
		case SMB2CommandName.Logoff:
			return num switch
			{
				4 => new LogoffResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.TreeConnect:
			return num switch
			{
				16 => new TreeConnectResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.TreeDisconnect:
			return num switch
			{
				4 => new TreeDisconnectResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.Create:
			return num switch
			{
				89 => new CreateResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.Close:
			return num switch
			{
				60 => new CloseResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.Flush:
			return num switch
			{
				4 => new FlushResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.Read:
			return num switch
			{
				17 => new ReadResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.Write:
			return num switch
			{
				17 => new WriteResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.Lock:
			return num switch
			{
				4 => new LockResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.IOCtl:
			return num switch
			{
				49 => new IOCtlResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.Cancel:
			if (num == 9)
			{
				return new ErrorResponse(buffer, offset);
			}
			throw new InvalidDataException();
		case SMB2CommandName.Echo:
			return num switch
			{
				4 => new EchoResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		case SMB2CommandName.QueryDirectory:
			if (num == 9)
			{
				if (LittleEndianConverter.ToUInt32(buffer, offset + 8) == 0)
				{
					return new QueryDirectoryResponse(buffer, offset);
				}
				return new ErrorResponse(buffer, offset);
			}
			throw new InvalidDataException();
		case SMB2CommandName.ChangeNotify:
			if (num == 9)
			{
				NTStatus nTStatus3 = (NTStatus)LittleEndianConverter.ToUInt32(buffer, offset + 8);
				if (nTStatus3 == NTStatus.STATUS_SUCCESS || nTStatus3 == NTStatus.STATUS_NOTIFY_CLEANUP || nTStatus3 == NTStatus.STATUS_NOTIFY_ENUM_DIR)
				{
					return new ChangeNotifyResponse(buffer, offset);
				}
				return new ErrorResponse(buffer, offset);
			}
			throw new InvalidDataException();
		case SMB2CommandName.QueryInfo:
			if (num == 9)
			{
				NTStatus nTStatus = (NTStatus)LittleEndianConverter.ToUInt32(buffer, offset + 8);
				if (nTStatus == NTStatus.STATUS_SUCCESS || nTStatus == NTStatus.STATUS_BUFFER_OVERFLOW)
				{
					return new QueryInfoResponse(buffer, offset);
				}
				return new ErrorResponse(buffer, offset);
			}
			throw new InvalidDataException();
		case SMB2CommandName.SetInfo:
			return num switch
			{
				2 => new SetInfoResponse(buffer, offset), 
				9 => new ErrorResponse(buffer, offset), 
				_ => throw new InvalidDataException(), 
			};
		default:
		{
			ushort num2 = (ushort)sMB2CommandName;
			throw new InvalidDataException("Invalid SMB2 command 0x" + num2.ToString("X4"));
		}
		}
	}

	public static List<SMB2Command> ReadResponseChain(byte[] buffer, int offset)
	{
		List<SMB2Command> list = new List<SMB2Command>();
		SMB2Command sMB2Command;
		do
		{
			sMB2Command = ReadResponse(buffer, offset);
			list.Add(sMB2Command);
			offset += (int)sMB2Command.Header.NextCommand;
		}
		while (sMB2Command.Header.NextCommand != 0);
		return list;
	}
}
