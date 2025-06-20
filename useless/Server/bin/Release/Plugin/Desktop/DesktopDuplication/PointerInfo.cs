using System.Drawing;
using SharpDX.DXGI;

namespace DesktopDuplication;

internal class PointerInfo
{
	public byte[] PtrShapeBuffer;

	public OutputDuplicatePointerShapeInformation ShapeInfo;

	public Point Position;

	public bool Visible;

	public int BufferSize;

	public int WhoUpdatedPositionLast;

	public long LastTimeStamp;
}
