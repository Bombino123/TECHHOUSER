using System.Collections.Generic;
using System.Data.Entity.Core.Mapping.ViewGeneration;

namespace System.Data.Entity.Core.Mapping;

internal struct InputForComputingCellGroups : IEquatable<InputForComputingCellGroups>, IEqualityComparer<InputForComputingCellGroups>
{
	internal readonly EntityContainerMapping ContainerMapping;

	internal readonly ConfigViewGenerator Config;

	internal InputForComputingCellGroups(EntityContainerMapping containerMapping, ConfigViewGenerator config)
	{
		ContainerMapping = containerMapping;
		Config = config;
	}

	public bool Equals(InputForComputingCellGroups other)
	{
		if (ContainerMapping.Equals(other.ContainerMapping))
		{
			return Config.Equals(other.Config);
		}
		return false;
	}

	public bool Equals(InputForComputingCellGroups one, InputForComputingCellGroups two)
	{
		if ((object)one == (object)two)
		{
			return true;
		}
		if ((object)one == null || (object)two == null)
		{
			return false;
		}
		return one.Equals(two);
	}

	public int GetHashCode(InputForComputingCellGroups value)
	{
		return value.GetHashCode();
	}

	public override int GetHashCode()
	{
		return ContainerMapping.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is InputForComputingCellGroups)
		{
			return Equals((InputForComputingCellGroups)obj);
		}
		return false;
	}

	public static bool operator ==(InputForComputingCellGroups input1, InputForComputingCellGroups input2)
	{
		if ((object)input1 == (object)input2)
		{
			return true;
		}
		return input1.Equals(input2);
	}

	public static bool operator !=(InputForComputingCellGroups input1, InputForComputingCellGroups input2)
	{
		return !(input1 == input2);
	}
}
