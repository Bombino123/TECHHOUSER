namespace NAudio.CoreAudioApi.Interfaces;

public static class AudioClientErrorCode
{
	public const int NotInitialized = -2004287487;

	public const int AlreadyInitialized = -2004287486;

	public const int WrongEndpointType = -2004287485;

	public const int DeviceInvalidated = -2004287484;

	public const int NotStopped = -2004287483;

	public const int BufferTooLarge = -2004287482;

	public const int OutOfOrder = -2004287481;

	public const int UnsupportedFormat = -2004287480;

	public const int InvalidSize = -2004287479;

	public const int DeviceInUse = -2004287478;

	public const int BufferOperationPending = -2004287477;

	public const int ThreadNotRegistered = -2004287476;

	public const int NoSingleProcess = -2004287475;

	public const int ExclusiveModeNotAllowed = -2004287474;

	public const int EndpointCreateFailed = -2004287473;

	public const int ServiceNotRunning = -2004287472;

	public const int EventHandleNotExpected = -2004287471;

	public const int ExclusiveModeOnly = -2004287470;

	public const int BufferDurationPeriodNotEqual = -2004287469;

	public const int EventHandleNotSet = -2004287468;

	public const int IncorrectBufferSize = -2004287467;

	public const int BufferSizeError = -2004287466;

	public const int CpuUsageExceeded = -2004287465;

	public const int BufferError = -2004287464;

	public const int BufferSizeNotAligned = -2004287463;

	public const int InvalidDevicePeriod = -2004287456;

	public const int InvalidStreamFlag = -2004287455;

	public const int EndpointOffloadNotCapable = -2004287454;

	public const int OutOfOffloadResources = -2004287453;

	public const int OffloadModeOnly = -2004287452;

	public const int NonOffloadModeOnly = -2004287451;

	public const int ResourcesInvalidated = -2004287450;

	public const int RawModeUnsupported = -2004287449;

	public const int EnginePeriodicityLocked = -2004287448;

	public const int EngineFormatLocked = -2004287447;

	public const int HeadTrackingEnabled = -2004287440;

	public const int HeadTrackingUnsupported = -2004287424;
}
