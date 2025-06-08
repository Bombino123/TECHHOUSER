namespace System.Data.Entity.Infrastructure.Annotations;

public sealed class AnnotationValues
{
	private readonly object _oldValue;

	private readonly object _newValue;

	public object OldValue => _oldValue;

	public object NewValue => _newValue;

	public AnnotationValues(object oldValue, object newValue)
	{
		_oldValue = oldValue;
		_newValue = newValue;
	}

	private bool Equals(AnnotationValues other)
	{
		if (object.Equals(_oldValue, other._oldValue))
		{
			return object.Equals(_newValue, other._newValue);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj is AnnotationValues)
		{
			return Equals((AnnotationValues)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((_oldValue != null) ? _oldValue.GetHashCode() : 0) * 397) ^ ((_newValue != null) ? _newValue.GetHashCode() : 0);
	}

	public static bool operator ==(AnnotationValues left, AnnotationValues right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(AnnotationValues left, AnnotationValues right)
	{
		return !object.Equals(left, right);
	}
}
