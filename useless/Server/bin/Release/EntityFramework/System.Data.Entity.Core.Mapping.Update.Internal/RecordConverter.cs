using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class RecordConverter
{
	private readonly UpdateTranslator m_updateTranslator;

	internal RecordConverter(UpdateTranslator updateTranslator)
	{
		m_updateTranslator = updateTranslator;
	}

	internal PropagatorResult ConvertOriginalValuesToPropagatorResult(IEntityStateEntry stateEntry, ModifiedPropertiesBehavior modifiedPropertiesBehavior)
	{
		return ConvertStateEntryToPropagatorResult(stateEntry, useCurrentValues: false, modifiedPropertiesBehavior);
	}

	internal PropagatorResult ConvertCurrentValuesToPropagatorResult(IEntityStateEntry stateEntry, ModifiedPropertiesBehavior modifiedPropertiesBehavior)
	{
		return ConvertStateEntryToPropagatorResult(stateEntry, useCurrentValues: true, modifiedPropertiesBehavior);
	}

	private PropagatorResult ConvertStateEntryToPropagatorResult(IEntityStateEntry stateEntry, bool useCurrentValues, ModifiedPropertiesBehavior modifiedPropertiesBehavior)
	{
		try
		{
			object obj;
			if (!useCurrentValues)
			{
				obj = (IExtendedDataRecord)stateEntry.OriginalValues;
			}
			else
			{
				IExtendedDataRecord currentValues = stateEntry.CurrentValues;
				obj = currentValues;
			}
			IExtendedDataRecord record = (IExtendedDataRecord)obj;
			bool isModified = false;
			return ExtractorMetadata.ExtractResultFromRecord(stateEntry, isModified, record, useCurrentValues, m_updateTranslator, modifiedPropertiesBehavior);
		}
		catch (Exception ex)
		{
			if (ex.RequiresContext())
			{
				throw EntityUtil.Update(Strings.Update_ErrorLoadingRecord, ex, stateEntry);
			}
			throw;
		}
	}
}
