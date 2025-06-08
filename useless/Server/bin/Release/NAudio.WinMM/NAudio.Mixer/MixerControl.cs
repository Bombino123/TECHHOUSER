using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NAudio.Mixer;

public abstract class MixerControl
{
	internal MixerInterop.MIXERCONTROL mixerControl;

	internal MixerInterop.MIXERCONTROLDETAILS mixerControlDetails;

	protected IntPtr mixerHandle;

	protected int nChannels;

	protected MixerFlags mixerHandleType;

	public string Name => mixerControl.szName;

	public MixerControlType ControlType => mixerControl.dwControlType;

	public bool IsBoolean => IsControlBoolean(mixerControl.dwControlType);

	public bool IsListText => IsControlListText(mixerControl.dwControlType);

	public bool IsSigned => IsControlSigned(mixerControl.dwControlType);

	public bool IsUnsigned => IsControlUnsigned(mixerControl.dwControlType);

	public bool IsCustom => IsControlCustom(mixerControl.dwControlType);

	public static IList<MixerControl> GetMixerControls(IntPtr mixerHandle, MixerLine mixerLine, MixerFlags mixerHandleType)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		List<MixerControl> list = new List<MixerControl>();
		if (mixerLine.ControlsCount > 0)
		{
			int num = Marshal.SizeOf<MixerInterop.MIXERCONTROL>();
			MixerInterop.MIXERLINECONTROLS mixerLineControls = default(MixerInterop.MIXERLINECONTROLS);
			IntPtr intPtr = Marshal.AllocHGlobal(num * mixerLine.ControlsCount);
			mixerLineControls.cbStruct = Marshal.SizeOf(mixerLineControls);
			mixerLineControls.dwLineID = mixerLine.LineId;
			mixerLineControls.cControls = mixerLine.ControlsCount;
			mixerLineControls.pamxctrl = intPtr;
			mixerLineControls.cbmxctrl = Marshal.SizeOf<MixerInterop.MIXERCONTROL>();
			try
			{
				MmResult val = MixerInterop.mixerGetLineControls(mixerHandle, ref mixerLineControls, MixerFlags.Mixer | mixerHandleType);
				if ((int)val != 0)
				{
					throw new MmException(val, "mixerGetLineControls");
				}
				for (int i = 0; i < mixerLineControls.cControls; i++)
				{
					MixerInterop.MIXERCONTROL mIXERCONTROL = Marshal.PtrToStructure<MixerInterop.MIXERCONTROL>((IntPtr)(intPtr.ToInt64() + num * i));
					MixerControl item = GetMixerControl(mixerHandle, mixerLine.LineId, mIXERCONTROL.dwControlID, mixerLine.Channels, mixerHandleType);
					list.Add(item);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
		return list;
	}

	public static MixerControl GetMixerControl(IntPtr mixerHandle, int nLineId, int controlId, int nChannels, MixerFlags mixerFlags)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		MixerInterop.MIXERLINECONTROLS mixerLineControls = default(MixerInterop.MIXERLINECONTROLS);
		MixerInterop.MIXERCONTROL structure = default(MixerInterop.MIXERCONTROL);
		IntPtr intPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
		mixerLineControls.cbStruct = Marshal.SizeOf(mixerLineControls);
		mixerLineControls.cControls = 1;
		mixerLineControls.dwControlID = controlId;
		mixerLineControls.cbmxctrl = Marshal.SizeOf(structure);
		mixerLineControls.pamxctrl = intPtr;
		mixerLineControls.dwLineID = nLineId;
		MmResult val = MixerInterop.mixerGetLineControls(mixerHandle, ref mixerLineControls, MixerFlags.ListText | mixerFlags);
		if ((int)val != 0)
		{
			Marshal.FreeCoTaskMem(intPtr);
			throw new MmException(val, "mixerGetLineControls");
		}
		structure = Marshal.PtrToStructure<MixerInterop.MIXERCONTROL>(mixerLineControls.pamxctrl);
		Marshal.FreeCoTaskMem(intPtr);
		if (IsControlBoolean(structure.dwControlType))
		{
			return new BooleanMixerControl(structure, mixerHandle, mixerFlags, nChannels);
		}
		if (IsControlSigned(structure.dwControlType))
		{
			return new SignedMixerControl(structure, mixerHandle, mixerFlags, nChannels);
		}
		if (IsControlUnsigned(structure.dwControlType))
		{
			return new UnsignedMixerControl(structure, mixerHandle, mixerFlags, nChannels);
		}
		if (IsControlListText(structure.dwControlType))
		{
			return new ListTextMixerControl(structure, mixerHandle, mixerFlags, nChannels);
		}
		if (IsControlCustom(structure.dwControlType))
		{
			return new CustomMixerControl(structure, mixerHandle, mixerFlags, nChannels);
		}
		throw new InvalidOperationException($"Unknown mixer control type {structure.dwControlType}");
	}

	protected void GetControlDetails()
	{
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		mixerControlDetails.cbStruct = Marshal.SizeOf(mixerControlDetails);
		mixerControlDetails.dwControlID = mixerControl.dwControlID;
		if (IsCustom)
		{
			mixerControlDetails.cChannels = 0;
		}
		else if ((mixerControl.fdwControl & (true ? 1u : 0u)) != 0)
		{
			mixerControlDetails.cChannels = 1;
		}
		else
		{
			mixerControlDetails.cChannels = nChannels;
		}
		if ((mixerControl.fdwControl & 2u) != 0)
		{
			mixerControlDetails.hwndOwner = (IntPtr)mixerControl.cMultipleItems;
		}
		else if (IsCustom)
		{
			mixerControlDetails.hwndOwner = IntPtr.Zero;
		}
		else
		{
			mixerControlDetails.hwndOwner = IntPtr.Zero;
		}
		if (IsBoolean)
		{
			mixerControlDetails.cbDetails = Marshal.SizeOf<MixerInterop.MIXERCONTROLDETAILS_BOOLEAN>();
		}
		else if (IsListText)
		{
			mixerControlDetails.cbDetails = Marshal.SizeOf<MixerInterop.MIXERCONTROLDETAILS_LISTTEXT>();
		}
		else if (IsSigned)
		{
			mixerControlDetails.cbDetails = Marshal.SizeOf<MixerInterop.MIXERCONTROLDETAILS_SIGNED>();
		}
		else if (IsUnsigned)
		{
			mixerControlDetails.cbDetails = Marshal.SizeOf<MixerInterop.MIXERCONTROLDETAILS_UNSIGNED>();
		}
		else
		{
			mixerControlDetails.cbDetails = mixerControl.Metrics.customData;
		}
		int num = mixerControlDetails.cbDetails * mixerControlDetails.cChannels;
		if ((mixerControl.fdwControl & 2u) != 0)
		{
			num *= (int)mixerControl.cMultipleItems;
		}
		IntPtr intPtr = Marshal.AllocCoTaskMem(num);
		mixerControlDetails.paDetails = intPtr;
		MmResult val = MixerInterop.mixerGetControlDetails(mixerHandle, ref mixerControlDetails, MixerFlags.Mixer | mixerHandleType);
		if ((int)val == 0)
		{
			GetDetails(mixerControlDetails.paDetails);
		}
		Marshal.FreeCoTaskMem(intPtr);
		if ((int)val != 0)
		{
			throw new MmException(val, "mixerGetControlDetails");
		}
	}

	protected abstract void GetDetails(IntPtr pDetails);

	private static bool IsControlBoolean(MixerControlType controlType)
	{
		switch (controlType)
		{
		case MixerControlType.BooleanMeter:
		case MixerControlType.Boolean:
		case MixerControlType.OnOff:
		case MixerControlType.Mute:
		case MixerControlType.Mono:
		case MixerControlType.Loudness:
		case MixerControlType.StereoEnhance:
		case MixerControlType.Button:
		case MixerControlType.SingleSelect:
		case MixerControlType.Mux:
		case MixerControlType.MultipleSelect:
		case MixerControlType.Mixer:
			return true;
		default:
			return false;
		}
	}

	private static bool IsControlListText(MixerControlType controlType)
	{
		if (controlType == MixerControlType.Equalizer || (uint)(controlType - 1879113728) <= 1u || (uint)(controlType - 1895890944) <= 1u)
		{
			return true;
		}
		return false;
	}

	private static bool IsControlSigned(MixerControlType controlType)
	{
		switch (controlType)
		{
		case MixerControlType.SignedMeter:
		case MixerControlType.PeakMeter:
		case MixerControlType.Signed:
		case MixerControlType.Decibels:
		case MixerControlType.Slider:
		case MixerControlType.Pan:
		case MixerControlType.QSoundPan:
			return true;
		default:
			return false;
		}
	}

	private static bool IsControlUnsigned(MixerControlType controlType)
	{
		switch (controlType)
		{
		case MixerControlType.UnsignedMeter:
		case MixerControlType.Unsigned:
		case MixerControlType.Percent:
		case MixerControlType.Fader:
		case MixerControlType.Volume:
		case MixerControlType.Bass:
		case MixerControlType.Treble:
		case MixerControlType.Equalizer:
		case MixerControlType.MicroTime:
		case MixerControlType.MilliTime:
			return true;
		default:
			return false;
		}
	}

	private static bool IsControlCustom(MixerControlType controlType)
	{
		return controlType == MixerControlType.Custom;
	}

	public override string ToString()
	{
		return $"{Name} {ControlType}";
	}
}
