using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class MMDeviceCollection : IEnumerable<MMDevice>, IEnumerable
{
	private readonly IMMDeviceCollection mmDeviceCollection;

	public int Count
	{
		get
		{
			Marshal.ThrowExceptionForHR(mmDeviceCollection.GetCount(out var numDevices));
			return numDevices;
		}
	}

	public MMDevice this[int index]
	{
		get
		{
			mmDeviceCollection.Item(index, out var device);
			return new MMDevice(device);
		}
	}

	internal MMDeviceCollection(IMMDeviceCollection parent)
	{
		mmDeviceCollection = parent;
	}

	public IEnumerator<MMDevice> GetEnumerator()
	{
		for (int index = 0; index < Count; index++)
		{
			yield return this[index];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
