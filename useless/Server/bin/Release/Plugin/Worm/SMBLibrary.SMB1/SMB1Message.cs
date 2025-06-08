using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class SMB1Message
{
	public SMB1Header Header;

	public List<SMB1Command> Commands = new List<SMB1Command>();

	public SMB1Message()
	{
		Header = new SMB1Header();
	}

	public SMB1Message(byte[] buffer)
	{
		Header = new SMB1Header(buffer);
		SMB1Command sMB1Command = SMB1Command.ReadCommand(buffer, 32, Header.Command, Header);
		Commands.Add(sMB1Command);
		while (sMB1Command is SMBAndXCommand)
		{
			SMBAndXCommand sMBAndXCommand = (SMBAndXCommand)sMB1Command;
			if (sMBAndXCommand.AndXCommand != CommandName.SMB_COM_NO_ANDX_COMMAND)
			{
				sMB1Command = SMB1Command.ReadCommand(buffer, sMBAndXCommand.AndXOffset, sMBAndXCommand.AndXCommand, Header);
				Commands.Add(sMB1Command);
				continue;
			}
			break;
		}
	}

	public byte[] GetBytes()
	{
		if (Commands.Count == 0)
		{
			throw new ArgumentException("Invalid command sequence");
		}
		for (int i = 0; i < Commands.Count - 1; i++)
		{
			if (!(Commands[i] is SMBAndXCommand))
			{
				throw new ArgumentException("Invalid command sequence");
			}
		}
		SMB1Command sMB1Command = Commands[Commands.Count - 1];
		if (sMB1Command is SMBAndXCommand)
		{
			((SMBAndXCommand)sMB1Command).AndXCommand = CommandName.SMB_COM_NO_ANDX_COMMAND;
		}
		List<byte[]> list = new List<byte[]>();
		int num = 32;
		byte[] bytes;
		for (int j = 0; j < Commands.Count - 1; j++)
		{
			((SMBAndXCommand)Commands[j]).AndXCommand = Commands[j + 1].CommandName;
			bytes = Commands[j].GetBytes(Header.UnicodeFlag);
			ushort andXOffset = (ushort)(num + bytes.Length);
			SMBAndXCommand.WriteAndXOffset(bytes, 0, andXOffset);
			list.Add(bytes);
			num += bytes.Length;
		}
		bytes = sMB1Command.GetBytes(Header.UnicodeFlag);
		list.Add(bytes);
		num += bytes.Length;
		Header.Command = Commands[0].CommandName;
		byte[] array = new byte[num];
		Header.WriteBytes(array, 0);
		int offset = 32;
		foreach (byte[] item in list)
		{
			ByteWriter.WriteBytes(array, ref offset, item);
		}
		return array;
	}

	public static SMB1Message GetSMB1Message(byte[] buffer)
	{
		if (!SMB1Header.IsValidSMB1Header(buffer))
		{
			throw new InvalidDataException("Invalid SMB header signature");
		}
		return new SMB1Message(buffer);
	}
}
