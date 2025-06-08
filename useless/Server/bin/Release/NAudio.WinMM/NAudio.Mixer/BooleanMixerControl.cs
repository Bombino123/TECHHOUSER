using System;
using System.Runtime.InteropServices;

namespace NAudio.Mixer;

public class BooleanMixerControl : MixerControl
{
	private MixerInterop.MIXERCONTROLDETAILS_BOOLEAN boolDetails;

	public bool Value
	{
		get
		{
			GetControlDetails();
			return boolDetails.fValue == 1;
		}
		set
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			boolDetails.fValue = (value ? 1 : 0);
			mixerControlDetails.paDetails = Marshal.AllocHGlobal(Marshal.SizeOf(boolDetails));
			Marshal.StructureToPtr(boolDetails, mixerControlDetails.paDetails, fDeleteOld: false);
			MmException.Try(MixerInterop.mixerSetControlDetails(mixerHandle, ref mixerControlDetails, MixerFlags.Mixer | mixerHandleType), "mixerSetControlDetails");
			Marshal.FreeHGlobal(mixerControlDetails.paDetails);
		}
	}

	internal BooleanMixerControl(MixerInterop.MIXERCONTROL mixerControl, IntPtr mixerHandle, MixerFlags mixerHandleType, int nChannels)
	{
		base.mixerControl = mixerControl;
		base.mixerHandle = mixerHandle;
		base.mixerHandleType = mixerHandleType;
		base.nChannels = nChannels;
		mixerControlDetails = default(MixerInterop.MIXERCONTROLDETAILS);
		GetControlDetails();
	}

	protected override void GetDetails(IntPtr pDetails)
	{
		boolDetails = Marshal.PtrToStructure<MixerInterop.MIXERCONTROLDETAILS_BOOLEAN>(pDetails);
	}
}
