using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class Connector
{
	private readonly IConnector connectorInterface;

	public ConnectorType Type
	{
		get
		{
			connectorInterface.GetType(out var type);
			return type;
		}
	}

	public DataFlow DataFlow
	{
		get
		{
			connectorInterface.GetDataFlow(out var flow);
			return flow;
		}
	}

	public bool IsConnected
	{
		get
		{
			connectorInterface.IsConnected(out var connected);
			return connected;
		}
	}

	public Connector ConnectedTo
	{
		get
		{
			connectorInterface.GetConnectedTo(out var conTo);
			return new Connector(conTo);
		}
	}

	public string ConnectedToConnectorId
	{
		get
		{
			connectorInterface.GetConnectorIdConnectedTo(out var id);
			return id;
		}
	}

	public string ConnectedToDeviceId
	{
		get
		{
			connectorInterface.GetDeviceIdConnectedTo(out var id);
			return id;
		}
	}

	public Part Part => new Part(connectorInterface as IPart);

	internal Connector(IConnector connector)
	{
		connectorInterface = connector;
	}

	public void ConnectTo(Connector other)
	{
		connectorInterface.ConnectTo(other.connectorInterface);
	}

	public void Disconnect()
	{
		connectorInterface.Disconnect();
	}
}
