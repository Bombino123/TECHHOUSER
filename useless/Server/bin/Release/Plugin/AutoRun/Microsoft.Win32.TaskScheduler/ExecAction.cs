using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlRoot("Exec", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = false)]
[ComVisible(true)]
public class ExecAction : Action
{
	internal const string PowerShellArgFormat = "-NoLogo -NonInteractive -WindowStyle Hidden -Command \"& {{<# {0}:{1} #> {2}}}\"";

	internal const string PowerShellPath = "powershell";

	internal const string ScriptIdentifer = "TSML_20140424";

	[DefaultValue("")]
	public string Arguments
	{
		get
		{
			if (v1Task != null)
			{
				return v1Task.GetParameters();
			}
			return GetProperty<string, IExecAction>("Arguments", "");
		}
		set
		{
			if (v1Task != null)
			{
				v1Task.SetParameters(value);
			}
			else
			{
				SetProperty<string, IExecAction>("Arguments", value);
			}
		}
	}

	[XmlElement("Command")]
	[DefaultValue("")]
	public string Path
	{
		get
		{
			if (v1Task != null)
			{
				return v1Task.GetApplicationName();
			}
			return GetProperty<string, IExecAction>("Path", "");
		}
		set
		{
			if (v1Task != null)
			{
				v1Task.SetApplicationName(value);
			}
			else
			{
				SetProperty<string, IExecAction>("Path", value);
			}
		}
	}

	[DefaultValue("")]
	public string WorkingDirectory
	{
		get
		{
			if (v1Task != null)
			{
				return v1Task.GetWorkingDirectory();
			}
			return GetProperty<string, IExecAction>("WorkingDirectory", "");
		}
		set
		{
			if (v1Task != null)
			{
				v1Task.SetWorkingDirectory(value);
			}
			else
			{
				SetProperty<string, IExecAction>("WorkingDirectory", value);
			}
		}
	}

	internal override TaskActionType InternalActionType => TaskActionType.Execute;

	public ExecAction()
	{
	}

	public ExecAction([NotNull] string path, string arguments = null, string workingDirectory = null)
	{
		Path = path;
		Arguments = arguments;
		WorkingDirectory = workingDirectory;
	}

	internal ExecAction([NotNull] ITask task)
		: base(task)
	{
	}

	internal ExecAction([NotNull] IAction action)
		: base(action)
	{
	}

	public static bool IsValidPath(string path, bool checkIfExists = true, bool throwOnException = false)
	{
		try
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (System.IO.Path.GetFileName(path) == string.Empty)
			{
				return false;
			}
			System.IO.Path.GetDirectoryName(path);
			System.IO.Path.GetFullPath(path);
			return true;
		}
		catch (Exception)
		{
			if (throwOnException)
			{
				throw;
			}
		}
		return false;
	}

	public override bool Equals(Action other)
	{
		if (base.Equals(other) && Path == ((ExecAction)other).Path && Arguments == ((ExecAction)other).Arguments)
		{
			return WorkingDirectory == ((ExecAction)other).WorkingDirectory;
		}
		return false;
	}

	public void SetValidatedPath([NotNull] string path, bool checkIfExists = true)
	{
		if (IsValidPath(path, checkIfExists, throwOnException: true))
		{
			Path = path;
		}
	}

	public override string ToString()
	{
		return string.Format(Resources.ExecAction, Path, Arguments, WorkingDirectory, Id);
	}

	internal static string BuildPowerShellCmd(string actionType, string cmd)
	{
		return string.Format("-NoLogo -NonInteractive -WindowStyle Hidden -Command \"& {{<# {0}:{1} #> {2}}}\"", "TSML_20140424", actionType, cmd);
	}

	internal static ExecAction ConvertToPowerShellAction(Action action)
	{
		return CreatePowerShellAction(action.ActionType.ToString(), action.GetPowerShellCommand());
	}

	internal static ExecAction CreatePowerShellAction(string actionType, string cmd)
	{
		return new ExecAction("powershell", BuildPowerShellCmd(actionType, cmd));
	}

	internal static Action FromPowerShellCommand(string p)
	{
		Match match = Regex.Match(p, "^Start-Process -FilePath '(?<p>[^']*)'(?: -ArgumentList '(?<a>[^']*)')?(?: -WorkingDirectory '(?<d>[^']*)')?;?\\s*$");
		if (!match.Success)
		{
			return null;
		}
		return new ExecAction(match.Groups["p"].Value, match.Groups["a"].Success ? match.Groups["a"].Value.Replace("''", "'") : null, match.Groups["d"].Success ? match.Groups["d"].Value : null);
	}

	internal override void CopyProperties(Action sourceAction)
	{
		if (sourceAction.GetType() == GetType())
		{
			base.CopyProperties(sourceAction);
			Path = ((ExecAction)sourceAction).Path;
			Arguments = ((ExecAction)sourceAction).Arguments;
			WorkingDirectory = ((ExecAction)sourceAction).WorkingDirectory;
		}
	}

	internal override void CreateV2Action(IActionCollection iActions)
	{
		iAction = iActions.Create(TaskActionType.Execute);
	}

	internal override string GetPowerShellCommand()
	{
		StringBuilder stringBuilder = new StringBuilder("Start-Process -FilePath '" + Path + "'");
		if (!string.IsNullOrEmpty(Arguments))
		{
			stringBuilder.Append(" -ArgumentList '" + Arguments.Replace("'", "''") + "'");
		}
		if (!string.IsNullOrEmpty(WorkingDirectory))
		{
			stringBuilder.Append(" -WorkingDirectory '" + WorkingDirectory + "'");
		}
		return stringBuilder.Append("; ").ToString();
	}

	internal string[] ParsePowerShellItems()
	{
		string path = Path;
		if (path == null || !path.EndsWith("powershell", StringComparison.InvariantCultureIgnoreCase))
		{
			string path2 = Path;
			if (path2 == null || !path2.EndsWith("powershell.exe", StringComparison.InvariantCultureIgnoreCase))
			{
				goto IL_00bb;
			}
		}
		string arguments = Arguments;
		if (arguments != null && arguments.Contains("TSML_20140424"))
		{
			Match match = Regex.Match(Arguments, "<# TSML_20140424:(?<type>\\w+) #> (?<cmd>.+)}\"$");
			if (match.Success)
			{
				return new string[2]
				{
					match.Groups["type"].Value,
					match.Groups["cmd"].Value
				};
			}
		}
		goto IL_00bb;
		IL_00bb:
		return null;
	}
}
