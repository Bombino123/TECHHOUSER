using System.ComponentModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Runtime.Serialization;

namespace System.Data.Entity.Core.Objects.DataClasses;

[Serializable]
[DataContract(IsReference = true)]
public abstract class StructuralObject : INotifyPropertyChanging, INotifyPropertyChanged
{
	public const string EntityKeyPropertyName = "-EntityKey-";

	internal abstract bool IsChangeTracked { get; }

	[field: NonSerialized]
	public event PropertyChangedEventHandler PropertyChanged;

	[field: NonSerialized]
	public event PropertyChangingEventHandler PropertyChanging;

	protected virtual void OnPropertyChanged(string property)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}

	protected virtual void OnPropertyChanging(string property)
	{
		if (this.PropertyChanging != null)
		{
			this.PropertyChanging(this, new PropertyChangingEventArgs(property));
		}
	}

	protected static DateTime DefaultDateTimeValue()
	{
		return DateTime.Now;
	}

	protected virtual void ReportPropertyChanging(string property)
	{
		Check.NotEmpty(property, "property");
		OnPropertyChanging(property);
	}

	protected virtual void ReportPropertyChanged(string property)
	{
		Check.NotEmpty(property, "property");
		OnPropertyChanged(property);
	}

	protected internal T GetValidValue<T>(T currentValue, string property, bool isNullable, bool isInitialized) where T : ComplexObject, new()
	{
		if (!isNullable && !isInitialized)
		{
			currentValue = SetValidValue(currentValue, new T(), property);
		}
		return currentValue;
	}

	internal abstract void ReportComplexPropertyChanging(string entityMemberName, ComplexObject complexObject, string complexMemberName);

	internal abstract void ReportComplexPropertyChanged(string entityMemberName, ComplexObject complexObject, string complexMemberName);

	protected internal static bool BinaryEquals(byte[] first, byte[] second)
	{
		if (first == second)
		{
			return true;
		}
		if (first == null || second == null)
		{
			return false;
		}
		return ByValueEqualityComparer.CompareBinaryValues(first, second);
	}

	protected internal static byte[] GetValidValue(byte[] currentValue)
	{
		if (currentValue == null)
		{
			return null;
		}
		return (byte[])currentValue.Clone();
	}

	protected internal static byte[] SetValidValue(byte[] value, bool isNullable, string propertyName)
	{
		if (value == null)
		{
			if (!isNullable)
			{
				EntityUtil.ThrowPropertyIsNotNullable(propertyName);
			}
			return value;
		}
		return (byte[])value.Clone();
	}

	protected internal static byte[] SetValidValue(byte[] value, bool isNullable)
	{
		return SetValidValue(value, isNullable, null);
	}

	protected internal static bool SetValidValue(bool value, string propertyName)
	{
		return value;
	}

	protected internal static bool SetValidValue(bool value)
	{
		return value;
	}

	protected internal static bool? SetValidValue(bool? value, string propertyName)
	{
		return value;
	}

	protected internal static bool? SetValidValue(bool? value)
	{
		return value;
	}

	protected internal static byte SetValidValue(byte value, string propertyName)
	{
		return value;
	}

	protected internal static byte SetValidValue(byte value)
	{
		return value;
	}

	protected internal static byte? SetValidValue(byte? value, string propertyName)
	{
		return value;
	}

	protected internal static byte? SetValidValue(byte? value)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static sbyte SetValidValue(sbyte value, string propertyName)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static sbyte SetValidValue(sbyte value)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static sbyte? SetValidValue(sbyte? value, string propertyName)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static sbyte? SetValidValue(sbyte? value)
	{
		return value;
	}

	protected internal static DateTime SetValidValue(DateTime value, string propertyName)
	{
		return value;
	}

	protected internal static DateTime SetValidValue(DateTime value)
	{
		return value;
	}

	protected internal static DateTime? SetValidValue(DateTime? value, string propertyName)
	{
		return value;
	}

	protected internal static DateTime? SetValidValue(DateTime? value)
	{
		return value;
	}

	protected internal static TimeSpan SetValidValue(TimeSpan value, string propertyName)
	{
		return value;
	}

	protected internal static TimeSpan SetValidValue(TimeSpan value)
	{
		return value;
	}

	protected internal static TimeSpan? SetValidValue(TimeSpan? value, string propertyName)
	{
		return value;
	}

	protected internal static TimeSpan? SetValidValue(TimeSpan? value)
	{
		return value;
	}

	protected internal static DateTimeOffset SetValidValue(DateTimeOffset value, string propertyName)
	{
		return value;
	}

	protected internal static DateTimeOffset SetValidValue(DateTimeOffset value)
	{
		return value;
	}

	protected internal static DateTimeOffset? SetValidValue(DateTimeOffset? value, string propertyName)
	{
		return value;
	}

	protected internal static DateTimeOffset? SetValidValue(DateTimeOffset? value)
	{
		return value;
	}

	protected internal static decimal SetValidValue(decimal value, string propertyName)
	{
		return value;
	}

	protected internal static decimal SetValidValue(decimal value)
	{
		return value;
	}

	protected internal static decimal? SetValidValue(decimal? value, string propertyName)
	{
		return value;
	}

	protected internal static decimal? SetValidValue(decimal? value)
	{
		return value;
	}

	protected internal static double SetValidValue(double value, string propertyName)
	{
		return value;
	}

	protected internal static double SetValidValue(double value)
	{
		return value;
	}

	protected internal static double? SetValidValue(double? value, string propertyName)
	{
		return value;
	}

	protected internal static double? SetValidValue(double? value)
	{
		return value;
	}

	protected internal static float SetValidValue(float value, string propertyName)
	{
		return value;
	}

	protected internal static float SetValidValue(float value)
	{
		return value;
	}

	protected internal static float? SetValidValue(float? value, string propertyName)
	{
		return value;
	}

	protected internal static float? SetValidValue(float? value)
	{
		return value;
	}

	protected internal static Guid SetValidValue(Guid value, string propertyName)
	{
		return value;
	}

	protected internal static Guid SetValidValue(Guid value)
	{
		return value;
	}

	protected internal static Guid? SetValidValue(Guid? value, string propertyName)
	{
		return value;
	}

	protected internal static Guid? SetValidValue(Guid? value)
	{
		return value;
	}

	protected internal static short SetValidValue(short value, string propertyName)
	{
		return value;
	}

	protected internal static short SetValidValue(short value)
	{
		return value;
	}

	protected internal static short? SetValidValue(short? value, string propertyName)
	{
		return value;
	}

	protected internal static short? SetValidValue(short? value)
	{
		return value;
	}

	protected internal static int SetValidValue(int value, string propertyName)
	{
		return value;
	}

	protected internal static int SetValidValue(int value)
	{
		return value;
	}

	protected internal static int? SetValidValue(int? value, string propertyName)
	{
		return value;
	}

	protected internal static int? SetValidValue(int? value)
	{
		return value;
	}

	protected internal static long SetValidValue(long value, string propertyName)
	{
		return value;
	}

	protected internal static long SetValidValue(long value)
	{
		return value;
	}

	protected internal static long? SetValidValue(long? value, string propertyName)
	{
		return value;
	}

	protected internal static long? SetValidValue(long? value)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static ushort SetValidValue(ushort value, string propertyName)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static ushort SetValidValue(ushort value)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static ushort? SetValidValue(ushort? value, string propertyName)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static ushort? SetValidValue(ushort? value)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static uint SetValidValue(uint value, string propertyName)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static uint SetValidValue(uint value)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static uint? SetValidValue(uint? value, string propertyName)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static uint? SetValidValue(uint? value)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static ulong SetValidValue(ulong value, string propertyName)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static ulong SetValidValue(ulong value)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static ulong? SetValidValue(ulong? value, string propertyName)
	{
		return value;
	}

	[CLSCompliant(false)]
	protected internal static ulong? SetValidValue(ulong? value)
	{
		return value;
	}

	protected internal static string SetValidValue(string value, bool isNullable, string propertyName)
	{
		if (value == null && !isNullable)
		{
			EntityUtil.ThrowPropertyIsNotNullable(propertyName);
		}
		return value;
	}

	protected internal static string SetValidValue(string value, bool isNullable)
	{
		return SetValidValue(value, isNullable, null);
	}

	protected internal static DbGeography SetValidValue(DbGeography value, bool isNullable, string propertyName)
	{
		if (value == null && !isNullable)
		{
			EntityUtil.ThrowPropertyIsNotNullable(propertyName);
		}
		return value;
	}

	protected internal static DbGeography SetValidValue(DbGeography value, bool isNullable)
	{
		return SetValidValue(value, isNullable, null);
	}

	protected internal static DbGeometry SetValidValue(DbGeometry value, bool isNullable, string propertyName)
	{
		if (value == null && !isNullable)
		{
			EntityUtil.ThrowPropertyIsNotNullable(propertyName);
		}
		return value;
	}

	protected internal static DbGeometry SetValidValue(DbGeometry value, bool isNullable)
	{
		return SetValidValue(value, isNullable, null);
	}

	protected internal T SetValidValue<T>(T oldValue, T newValue, string property) where T : ComplexObject
	{
		if (newValue == null && IsChangeTracked)
		{
			throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(property));
		}
		oldValue?.DetachFromParent();
		newValue?.AttachToParent(this, property);
		return newValue;
	}

	protected internal static TComplex VerifyComplexObjectIsNotNull<TComplex>(TComplex complexObject, string propertyName) where TComplex : ComplexObject
	{
		if (complexObject == null)
		{
			EntityUtil.ThrowPropertyIsNotNullable(propertyName);
		}
		return complexObject;
	}
}
