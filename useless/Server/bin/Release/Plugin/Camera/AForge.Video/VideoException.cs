using System;

namespace AForge.Video;

public class VideoException : Exception
{
	public VideoException(string message)
		: base(message)
	{
	}
}
