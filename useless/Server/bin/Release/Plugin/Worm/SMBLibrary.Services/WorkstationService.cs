using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class WorkstationService : RemoteService
{
	public const string ServicePipeName = "wkssvc";

	public static readonly Guid ServiceInterfaceGuid = new Guid("6BFFD098-A112-3610-9833-46C3F87E345A");

	public const int ServiceVersion = 1;

	private uint m_platformID;

	private string m_computerName;

	private string m_lanGroup;

	private uint m_verMajor;

	private uint m_verMinor;

	public override Guid InterfaceGuid => ServiceInterfaceGuid;

	public override string PipeName => "wkssvc";

	public WorkstationService(string computerName, string lanGroup)
	{
		m_platformID = 500u;
		m_computerName = computerName;
		m_lanGroup = lanGroup;
		m_verMajor = 5u;
		m_verMinor = 2u;
	}

	public override byte[] GetResponseBytes(ushort opNum, byte[] requestBytes)
	{
		if (opNum == 0)
		{
			NetrWkstaGetInfoRequest request = new NetrWkstaGetInfoRequest(requestBytes);
			return GetNetrWkstaGetInfoResponse(request).GetBytes();
		}
		throw new UnsupportedOpNumException();
	}

	public NetrWkstaGetInfoResponse GetNetrWkstaGetInfoResponse(NetrWkstaGetInfoRequest request)
	{
		NetrWkstaGetInfoResponse netrWkstaGetInfoResponse = new NetrWkstaGetInfoResponse();
		switch (request.Level)
		{
		case 100u:
		{
			WorkstationInfo100 workstationInfo2 = new WorkstationInfo100();
			workstationInfo2.PlatformID = m_platformID;
			workstationInfo2.ComputerName.Value = m_computerName;
			workstationInfo2.LanGroup.Value = m_lanGroup;
			workstationInfo2.VerMajor = m_verMajor;
			workstationInfo2.VerMinor = m_verMinor;
			netrWkstaGetInfoResponse.WkstaInfo = new WorkstationInfo(workstationInfo2);
			netrWkstaGetInfoResponse.Result = Win32Error.ERROR_SUCCESS;
			return netrWkstaGetInfoResponse;
		}
		case 101u:
		{
			WorkstationInfo101 workstationInfo = new WorkstationInfo101();
			workstationInfo.PlatformID = m_platformID;
			workstationInfo.ComputerName.Value = m_computerName;
			workstationInfo.LanGroup.Value = m_lanGroup;
			workstationInfo.VerMajor = m_verMajor;
			workstationInfo.VerMinor = m_verMinor;
			workstationInfo.LanRoot.Value = m_lanGroup;
			netrWkstaGetInfoResponse.WkstaInfo = new WorkstationInfo(workstationInfo);
			netrWkstaGetInfoResponse.Result = Win32Error.ERROR_SUCCESS;
			return netrWkstaGetInfoResponse;
		}
		case 102u:
		case 502u:
			netrWkstaGetInfoResponse.WkstaInfo = new WorkstationInfo(request.Level);
			netrWkstaGetInfoResponse.Result = Win32Error.ERROR_NOT_SUPPORTED;
			return netrWkstaGetInfoResponse;
		default:
			netrWkstaGetInfoResponse.WkstaInfo = new WorkstationInfo(request.Level);
			netrWkstaGetInfoResponse.Result = Win32Error.ERROR_INVALID_LEVEL;
			return netrWkstaGetInfoResponse;
		}
	}
}
