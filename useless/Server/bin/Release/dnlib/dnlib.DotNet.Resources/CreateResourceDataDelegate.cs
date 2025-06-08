namespace dnlib.DotNet.Resources;

public delegate IResourceData CreateResourceDataDelegate(ResourceDataFactory resourceDataFactory, UserResourceType type, byte[] serializedData, SerializationFormat format);
