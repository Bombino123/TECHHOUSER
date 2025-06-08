using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlType(IncludeInSchema = true)]
[XmlRoot("ComHandler", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = false)]
[ComVisible(true)]
public class ComHandlerAction : Action, IBindAsExecAction
{
	public Guid ClassId
	{
		get
		{
			return new Guid(GetProperty<string, IComHandlerAction>("ClassId", Guid.Empty.ToString()));
		}
		set
		{
			SetProperty<string, IComHandlerAction>("ClassId", value.ToString());
		}
	}

	public string ClassName => GetNameForCLSID(ClassId);

	[DefaultValue(null)]
	[CanBeNull]
	public string Data
	{
		get
		{
			return GetProperty<string, IComHandlerAction>("Data");
		}
		set
		{
			SetProperty<string, IComHandlerAction>("Data", value);
		}
	}

	internal override TaskActionType InternalActionType => TaskActionType.ComHandler;

	public ComHandlerAction()
	{
	}

	public ComHandlerAction(Guid classId, [CanBeNull] string data)
	{
		ClassId = classId;
		Data = data;
	}

	internal ComHandlerAction([NotNull] ITask task)
		: base(task)
	{
	}

	internal ComHandlerAction([NotNull] IAction action)
		: base(action)
	{
	}

	public override bool Equals(Action other)
	{
		if (base.Equals(other) && ClassId == ((ComHandlerAction)other).ClassId)
		{
			return Data == ((ComHandlerAction)other).Data;
		}
		return false;
	}

	public override string ToString()
	{
		return string.Format(Resources.ComHandlerAction, ClassId, Data, Id, ClassName);
	}

	internal static Action FromPowerShellCommand(string p)
	{
		Match match = Regex.Match(p, "^\\[Reflection.Assembly\\]::LoadFile\\('(?:[^']*)'\\); \\[Microsoft.Win32.TaskScheduler.TaskService\\]::RunComHandlerAction\\(\\[GUID\\]\\('(?<g>[^']*)'\\), '(?<d>[^']*)'\\);?\\s*$");
		if (!match.Success)
		{
			return null;
		}
		return new ComHandlerAction(new Guid(match.Groups["g"].Value), match.Groups["d"].Value.Replace("''", "'"));
	}

	internal override void CopyProperties(Action sourceAction)
	{
		if (sourceAction.GetType() == GetType())
		{
			base.CopyProperties(sourceAction);
			ClassId = ((ComHandlerAction)sourceAction).ClassId;
			Data = ((ComHandlerAction)sourceAction).Data;
		}
	}

	internal override void CreateV2Action([NotNull] IActionCollection iActions)
	{
		iAction = iActions.Create(TaskActionType.ComHandler);
	}

	internal override string GetPowerShellCommand()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[Reflection.Assembly]::LoadFile('" + Assembly.GetExecutingAssembly().Location + "'); ");
		stringBuilder.Append(string.Format("[Microsoft.Win32.TaskScheduler.TaskService]::RunComHandlerAction([GUID]('{0:D}'), '{1}'); ", ClassId, Data?.Replace("'", "''") ?? string.Empty));
		return stringBuilder.ToString();
	}

	[CanBeNull]
	private static string GetNameForCLSID(Guid guid)
	{
		using (RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("CLSID", writable: false))
		{
			if (registryKey != null)
			{
				using (RegistryKey registryKey2 = registryKey.OpenSubKey(guid.ToString("B"), writable: false))
				{
					return registryKey2?.GetValue(null) as string;
				}
			}
		}
		return null;
	}
}
