using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class LockHelper
{
	internal static SMB2Command GetLockResponse(LockRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		OpenFileObject openFileObject = state.GetSession(request.Header.SessionID).GetOpenFileObject(request.FileId);
		if (openFileObject == null)
		{
			state.LogToServer(Severity.Verbose, "Lock failed. Invalid FileId. (SessionID: {0}, TreeID: {1}, FileId: {2})", request.Header.SessionID, request.Header.TreeID, request.FileId.Volatile);
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_FILE_CLOSED);
		}
		if (request.Locks.Count == 0)
		{
			state.LogToServer(Severity.Verbose, "Lock: Invalid number of locks, must be greater than 0.");
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
		}
		bool unlock = request.Locks[0].Unlock;
		foreach (LockElement @lock in request.Locks)
		{
			if (unlock)
			{
				if (@lock.SharedLock || @lock.ExclusiveLock)
				{
					state.LogToServer(Severity.Verbose, "Lock: Invalid parameter: Lock in a series of unlocks.");
					return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
				}
				continue;
			}
			if (@lock.Unlock)
			{
				state.LogToServer(Severity.Verbose, "Lock: Invalid parameter: Unlock in a series of locks.");
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
			}
			if (@lock.SharedLock && @lock.ExclusiveLock)
			{
				state.LogToServer(Severity.Verbose, "Lock: Invalid parameter: SMB2_LOCKFLAG_SHARED_LOCK and SMB2_LOCKFLAG_EXCLUSIVE_LOCK are mutually exclusive.");
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
			}
			if (request.Locks.Count > 1 && !@lock.FailImmediately)
			{
				state.LogToServer(Severity.Verbose, "Lock: Invalid parameter: SMB2_LOCKFLAG_FAIL_IMMEDIATELY not set in a series of locks.");
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_INVALID_PARAMETER);
			}
		}
		for (int i = 0; i < request.Locks.Count; i++)
		{
			LockElement lockElement = request.Locks[i];
			if (unlock)
			{
				NTStatus nTStatus = share.FileStore.UnlockFile(openFileObject.Handle, (long)lockElement.Offset, (long)lockElement.Length);
				if (nTStatus != 0)
				{
					state.LogToServer(Severity.Information, "Lock: Unlocking '{0}{1}' failed. Offset: {2}, Length: {3}. NTStatus: {4}.", share.Name, openFileObject.Path, lockElement.Offset, lockElement.Length, nTStatus);
					return new ErrorResponse(request.CommandName, nTStatus);
				}
				state.LogToServer(Severity.Information, "Lock: Unlocking '{0}{1}' succeeded. Offset: {2}, Length: {3}.", share.Name, openFileObject.Path, lockElement.Offset, lockElement.Length);
				continue;
			}
			NTStatus nTStatus2 = share.FileStore.LockFile(openFileObject.Handle, (long)lockElement.Offset, (long)lockElement.Length, lockElement.ExclusiveLock);
			if (nTStatus2 != 0)
			{
				state.LogToServer(Severity.Information, "Lock: Locking '{0}{1}' failed. Offset: {2}, Length: {3}. NTStatus: {4}.", share.Name, openFileObject.Path, lockElement.Offset, lockElement.Length, nTStatus2);
				for (int j = 0; j < i; j++)
				{
					share.FileStore.UnlockFile(openFileObject.Handle, (long)request.Locks[j].Offset, (long)request.Locks[j].Length);
				}
				return new ErrorResponse(request.CommandName, nTStatus2);
			}
			state.LogToServer(Severity.Information, "Lock: Locking '{0}{1}' succeeded. Offset: {2}, Length: {3}.", share.Name, openFileObject.Path, lockElement.Offset, lockElement.Length);
		}
		return new LockResponse();
	}
}
