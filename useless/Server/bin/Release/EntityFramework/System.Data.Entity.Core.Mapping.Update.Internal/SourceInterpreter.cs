using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class SourceInterpreter
{
	private readonly List<IEntityStateEntry> m_stateEntries;

	private readonly UpdateTranslator m_translator;

	private readonly EntitySet m_sourceTable;

	private SourceInterpreter(UpdateTranslator translator, EntitySet sourceTable)
	{
		m_stateEntries = new List<IEntityStateEntry>();
		m_translator = translator;
		m_sourceTable = sourceTable;
	}

	internal static ReadOnlyCollection<IEntityStateEntry> GetAllStateEntries(PropagatorResult source, UpdateTranslator translator, EntitySet sourceTable)
	{
		SourceInterpreter sourceInterpreter = new SourceInterpreter(translator, sourceTable);
		sourceInterpreter.RetrieveResultMarkup(source);
		return new ReadOnlyCollection<IEntityStateEntry>(sourceInterpreter.m_stateEntries);
	}

	private void RetrieveResultMarkup(PropagatorResult source)
	{
		if (source.Identifier != -1)
		{
			do
			{
				if (source.StateEntry != null)
				{
					m_stateEntries.Add(source.StateEntry);
					if (source.Identifier != -1)
					{
						if (m_translator.KeyManager.TryGetIdentifierOwner(source.Identifier, out var owner) && owner.StateEntry != null && ExtentInScope(owner.StateEntry.EntitySet))
						{
							m_stateEntries.Add(owner.StateEntry);
						}
						foreach (IEntityStateEntry dependentStateEntry in m_translator.KeyManager.GetDependentStateEntries(source.Identifier))
						{
							m_stateEntries.Add(dependentStateEntry);
						}
					}
				}
				source = source.Next;
			}
			while (source != null);
		}
		else if (!source.IsSimple && !source.IsNull)
		{
			PropagatorResult[] memberValues = source.GetMemberValues();
			foreach (PropagatorResult source2 in memberValues)
			{
				RetrieveResultMarkup(source2);
			}
		}
	}

	private bool ExtentInScope(EntitySetBase extent)
	{
		if (extent == null)
		{
			return false;
		}
		return m_translator.ViewLoader.GetAffectedTables(extent, m_translator.MetadataWorkspace).Contains(m_sourceTable);
	}
}
