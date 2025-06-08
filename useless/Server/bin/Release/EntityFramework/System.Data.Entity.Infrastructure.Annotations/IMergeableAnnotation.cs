namespace System.Data.Entity.Infrastructure.Annotations;

public interface IMergeableAnnotation
{
	CompatibilityResult IsCompatibleWith(object other);

	object MergeWith(object other);
}
