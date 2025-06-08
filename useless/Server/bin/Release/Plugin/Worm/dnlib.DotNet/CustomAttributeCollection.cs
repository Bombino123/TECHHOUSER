using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.Utils;

namespace dnlib.DotNet;

[ComVisible(true)]
public class CustomAttributeCollection : LazyList<CustomAttribute, object>
{
	public CustomAttributeCollection()
	{
	}

	public CustomAttributeCollection(int length, object context, Func<object, int, CustomAttribute> readOriginalValue)
		: base(length, context, readOriginalValue)
	{
	}

	public bool IsDefined(string fullName)
	{
		return Find(fullName) != null;
	}

	public void RemoveAll(string fullName)
	{
		if (fullName == null)
		{
			return;
		}
		for (int num = base.Count - 1; num >= 0; num--)
		{
			CustomAttribute customAttribute = base[num];
			if (customAttribute != null && fullName.EndsWith(customAttribute.TypeName, StringComparison.Ordinal) && customAttribute.TypeFullName == fullName)
			{
				RemoveAt(num);
			}
		}
	}

	public CustomAttribute Find(string fullName)
	{
		if (fullName == null)
		{
			return null;
		}
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				CustomAttribute current = enumerator.Current;
				if (current != null && fullName.EndsWith(current.TypeName, StringComparison.Ordinal) && current.TypeFullName == fullName)
				{
					return current;
				}
			}
		}
		return null;
	}

	public IEnumerable<CustomAttribute> FindAll(string fullName)
	{
		if (fullName == null)
		{
			yield break;
		}
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			CustomAttribute current = enumerator.Current;
			if (current != null && fullName.EndsWith(current.TypeName, StringComparison.Ordinal) && current.TypeFullName == fullName)
			{
				yield return current;
			}
		}
	}

	public CustomAttribute Find(IType attrType)
	{
		return Find(attrType, (SigComparerOptions)0u);
	}

	public CustomAttribute Find(IType attrType, SigComparerOptions options)
	{
		SigComparer sigComparer = new SigComparer(options);
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				CustomAttribute current = enumerator.Current;
				if (sigComparer.Equals(current.AttributeType, attrType))
				{
					return current;
				}
			}
		}
		return null;
	}

	public IEnumerable<CustomAttribute> FindAll(IType attrType)
	{
		return FindAll(attrType, (SigComparerOptions)0u);
	}

	public IEnumerable<CustomAttribute> FindAll(IType attrType, SigComparerOptions options)
	{
		SigComparer comparer = new SigComparer(options);
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			CustomAttribute current = enumerator.Current;
			if (comparer.Equals(current.AttributeType, attrType))
			{
				yield return current;
			}
		}
	}
}
