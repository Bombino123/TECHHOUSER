using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NAudio.Utils;

namespace NAudio.Wave;

public class CueList
{
	private readonly List<Cue> cues = new List<Cue>();

	public int[] CuePositions
	{
		get
		{
			int[] array = new int[cues.Count];
			for (int i = 0; i < cues.Count; i++)
			{
				array[i] = cues[i].Position;
			}
			return array;
		}
	}

	public string[] CueLabels
	{
		get
		{
			string[] array = new string[cues.Count];
			for (int i = 0; i < cues.Count; i++)
			{
				array[i] = cues[i].Label;
			}
			return array;
		}
	}

	public int Count => cues.Count;

	public Cue this[int index] => cues[index];

	public CueList()
	{
	}

	public void Add(Cue cue)
	{
		cues.Add(cue);
	}

	internal CueList(byte[] cueChunkData, byte[] listChunkData)
	{
		int num = BitConverter.ToInt32(cueChunkData, 0);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int[] array = new int[num];
		int num2 = 0;
		int num3 = 4;
		while (cueChunkData.Length - num3 >= 24)
		{
			dictionary[BitConverter.ToInt32(cueChunkData, num3)] = num2;
			array[num2] = BitConverter.ToInt32(cueChunkData, num3 + 20);
			num3 += 24;
			num2++;
		}
		string[] array2 = new string[num];
		int num4 = 0;
		int num5 = ChunkIdentifier.ChunkIdentifierToInt32("labl");
		for (int i = 4; listChunkData.Length - i >= 16; i += num4 + num4 % 2 + 12)
		{
			if (BitConverter.ToInt32(listChunkData, i) == num5)
			{
				num4 = BitConverter.ToInt32(listChunkData, i + 4) - 4;
				int key = BitConverter.ToInt32(listChunkData, i + 8);
				num2 = dictionary[key];
				array2[num2] = Encoding.UTF8.GetString(listChunkData, i + 12, num4 - 1);
			}
		}
		for (int j = 0; j < num; j++)
		{
			cues.Add(new Cue(array[j], array2[j]));
		}
	}

	internal byte[] GetRiffChunks()
	{
		if (Count == 0)
		{
			return null;
		}
		int num = 12 + 24 * Count;
		int num2 = 12;
		for (int i = 0; i < Count; i++)
		{
			int num3 = Encoding.UTF8.GetBytes(this[i].Label).Length + 1;
			num2 += num3 + num3 % 2 + 12;
		}
		byte[] array = new byte[num + num2];
		int value = ChunkIdentifier.ChunkIdentifierToInt32("cue ");
		int value2 = ChunkIdentifier.ChunkIdentifierToInt32("data");
		int value3 = ChunkIdentifier.ChunkIdentifierToInt32("LIST");
		int value4 = ChunkIdentifier.ChunkIdentifierToInt32("adtl");
		int value5 = ChunkIdentifier.ChunkIdentifierToInt32("labl");
		using MemoryStream output = new MemoryStream(array);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.Write(value);
		binaryWriter.Write(num - 8);
		binaryWriter.Write(Count);
		for (int j = 0; j < Count; j++)
		{
			int position = this[j].Position;
			binaryWriter.Write(j);
			binaryWriter.Write(position);
			binaryWriter.Write(value2);
			binaryWriter.Seek(8, SeekOrigin.Current);
			binaryWriter.Write(position);
		}
		binaryWriter.Write(value3);
		binaryWriter.Write(num2 - 8);
		binaryWriter.Write(value4);
		for (int k = 0; k < Count; k++)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(this[k].Label);
			binaryWriter.Write(value5);
			binaryWriter.Write(bytes.Length + 1 + 4);
			binaryWriter.Write(k);
			binaryWriter.Write(bytes);
			if (bytes.Length % 2 == 0)
			{
				binaryWriter.Seek(2, SeekOrigin.Current);
			}
			else
			{
				binaryWriter.Seek(1, SeekOrigin.Current);
			}
		}
		return array;
	}

	internal static CueList FromChunks(WaveFileReader reader)
	{
		CueList result = null;
		byte[] array = null;
		byte[] array2 = null;
		foreach (RiffChunk extraChunk in reader.ExtraChunks)
		{
			if (extraChunk.IdentifierAsString.ToLower() == "cue ")
			{
				array = reader.GetChunkData(extraChunk);
			}
			else if (extraChunk.IdentifierAsString.ToLower() == "list")
			{
				array2 = reader.GetChunkData(extraChunk);
			}
		}
		if (array != null && array2 != null)
		{
			result = new CueList(array, array2);
		}
		return result;
	}
}
