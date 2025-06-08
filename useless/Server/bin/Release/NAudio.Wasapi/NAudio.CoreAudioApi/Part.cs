using System;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wasapi.CoreAudioApi;

namespace NAudio.CoreAudioApi;

public class Part
{
	private const int E_NOTFOUND = -2147023728;

	private readonly IPart partInterface;

	private DeviceTopology deviceTopology;

	private static Guid IID_IAudioVolumeLevel = new Guid("7FB7B48F-531D-44A2-BCB3-5AD5A134B3DC");

	private static Guid IID_IAudioMute = new Guid("DF45AEEA-B74A-4B6B-AFAD-2366B6AA012E");

	private static Guid IID_IAudioEndpointVolume = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");

	private static Guid IID_IKsJackDescription = new Guid("4509F757-2D46-4637-8E62-CE7DB944F57B");

	public string Name
	{
		get
		{
			partInterface.GetName(out var name);
			return name;
		}
	}

	public uint LocalId
	{
		get
		{
			partInterface.GetLocalId(out var id);
			return id;
		}
	}

	public string GlobalId
	{
		get
		{
			partInterface.GetGlobalId(out var id);
			return id;
		}
	}

	public PartTypeEnum PartType
	{
		get
		{
			partInterface.GetPartType(out var partType);
			return partType;
		}
	}

	public Guid GetSubType
	{
		get
		{
			partInterface.GetSubType(out var subType);
			return subType;
		}
	}

	public uint ControlInterfaceCount
	{
		get
		{
			partInterface.GetControlInterfaceCount(out var count);
			return count;
		}
	}

	public PartsList PartsIncoming
	{
		get
		{
			IPartsList parts;
			int num = partInterface.EnumPartsIncoming(out parts);
			return num switch
			{
				-2147023728 => new PartsList(null), 
				0 => new PartsList(parts), 
				_ => throw new COMException("EnumPartsIncoming", num), 
			};
		}
	}

	public PartsList PartsOutgoing
	{
		get
		{
			IPartsList parts;
			int num = partInterface.EnumPartsOutgoing(out parts);
			return num switch
			{
				-2147023728 => new PartsList(null), 
				0 => new PartsList(parts), 
				_ => throw new COMException("EnumPartsOutgoing", num), 
			};
		}
	}

	public DeviceTopology DeviceTopology
	{
		get
		{
			if (deviceTopology == null)
			{
				GetDeviceTopology();
			}
			return deviceTopology;
		}
	}

	public AudioVolumeLevel AudioVolumeLevel
	{
		get
		{
			if (partInterface.Activate(ClsCtx.ALL, ref IID_IAudioVolumeLevel, out var interfacePointer) != 0)
			{
				return null;
			}
			return new AudioVolumeLevel(interfacePointer as IAudioVolumeLevel);
		}
	}

	public AudioMute AudioMute
	{
		get
		{
			if (partInterface.Activate(ClsCtx.ALL, ref IID_IAudioMute, out var interfacePointer) != 0)
			{
				return null;
			}
			return new AudioMute(interfacePointer as IAudioMute);
		}
	}

	public KsJackDescription JackDescription
	{
		get
		{
			if (partInterface.Activate(ClsCtx.ALL, ref IID_IKsJackDescription, out var interfacePointer) != 0)
			{
				return null;
			}
			return new KsJackDescription(interfacePointer as IKsJackDescription);
		}
	}

	internal Part(IPart part)
	{
		partInterface = part;
	}

	public IControlInterface GetControlInterface(uint index)
	{
		partInterface.GetControlInterface(index, out var controlInterface);
		return controlInterface;
	}

	private void GetDeviceTopology()
	{
		Marshal.ThrowExceptionForHR(partInterface.GetTopologyObject(out var topologyObject));
		deviceTopology = new DeviceTopology(topologyObject as IDeviceTopology);
	}
}
