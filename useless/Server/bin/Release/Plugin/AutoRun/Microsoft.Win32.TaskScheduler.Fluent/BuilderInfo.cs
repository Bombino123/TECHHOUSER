using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler.Fluent;

internal sealed class BuilderInfo
{
	public TaskDefinition td;

	public TaskService ts;

	public BuilderInfo([NotNull] TaskService taskSvc)
	{
		ts = taskSvc;
		td = ts.NewTask();
	}
}
