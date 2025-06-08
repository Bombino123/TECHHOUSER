using System;
using SMBLibrary.SMB2;
using Utilities;

namespace SMBLibrary.Server.SMB2;

internal class TreeConnectHelper
{
	internal static SMB2Command GetTreeConnectResponse(TreeConnectRequest request, SMB2ConnectionState state, NamedPipeShare services, SMBShareCollection shares)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		TreeConnectResponse treeConnectResponse = new TreeConnectResponse();
		string shareName = ServerPathUtils.GetShareName(request.Path);
		ISMBShare iSMBShare;
		ShareType shareType;
		ShareFlags shareFlags;
		if (string.Equals(shareName, "IPC$", StringComparison.OrdinalIgnoreCase))
		{
			iSMBShare = services;
			shareType = ShareType.Pipe;
			shareFlags = ShareFlags.NoCaching;
		}
		else
		{
			iSMBShare = shares.GetShareFromName(shareName);
			if (iSMBShare == null)
			{
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_OBJECT_PATH_NOT_FOUND);
			}
			shareType = ShareType.Disk;
			shareFlags = GetShareCachingFlags(((FileSystemShare)iSMBShare).CachingPolicy);
			if (!((FileSystemShare)iSMBShare).HasReadAccess(session.SecurityContext, "\\"))
			{
				state.LogToServer(Severity.Verbose, "Tree Connect to '{0}' failed. User '{1}' was denied access.", iSMBShare.Name, session.UserName);
				return new ErrorResponse(request.CommandName, NTStatus.STATUS_ACCESS_DENIED);
			}
		}
		uint? num = session.AddConnectedTree(iSMBShare);
		if (!num.HasValue)
		{
			return new ErrorResponse(request.CommandName, NTStatus.STATUS_INSUFF_SERVER_RESOURCES);
		}
		state.LogToServer(Severity.Information, "Tree Connect: User '{0}' connected to '{1}' (SessionID: {2}, TreeID: {3})", session.UserName, iSMBShare.Name, request.Header.SessionID, num.Value);
		treeConnectResponse.Header.TreeID = num.Value;
		treeConnectResponse.ShareType = shareType;
		treeConnectResponse.ShareFlags = shareFlags;
		treeConnectResponse.MaximalAccess = (AccessMask)2032063u;
		return treeConnectResponse;
	}

	private static ShareFlags GetShareCachingFlags(CachingPolicy cachingPolicy)
	{
		return cachingPolicy switch
		{
			CachingPolicy.ManualCaching => ShareFlags.ManualCaching, 
			CachingPolicy.AutoCaching => ShareFlags.AutoCaching, 
			CachingPolicy.VideoCaching => ShareFlags.VdoCaching, 
			_ => ShareFlags.NoCaching, 
		};
	}

	internal static SMB2Command GetTreeDisconnectResponse(TreeDisconnectRequest request, ISMBShare share, SMB2ConnectionState state)
	{
		SMB2Session session = state.GetSession(request.Header.SessionID);
		session.DisconnectTree(request.Header.TreeID);
		state.LogToServer(Severity.Information, "Tree Disconnect: User '{0}' disconnected from '{1}' (SessionID: {2}, TreeID: {3})", session.UserName, share.Name, request.Header.SessionID, request.Header.TreeID);
		return new TreeDisconnectResponse();
	}
}
