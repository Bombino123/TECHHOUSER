using System.Runtime.InteropServices;

namespace NAudio.CoreAudioApi.Interfaces;

[ComImport]
[Guid("7FB7B48F-531D-44A2-BCB3-5AD5A134B3DC")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioVolumeLevel : IPerChannelDbLevel
{
}
