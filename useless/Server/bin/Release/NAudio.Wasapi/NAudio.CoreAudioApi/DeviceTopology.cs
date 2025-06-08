using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class DeviceTopology
{
	private readonly IDeviceTopology deviceTopologyInterface;

	public uint ConnectorCount
	{
		get
		{
			deviceTopologyInterface.GetConnectorCount(out var count);
			return count;
		}
	}

	public string DeviceId
	{
		get
		{
			deviceTopologyInterface.GetDeviceId(out var id);
			return id;
		}
	}

	internal DeviceTopology(IDeviceTopology deviceTopology)
	{
		deviceTopologyInterface = deviceTopology;
	}

	public Connector GetConnector(uint index)
	{
		deviceTopologyInterface.GetConnector(index, out var connector);
		return new Connector(connector);
	}
}
