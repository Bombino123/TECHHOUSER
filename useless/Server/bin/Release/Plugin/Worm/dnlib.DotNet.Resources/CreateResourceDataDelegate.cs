using System.Runtime.InteropServices;

namespace dnlib.DotNet.Resources;

[ComVisible(true)]
public delegate IResourceData CreateResourceDataDelegate(ResourceDataFactory resourceDataFactory, UserResourceType type, byte[] serializedData, SerializationFormat format);
