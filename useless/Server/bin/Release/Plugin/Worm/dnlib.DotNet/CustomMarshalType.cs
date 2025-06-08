using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class CustomMarshalType : MarshalType
{
	private UTF8String guid;

	private UTF8String nativeTypeName;

	private ITypeDefOrRef custMarshaler;

	private UTF8String cookie;

	public UTF8String Guid
	{
		get
		{
			return guid;
		}
		set
		{
			guid = value;
		}
	}

	public UTF8String NativeTypeName
	{
		get
		{
			return nativeTypeName;
		}
		set
		{
			nativeTypeName = value;
		}
	}

	public ITypeDefOrRef CustomMarshaler
	{
		get
		{
			return custMarshaler;
		}
		set
		{
			custMarshaler = value;
		}
	}

	public UTF8String Cookie
	{
		get
		{
			return cookie;
		}
		set
		{
			cookie = value;
		}
	}

	public CustomMarshalType()
		: this(null, null, null, null)
	{
	}

	public CustomMarshalType(UTF8String guid)
		: this(guid, null, null, null)
	{
	}

	public CustomMarshalType(UTF8String guid, UTF8String nativeTypeName)
		: this(guid, nativeTypeName, null, null)
	{
	}

	public CustomMarshalType(UTF8String guid, UTF8String nativeTypeName, ITypeDefOrRef custMarshaler)
		: this(guid, nativeTypeName, custMarshaler, null)
	{
	}

	public CustomMarshalType(UTF8String guid, UTF8String nativeTypeName, ITypeDefOrRef custMarshaler, UTF8String cookie)
		: base(NativeType.CustomMarshaler)
	{
		this.guid = guid;
		this.nativeTypeName = nativeTypeName;
		this.custMarshaler = custMarshaler;
		this.cookie = cookie;
	}

	public override string ToString()
	{
		return $"{nativeType} ({guid}, {nativeTypeName}, {custMarshaler}, {cookie})";
	}
}
