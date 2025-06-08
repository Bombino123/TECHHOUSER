namespace dnlib.DotNet.Emit;

public sealed class ExceptionHandler
{
	public Instruction TryStart;

	public Instruction TryEnd;

	public Instruction FilterStart;

	public Instruction HandlerStart;

	public Instruction HandlerEnd;

	public ITypeDefOrRef CatchType;

	public ExceptionHandlerType HandlerType;

	public bool IsCatch => (HandlerType & (ExceptionHandlerType.Filter | ExceptionHandlerType.Finally | ExceptionHandlerType.Fault)) == 0;

	public bool IsFilter => (HandlerType & ExceptionHandlerType.Filter) != 0;

	public bool IsFinally => (HandlerType & ExceptionHandlerType.Finally) != 0;

	public bool IsFault => (HandlerType & ExceptionHandlerType.Fault) != 0;

	public ExceptionHandler()
	{
	}

	public ExceptionHandler(ExceptionHandlerType handlerType)
	{
		HandlerType = handlerType;
	}
}
