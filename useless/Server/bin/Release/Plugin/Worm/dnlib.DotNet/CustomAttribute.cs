using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class CustomAttribute : ICustomAttribute
{
	private ICustomAttributeType ctor;

	private byte[] rawData;

	private readonly IList<CAArgument> arguments;

	private readonly IList<CANamedArgument> namedArguments;

	private uint caBlobOffset;

	public ICustomAttributeType Constructor
	{
		get
		{
			return ctor;
		}
		set
		{
			ctor = value;
		}
	}

	public ITypeDefOrRef AttributeType => ctor?.DeclaringType;

	public string TypeFullName
	{
		get
		{
			if (ctor is MemberRef memberRef)
			{
				return memberRef.GetDeclaringTypeFullName() ?? string.Empty;
			}
			if (ctor is MethodDef { DeclaringType: { } declaringType })
			{
				return declaringType.FullName;
			}
			return string.Empty;
		}
	}

	internal string TypeName
	{
		get
		{
			if (ctor is MemberRef memberRef)
			{
				return memberRef.GetDeclaringTypeName() ?? string.Empty;
			}
			if (ctor is MethodDef { DeclaringType: { } declaringType })
			{
				return declaringType.Name;
			}
			return string.Empty;
		}
	}

	public bool IsRawBlob => rawData != null;

	public byte[] RawData => rawData;

	public IList<CAArgument> ConstructorArguments => arguments;

	public bool HasConstructorArguments => arguments.Count > 0;

	public IList<CANamedArgument> NamedArguments => namedArguments;

	public bool HasNamedArguments => namedArguments.Count > 0;

	public IEnumerable<CANamedArgument> Fields
	{
		get
		{
			IList<CANamedArgument> namedArguments = this.namedArguments;
			int count = namedArguments.Count;
			for (int i = 0; i < count; i++)
			{
				CANamedArgument cANamedArgument = namedArguments[i];
				if (cANamedArgument.IsField)
				{
					yield return cANamedArgument;
				}
			}
		}
	}

	public IEnumerable<CANamedArgument> Properties
	{
		get
		{
			IList<CANamedArgument> namedArguments = this.namedArguments;
			int count = namedArguments.Count;
			for (int i = 0; i < count; i++)
			{
				CANamedArgument cANamedArgument = namedArguments[i];
				if (cANamedArgument.IsProperty)
				{
					yield return cANamedArgument;
				}
			}
		}
	}

	public uint BlobOffset => caBlobOffset;

	public CustomAttribute(ICustomAttributeType ctor, byte[] rawData)
		: this(ctor, null, null, 0u)
	{
		this.rawData = rawData;
	}

	public CustomAttribute(ICustomAttributeType ctor)
		: this(ctor, null, null, 0u)
	{
	}

	public CustomAttribute(ICustomAttributeType ctor, IEnumerable<CAArgument> arguments)
		: this(ctor, arguments, null)
	{
	}

	public CustomAttribute(ICustomAttributeType ctor, IEnumerable<CANamedArgument> namedArguments)
		: this(ctor, null, namedArguments)
	{
	}

	public CustomAttribute(ICustomAttributeType ctor, IEnumerable<CAArgument> arguments, IEnumerable<CANamedArgument> namedArguments)
		: this(ctor, arguments, namedArguments, 0u)
	{
	}

	public CustomAttribute(ICustomAttributeType ctor, IEnumerable<CAArgument> arguments, IEnumerable<CANamedArgument> namedArguments, uint caBlobOffset)
	{
		this.ctor = ctor;
		this.arguments = ((arguments == null) ? new List<CAArgument>() : new List<CAArgument>(arguments));
		this.namedArguments = ((namedArguments == null) ? new List<CANamedArgument>() : new List<CANamedArgument>(namedArguments));
		this.caBlobOffset = caBlobOffset;
	}

	internal CustomAttribute(ICustomAttributeType ctor, List<CAArgument> arguments, List<CANamedArgument> namedArguments, uint caBlobOffset)
	{
		this.ctor = ctor;
		this.arguments = arguments ?? new List<CAArgument>();
		this.namedArguments = namedArguments ?? new List<CANamedArgument>();
		this.caBlobOffset = caBlobOffset;
	}

	public CANamedArgument GetField(string name)
	{
		return GetNamedArgument(name, isField: true);
	}

	public CANamedArgument GetField(UTF8String name)
	{
		return GetNamedArgument(name, isField: true);
	}

	public CANamedArgument GetProperty(string name)
	{
		return GetNamedArgument(name, isField: false);
	}

	public CANamedArgument GetProperty(UTF8String name)
	{
		return GetNamedArgument(name, isField: false);
	}

	public CANamedArgument GetNamedArgument(string name, bool isField)
	{
		IList<CANamedArgument> list = namedArguments;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			CANamedArgument cANamedArgument = list[i];
			if (cANamedArgument.IsField == isField && UTF8String.ToSystemStringOrEmpty(cANamedArgument.Name) == name)
			{
				return cANamedArgument;
			}
		}
		return null;
	}

	public CANamedArgument GetNamedArgument(UTF8String name, bool isField)
	{
		IList<CANamedArgument> list = namedArguments;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			CANamedArgument cANamedArgument = list[i];
			if (cANamedArgument.IsField == isField && UTF8String.Equals(cANamedArgument.Name, name))
			{
				return cANamedArgument;
			}
		}
		return null;
	}

	public override string ToString()
	{
		return TypeFullName;
	}
}
