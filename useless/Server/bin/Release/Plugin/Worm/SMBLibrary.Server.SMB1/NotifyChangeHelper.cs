using System.Collections.Generic;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class NotifyChangeHelper
{
	internal static void ProcessNTTransactNotifyChangeRequest(SMB1Header header, uint maxParameterCount, NTTransactNotifyChangeRequest subcommand, ISMBShare share, SMB1ConnectionState state)
	{
		OpenFileObject openFileObject = state.GetSession(header.UID).GetOpenFileObject(subcommand.FID);
		SMB1AsyncContext sMB1AsyncContext = state.CreateAsyncContext(header.UID, header.TID, header.PID, header.MID, subcommand.FID, state);
		lock (sMB1AsyncContext)
		{
			header.Status = share.FileStore.NotifyChange(out sMB1AsyncContext.IORequest, openFileObject.Handle, subcommand.CompletionFilter, subcommand.WatchTree, (int)maxParameterCount, OnNotifyChangeCompleted, sMB1AsyncContext);
			if (header.Status == NTStatus.STATUS_PENDING)
			{
				state.LogToServer(Severity.Verbose, "NotifyChange: Monitoring of '{0}{1}' started. PID: {2}. MID: {3}.", share.Name, openFileObject.Path, sMB1AsyncContext.PID, sMB1AsyncContext.MID);
			}
			else if (header.Status == NTStatus.STATUS_NOT_SUPPORTED)
			{
				header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
			}
		}
	}

	private static void OnNotifyChangeCompleted(NTStatus status, byte[] buffer, object context)
	{
		SMB1AsyncContext sMB1AsyncContext = (SMB1AsyncContext)context;
		lock (sMB1AsyncContext)
		{
			SMB1ConnectionState connection = sMB1AsyncContext.Connection;
			connection.RemoveAsyncContext(sMB1AsyncContext);
			SMB1Session session = connection.GetSession(sMB1AsyncContext.UID);
			if (session == null)
			{
				return;
			}
			OpenFileObject openFileObject = session.GetOpenFileObject(sMB1AsyncContext.FileID);
			if (openFileObject != null)
			{
				connection.LogToServer(Severity.Verbose, "NotifyChange: Monitoring of '{0}{1}' completed. NTStatus: {2}. PID: {3}. MID: {4}.", openFileObject.ShareName, openFileObject.Path, status, sMB1AsyncContext.PID, sMB1AsyncContext.MID);
			}
			SMB1Header sMB1Header = new SMB1Header();
			sMB1Header.Command = CommandName.SMB_COM_NT_TRANSACT;
			sMB1Header.Status = status;
			sMB1Header.Flags = HeaderFlags.CaseInsensitive | HeaderFlags.CanonicalizedPaths | HeaderFlags.Reply;
			sMB1Header.Flags2 = HeaderFlags2.LongNamesAllowed | HeaderFlags2.NTStatusCode | HeaderFlags2.Unicode;
			sMB1Header.UID = sMB1AsyncContext.UID;
			sMB1Header.TID = sMB1AsyncContext.TID;
			sMB1Header.PID = sMB1AsyncContext.PID;
			sMB1Header.MID = sMB1AsyncContext.MID;
			if (status == NTStatus.STATUS_SUCCESS)
			{
				NTTransactNotifyChangeResponse obj = new NTTransactNotifyChangeResponse
				{
					FileNotifyInformationBytes = buffer
				};
				byte[] setup = obj.GetSetup();
				byte[] parameters = obj.GetParameters(isUnicode: false);
				byte[] data = obj.GetData();
				List<SMB1Command> nTTransactResponse = NTTransactHelper.GetNTTransactResponse(setup, parameters, data, sMB1AsyncContext.Connection.MaxBufferSize);
				if (nTTransactResponse.Count == 1)
				{
					SMB1Message sMB1Message = new SMB1Message();
					sMB1Message.Header = sMB1Header;
					sMB1Message.Commands.Add(nTTransactResponse[0]);
					SMBServer.EnqueueMessage(sMB1AsyncContext.Connection, sMB1Message);
				}
				else
				{
					sMB1Header.Status = NTStatus.STATUS_NOTIFY_ENUM_DIR;
					ErrorResponse item = new ErrorResponse(CommandName.SMB_COM_NT_TRANSACT);
					SMB1Message sMB1Message2 = new SMB1Message();
					sMB1Message2.Header = sMB1Header;
					sMB1Message2.Commands.Add(item);
					SMBServer.EnqueueMessage(sMB1AsyncContext.Connection, sMB1Message2);
				}
			}
			else
			{
				ErrorResponse item2 = new ErrorResponse(CommandName.SMB_COM_NT_TRANSACT);
				SMB1Message sMB1Message3 = new SMB1Message();
				sMB1Message3.Header = sMB1Header;
				sMB1Message3.Commands.Add(item2);
				SMBServer.EnqueueMessage(sMB1AsyncContext.Connection, sMB1Message3);
			}
		}
	}
}
