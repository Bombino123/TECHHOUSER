namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal struct TranslatorArg
{
	internal readonly Type RequestedType;

	internal TranslatorArg(Type requestedType)
	{
		RequestedType = requestedType;
	}
}
