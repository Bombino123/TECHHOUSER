using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace SmbWorm;

internal class ScanHosts
{
	public delegate void SuccuesScanHandler(string host, int port);

	public event SuccuesScanHandler SuccuesScan;

	private void PortScan(string host, int port)
	{
		try
		{
			new TcpClient().Connect(host, port);
			if (this.SuccuesScan != null)
			{
				this.SuccuesScan(host, port);
			}
		}
		catch
		{
		}
	}

	public void Scan(string[] hosts, int port)
	{
		List<Thread> list = new List<Thread>();
		foreach (string host in hosts)
		{
			list.Add(new Thread((ThreadStart)delegate
			{
				PortScan(host, port);
			}));
		}
		foreach (Thread item in list)
		{
			item.Start();
		}
		foreach (Thread item2 in list)
		{
			item2.Join();
		}
	}
}
