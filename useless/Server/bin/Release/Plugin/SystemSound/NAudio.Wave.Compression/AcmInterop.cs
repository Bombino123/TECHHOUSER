using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave.Compression;

internal class AcmInterop
{
	public delegate bool AcmDriverEnumCallback(IntPtr hAcmDriverId, IntPtr instance, AcmDriverDetailsSupportFlags flags);

	public delegate bool AcmFormatEnumCallback(IntPtr hAcmDriverId, ref AcmFormatDetails formatDetails, IntPtr dwInstance, AcmDriverDetailsSupportFlags flags);

	public delegate bool AcmFormatTagEnumCallback(IntPtr hAcmDriverId, ref AcmFormatTagDetails formatTagDetails, IntPtr dwInstance, AcmDriverDetailsSupportFlags flags);

	public delegate bool AcmFormatChooseHookProc(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmDriverAdd(out IntPtr driverHandle, IntPtr driverModule, IntPtr driverFunctionAddress, int priority, AcmDriverAddFlags flags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmDriverRemove(IntPtr driverHandle, int removeFlags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmDriverClose(IntPtr hAcmDriver, int closeFlags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmDriverEnum(AcmDriverEnumCallback fnCallback, IntPtr dwInstance, AcmDriverEnumFlags flags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmDriverDetails(IntPtr hAcmDriver, ref AcmDriverDetails driverDetails, int reserved);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmDriverOpen(out IntPtr pAcmDriver, IntPtr hAcmDriverId, int openFlags);

	[DllImport("msacm32.dll", EntryPoint = "acmFormatChooseW")]
	public static extern MmResult acmFormatChoose(ref AcmFormatChoose formatChoose);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmFormatEnum(IntPtr hAcmDriver, ref AcmFormatDetails formatDetails, AcmFormatEnumCallback callback, IntPtr instance, AcmFormatEnumFlags flags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmFormatSuggest(IntPtr hAcmDriver, [In][MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "NAudio.Wave.WaveFormatCustomMarshaler")] WaveFormat sourceFormat, [In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "NAudio.Wave.WaveFormatCustomMarshaler")] WaveFormat destFormat, int sizeDestFormat, AcmFormatSuggestFlags suggestFlags);

	[DllImport("msacm32.dll", EntryPoint = "acmFormatSuggest")]
	public static extern MmResult acmFormatSuggest2(IntPtr hAcmDriver, IntPtr sourceFormatPointer, IntPtr destFormatPointer, int sizeDestFormat, AcmFormatSuggestFlags suggestFlags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmFormatTagEnum(IntPtr hAcmDriver, ref AcmFormatTagDetails formatTagDetails, AcmFormatTagEnumCallback callback, IntPtr instance, int reserved);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmMetrics(IntPtr hAcmObject, AcmMetrics metric, out int output);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmStreamOpen(out IntPtr hAcmStream, IntPtr hAcmDriver, [In][MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "NAudio.Wave.WaveFormatCustomMarshaler")] WaveFormat sourceFormat, [In][MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "NAudio.Wave.WaveFormatCustomMarshaler")] WaveFormat destFormat, [In] WaveFilter waveFilter, IntPtr callback, IntPtr instance, AcmStreamOpenFlags openFlags);

	[DllImport("msacm32.dll", EntryPoint = "acmStreamOpen")]
	public static extern MmResult acmStreamOpen2(out IntPtr hAcmStream, IntPtr hAcmDriver, IntPtr sourceFormatPointer, IntPtr destFormatPointer, [In] WaveFilter waveFilter, IntPtr callback, IntPtr instance, AcmStreamOpenFlags openFlags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmStreamClose(IntPtr hAcmStream, int closeFlags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmStreamConvert(IntPtr hAcmStream, [In][Out] AcmStreamHeaderStruct streamHeader, AcmStreamConvertFlags streamConvertFlags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmStreamPrepareHeader(IntPtr hAcmStream, [In][Out] AcmStreamHeaderStruct streamHeader, int prepareFlags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmStreamReset(IntPtr hAcmStream, int resetFlags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmStreamSize(IntPtr hAcmStream, int inputBufferSize, out int outputBufferSize, AcmStreamSizeFlags flags);

	[DllImport("msacm32.dll")]
	public static extern MmResult acmStreamUnprepareHeader(IntPtr hAcmStream, [In][Out] AcmStreamHeaderStruct streamHeader, int flags);
}
