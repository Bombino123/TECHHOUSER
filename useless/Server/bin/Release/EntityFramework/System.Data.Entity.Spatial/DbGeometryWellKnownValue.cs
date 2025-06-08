using System.Runtime.Serialization;

namespace System.Data.Entity.Spatial;

[DataContract]
public sealed class DbGeometryWellKnownValue
{
	[DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
	public int CoordinateSystemId { get; set; }

	[DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
	public string WellKnownText { get; set; }

	[DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
	public byte[] WellKnownBinary { get; set; }
}
