using System.Collections.Generic;

namespace GMap.NET.MapProviders;

public class AddressComponent
{
	public string long_name { get; set; }

	public string short_name { get; set; }

	public List<string> types { get; set; }
}
