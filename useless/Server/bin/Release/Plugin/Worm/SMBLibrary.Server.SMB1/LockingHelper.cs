using System.Collections.Generic;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class LockingHelper
{
	internal static List<SMB1Command> GetLockingAndXResponse(SMB1Header header, LockingAndXRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		OpenFileObject openFileObject = state.GetSession(header.UID).GetOpenFileObject(request.FID);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Locking failed. Invalid FID. (UID: {0}, TID: {1}, FID: {2})", header.UID, header.TID, request.FID);
			header.Status = NTStatus.STATUS_INVALID_HANDLE;
			return new ErrorResponse(request.CommandName);
		}
		if ((int)(request.TypeOfLock & LockType.CHANGE_LOCKTYPE) > 0)
		{
			state.LogToServer(Severity.Verbose, "Locking failed. CHANGE_LOCKTYPE is not supported.");
			header.Status = NTStatus.STATUS_NOT_IMPLEMENTED;
			return new ErrorResponse(request.CommandName);
		}
		if (request.Unlocks.Count == 0 && request.Locks.Count == 0)
		{
			return new List<SMB1Command>();
		}
		for (int i = 0; i < request.Unlocks.Count; i++)
		{
			LockingRange lockingRange = request.Unlocks[i];
			header.Status = share.FileStore.UnlockFile(openFileObject.Handle, (long)lockingRange.ByteOffset, (long)lockingRange.LengthInBytes);
			if (header.Status != 0)
			{
				state.LogToServer(Severity.Verbose, "Locking: Unlocking '{0}{1}' failed. Offset: {2}, Length: {3}. NTStatus: {4}.", share.Name, openFileObject.Path, lockingRange.ByteOffset, lockingRange.LengthInBytes, header.Status);
				return new ErrorResponse(request.CommandName);
			}
			state.LogToServer(Severity.Verbose, "Locking: Unlocking '{0}{1}' succeeded. Offset: {2}, Length: {3}.", share.Name, openFileObject.Path, lockingRange.ByteOffset, lockingRange.LengthInBytes);
		}
		for (int j = 0; j < request.Locks.Count; j++)
		{
			LockingRange lockingRange2 = request.Locks[j];
			bool exclusiveLock = (request.TypeOfLock & LockType.SHARED_LOCK) == 0;
			header.Status = share.FileStore.LockFile(openFileObject.Handle, (long)lockingRange2.ByteOffset, (long)lockingRange2.LengthInBytes, exclusiveLock);
			if (header.Status != 0)
			{
				state.LogToServer(Severity.Verbose, "Locking: Locking '{0}{1}' failed. Offset: {2}, Length: {3}. NTStatus: {4}.", share.Name, openFileObject.Path, lockingRange2.ByteOffset, lockingRange2.LengthInBytes, header.Status);
				for (int k = 0; k < j; k++)
				{
					share.FileStore.UnlockFile(openFileObject.Handle, (long)request.Locks[k].ByteOffset, (long)request.Locks[k].LengthInBytes);
				}
				return new ErrorResponse(request.CommandName);
			}
			state.LogToServer(Severity.Verbose, "Locking: Locking '{0}{1}' succeeded. Offset: {2}, Length: {3}.", share.Name, openFileObject.Path, lockingRange2.ByteOffset, lockingRange2.LengthInBytes);
		}
		return new LockingAndXResponse();
	}
}
