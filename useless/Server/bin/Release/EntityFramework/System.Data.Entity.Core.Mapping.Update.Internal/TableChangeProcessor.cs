using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class TableChangeProcessor
{
	private readonly EntitySet m_table;

	private readonly int[] m_keyOrdinals;

	internal EntitySet Table => m_table;

	internal int[] KeyOrdinals => m_keyOrdinals;

	internal TableChangeProcessor(EntitySet table)
	{
		m_table = table;
		m_keyOrdinals = InitializeKeyOrdinals(table);
	}

	protected TableChangeProcessor()
	{
	}

	internal bool IsKeyProperty(int propertyOrdinal)
	{
		int[] keyOrdinals = m_keyOrdinals;
		foreach (int num in keyOrdinals)
		{
			if (propertyOrdinal == num)
			{
				return true;
			}
		}
		return false;
	}

	private static int[] InitializeKeyOrdinals(EntitySet table)
	{
		EntityType elementType = table.ElementType;
		IList<EdmMember> keyMembers = elementType.KeyMembers;
		IBaseList<EdmMember> allStructuralMembers = TypeHelpers.GetAllStructuralMembers(elementType);
		int[] array = new int[keyMembers.Count];
		for (int i = 0; i < keyMembers.Count; i++)
		{
			EdmMember item = keyMembers[i];
			array[i] = allStructuralMembers.IndexOf(item);
		}
		return array;
	}

	internal List<UpdateCommand> CompileCommands(ChangeNode changeNode, UpdateCompiler compiler)
	{
		Set<CompositeKey> set = new Set<CompositeKey>(compiler.m_translator.KeyComparer);
		Dictionary<CompositeKey, PropagatorResult> dictionary = ProcessKeys(compiler, changeNode.Deleted, set);
		Dictionary<CompositeKey, PropagatorResult> dictionary2 = ProcessKeys(compiler, changeNode.Inserted, set);
		List<UpdateCommand> list = new List<UpdateCommand>(dictionary.Count + dictionary2.Count);
		foreach (CompositeKey item in set)
		{
			PropagatorResult value;
			bool flag = dictionary.TryGetValue(item, out value);
			PropagatorResult value2;
			bool flag2 = dictionary2.TryGetValue(item, out value2);
			try
			{
				if (!flag)
				{
					list.Add(compiler.BuildInsertCommand(value2, this));
					continue;
				}
				if (!flag2)
				{
					list.Add(compiler.BuildDeleteCommand(value, this));
					continue;
				}
				UpdateCommand updateCommand = compiler.BuildUpdateCommand(value, value2, this);
				if (updateCommand != null)
				{
					list.Add(updateCommand);
				}
			}
			catch (Exception ex)
			{
				if (ex.RequiresContext())
				{
					List<IEntityStateEntry> list2 = new List<IEntityStateEntry>();
					if (value != null)
					{
						list2.AddRange(SourceInterpreter.GetAllStateEntries(value, compiler.m_translator, m_table));
					}
					if (value2 != null)
					{
						list2.AddRange(SourceInterpreter.GetAllStateEntries(value2, compiler.m_translator, m_table));
					}
					throw new UpdateException(Strings.Update_GeneralExecutionException, ex, list2.Cast<ObjectStateEntry>().Distinct());
				}
				throw;
			}
		}
		return list;
	}

	private Dictionary<CompositeKey, PropagatorResult> ProcessKeys(UpdateCompiler compiler, List<PropagatorResult> changes, Set<CompositeKey> keys)
	{
		Dictionary<CompositeKey, PropagatorResult> dictionary = new Dictionary<CompositeKey, PropagatorResult>(compiler.m_translator.KeyComparer);
		foreach (PropagatorResult change in changes)
		{
			PropagatorResult propagatorResult = change;
			CompositeKey compositeKey = new CompositeKey(GetKeyConstants(propagatorResult));
			if (dictionary.TryGetValue(compositeKey, out var value))
			{
				DiagnoseKeyCollision(compiler, change, compositeKey, value);
			}
			dictionary.Add(compositeKey, propagatorResult);
			keys.Add(compositeKey);
		}
		return dictionary;
	}

	private void DiagnoseKeyCollision(UpdateCompiler compiler, PropagatorResult change, CompositeKey key, PropagatorResult other)
	{
		KeyManager keyManager = compiler.m_translator.KeyManager;
		CompositeKey compositeKey = new CompositeKey(GetKeyConstants(other));
		bool flag = true;
		int num = 0;
		while (flag && num < key.KeyComponents.Length)
		{
			int identifier = key.KeyComponents[num].Identifier;
			int identifier2 = compositeKey.KeyComponents[num].Identifier;
			if (!keyManager.GetPrincipals(identifier).Intersect(keyManager.GetPrincipals(identifier2)).Any())
			{
				flag = false;
			}
			num++;
		}
		if (flag)
		{
			IEnumerable<IEntityStateEntry> source = SourceInterpreter.GetAllStateEntries(change, compiler.m_translator, m_table).Concat(SourceInterpreter.GetAllStateEntries(other, compiler.m_translator, m_table));
			throw new UpdateException(Strings.Update_DuplicateKeys, null, source.Cast<ObjectStateEntry>().Distinct());
		}
		HashSet<IEntityStateEntry> hashSet = null;
		foreach (PropagatorResult item in key.KeyComponents.Concat(compositeKey.KeyComponents))
		{
			HashSet<IEntityStateEntry> hashSet2 = new HashSet<IEntityStateEntry>();
			foreach (int dependent in keyManager.GetDependents(item.Identifier))
			{
				if (keyManager.TryGetIdentifierOwner(dependent, out var owner) && owner.StateEntry != null)
				{
					hashSet2.Add(owner.StateEntry);
				}
			}
			if (hashSet == null)
			{
				hashSet = new HashSet<IEntityStateEntry>(hashSet2);
			}
			else
			{
				hashSet.IntersectWith(hashSet2);
			}
		}
		throw new UpdateException(Strings.Update_GeneralExecutionException, new ConstraintException(Strings.Update_ReferentialConstraintIntegrityViolation), hashSet.Cast<ObjectStateEntry>().Distinct());
	}

	private PropagatorResult[] GetKeyConstants(PropagatorResult row)
	{
		PropagatorResult[] array = new PropagatorResult[m_keyOrdinals.Length];
		for (int i = 0; i < m_keyOrdinals.Length; i++)
		{
			PropagatorResult memberValue = row.GetMemberValue(m_keyOrdinals[i]);
			array[i] = memberValue;
		}
		return array;
	}
}
