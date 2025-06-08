using System;
using System.Globalization;
using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

internal class CultureSwitcher : IDisposable
{
	private readonly CultureInfo cur;

	private readonly CultureInfo curUI;

	public CultureSwitcher([NotNull] CultureInfo culture)
	{
		cur = Thread.CurrentThread.CurrentCulture;
		curUI = Thread.CurrentThread.CurrentUICulture;
		Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = culture);
	}

	public void Dispose()
	{
		Thread.CurrentThread.CurrentCulture = cur;
		Thread.CurrentThread.CurrentUICulture = curUI;
	}
}
