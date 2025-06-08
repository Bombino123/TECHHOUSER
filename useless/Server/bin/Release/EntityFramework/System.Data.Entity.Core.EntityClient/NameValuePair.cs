using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.EntityClient;

internal sealed class NameValuePair
{
	private NameValuePair _next;

	internal NameValuePair Next
	{
		get
		{
			return _next;
		}
		set
		{
			if (_next != null || value == null)
			{
				throw new InvalidOperationException(Strings.ADP_InternalProviderError(1014));
			}
			_next = value;
		}
	}
}
