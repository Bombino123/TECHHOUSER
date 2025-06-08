using System;
using SMBLibrary.SMB1;
using Utilities;

namespace SMBLibrary.Server.SMB1;

internal class TreeConnectHelper
{
	internal static SMB1Command GetTreeConnectResponse(SMB1Header header, TreeConnectAndXRequest request, SMB1ConnectionState state, NamedPipeShare services, SMBShareCollection shares)
	{
		SMB1Session session = state.GetSession(header.UID);
		bool flag = (int)(request.Flags & TreeConnectFlags.ExtendedResponse) > 0;
		string shareName = ServerPathUtils.GetShareName(request.Path);
		ISMBShare iSMBShare;
		ServiceName serviceName;
		OptionalSupportFlags supportFlags;
		if (string.Equals(shareName, "IPC$", StringComparison.OrdinalIgnoreCase))
		{
			if (request.Service != ServiceName.AnyType && request.Service != ServiceName.NamedPipe)
			{
				header.Status = NTStatus.STATUS_BAD_DEVICE_TYPE;
				return new ErrorResponse(request.CommandName);
			}
			iSMBShare = services;
			serviceName = ServiceName.NamedPipe;
			supportFlags = OptionalSupportFlags.SMB_CSC_NO_CACHING | OptionalSupportFlags.SMB_SUPPORT_SEARCH_BITS;
		}
		else
		{
			iSMBShare = shares.GetShareFromName(shareName);
			if (iSMBShare == null)
			{
				header.Status = NTStatus.STATUS_OBJECT_PATH_NOT_FOUND;
				return new ErrorResponse(request.CommandName);
			}
			if (request.Service != ServiceName.AnyType && request.Service != 0)
			{
				header.Status = NTStatus.STATUS_BAD_DEVICE_TYPE;
				return new ErrorResponse(request.CommandName);
			}
			serviceName = ServiceName.DiskShare;
			supportFlags = OptionalSupportFlags.SMB_SUPPORT_SEARCH_BITS | GetCachingSupportFlags(((FileSystemShare)iSMBShare).CachingPolicy);
			if (!((FileSystemShare)iSMBShare).HasReadAccess(session.SecurityContext, "\\"))
			{
				state.LogToServer(Severity.Verbose, "Tree Connect to '{0}' failed. User '{1}' was denied access.", iSMBShare.Name, session.UserName);
				header.Status = NTStatus.STATUS_ACCESS_DENIED;
				return new ErrorResponse(request.CommandName);
			}
		}
		ushort? num = session.AddConnectedTree(iSMBShare);
		if (!num.HasValue)
		{
			header.Status = NTStatus.STATUS_INSUFF_SERVER_RESOURCES;
			return new ErrorResponse(request.CommandName);
		}
		state.LogToServer(Severity.Information, "Tree Connect: User '{0}' connected to '{1}' (UID: {2}, TID: {3})", session.UserName, iSMBShare.Name, header.UID, num.Value);
		header.TID = num.Value;
		if (flag)
		{
			return CreateTreeConnectResponseExtended(serviceName, supportFlags);
		}
		return CreateTreeConnectResponse(serviceName, supportFlags);
	}

	private static OptionalSupportFlags GetCachingSupportFlags(CachingPolicy cachingPolicy)
	{
		return cachingPolicy switch
		{
			CachingPolicy.ManualCaching => OptionalSupportFlags.SMB_CSC_CACHE_MANUAL_REINT, 
			CachingPolicy.AutoCaching => OptionalSupportFlags.SMB_CSC_CACHE_AUTO_REINT, 
			CachingPolicy.VideoCaching => OptionalSupportFlags.SMB_CSC_CACHE_VDO, 
			_ => OptionalSupportFlags.SMB_CSC_NO_CACHING, 
		};
	}

	private static TreeConnectAndXResponse CreateTreeConnectResponse(ServiceName serviceName, OptionalSupportFlags supportFlags)
	{
		return new TreeConnectAndXResponse
		{
			OptionalSupport = supportFlags,
			NativeFileSystem = string.Empty,
			Service = serviceName
		};
	}

	private static TreeConnectAndXResponseExtended CreateTreeConnectResponseExtended(ServiceName serviceName, OptionalSupportFlags supportFlags)
	{
		return new TreeConnectAndXResponseExtended
		{
			OptionalSupport = supportFlags,
			MaximalShareAccessRights = (AccessMask)2032063u,
			GuestMaximalShareAccessRights = (AccessMask)1180059u,
			NativeFileSystem = string.Empty,
			Service = serviceName
		};
	}

	internal static SMB1Command GetTreeDisconnectResponse(SMB1Header header, TreeDisconnectRequest request, ISMBShare share, SMB1ConnectionState state)
	{
		SMB1Session session = state.GetSession(header.UID);
		session.DisconnectTree(header.TID);
		state.LogToServer(Severity.Information, "Tree Disconnect: User '{0}' disconnected from '{1}' (UID: {2}, TID: {3})", session.UserName, share.Name, header.UID, header.TID);
		return new TreeDisconnectResponse();
	}
}
