namespace SharpDX.Direct3D11;

public struct StreamOutputBufferBinding
{
	private Buffer _buffer;

	private int _offset;

	public Buffer Buffer
	{
		get
		{
			return _buffer;
		}
		set
		{
			_buffer = value;
		}
	}

	public int Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = value;
		}
	}

	public StreamOutputBufferBinding(Buffer buffer, int offset)
	{
		_buffer = buffer;
		_offset = offset;
	}
}
