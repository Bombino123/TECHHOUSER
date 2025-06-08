using System;
using System.Collections.Generic;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

public sealed class MethodBodyChunks : IChunk
{
	private readonly struct ReusedMethodInfo
	{
		public readonly MethodBody MethodBody;

		public readonly RVA RVA;

		public ReusedMethodInfo(MethodBody methodBody, RVA rva)
		{
			MethodBody = methodBody;
			RVA = rva;
		}
	}

	private const uint FAT_BODY_ALIGNMENT = 4u;

	private Dictionary<MethodBody, MethodBody> tinyMethodsDict;

	private Dictionary<MethodBody, MethodBody> fatMethodsDict;

	private readonly List<MethodBody> tinyMethods;

	private readonly List<MethodBody> fatMethods;

	private readonly List<ReusedMethodInfo> reusedMethods;

	private readonly Dictionary<uint, MethodBody> rvaToReusedMethod;

	private readonly bool shareBodies;

	private FileOffset offset;

	private RVA rva;

	private uint length;

	private bool setOffsetCalled;

	private readonly bool alignFatBodies;

	private uint savedBytes;

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public uint SavedBytes => savedBytes;

	internal bool CanReuseOldBodyLocation { get; set; }

	internal bool ReusedAllMethodBodyLocations
	{
		get
		{
			if (tinyMethods.Count == 0)
			{
				return fatMethods.Count == 0;
			}
			return false;
		}
	}

	internal bool HasReusedMethods => reusedMethods.Count > 0;

	public MethodBodyChunks(bool shareBodies)
	{
		this.shareBodies = shareBodies;
		alignFatBodies = true;
		if (shareBodies)
		{
			tinyMethodsDict = new Dictionary<MethodBody, MethodBody>();
			fatMethodsDict = new Dictionary<MethodBody, MethodBody>();
		}
		tinyMethods = new List<MethodBody>();
		fatMethods = new List<MethodBody>();
		reusedMethods = new List<ReusedMethodInfo>();
		rvaToReusedMethod = new Dictionary<uint, MethodBody>();
	}

	public MethodBody Add(MethodBody methodBody)
	{
		return Add(methodBody, (RVA)0u, 0u);
	}

	internal MethodBody Add(MethodBody methodBody, RVA origRva, uint origSize)
	{
		if (setOffsetCalled)
		{
			throw new InvalidOperationException("SetOffset() has already been called");
		}
		if (CanReuseOldBodyLocation && origRva != 0 && origSize != 0 && methodBody.CanReuse(origRva, origSize))
		{
			if (!rvaToReusedMethod.TryGetValue((uint)origRva, out var value))
			{
				rvaToReusedMethod.Add((uint)origRva, methodBody);
				reusedMethods.Add(new ReusedMethodInfo(methodBody, origRva));
				return methodBody;
			}
			if (methodBody.Equals(value))
			{
				return value;
			}
		}
		if (shareBodies)
		{
			Dictionary<MethodBody, MethodBody> dictionary = (methodBody.IsFat ? fatMethodsDict : tinyMethodsDict);
			if (dictionary.TryGetValue(methodBody, out var value2))
			{
				savedBytes += (uint)methodBody.GetApproximateSizeOfMethodBody();
				return value2;
			}
			dictionary[methodBody] = methodBody;
		}
		(methodBody.IsFat ? fatMethods : tinyMethods).Add(methodBody);
		return methodBody;
	}

	public bool Remove(MethodBody methodBody)
	{
		if (methodBody == null)
		{
			throw new ArgumentNullException("methodBody");
		}
		if (setOffsetCalled)
		{
			throw new InvalidOperationException("SetOffset() has already been called");
		}
		if (CanReuseOldBodyLocation)
		{
			throw new InvalidOperationException("Reusing old body locations is enabled. Can't remove bodies.");
		}
		return (methodBody.IsFat ? fatMethods : tinyMethods).Remove(methodBody);
	}

	internal void InitializeReusedMethodBodies(Func<RVA, FileOffset> getNewFileOffset)
	{
		foreach (ReusedMethodInfo reusedMethod in reusedMethods)
		{
			FileOffset fileOffset = getNewFileOffset(reusedMethod.RVA);
			reusedMethod.MethodBody.SetOffset(fileOffset, reusedMethod.RVA);
		}
	}

	internal void WriteReusedMethodBodies(DataWriter writer, long destStreamBaseOffset)
	{
		foreach (ReusedMethodInfo reusedMethod in reusedMethods)
		{
			if (reusedMethod.MethodBody.RVA != reusedMethod.RVA)
			{
				throw new InvalidOperationException();
			}
			writer.Position = destStreamBaseOffset + (long)reusedMethod.MethodBody.FileOffset;
			reusedMethod.MethodBody.VerifyWriteTo(writer);
		}
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		setOffsetCalled = true;
		this.offset = offset;
		this.rva = rva;
		tinyMethodsDict = null;
		fatMethodsDict = null;
		RVA rVA = rva;
		foreach (MethodBody tinyMethod in tinyMethods)
		{
			tinyMethod.SetOffset(offset, rVA);
			uint fileLength = tinyMethod.GetFileLength();
			rVA += fileLength;
			offset += fileLength;
		}
		foreach (MethodBody fatMethod in fatMethods)
		{
			if (alignFatBodies)
			{
				uint num = rVA.AlignUp(4u) - rVA;
				rVA += num;
				offset += num;
			}
			fatMethod.SetOffset(offset, rVA);
			uint fileLength2 = fatMethod.GetFileLength();
			rVA += fileLength2;
			offset += fileLength2;
		}
		length = rVA - rva;
	}

	public uint GetFileLength()
	{
		return length;
	}

	public uint GetVirtualSize()
	{
		return GetFileLength();
	}

	public uint CalculateAlignment()
	{
		return 0u;
	}

	public void WriteTo(DataWriter writer)
	{
		RVA rVA = rva;
		foreach (MethodBody tinyMethod in tinyMethods)
		{
			tinyMethod.VerifyWriteTo(writer);
			rVA += tinyMethod.GetFileLength();
		}
		foreach (MethodBody fatMethod in fatMethods)
		{
			if (alignFatBodies)
			{
				int num = (int)(rVA.AlignUp(4u) - rVA);
				writer.WriteZeroes(num);
				rVA = (RVA)((uint)rVA + (uint)num);
			}
			fatMethod.VerifyWriteTo(writer);
			rVA += fatMethod.GetFileLength();
		}
	}
}
