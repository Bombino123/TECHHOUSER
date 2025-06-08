using System;
using System.IO;
using System.Media;
using System.Text;

namespace Plugin.Handler;

internal class HandlePlayAudio
{
	private const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

	public static Random Random = new Random();

	public void Play(byte[] wavfile)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		string text = Path.Combine(Path.GetTempPath(), GetRandomString(6) + ".wav");
		using (FileStream fileStream = new FileStream(text, FileMode.Create))
		{
			fileStream.Write(wavfile, 0, wavfile.Length);
		}
		SoundPlayer val = new SoundPlayer(text);
		val.Load();
		val.Play();
	}

	public static string GetRandomString(int length)
	{
		StringBuilder stringBuilder = new StringBuilder(length);
		for (int i = 0; i < length; i++)
		{
			stringBuilder.Append("abcdefghijklmnopqrstuvwxyz"[Random.Next("abcdefghijklmnopqrstuvwxyz".Length)]);
		}
		return stringBuilder.ToString();
	}
}
