using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace DesktopDuplication;

public class DesktopDuplicator
{
	private SharpDX.Direct3D11.Device mDevice;

	private Texture2DDescription mTextureDesc;

	private OutputDescription mOutputDesc;

	private OutputDuplication mDeskDupl;

	private Texture2D desktopImageTexture;

	private OutputDuplicateFrameInformation frameInfo;

	private int mWhichOutputDevice = -1;

	private Bitmap finalImage1;

	private Bitmap finalImage2;

	private bool isFinalImage1;

	private Bitmap FinalImage
	{
		get
		{
			if (!isFinalImage1)
			{
				return finalImage2;
			}
			return finalImage1;
		}
		set
		{
			if (isFinalImage1)
			{
				finalImage2 = value;
				if (finalImage1 != null)
				{
					((Image)finalImage1).Dispose();
				}
			}
			else
			{
				finalImage1 = value;
				if (finalImage2 != null)
				{
					((Image)finalImage2).Dispose();
				}
			}
			isFinalImage1 = !isFinalImage1;
		}
	}

	public DesktopDuplicator(int whichMonitor)
		: this(0, whichMonitor)
	{
	}

	public DesktopDuplicator(int whichGraphicsCardAdapter, int whichOutputDevice)
	{
		mWhichOutputDevice = whichOutputDevice;
		Adapter1 adapter = null;
		try
		{
			adapter = new Factory1().GetAdapter1(whichGraphicsCardAdapter);
		}
		catch (SharpDXException)
		{
			throw new DesktopDuplicationException("Could not find the specified graphics card adapter.");
		}
		mDevice = new SharpDX.Direct3D11.Device(adapter);
		Output output = null;
		try
		{
			output = adapter.GetOutput(whichOutputDevice);
		}
		catch (SharpDXException)
		{
			throw new DesktopDuplicationException("Could not find the specified output device.");
		}
		Output1 output2 = output.QueryInterface<Output1>();
		mOutputDesc = output.Description;
		mTextureDesc = new Texture2DDescription
		{
			CpuAccessFlags = CpuAccessFlags.Read,
			BindFlags = BindFlags.None,
			Format = Format.B8G8R8A8_UNorm,
			Width = mOutputDesc.DesktopBounds.Right,
			Height = mOutputDesc.DesktopBounds.Bottom,
			OptionFlags = ResourceOptionFlags.None,
			MipLevels = 1,
			ArraySize = 1,
			SampleDescription = 
			{
				Count = 1,
				Quality = 0
			},
			Usage = ResourceUsage.Staging
		};
		try
		{
			mDeskDupl = output2.DuplicateOutput(mDevice);
		}
		catch (SharpDXException ex3)
		{
			if (ex3.ResultCode.Code == SharpDX.DXGI.ResultCode.NotCurrentlyAvailable.Result.Code)
			{
				throw new DesktopDuplicationException("There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.");
			}
		}
	}

	public void Close()
	{
		mDeskDupl?.Dispose();
		mDevice?.Dispose();
	}

	public DesktopFrame GetLatestFrame()
	{
		DesktopFrame desktopFrame = new DesktopFrame();
		if (RetrieveFrame())
		{
			return null;
		}
		try
		{
			RetrieveFrameMetadata(desktopFrame);
			RetrieveCursorMetadata(desktopFrame);
			ProcessFrame(desktopFrame);
		}
		catch
		{
			ReleaseFrame();
		}
		try
		{
			ReleaseFrame();
		}
		catch
		{
		}
		return desktopFrame;
	}

	private bool RetrieveFrame()
	{
		if (desktopImageTexture == null)
		{
			desktopImageTexture = new Texture2D(mDevice, mTextureDesc);
		}
		SharpDX.DXGI.Resource desktopResourceOut = null;
		frameInfo = default(OutputDuplicateFrameInformation);
		try
		{
			mDeskDupl.AcquireNextFrame(50, out frameInfo, out desktopResourceOut);
		}
		catch (SharpDXException ex)
		{
			if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
			{
				return true;
			}
			if (ex.ResultCode.Failure)
			{
				throw new DesktopDuplicationException("Failed to acquire next frame.");
			}
		}
		using (Texture2D source = desktopResourceOut.QueryInterface<Texture2D>())
		{
			mDevice.ImmediateContext.CopyResource(source, desktopImageTexture);
		}
		desktopResourceOut.Dispose();
		return false;
	}

