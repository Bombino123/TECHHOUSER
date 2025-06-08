namespace SharpDX.Direct3D11;

public struct VertexBufferBinding
{
	private Buffer m_Buffer;

	private int m_Stride;

	private int m_Offset;

	public Buffer Buffer
	{
		get
		{
			return m_Buffer;
		}
		set
		{
			m_Buffer = value;
		}
	}

	public int Stride
	{
		get
		{
			return m_Stride;
		}
		set
		{
			m_Stride = value;
		}
	}

	public int Offset
	{
		get
		{
			return m_Offset;
		}
		set
		{
			m_Offset = value;
		}
	}

	public VertexBufferBinding(Buffer buffer, int stride, int offset)
	{
		m_Buffer = buffer;
		m_Stride = stride;
		m_Offset = offset;
	}
}
