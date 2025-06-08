using System;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class PartsList
{
	private IPartsList partsListInterface;

	public uint Count
	{
		get
		{
			uint count = 0u;
			if (partsListInterface != null)
			{
				partsListInterface.GetCount(out count);
			}
			return count;
		}
	}

	public Part this[uint index]
	{
		get
		{
			if (partsListInterface == null)
			{
				throw new IndexOutOfRangeException();
			}
			partsListInterface.GetPart(index, out var part);
			return new Part(part);
		}
	}

	internal PartsList(IPartsList partsList)
	{
		partsListInterface = partsList;
	}
}
