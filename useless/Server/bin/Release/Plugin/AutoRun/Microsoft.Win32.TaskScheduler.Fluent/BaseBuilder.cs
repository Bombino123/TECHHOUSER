using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler.Fluent;

[ComVisible(true)]
public abstract class BaseBuilder
{
	internal BuilderInfo tb;

	public SettingsBuilder When => new SettingsBuilder(tb);

	internal TaskDefinition TaskDef => tb.td;

	internal BaseBuilder(BuilderInfo taskBuilder)
	{
		tb = taskBuilder;
	}

	public Task AsTask([NotNull] string name)
	{
		return tb.ts.RootFolder.RegisterTaskDefinition(name, TaskDef);
	}

	public Task AsTask([NotNull] string name, TaskCreation createType, string userId, string password = null, TaskLogonType logonType = TaskLogonType.S4U)
	{
		return tb.ts.RootFolder.RegisterTaskDefinition(name, TaskDef, createType, userId, password, logonType);
	}
}
