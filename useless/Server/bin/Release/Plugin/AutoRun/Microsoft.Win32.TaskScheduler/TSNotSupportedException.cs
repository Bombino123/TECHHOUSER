using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[Serializable]
[DebuggerStepThrough]
[PublicAPI]
[ComVisible(true)]
public abstract class TSNotSupportedException : Exception
{
	protected readonly TaskCompatibility min;

	private readonly string myMessage;

	public override string Message => myMessage;

	public TaskCompatibility MinimumSupportedVersion => min;

	internal abstract string LibName { get; }

	protected TSNotSupportedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
		try
		{
			min = (TaskCompatibility)serializationInfo.GetValue("min", typeof(TaskCompatibility));
		}
		catch
		{
			min = TaskCompatibility.V1;
		}
	}

	internal TSNotSupportedException(TaskCompatibility minComp)
	{
		min = minComp;
		MethodBase method = new StackTrace().GetFrame(2).GetMethod();
		myMessage = method.DeclaringType?.Name + "." + method.Name + " is not supported on " + LibName;
	}

	internal TSNotSupportedException(string message, TaskCompatibility minComp)
	{
		myMessage = message;
		min = minComp;
	}

	[SecurityCritical]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("min", min);
		base.GetObjectData(info, context);
	}
}
