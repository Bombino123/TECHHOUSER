using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Utils;

internal static class ExceptionHelpers
{
	internal static void ThrowMappingException(ErrorLog.Record errorRecord, ConfigViewGenerator config)
	{
		InternalMappingException ex = new InternalMappingException(errorRecord.ToUserString(), errorRecord);
		if (config.IsNormalTracing)
		{
			ex.ErrorLog.PrintTrace();
		}
		throw ex;
	}

	internal static void ThrowMappingException(ErrorLog errorLog, ConfigViewGenerator config)
	{
		InternalMappingException ex = new InternalMappingException(errorLog.ToUserString(), errorLog);
		if (config.IsNormalTracing)
		{
			ex.ErrorLog.PrintTrace();
		}
		throw ex;
	}
}
