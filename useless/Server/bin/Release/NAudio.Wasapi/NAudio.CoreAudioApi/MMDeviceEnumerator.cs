using System;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class MMDeviceEnumerator : IDisposable
{
	private IMMDeviceEnumerator realEnumerator;

	public MMDeviceEnumerator()
	{
		if (Environment.OSVersion.Version.Major < 6)
		{
			throw new NotSupportedException("This functionality is only supported on Windows Vista or newer.");
		}
		realEnumerator = new MMDeviceEnumeratorComObject() as IMMDeviceEnumerator;
	}

	public MMDeviceCollection EnumerateAudioEndPoints(DataFlow dataFlow, DeviceState dwStateMask)
	{
		Marshal.ThrowExceptionForHR(realEnumerator.EnumAudioEndpoints(dataFlow, dwStateMask, out var devices));
		return new MMDeviceCollection(devices);
	}

	public MMDevice GetDefaultAudioEndpoint(DataFlow dataFlow, Role role)
	{
		Marshal.ThrowExceptionForHR(realEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out var endpoint));
		return new MMDevice(endpoint);
	}

	public bool HasDefaultAudioEndpoint(DataFlow dataFlow, Role role)
	{
		IMMDevice endpoint;
		int defaultAudioEndpoint = realEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out endpoint);
		switch (defaultAudioEndpoint)
		{
		case 0:
			Marshal.ReleaseComObject(endpoint);
			return true;
		case -2147023728:
			return false;
		default:
			Marshal.ThrowExceptionForHR(defaultAudioEndpoint);
			return false;
		}
	}

	public MMDevice GetDevice(string id)
	{
		Marshal.ThrowExceptionForHR(realEnumerator.GetDevice(id, out var deviceName));
		return new MMDevice(deviceName);
	}

	public int RegisterEndpointNotificationCallback([In][MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client)
	{
		return realEnumerator.RegisterEndpointNotificationCallback(client);
	}

	public int UnregisterEndpointNotificationCallback([In][MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client)
	{
		return realEnumerator.UnregisterEndpointNotificationCallback(client);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && realEnumerator != null)
		{
			Marshal.ReleaseComObject(realEnumerator);
			realEnumerator = null;
		}
	}
}
