using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq.Expressions;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal class RecordStateScratchpad
{
	private readonly List<RecordStateScratchpad> _nestedRecordStateScratchpads = new List<RecordStateScratchpad>();

	internal int StateSlotNumber { get; set; }

	internal int ColumnCount { get; set; }

	internal DataRecordInfo DataRecordInfo { get; set; }

	internal Expression GatherData { get; set; }

	internal string[] PropertyNames { get; set; }

	internal TypeUsage[] TypeUsages { get; set; }

	internal RecordStateFactory Compile()
	{
		RecordStateFactory[] array = new RecordStateFactory[_nestedRecordStateScratchpads.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _nestedRecordStateScratchpads[i].Compile();
		}
		return (RecordStateFactory)Activator.CreateInstance(typeof(RecordStateFactory), StateSlotNumber, ColumnCount, array, DataRecordInfo, GatherData, PropertyNames, TypeUsages);
	}
}
