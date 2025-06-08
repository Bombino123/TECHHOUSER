using System.Collections.Generic;

namespace GMap.NET.MapProviders;

public class Error
{
	public int code { get; set; }

	public string message { get; set; }

	public string status { get; set; }

	public List<Detail> details { get; set; }
}
