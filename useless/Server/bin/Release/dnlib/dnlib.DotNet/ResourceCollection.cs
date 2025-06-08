using System;
using dnlib.Utils;

namespace dnlib.DotNet;

public class ResourceCollection : LazyList<Resource, object>
{
	public ResourceCollection()
	{
	}

	public ResourceCollection(IListListener<Resource> listener)
		: base(listener)
	{
	}

	public ResourceCollection(int length, object context, Func<object, int, Resource> readOriginalValue)
		: base(length, context, readOriginalValue)
	{
	}

	public int IndexOf(UTF8String name)
	{
		int num = -1;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Resource current = enumerator.Current;
				num++;
				if (current != null && current.Name == name)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public int IndexOfEmbeddedResource(UTF8String name)
	{
		int num = -1;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Resource current = enumerator.Current;
				num++;
				if (current != null && current.ResourceType == ResourceType.Embedded && current.Name == name)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public int IndexOfAssemblyLinkedResource(UTF8String name)
	{
		int num = -1;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Resource current = enumerator.Current;
				num++;
				if (current != null && current.ResourceType == ResourceType.AssemblyLinked && current.Name == name)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public int IndexOfLinkedResource(UTF8String name)
	{
		int num = -1;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Resource current = enumerator.Current;
				num++;
				if (current != null && current.ResourceType == ResourceType.Linked && current.Name == name)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public Resource Find(UTF8String name)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Resource current = enumerator.Current;
				if (current != null && current.Name == name)
				{
					return current;
				}
			}
		}
		return null;
	}

	public EmbeddedResource FindEmbeddedResource(UTF8String name)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Resource current = enumerator.Current;
				if (current != null && current.ResourceType == ResourceType.Embedded && current.Name == name)
				{
					return (EmbeddedResource)current;
				}
			}
		}
		return null;
	}

	public AssemblyLinkedResource FindAssemblyLinkedResource(UTF8String name)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Resource current = enumerator.Current;
				if (current != null && current.ResourceType == ResourceType.AssemblyLinked && current.Name == name)
				{
					return (AssemblyLinkedResource)current;
				}
			}
		}
		return null;
	}

	public LinkedResource FindLinkedResource(UTF8String name)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Resource current = enumerator.Current;
				if (current != null && current.ResourceType == ResourceType.Linked && current.Name == name)
				{
					return (LinkedResource)current;
				}
			}
		}
		return null;
	}
}
