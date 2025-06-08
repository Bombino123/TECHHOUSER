namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal struct ExtractedStateEntry
{
	internal readonly EntityState State;

	internal readonly PropagatorResult Original;

	internal readonly PropagatorResult Current;

	internal readonly IEntityStateEntry Source;

	internal ExtractedStateEntry(EntityState state, PropagatorResult original, PropagatorResult current, IEntityStateEntry source)
	{
		State = state;
		Original = original;
		Current = current;
		Source = source;
	}

	internal ExtractedStateEntry(UpdateTranslator translator, IEntityStateEntry stateEntry)
	{
		State = stateEntry.State;
		Source = stateEntry;
		switch (stateEntry.State)
		{
		case EntityState.Deleted:
			Original = translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(stateEntry, ModifiedPropertiesBehavior.AllModified);
			Current = null;
			break;
		case EntityState.Unchanged:
			Original = translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(stateEntry, ModifiedPropertiesBehavior.NoneModified);
			Current = translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(stateEntry, ModifiedPropertiesBehavior.NoneModified);
			break;
		case EntityState.Modified:
			Original = translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(stateEntry, ModifiedPropertiesBehavior.SomeModified);
			Current = translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(stateEntry, ModifiedPropertiesBehavior.SomeModified);
			break;
		case EntityState.Added:
			Original = null;
			Current = translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(stateEntry, ModifiedPropertiesBehavior.AllModified);
			break;
		default:
			Original = null;
			Current = null;
			break;
		}
	}
}
