using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ServerService : RemoteService
{
	public const string ServicePipeName = "srvsvc";

	public static readonly Guid ServiceInterfaceGuid = new Guid("4B324FC8-1670-01D3-1278-5A47BF6EE188");

	public const int ServiceVersion = 3;

	public const int MaxPreferredLength = -1;

	private PlatformName m_platformID;

	private string m_serverName;

	private uint m_verMajor;

	private uint m_verMinor;

	private ServerType m_serverType;

	private List<string> m_shares;

	public override Guid InterfaceGuid => ServiceInterfaceGuid;

	public override string PipeName => "srvsvc";

	public ServerService(string serverName, List<string> shares)
	{
		m_platformID = PlatformName.NT;
		m_serverName = serverName;
		m_verMajor = 5u;
		m_verMinor = 2u;
		m_serverType = ServerType.Workstation | ServerType.Server | ServerType.WindowsNT | ServerType.ServerNT | ServerType.MasterBrowser;
		m_shares = shares;
	}

	public override byte[] GetResponseBytes(ushort opNum, byte[] requestBytes)
	{
		switch ((ServerServiceOpName)opNum)
		{
		case ServerServiceOpName.NetrShareEnum:
			return GetNetrShareEnumResponse(requestBytes).GetBytes();
		case ServerServiceOpName.NetrShareGetInfo:
		{
			NetrShareGetInfoRequest request2 = new NetrShareGetInfoRequest(requestBytes);
			return GetNetrShareGetInfoResponse(request2).GetBytes();
		}
		case ServerServiceOpName.NetrServerGetInfo:
		{
			NetrServerGetInfoRequest request = new NetrServerGetInfoRequest(requestBytes);
			return GetNetrWkstaGetInfoResponse(request).GetBytes();
		}
		default:
			throw new UnsupportedOpNumException();
		}
	}

	public NetrShareEnumResponse GetNetrShareEnumResponse(byte[] requestBytes)
	{
		NetrShareEnumResponse netrShareEnumResponse = new NetrShareEnumResponse();
		NetrShareEnumRequest netrShareEnumRequest;
		try
		{
			netrShareEnumRequest = new NetrShareEnumRequest(requestBytes);
		}
		catch (UnsupportedLevelException ex)
		{
			netrShareEnumResponse.InfoStruct = new ShareEnum(ex.Level);
			netrShareEnumResponse.Result = Win32Error.ERROR_NOT_SUPPORTED;
			return netrShareEnumResponse;
		}
		catch (InvalidLevelException ex2)
		{
			netrShareEnumResponse.InfoStruct = new ShareEnum(ex2.Level);
			netrShareEnumResponse.Result = Win32Error.ERROR_INVALID_LEVEL;
			return netrShareEnumResponse;
		}
		switch (netrShareEnumRequest.InfoStruct.Level)
		{
		case 0u:
		{
			ShareInfo0Container shareInfo0Container = new ShareInfo0Container();
			foreach (string share in m_shares)
			{
				shareInfo0Container.Add(new ShareInfo0Entry(share));
			}
			netrShareEnumResponse.InfoStruct = new ShareEnum(shareInfo0Container);
			netrShareEnumResponse.TotalEntries = (uint)m_shares.Count;
			netrShareEnumResponse.Result = Win32Error.ERROR_SUCCESS;
			return netrShareEnumResponse;
		}
		case 1u:
		{
			ShareInfo1Container shareInfo1Container = new ShareInfo1Container();
			foreach (string share2 in m_shares)
			{
				shareInfo1Container.Add(new ShareInfo1Entry(share2, new ShareTypeExtended(ShareType.DiskDrive)));
			}
			netrShareEnumResponse.InfoStruct = new ShareEnum(shareInfo1Container);
			netrShareEnumResponse.TotalEntries = (uint)m_shares.Count;
			netrShareEnumResponse.Result = Win32Error.ERROR_SUCCESS;
			return netrShareEnumResponse;
		}
		case 2u:
		case 501u:
		case 502u:
		case 503u:
			netrShareEnumResponse.InfoStruct = new ShareEnum(netrShareEnumRequest.InfoStruct.Level);
			netrShareEnumResponse.Result = Win32Error.ERROR_NOT_SUPPORTED;
			return netrShareEnumResponse;
		default:
			netrShareEnumResponse.InfoStruct = new ShareEnum(netrShareEnumRequest.InfoStruct.Level);
			netrShareEnumResponse.Result = Win32Error.ERROR_INVALID_LEVEL;
			return netrShareEnumResponse;
		}
	}

	public NetrShareGetInfoResponse GetNetrShareGetInfoResponse(NetrShareGetInfoRequest request)
	{
		int num = IndexOfShare(request.NetName);
		NetrShareGetInfoResponse netrShareGetInfoResponse = new NetrShareGetInfoResponse();
		if (num == -1)
		{
			netrShareGetInfoResponse.InfoStruct = new ShareInfo(request.Level);
			netrShareGetInfoResponse.Result = Win32Error.NERR_NetNameNotFound;
			return netrShareGetInfoResponse;
		}
		switch (request.Level)
		{
		case 0u:
		{
			ShareInfo0Entry info3 = new ShareInfo0Entry(m_shares[num]);
			netrShareGetInfoResponse.InfoStruct = new ShareInfo(info3);
			netrShareGetInfoResponse.Result = Win32Error.ERROR_SUCCESS;
			return netrShareGetInfoResponse;
		}
		case 1u:
		{
			ShareInfo1Entry info2 = new ShareInfo1Entry(m_shares[num], new ShareTypeExtended(ShareType.DiskDrive));
			netrShareGetInfoResponse.InfoStruct = new ShareInfo(info2);
			netrShareGetInfoResponse.Result = Win32Error.ERROR_SUCCESS;
			return netrShareGetInfoResponse;
		}
		case 2u:
		{
			ShareInfo2Entry info = new ShareInfo2Entry(m_shares[num], new ShareTypeExtended(ShareType.DiskDrive));
			netrShareGetInfoResponse.InfoStruct = new ShareInfo(info);
			netrShareGetInfoResponse.Result = Win32Error.ERROR_SUCCESS;
			return netrShareGetInfoResponse;
		}
		case 501u:
		case 502u:
		case 503u:
		case 1005u:
			netrShareGetInfoResponse.InfoStruct = new ShareInfo(request.Level);
			netrShareGetInfoResponse.Result = Win32Error.ERROR_NOT_SUPPORTED;
			return netrShareGetInfoResponse;
		default:
			netrShareGetInfoResponse.InfoStruct = new ShareInfo(request.Level);
			netrShareGetInfoResponse.Result = Win32Error.ERROR_INVALID_LEVEL;
			return netrShareGetInfoResponse;
		}
	}

	public NetrServerGetInfoResponse GetNetrWkstaGetInfoResponse(NetrServerGetInfoRequest request)
	{
		NetrServerGetInfoResponse netrServerGetInfoResponse = new NetrServerGetInfoResponse();
		switch (request.Level)
		{
		case 100u:
		{
			ServerInfo100 serverInfo2 = new ServerInfo100();
			serverInfo2.PlatformID = m_platformID;
			serverInfo2.ServerName.Value = m_serverName;
			netrServerGetInfoResponse.InfoStruct = new ServerInfo(serverInfo2);
			netrServerGetInfoResponse.Result = Win32Error.ERROR_SUCCESS;
			return netrServerGetInfoResponse;
		}
		case 101u:
		{
			ServerInfo101 serverInfo = new ServerInfo101();
			serverInfo.PlatformID = m_platformID;
			serverInfo.ServerName.Value = m_serverName;
			serverInfo.VerMajor = m_verMajor;
			serverInfo.VerMinor = m_verMinor;
			serverInfo.Type = m_serverType;
			serverInfo.Comment.Value = string.Empty;
			netrServerGetInfoResponse.InfoStruct = new ServerInfo(serverInfo);
			netrServerGetInfoResponse.Result = Win32Error.ERROR_SUCCESS;
			return netrServerGetInfoResponse;
		}
		case 102u:
		case 103u:
		case 502u:
		case 503u:
			netrServerGetInfoResponse.InfoStruct = new ServerInfo(request.Level);
			netrServerGetInfoResponse.Result = Win32Error.ERROR_NOT_SUPPORTED;
			return netrServerGetInfoResponse;
		default:
			netrServerGetInfoResponse.InfoStruct = new ServerInfo(request.Level);
			netrServerGetInfoResponse.Result = Win32Error.ERROR_INVALID_LEVEL;
			return netrServerGetInfoResponse;
		}
	}

	private int IndexOfShare(string shareName)
	{
		for (int i = 0; i < m_shares.Count; i++)
		{
			if (m_shares[i].Equals(shareName, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}
}
