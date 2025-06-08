using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using NAudio.Utils;

namespace NAudio.Wave.Compression;

public class AcmDriver : IDisposable
{
	private static List<AcmDriver> drivers;

	private AcmDriverDetails details;

	private IntPtr driverId;

	private IntPtr driverHandle;

	private List<AcmFormatTag> formatTags;

	private List<AcmFormat> tempFormatsList;

	private IntPtr localDllHandle;

	public int MaxFormatSize
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			MmException.Try(AcmInterop.acmMetrics(driverHandle, AcmMetrics.MaxSizeFormat, out var output), "acmMetrics");
			return output;
		}
	}

	public string ShortName => details.shortName;

	public string LongName => details.longName;

	public IntPtr DriverId => driverId;

	public IEnumerable<AcmFormatTag> FormatTags
	{
		get
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			if (formatTags == null)
			{
				if (driverHandle == IntPtr.Zero)
				{
					throw new InvalidOperationException("Driver must be opened first");
				}
				formatTags = new List<AcmFormatTag>();
				AcmFormatTagDetails formatTagDetails = default(AcmFormatTagDetails);
				formatTagDetails.structureSize = Marshal.SizeOf(formatTagDetails);
				MmException.Try(AcmInterop.acmFormatTagEnum(driverHandle, ref formatTagDetails, AcmFormatTagEnumCallback, IntPtr.Zero, 0), "acmFormatTagEnum");
			}
			return formatTags;
		}
	}

	public static bool IsCodecInstalled(string shortName)
	{
		foreach (AcmDriver item in EnumerateAcmDrivers())
		{
			if (item.ShortName == shortName)
			{
				return true;
			}
		}
		return false;
	}

	public static AcmDriver AddLocalDriver(string driverFile)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		IntPtr intPtr = NativeMethods.LoadLibrary(driverFile);
		if (intPtr == IntPtr.Zero)
		{
			throw new ArgumentException("Failed to load driver file");
		}
		IntPtr procAddress = NativeMethods.GetProcAddress(intPtr, "DriverProc");
		if (procAddress == IntPtr.Zero)
		{
			NativeMethods.FreeLibrary(intPtr);
			throw new ArgumentException("Failed to discover DriverProc");
		}
		IntPtr hAcmDriver;
		MmResult val = AcmInterop.acmDriverAdd(out hAcmDriver, intPtr, procAddress, 0, AcmDriverAddFlags.Function);
		if ((int)val != 0)
		{
			NativeMethods.FreeLibrary(intPtr);
			throw new MmException(val, "acmDriverAdd");
		}
		AcmDriver acmDriver = new AcmDriver(hAcmDriver);
		if (string.IsNullOrEmpty(acmDriver.details.longName))
		{
			acmDriver.details.longName = "Local driver: " + Path.GetFileName(driverFile);
			acmDriver.localDllHandle = intPtr;
		}
		return acmDriver;
	}

	public static void RemoveLocalDriver(AcmDriver localDriver)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (localDriver.localDllHandle == IntPtr.Zero)
		{
			throw new ArgumentException("Please pass in the AcmDriver returned by the AddLocalDriver method");
		}
		MmResult val = AcmInterop.acmDriverRemove(localDriver.driverId, 0);
		NativeMethods.FreeLibrary(localDriver.localDllHandle);
		MmException.Try(val, "acmDriverRemove");
	}

	public static bool ShowFormatChooseDialog(IntPtr ownerWindowHandle, string windowTitle, AcmFormatEnumFlags enumFlags, WaveFormat enumFormat, out WaveFormat selectedFormat, out string selectedFormatDescription, out string selectedFormatTagDescription)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Invalid comparison between Unknown and I4
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Invalid comparison between Unknown and I4
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		AcmFormatChoose formatChoose = default(AcmFormatChoose);
		formatChoose.structureSize = Marshal.SizeOf(formatChoose);
		formatChoose.styleFlags = AcmFormatChooseStyleFlags.None;
		formatChoose.ownerWindowHandle = ownerWindowHandle;
		int num = 200;
		formatChoose.selectedWaveFormatPointer = Marshal.AllocHGlobal(num);
		formatChoose.selectedWaveFormatByteSize = num;
		formatChoose.title = windowTitle;
		formatChoose.name = null;
		formatChoose.formatEnumFlags = enumFlags;
		formatChoose.waveFormatEnumPointer = IntPtr.Zero;
		if (enumFormat != null)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf<WaveFormat>(enumFormat));
			Marshal.StructureToPtr<WaveFormat>(enumFormat, intPtr, fDeleteOld: false);
			formatChoose.waveFormatEnumPointer = intPtr;
		}
		formatChoose.instanceHandle = IntPtr.Zero;
		formatChoose.templateName = null;
		MmResult val = AcmInterop.acmFormatChoose(ref formatChoose);
		selectedFormat = null;
		selectedFormatDescription = null;
		selectedFormatTagDescription = null;
		if ((int)val == 0)
		{
			selectedFormat = WaveFormat.MarshalFromPtr(formatChoose.selectedWaveFormatPointer);
			selectedFormatDescription = formatChoose.formatDescription;
			selectedFormatTagDescription = formatChoose.formatTagDescription;
		}
		Marshal.FreeHGlobal(formatChoose.waveFormatEnumPointer);
		Marshal.FreeHGlobal(formatChoose.selectedWaveFormatPointer);
		if ((int)val != 515 && (int)val != 0)
		{
			throw new MmException(val, "acmFormatChoose");
		}
		return (int)val == 0;
	}

	public static AcmDriver FindByShortName(string shortName)
	{
		foreach (AcmDriver item in EnumerateAcmDrivers())
		{
			if (item.ShortName == shortName)
			{
				return item;
			}
		}
		return null;
	}

	public static IEnumerable<AcmDriver> EnumerateAcmDrivers()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		drivers = new List<AcmDriver>();
		MmException.Try(AcmInterop.acmDriverEnum(DriverEnumCallback, IntPtr.Zero, (AcmDriverEnumFlags)0), "acmDriverEnum");
		return drivers;
	}

	private static bool DriverEnumCallback(IntPtr hAcmDriver, IntPtr dwInstance, AcmDriverDetailsSupportFlags flags)
	{
		drivers.Add(new AcmDriver(hAcmDriver));
		return true;
	}

	private AcmDriver(IntPtr hAcmDriver)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		driverId = hAcmDriver;
		details = default(AcmDriverDetails);
		details.structureSize = Marshal.SizeOf(details);
		MmException.Try(AcmInterop.acmDriverDetails(hAcmDriver, ref details, 0), "acmDriverDetails");
	}

	public override string ToString()
	{
		return LongName;
	}

	public IEnumerable<AcmFormat> GetFormats(AcmFormatTag formatTag)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected I4, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (driverHandle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Driver must be opened first");
		}
		tempFormatsList = new List<AcmFormat>();
		AcmFormatDetails formatDetails = default(AcmFormatDetails);
		formatDetails.structSize = Marshal.SizeOf(formatDetails);
		formatDetails.waveFormatByteSize = 1024;
		formatDetails.waveFormatPointer = Marshal.AllocHGlobal(formatDetails.waveFormatByteSize);
		formatDetails.formatTag = (int)formatTag.FormatTag;
		MmResult val = AcmInterop.acmFormatEnum(driverHandle, ref formatDetails, AcmFormatEnumCallback, IntPtr.Zero, AcmFormatEnumFlags.None);
		Marshal.FreeHGlobal(formatDetails.waveFormatPointer);
		MmException.Try(val, "acmFormatEnum");
		return tempFormatsList;
	}

	public void Open()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (driverHandle == IntPtr.Zero)
		{
			MmException.Try(AcmInterop.acmDriverOpen(out driverHandle, DriverId, 0), "acmDriverOpen");
		}
	}

	public void Close()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (driverHandle != IntPtr.Zero)
		{
			MmException.Try(AcmInterop.acmDriverClose(driverHandle, 0), "acmDriverClose");
			driverHandle = IntPtr.Zero;
		}
	}

	private bool AcmFormatTagEnumCallback(IntPtr hAcmDriverId, ref AcmFormatTagDetails formatTagDetails, IntPtr dwInstance, AcmDriverDetailsSupportFlags flags)
	{
		formatTags.Add(new AcmFormatTag(formatTagDetails));
		return true;
	}

	private bool AcmFormatEnumCallback(IntPtr hAcmDriverId, ref AcmFormatDetails formatDetails, IntPtr dwInstance, AcmDriverDetailsSupportFlags flags)
	{
		tempFormatsList.Add(new AcmFormat(formatDetails));
		return true;
	}

	public void Dispose()
	{
		if (driverHandle != IntPtr.Zero)
		{
			Close();
			GC.SuppressFinalize(this);
		}
	}
}
