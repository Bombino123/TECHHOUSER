using System.Collections.Generic;
using Newtonsoft.Json;

namespace GMap.NET.Entity;

public class OpenStreetMapGraphHopperRouteEntity
{
	public class Details
	{
	}

	public class Hints
	{
		[JsonProperty("visited_nodes.sum")]
		public int VisitedNodesSum { get; set; }

		[JsonProperty("visited_nodes.average")]
		public double VisitedNodesAverage { get; set; }
	}

	public class Info
	{
		public List<string> copyrights { get; set; }

		public int took { get; set; }
	}

	public class Instruction
	{
		public double distance { get; set; }

		public double heading { get; set; }

		public int sign { get; set; }

		public List<int> interval { get; set; }

		public string text { get; set; }

		public int time { get; set; }

		public string street_name { get; set; }

		public double? last_heading { get; set; }
	}

	public class Path
	{
		public double distance { get; set; }

		public double weight { get; set; }

		public int time { get; set; }

		public int transfers { get; set; }

		public bool points_encoded { get; set; }

		public List<double> bbox { get; set; }

		public string points { get; set; }

		public List<Instruction> instructions { get; set; }

		public List<object> legs { get; set; }

		public Details details { get; set; }

		public double ascend { get; set; }

		public double descend { get; set; }

		public string snapped_waypoints { get; set; }
	}

	public Hints hints { get; set; }

	public Info info { get; set; }

	public List<Path> paths { get; set; }
}
