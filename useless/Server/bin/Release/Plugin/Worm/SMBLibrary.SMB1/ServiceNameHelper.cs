using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class ServiceNameHelper
{
	public static string GetServiceString(ServiceName serviceName)
	{
		return serviceName switch
		{
			ServiceName.DiskShare => "A:", 
			ServiceName.PrinterShare => "LPT1:", 
			ServiceName.NamedPipe => "IPC", 
			ServiceName.SerialCommunicationsDevice => "COMM", 
			_ => "?????", 
		};
	}

	public static ServiceName GetServiceName(string serviceString)
	{
		return serviceString switch
		{
			"A:" => ServiceName.DiskShare, 
			"LPT1:" => ServiceName.PrinterShare, 
			"IPC" => ServiceName.NamedPipe, 
			"COMM" => ServiceName.SerialCommunicationsDevice, 
			_ => ServiceName.AnyType, 
		};
	}
}
