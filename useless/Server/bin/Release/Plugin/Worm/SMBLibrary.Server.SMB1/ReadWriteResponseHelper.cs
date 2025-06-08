using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class ReadWriteResponseHelper
{
	internal static SMB1Command GetReadResponse(SMB1Header header, ReadRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		OpenFileObject openFileObject = session.GetOpenFileObject(request.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Read failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, request.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return new ErrorResponse(request.CommandName);
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasReadAccess(session.SecurityContext, openFileObject.Path))
		{
			state.LogToServer(Severity.Verbose, "Read from '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = share.FileStore.ReadFile(out var data, openFileObject.Handle, request.ReadOffsetInBytes, request.CountOfBytesToRead);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Read from '{0}{1}' failed. NTStatus: {2}. (FID: {3})", share.Name, openFileObject.Path, header.Status, request.FID);
			return new ErrorResponse(request.CommandName);
		}
		return new ReadResponse
		{
			Bytes = data,
			CountOfBytesReturned = (ushort)data.Length
		};
	}

	internal static SMB1Command GetReadResponse(SMB1Header header, ReadAndXRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		OpenFileObject openFileObject = session.GetOpenFileObject(request.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Read failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, request.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return new ErrorResponse(request.CommandName);
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasReadAccess(session.SecurityContext, openFileObject.Path))
		{
			state.LogToServer(Severity.Verbose, "Read from '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		uint maxCount = request.MaxCount;
		if (share is FileSystemShare && state.LargeRead)
		{
			maxCount = request.MaxCountLarge;
		}
		header.Status = share.FileStore.ReadFile(out var data, openFileObject.Handle, (long)request.Offset, (int)maxCount);
		if (header.Status == NTStatus.STATUS_END_OF_FILE)
		{
			data = new byte[0];
			header.Status = NTStatus.STATUS_SUCCESS;
		}
		else if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Read from '{0}{1}' failed. NTStatus: {2}. (FID: {3})", share.Name, openFileObject.Path, header.Status, request.FID);
			return new ErrorResponse(request.CommandName);
		}
		ReadAndXResponse readAndXResponse = new ReadAndXResponse();
		if (share is FileSystemShare)
		{
			readAndXResponse.Available = ushort.MaxValue;
		}
		readAndXResponse.Data = data;
		return readAndXResponse;
	}

	internal static SMB1Command GetWriteResponse(SMB1Header header, WriteRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		OpenFileObject openFileObject = session.GetOpenFileObject(request.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Write failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, request.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return new ErrorResponse(request.CommandName);
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, openFileObject.Path))
		{
			state.LogToServer(Severity.Verbose, "Write to '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = share.FileStore.WriteFile(out var numberOfBytesWritten, openFileObject.Handle, request.WriteOffsetInBytes, request.Data);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Write to '{0}{1}' failed. NTStatus: {2}. (FID: {3})", share.Name, openFileObject.Path, header.Status, request.FID);
			return new ErrorResponse(request.CommandName);
		}
		return new WriteResponse
		{
			CountOfBytesWritten = (ushort)numberOfBytesWritten
		};
	}

	internal static SMB1Command GetWriteResponse(SMB1Header header, WriteAndXRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		OpenFileObject openFileObject = session.GetOpenFileObject(request.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Write failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, request.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return new ErrorResponse(request.CommandName);
		}
		if (share is FileSystemShare && !((FileSystemShare)share).HasWriteAccess(session.SecurityContext, openFileObject.Path))
		{
			state.LogToServer(Severity.Verbose, "Write to '{0}{1}' failed. User '{2}' was denied access.", share.Name, openFileObject.Path, session.UserName);
			header.Status = NTStatus.STATUS_ACCESS_DENIED;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = share.FileStore.WriteFile(out var numberOfBytesWritten, openFileObject.Handle, (long)request.Offset, request.Data);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Write to '{0}{1}' failed. NTStatus: {2}. (FID: {3})", share.Name, openFileObject.Path, header.Status, request.FID);
			return new ErrorResponse(request.CommandName);
		}
		WriteAndXResponse writeAndXResponse = new WriteAndXResponse();
		writeAndXResponse.Count = (uint)numberOfBytesWritten;
		if (share is FileSystemShare)
		{
			writeAndXResponse.Available = ushort.MaxValue;
		}
		return writeAndXResponse;
	}

	internal static SMB1Command GetFlushResponse(SMB1Header header, FlushRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		if (request.FID == ushort.MaxValue)
		{
			return new FlushResponse();
		}
		OpenFileObject openFileObject = session.GetOpenFileObject(request.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Flush failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, request.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return new ErrorResponse(request.CommandName);
		}
		header.Status = share.FileStore.FlushFileBuffers(openFileObject.Handle);
		if (header.Status != 0)
		{
			state.LogToServer(Severity.Verbose, "Flush '{0}{1}' failed. NTStatus: {2}. (FID: {3})", share.Name, openFileObject.Path, header.Status, request.FID);
			return new ErrorResponse(request.CommandName);
		}
		return new FlushResponse();
	}
}
