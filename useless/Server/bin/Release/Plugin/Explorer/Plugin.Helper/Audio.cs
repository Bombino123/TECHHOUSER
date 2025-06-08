using System;
using System.Windows.Media;

namespace Plugin.Helper;

internal class Audio
{
	public static void Play(string filePath)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		MediaPlayer val = new MediaPlayer();
		val.Open(new Uri(filePath, UriKind.RelativeOrAbsolute));
		val.Play();
	}
}
