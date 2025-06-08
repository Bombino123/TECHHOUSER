using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class CompositeKey
{
	private class CompositeKeyComparer : IEqualityComparer<CompositeKey>
	{
		private readonly KeyManager _manager;

		internal CompositeKeyComparer(KeyManager manager)
		{
			_manager = manager;
		}

		public bool Equals(CompositeKey left, CompositeKey right)
		{
			if (left == right)
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			if (left.KeyComponents.Length != right.KeyComponents.Length)
			{
				return false;
			}
			for (int i = 0; i < left.KeyComponents.Length; i++)
			{
				PropagatorResult propagatorResult = left.KeyComponents[i];
				PropagatorResult propagatorResult2 = right.KeyComponents[i];
				if (propagatorResult.Identifier != -1)
				{
					if (propagatorResult2.Identifier == -1 || _manager.GetCliqueIdentifier(propagatorResult.Identifier) != _manager.GetCliqueIdentifier(propagatorResult2.Identifier))
					{
						return false;
					}
				}
				else if (propagatorResult2.Identifier != -1 || !ByValueEqualityComparer.Default.Equals(propagatorResult.GetSimpleValue(), propagatorResult2.GetSimpleValue()))
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(CompositeKey key)
		{
			int num = 0;
			PropagatorResult[] keyComponents = key.KeyComponents;
			foreach (PropagatorResult keyComponent in keyComponents)
			{
				num = (num << 5) ^ GetComponentHashCode(keyComponent);
			}
			return num;
		}

		private int GetComponentHashCode(PropagatorResult keyComponent)
		{
			if (keyComponent.Identifier == -1)
			{
				return ByValueEqualityComparer.Default.GetHashCode(keyComponent.GetSimpleValue());
			}
			return _manager.GetCliqueIdentifier(keyComponent.Identifier).GetHashCode();
		}
	}

	internal readonly PropagatorResult[] KeyComponents;

	internal CompositeKey(PropagatorResult[] constants)
	{
		KeyComponents = constants;
	}

	internal static IEqualityComparer<CompositeKey> CreateComparer(KeyManager keyManager)
	{
		return new CompositeKeyComparer(keyManager);
	}

	internal CompositeKey Merge(KeyManager keyManager, CompositeKey other)
	{
		PropagatorResult[] array = new PropagatorResult[KeyComponents.Length];
		for (int i = 0; i < KeyComponents.Length; i++)
		{
			array[i] = KeyComponents[i].Merge(keyManager, other.KeyComponents[i]);
		}
		return new CompositeKey(array);
	}
}
