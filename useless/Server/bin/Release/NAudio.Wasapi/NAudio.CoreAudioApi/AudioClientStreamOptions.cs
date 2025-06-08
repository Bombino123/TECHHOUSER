using System;

namespace NAudio.CoreAudioApi;

[Flags]
public enum AudioClientStreamOptions
{
	None = 0,
	Raw = 1,
	MatchFormat = 2,
	Ambisonics = 4
}
