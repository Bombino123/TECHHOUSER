namespace SharpDX.Direct3D11;

public static class ResultCode
{
	public static readonly ResultDescriptor TooManyUniqueStateObjects = new ResultDescriptor(-2005139455, "SharpDX.Direct3D11", "D3D11_ERROR_TOO_MANY_UNIQUE_STATE_OBJECTS", "TooManyUniqueStateObjects");

	public static readonly ResultDescriptor FileNotFound = new ResultDescriptor(-2005139454, "SharpDX.Direct3D11", "D3D11_ERROR_FILE_NOT_FOUND", "FileNotFound");

	public static readonly ResultDescriptor TooManyUniqueViewObjects = new ResultDescriptor(-2005139453, "SharpDX.Direct3D11", "D3D11_ERROR_TOO_MANY_UNIQUE_VIEW_OBJECTS", "TooManyUniqueViewObjects");

	public static readonly ResultDescriptor DeferredContextMapWithoutInitialDiscard = new ResultDescriptor(-2005139452, "SharpDX.Direct3D11", "D3D11_ERROR_DEFERRED_CONTEXT_MAP_WITHOUT_INITIAL_DISCARD", "DeferredContextMapWithoutInitialDiscard");
}
