using System;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class TransactionSubcommandHelper
{
	internal static TransactionTransactNamedPipeResponse GetSubcommandResponse(SMB1Header header, uint maxDataCount, TransactionTransactNamedPipeRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		OpenFileObject openFileObject = state.GetSession(header.UID).GetOpenFileObject(subcommand.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "TransactNamedPipe failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, subcommand.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return null;
		}
		header.Status = share.FileStore.DeviceIOControl(openFileObject.Handle, 1163287u, subcommand.WriteData, out var output, (int)maxDataCount);
		if (header.Status != 0 && header.Status != NTStatus.STATUS_BUFFER_OVERFLOW)
		{
			state.LogToServer(Severity.Verbose, "TransactNamedPipe failed. NTStatus: {0}.", header.Status);
			return null;
		}
		return new TransactionTransactNamedPipeResponse
		{
			ReadData = output
		};
	}

	internal static void ProcessSubcommand(SMB1Header header, uint timeout, string name, TransactionWaitNamedPipeRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		if (!name.StartsWith("\\PIPE\\", StringComparison.OrdinalIgnoreCase))
		{
			state.LogToServer(Severity.Verbose, "TransactWaitNamedPipe failed. Invalid pipe name: {0}.", name);
			header.Status = NTStatus.STATUS_INVALID_SMB;
		}
		string text = name.Substring(6);
		byte[] bytes = new PipeWaitRequest
		{
			Timeout = timeout,
			TimeSpecified = true,
			Name = text
		}.GetBytes();
		header.Status = share.FileStore.DeviceIOControl(null, 1114136u, bytes, out var _, 0);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "TransactWaitNamedPipe failed. Pipe name: {0}. NTStatus: {1}.", text, header.Status);
		}
		else
		{
			state.LogToServer(Severity.Verbose, "TransactWaitNamedPipe succeeded. Pipe name: {0}.", text);
		}
	}
}
