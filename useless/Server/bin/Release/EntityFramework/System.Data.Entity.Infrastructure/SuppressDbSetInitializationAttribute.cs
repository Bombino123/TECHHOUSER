namespace System.Data.Entity.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
public sealed class SuppressDbSetInitializationAttribute : Attribute
{
}
