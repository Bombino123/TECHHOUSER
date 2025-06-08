using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using SMBLibrary.NetBios;

namespace SMBLibrary.Server;

[ComVisible(true)]
public class NameServer
{
	public static readonly int NetBiosNameServicePort = 137;

	public static readonly string WorkgroupName = "WORKGROUP";

	private IPAddress m_serverAddress;

	private IPAddress m_broadcastAddress;

	private UdpClient m_client;

	private bool m_listening;

	public NameServer(IPAddress serverAddress, IPAddress subnetMask)
	{
		if (serverAddress.AddressFamily != AddressFamily.InterNetwork)
		{
			throw new ArgumentException("NetBIOS name service can only supply IPv4 addresses");
		}
		if (object.Equals(serverAddress, IPAddress.Any))
		{
			throw new ArgumentException("NetBIOS name service requires an IPAddress that is associated with a specific network interface");
		}
		m_serverAddress = serverAddress;
		m_broadcastAddress = GetBroadcastAddress(serverAddress, subnetMask);
	}

	public void Start()
	{
		if (!m_listening)
		{
			m_listening = true;
			m_client = new UdpClient(new IPEndPoint(m_serverAddress, NetBiosNameServicePort));
			m_client.BeginReceive(ReceiveCallback, null);
			new Thread(RegisterNetBIOSName).Start();
		}
	}

	public void Stop()
	{
		m_listening = false;
		m_client.Close();
	}

	private void ReceiveCallback(IAsyncResult result)
	{
		if (!m_listening)
		{
			return;
		}
		IPEndPoint remoteEP = null;
		byte[] array;
		try
		{
			array = m_client.EndReceive(result, ref remoteEP);
		}
		catch (ObjectDisposedException)
		{
			return;
		}
		catch (SocketException)
		{
			return;
		}
		if (array.Length > 12 && new NameServicePacketHeader(array, 0).OpCode == NameServiceOperation.QueryRequest)
		{
			NameQueryRequest nameQueryRequest = null;
			try
			{
				nameQueryRequest = new NameQueryRequest(array, 0);
			}
			catch
			{
			}
			if (nameQueryRequest != null)
			{
				if (nameQueryRequest.Question.Type == NameRecordType.NB)
				{
					string nameFromMSNetBiosName = NetBiosUtils.GetNameFromMSNetBiosName(nameQueryRequest.Question.Name);
					NetBiosSuffix netBiosSuffix = (NetBiosSuffix)nameQueryRequest.Question.Name[15];
					if (string.Equals(nameFromMSNetBiosName, Environment.MachineName, StringComparison.OrdinalIgnoreCase) && (netBiosSuffix == NetBiosSuffix.WorkstationService || netBiosSuffix == NetBiosSuffix.FileServiceService))
					{
						byte[] bytes = new PositiveNameQueryResponse
						{
							Header = 
							{
								TransactionID = nameQueryRequest.Header.TransactionID
							},
							Resource = 
							{
								Name = nameQueryRequest.Question.Name
							},
							Addresses = { 
							{
								m_serverAddress.GetAddressBytes(),
								default(NameFlags)
							} }
						}.GetBytes();
						m_client.Send(bytes, bytes.Length, remoteEP);
					}
				}
				else
				{
					NodeStatusResponse obj2 = new NodeStatusResponse
					{
						Header = 
						{
							TransactionID = nameQueryRequest.Header.TransactionID
						},
						Resource = 
						{
							Name = nameQueryRequest.Question.Name
						}
					};
					NameFlags value = default(NameFlags);
					string mSNetBiosName = NetBiosUtils.GetMSNetBiosName(Environment.MachineName, NetBiosSuffix.WorkstationService);
					string mSNetBiosName2 = NetBiosUtils.GetMSNetBiosName(Environment.MachineName, NetBiosSuffix.FileServiceService);
					NameFlags value2 = new NameFlags
					{
						WorkGroup = true
					};
					string mSNetBiosName3 = NetBiosUtils.GetMSNetBiosName(WorkgroupName, NetBiosSuffix.WorkstationService);
					obj2.Names.Add(mSNetBiosName, value);
					obj2.Names.Add(mSNetBiosName2, value);
					obj2.Names.Add(mSNetBiosName3, value2);
					byte[] bytes2 = obj2.GetBytes();
					try
					{
						m_client.Send(bytes2, bytes2.Length, remoteEP);
					}
					catch (ObjectDisposedException)
					{
					}
				}
			}
		}
		try
		{
			m_client.BeginReceive(ReceiveCallback, null);
		}
		catch (ObjectDisposedException)
		{
		}
		catch (SocketException)
		{
		}
	}

	private void RegisterNetBIOSName()
	{
		NameRegistrationRequest request = new NameRegistrationRequest(Environment.MachineName, NetBiosSuffix.WorkstationService, m_serverAddress);
		NameRegistrationRequest request2 = new NameRegistrationRequest(Environment.MachineName, NetBiosSuffix.FileServiceService, m_serverAddress);
		NameRegistrationRequest nameRegistrationRequest = new NameRegistrationRequest(WorkgroupName, NetBiosSuffix.WorkstationService, m_serverAddress);
		nameRegistrationRequest.NameFlags.WorkGroup = true;
		RegisterName(request);
		RegisterName(request2);
		RegisterName(nameRegistrationRequest);
	}

	private void RegisterName(NameRegistrationRequest request)
	{
		byte[] bytes = request.GetBytes();
		IPEndPoint endPoint = new IPEndPoint(m_broadcastAddress, NetBiosNameServicePort);
		for (int i = 0; i < 4; i++)
		{
			try
			{
				m_client.Send(bytes, bytes.Length, endPoint);
			}
			catch (ObjectDisposedException)
			{
			}
			if (i < 3)
			{
				Thread.Sleep(250);
			}
		}
	}

	public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
	{
		byte[] addressBytes = address.GetAddressBytes();
		byte[] addressBytes2 = subnetMask.GetAddressBytes();
		byte[] array = new byte[addressBytes.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (byte)(addressBytes[i] | (addressBytes2[i] ^ 0xFFu));
		}
		return new IPAddress(array);
	}
}
