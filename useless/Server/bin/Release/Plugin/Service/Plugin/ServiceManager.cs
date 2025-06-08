using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;

namespace Plugin;

internal class ServiceManager
{
	private static ServiceController GetService(string name)
	{
		return ServiceController.GetServices().ToList().Find((ServiceController item) => item.ServiceName == name);
	}

	public static object[] Update()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		List<object> list = new List<object>();
		list.AddRange(new object[2] { "Service", "List" });
		ServiceController[] services = ServiceController.GetServices();
		foreach (ServiceController val in services)
		{
			object[] obj = new object[5] { val.ServiceName, val.DisplayName, null, null, null };
			ServiceType serviceType = val.ServiceType;
			obj[2] = ((object)(ServiceType)(ref serviceType)).ToString();
			ServiceControllerStatus status = val.Status;
			obj[3] = ((object)(ServiceControllerStatus)(ref status)).ToString();
			obj[4] = val.CanStop.ToString();
			list.AddRange(obj);
		}
		return list.ToArray();
	}

	public static object[] Pause(string name)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			GetService(name).Pause();
		}
		catch
		{
		}
		object[] obj2 = new object[4] { "Service", "Status", name, null };
		ServiceControllerStatus status = GetService(name).Status;
		obj2[3] = ((object)(ServiceControllerStatus)(ref status)).ToString();
		return obj2;
	}

	public static object[] Stop(string name)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			GetService(name).Stop();
		}
		catch
		{
		}
		object[] obj2 = new object[4] { "Service", "Status", name, null };
		ServiceControllerStatus status = GetService(name).Status;
		obj2[3] = ((object)(ServiceControllerStatus)(ref status)).ToString();
		return obj2;
	}

	public static object[] Start(string name)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			GetService(name).Start();
		}
		catch
		{
		}
		object[] obj2 = new object[4] { "Service", "Status", name, null };
		ServiceControllerStatus status = GetService(name).Status;
		obj2[3] = ((object)(ServiceControllerStatus)(ref status)).ToString();
		return obj2;
	}
}
