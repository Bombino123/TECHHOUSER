using System;
using System.IO;

namespace GMap.NET;

public abstract class PureImage : IDisposable
{
	public MemoryStream Data;

	internal bool IsParent;

	internal long Ix;

	internal long Xoff;

	internal long Yoff;

	public abstract void Dispose();
}