	private void RetrieveFrameMetadata(DesktopFrame frame)
	{
		if (frameInfo.TotalMetadataBufferSize > 0)
		{
			int moveRectsBufferSizeRequiredRef = 0;
			OutputDuplicateMoveRectangle[] array = new OutputDuplicateMoveRectangle[frameInfo.TotalMetadataBufferSize];
			mDeskDupl.GetFrameMoveRects(array.Length, array, out moveRectsBufferSizeRequiredRef);
			frame.MovedRegions = new MovedRegion[moveRectsBufferSizeRequiredRef / Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle))];
			for (int i = 0; i < frame.MovedRegions.Length; i++)
			{
				frame.MovedRegions[i] = new MovedRegion
				{
					Source = new Point(array[i].SourcePoint.X, array[i].SourcePoint.Y),
					Destination = new Rectangle(array[i].DestinationRect.Left, array[i].DestinationRect.Top, array[i].DestinationRect.Right, array[i].DestinationRect.Bottom)
				};
			}
			int dirtyRectsBufferSizeRequiredRef = 0;
			RawRectangle[] array2 = new RawRectangle[frameInfo.TotalMetadataBufferSize];
			mDeskDupl.GetFrameDirtyRects(array2.Length, array2, out dirtyRectsBufferSizeRequiredRef);
			frame.UpdatedRegions = new Rectangle[dirtyRectsBufferSizeRequiredRef / Marshal.SizeOf(typeof(Rectangle))];
			for (int j = 0; j < frame.UpdatedRegions.Length; j++)
			{
				frame.UpdatedRegions[j] = new Rectangle(array2[j].Left, array2[j].Top, array2[j].Right, array2[j].Bottom);
			}
		}
		else
		{
			frame.MovedRegions = new MovedRegion[0];
			frame.UpdatedRegions = new Rectangle[0];
		}
	}

	private unsafe void RetrieveCursorMetadata(DesktopFrame frame)
	{
		PointerInfo pointerInfo = new PointerInfo();
		if (frameInfo.LastMouseUpdateTime == 0L)
		{
			return;
		}
		bool flag = true;
		if (!frameInfo.PointerPosition.Visible && pointerInfo.WhoUpdatedPositionLast != mWhichOutputDevice)
		{
			flag = false;
		}
		if ((bool)frameInfo.PointerPosition.Visible && pointerInfo.Visible && pointerInfo.WhoUpdatedPositionLast != mWhichOutputDevice && pointerInfo.LastTimeStamp > frameInfo.LastMouseUpdateTime)
		{
			flag = false;
		}
		if (flag)
		{
			pointerInfo.Position = new Point(frameInfo.PointerPosition.Position.X, frameInfo.PointerPosition.Position.Y);
			pointerInfo.WhoUpdatedPositionLast = mWhichOutputDevice;
			pointerInfo.LastTimeStamp = frameInfo.LastMouseUpdateTime;
			pointerInfo.Visible = frameInfo.PointerPosition.Visible;
		}
		if (frameInfo.PointerShapeBufferSize == 0)
		{
			return;
		}
		if (frameInfo.PointerShapeBufferSize > pointerInfo.BufferSize)
		{
			pointerInfo.PtrShapeBuffer = new byte[frameInfo.PointerShapeBufferSize];
			pointerInfo.BufferSize = frameInfo.PointerShapeBufferSize;
		}
		try
		{
			fixed (byte* ptr = pointerInfo.PtrShapeBuffer)
			{
				mDeskDupl.GetFramePointerShape(frameInfo.PointerShapeBufferSize, (IntPtr)ptr, out pointerInfo.BufferSize, out pointerInfo.ShapeInfo);
			}
		}
		catch (SharpDXException ex)
		{
			if (ex.ResultCode.Failure)
			{
				throw new DesktopDuplicationException("Failed to get frame pointer shape.");
			}
		}
		frame.CursorLocation = new Point(pointerInfo.Position.X, pointerInfo.Position.Y);
	}

	private void ProcessFrame(DesktopFrame frame)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		DataBox dataBox = mDevice.ImmediateContext.MapSubresource(desktopImageTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
		FinalImage = new Bitmap(mOutputDesc.DesktopBounds.Right, mOutputDesc.DesktopBounds.Bottom, (PixelFormat)139273);
		Rectangle rectangle = new Rectangle(0, 0, mOutputDesc.DesktopBounds.Right, mOutputDesc.DesktopBounds.Bottom);
		BitmapData val = FinalImage.LockBits(rectangle, (ImageLockMode)2, ((Image)FinalImage).PixelFormat);
		IntPtr intPtr = dataBox.DataPointer;
		IntPtr intPtr2 = val.Scan0;
		for (int i = 0; i < mOutputDesc.DesktopBounds.Bottom; i++)
		{
			Utilities.CopyMemory(intPtr2, intPtr, mOutputDesc.DesktopBounds.Right * 4);
			intPtr = IntPtr.Add(intPtr, dataBox.RowPitch);
			intPtr2 = IntPtr.Add(intPtr2, val.Stride);
		}
		FinalImage.UnlockBits(val);
		mDevice.ImmediateContext.UnmapSubresource(desktopImageTexture, 0);
		frame.DesktopImage = FinalImage;
	}

	private void ReleaseFrame()
	{
		try
		{
			mDeskDupl.ReleaseFrame();
		}
		catch (SharpDXException ex)
		{
			if (ex.ResultCode.Failure)
			{
				throw new DesktopDuplicationException("Failed to release frame.");
			}
		}
	}
}
