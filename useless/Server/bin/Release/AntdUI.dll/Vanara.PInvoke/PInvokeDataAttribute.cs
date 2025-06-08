using System;

namespace Vanara.PInvoke;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = true, Inherited = false)]
public class PInvokeDataAttribute : Attribute
{
	public string Dll { get; set; }

	public string Header { get; set; }

	public PInvokeClient MinClient { get; set; }

	public string MSDNShortId { get; set; }

	public PInvokeDataAttribute(string header)
	{
		Header = header;
	}
}
