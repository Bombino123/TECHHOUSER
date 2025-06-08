using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace NAudio.Wave.Asio;

public class AsioDriver
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	private class AsioDriverVTable
	{
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate int ASIOInit(IntPtr _pUnknown, IntPtr sysHandle);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate void ASIOgetDriverName(IntPtr _pUnknown, StringBuilder name);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate int ASIOgetDriverVersion(IntPtr _pUnknown);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate void ASIOgetErrorMessage(IntPtr _pUnknown, StringBuilder errorMessage);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOstart(IntPtr _pUnknown);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOstop(IntPtr _pUnknown);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOgetChannels(IntPtr _pUnknown, out int numInputChannels, out int numOutputChannels);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOgetLatencies(IntPtr _pUnknown, out int inputLatency, out int outputLatency);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOgetBufferSize(IntPtr _pUnknown, out int minSize, out int maxSize, out int preferredSize, out int granularity);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOcanSampleRate(IntPtr _pUnknown, double sampleRate);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOgetSampleRate(IntPtr _pUnknown, out double sampleRate);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOsetSampleRate(IntPtr _pUnknown, double sampleRate);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOgetClockSources(IntPtr _pUnknown, out long clocks, int numSources);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOsetClockSource(IntPtr _pUnknown, int reference);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOgetSamplePosition(IntPtr _pUnknown, out long samplePos, ref Asio64Bit timeStamp);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOgetChannelInfo(IntPtr _pUnknown, ref AsioChannelInfo info);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOcreateBuffers(IntPtr _pUnknown, IntPtr bufferInfos, int numChannels, int bufferSize, IntPtr callbacks);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOdisposeBuffers(IntPtr _pUnknown);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOcontrolPanel(IntPtr _pUnknown);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOfuture(IntPtr _pUnknown, int selector, IntPtr opt);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate AsioError ASIOoutputReady(IntPtr _pUnknown);

		public ASIOInit init;

		public ASIOgetDriverName getDriverName;

		public ASIOgetDriverVersion getDriverVersion;

		public ASIOgetErrorMessage getErrorMessage;

		public ASIOstart start;

		public ASIOstop stop;

		public ASIOgetChannels getChannels;

		public ASIOgetLatencies getLatencies;

		public ASIOgetBufferSize getBufferSize;

		public ASIOcanSampleRate canSampleRate;

		public ASIOgetSampleRate getSampleRate;

		public ASIOsetSampleRate setSampleRate;

		public ASIOgetClockSources getClockSources;

		public ASIOsetClockSource setClockSource;

		public ASIOgetSamplePosition getSamplePosition;

		public ASIOgetChannelInfo getChannelInfo;

		public ASIOcreateBuffers createBuffers;

		public ASIOdisposeBuffers disposeBuffers;

		public ASIOcontrolPanel controlPanel;

		public ASIOfuture future;

		public ASIOoutputReady outputReady;
	}

	private IntPtr pAsioComObject;

	private IntPtr pinnedcallbacks;

	private AsioDriverVTable asioDriverVTable;

	private AsioDriver()
	{
	}

	public static string[] GetAsioDriverNames()
	{
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\ASIO");
		string[] result = new string[0];
		if (registryKey != null)
		{
			result = registryKey.GetSubKeyNames();
			registryKey.Close();
		}
		return result;
	}

	public static AsioDriver GetAsioDriverByName(string name)
	{
		return GetAsioDriverByGuid(new Guid((Registry.LocalMachine.OpenSubKey("SOFTWARE\\ASIO\\" + name) ?? throw new ArgumentException($"Driver Name {name} doesn't exist")).GetValue("CLSID").ToString()));
	}

	public static AsioDriver GetAsioDriverByGuid(Guid guid)
	{
		AsioDriver asioDriver = new AsioDriver();
		asioDriver.InitFromGuid(guid);
		return asioDriver;
	}

	public bool Init(IntPtr sysHandle)
	{
		return asioDriverVTable.init(pAsioComObject, sysHandle) == 1;
	}

	public string GetDriverName()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		asioDriverVTable.getDriverName(pAsioComObject, stringBuilder);
		return stringBuilder.ToString();
	}

	public int GetDriverVersion()
	{
		return asioDriverVTable.getDriverVersion(pAsioComObject);
	}

	public string GetErrorMessage()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		asioDriverVTable.getErrorMessage(pAsioComObject, stringBuilder);
		return stringBuilder.ToString();
	}

	public void Start()
	{
		HandleException(asioDriverVTable.start(pAsioComObject), "start");
	}

	public AsioError Stop()
	{
		return asioDriverVTable.stop(pAsioComObject);
	}

	public void GetChannels(out int numInputChannels, out int numOutputChannels)
	{
		HandleException(asioDriverVTable.getChannels(pAsioComObject, out numInputChannels, out numOutputChannels), "getChannels");
	}

	public AsioError GetLatencies(out int inputLatency, out int outputLatency)
	{
		return asioDriverVTable.getLatencies(pAsioComObject, out inputLatency, out outputLatency);
	}

	public void GetBufferSize(out int minSize, out int maxSize, out int preferredSize, out int granularity)
	{
		HandleException(asioDriverVTable.getBufferSize(pAsioComObject, out minSize, out maxSize, out preferredSize, out granularity), "getBufferSize");
	}

	public bool CanSampleRate(double sampleRate)
	{
		AsioError asioError = asioDriverVTable.canSampleRate(pAsioComObject, sampleRate);
		switch (asioError)
		{
		case AsioError.ASE_NoClock:
			return false;
		case AsioError.ASE_OK:
			return true;
		default:
			HandleException(asioError, "canSampleRate");
			return false;
		}
	}

	public double GetSampleRate()
	{
		HandleException(asioDriverVTable.getSampleRate(pAsioComObject, out var sampleRate), "getSampleRate");
		return sampleRate;
	}

	public void SetSampleRate(double sampleRate)
	{
		HandleException(asioDriverVTable.setSampleRate(pAsioComObject, sampleRate), "setSampleRate");
	}

	public void GetClockSources(out long clocks, int numSources)
	{
		HandleException(asioDriverVTable.getClockSources(pAsioComObject, out clocks, numSources), "getClockSources");
	}

	public void SetClockSource(int reference)
	{
		HandleException(asioDriverVTable.setClockSource(pAsioComObject, reference), "setClockSources");
	}

	public void GetSamplePosition(out long samplePos, ref Asio64Bit timeStamp)
	{
		HandleException(asioDriverVTable.getSamplePosition(pAsioComObject, out samplePos, ref timeStamp), "getSamplePosition");
	}

	public AsioChannelInfo GetChannelInfo(int channelNumber, bool trueForInputInfo)
	{
		AsioChannelInfo asioChannelInfo = default(AsioChannelInfo);
		asioChannelInfo.channel = channelNumber;
		asioChannelInfo.isInput = trueForInputInfo;
		AsioChannelInfo info = asioChannelInfo;
		HandleException(asioDriverVTable.getChannelInfo(pAsioComObject, ref info), "getChannelInfo");
		return info;
	}

	public void CreateBuffers(IntPtr bufferInfos, int numChannels, int bufferSize, ref AsioCallbacks callbacks)
	{
		pinnedcallbacks = Marshal.AllocHGlobal(Marshal.SizeOf((object)callbacks));
		Marshal.StructureToPtr((object)callbacks, pinnedcallbacks, fDeleteOld: false);
		HandleException(asioDriverVTable.createBuffers(pAsioComObject, bufferInfos, numChannels, bufferSize, pinnedcallbacks), "createBuffers");
	}

	public AsioError DisposeBuffers()
	{
		AsioError result = asioDriverVTable.disposeBuffers(pAsioComObject);
		Marshal.FreeHGlobal(pinnedcallbacks);
		return result;
	}

	public void ControlPanel()
	{
		HandleException(asioDriverVTable.controlPanel(pAsioComObject), "controlPanel");
	}

	public void Future(int selector, IntPtr opt)
	{
		HandleException(asioDriverVTable.future(pAsioComObject, selector, opt), "future");
	}

	public AsioError OutputReady()
	{
		return asioDriverVTable.outputReady(pAsioComObject);
	}

	public void ReleaseComAsioDriver()
	{
		Marshal.Release(pAsioComObject);
	}

	private void HandleException(AsioError error, string methodName)
	{
		if (error != 0 && error != AsioError.ASE_SUCCESS)
		{
			throw new AsioException($"Error code [{AsioException.getErrorName(error)}] while calling ASIO method <{methodName}>, {GetErrorMessage()}")
			{
				Error = error
			};
		}
	}

	private void InitFromGuid(Guid asioGuid)
	{
		int num = CoCreateInstance(ref asioGuid, IntPtr.Zero, 1u, ref asioGuid, out pAsioComObject);
		if (num != 0)
		{
			throw new COMException("Unable to instantiate ASIO. Check if STAThread is set", num);
		}
		IntPtr ptr = Marshal.ReadIntPtr(pAsioComObject);
		asioDriverVTable = new AsioDriverVTable();
		FieldInfo[] fields = typeof(AsioDriverVTable).GetFields();
		for (int i = 0; i < fields.Length; i++)
		{
			FieldInfo fieldInfo = fields[i];
			object delegateForFunctionPointer = Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(ptr, (i + 3) * IntPtr.Size), fieldInfo.FieldType);
			fieldInfo.SetValue(asioDriverVTable, delegateForFunctionPointer);
		}
	}

	[DllImport("ole32.Dll")]
	private static extern int CoCreateInstance(ref Guid clsid, IntPtr inner, uint context, ref Guid uuid, out IntPtr rReturnedComObject);
}
