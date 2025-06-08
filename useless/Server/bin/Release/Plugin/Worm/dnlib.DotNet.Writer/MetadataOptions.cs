using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using dnlib.DotNet.MD;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class MetadataOptions
{
	private MetadataHeaderOptions metadataHeaderOptions;

	private MetadataHeaderOptions debugMetadataHeaderOptions;

	private TablesHeapOptions tablesHeapOptions;

	private List<IHeap> customHeaps;

	public MetadataFlags Flags;

	public MetadataHeaderOptions MetadataHeaderOptions
	{
		get
		{
			return metadataHeaderOptions ?? (metadataHeaderOptions = new MetadataHeaderOptions());
		}
		set
		{
			metadataHeaderOptions = value;
		}
	}

	public MetadataHeaderOptions DebugMetadataHeaderOptions
	{
		get
		{
			return debugMetadataHeaderOptions ?? (debugMetadataHeaderOptions = MetadataHeaderOptions.CreatePortablePdbV1_0());
		}
		set
		{
			debugMetadataHeaderOptions = value;
		}
	}

	public TablesHeapOptions TablesHeapOptions
	{
		get
		{
			return tablesHeapOptions ?? (tablesHeapOptions = new TablesHeapOptions());
		}
		set
		{
			tablesHeapOptions = value;
		}
	}

	public TablesHeapOptions DebugTablesHeapOptions
	{
		get
		{
			return tablesHeapOptions ?? (tablesHeapOptions = TablesHeapOptions.CreatePortablePdbV1_0());
		}
		set
		{
			tablesHeapOptions = value;
		}
	}

	public List<IHeap> CustomHeaps => customHeaps ?? (customHeaps = new List<IHeap>());

	public event EventHandler2<MetadataHeapsAddedEventArgs> MetadataHeapsAdded;

	internal void RaiseMetadataHeapsAdded(MetadataHeapsAddedEventArgs e)
	{
		this.MetadataHeapsAdded?.Invoke(e.Metadata, e);
	}

	public void PreserveHeapOrder(ModuleDef module, bool addCustomHeaps)
	{
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		if (!(module is ModuleDefMD moduleDefMD))
		{
			return;
		}
		if (addCustomHeaps)
		{
			IEnumerable<DataReaderHeap> source = from a in moduleDefMD.Metadata.AllStreams
				where (object)a.GetType() == typeof(CustomDotNetStream)
				select new DataReaderHeap(a);
			CustomHeaps.AddRange(source.OfType<IHeap>());
		}
		Dictionary<DotNetStream, int> streamToOrder = new Dictionary<DotNetStream, int>(moduleDefMD.Metadata.AllStreams.Count);
		int i = 0;
		int num = 0;
		for (; i < moduleDefMD.Metadata.AllStreams.Count; i++)
		{
			DotNetStream dotNetStream = moduleDefMD.Metadata.AllStreams[i];
			if (dotNetStream.StartOffset != 0)
			{
				streamToOrder.Add(dotNetStream, num++);
			}
		}
		Dictionary<string, int> nameToOrder = new Dictionary<string, int>(moduleDefMD.Metadata.AllStreams.Count, StringComparer.Ordinal);
		int j = 0;
		int num2 = 0;
		for (; j < moduleDefMD.Metadata.AllStreams.Count; j++)
		{
			DotNetStream dotNetStream2 = moduleDefMD.Metadata.AllStreams[j];
			if (dotNetStream2.StartOffset != 0)
			{
				bool flag = dotNetStream2 is BlobStream || dotNetStream2 is GuidStream || dotNetStream2 is PdbStream || dotNetStream2 is StringsStream || dotNetStream2 is TablesStream || dotNetStream2 is USStream;
				if (!nameToOrder.ContainsKey(dotNetStream2.Name) || flag)
				{
					nameToOrder[dotNetStream2.Name] = num2;
				}
				num2++;
			}
		}
		MetadataHeapsAdded += delegate(object s, MetadataHeapsAddedEventArgs e)
		{
			e.Heaps.Sort(delegate(IHeap a, IHeap b)
			{
				int order = GetOrder(streamToOrder, nameToOrder, a);
				int order2 = GetOrder(streamToOrder, nameToOrder, b);
				int num3 = order - order2;
				return (num3 != 0) ? num3 : StringComparer.Ordinal.Compare(a.Name, b.Name);
			});
		};
	}

	private static int GetOrder(Dictionary<DotNetStream, int> streamToOrder, Dictionary<string, int> nameToOrder, IHeap heap)
	{
		if (heap is DataReaderHeap { OptionalOriginalStream: { } optionalOriginalStream } && streamToOrder.TryGetValue(optionalOriginalStream, out var value))
		{
			return value;
		}
		if (nameToOrder.TryGetValue(heap.Name, out value))
		{
			return value;
		}
		return int.MaxValue;
	}

	public MetadataOptions()
	{
	}

	public MetadataOptions(MetadataFlags flags)
	{
		Flags = flags;
	}

	public MetadataOptions(MetadataHeaderOptions mdhOptions)
	{
		metadataHeaderOptions = mdhOptions;
	}

	public MetadataOptions(MetadataHeaderOptions mdhOptions, MetadataFlags flags)
	{
		Flags = flags;
		metadataHeaderOptions = mdhOptions;
	}
}
