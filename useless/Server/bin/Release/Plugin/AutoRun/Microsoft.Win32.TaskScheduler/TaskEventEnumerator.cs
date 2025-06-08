using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public sealed class TaskEventEnumerator : IEnumerator<TaskEvent>, IDisposable, IEnumerator
{
	private EventRecord curRec;

	private EventLogReader log;

	public TaskEvent Current => new TaskEvent(curRec);

	object IEnumerator.Current => Current;

	internal TaskEventEnumerator([NotNull] EventLogReader log)
	{
		this.log = log;
	}

	public void Dispose()
	{
		log.CancelReading();
		log.Dispose();
		log = null;
	}

	public bool MoveNext()
	{
		return (curRec = log.ReadEvent()) != null;
	}

	public void Reset()
	{
		log.Seek(SeekOrigin.Begin, 0L);
	}

	public void Seek(EventBookmark bookmark, long offset = 0L)
	{
		log.Seek(bookmark, offset);
	}

	public void Seek(SeekOrigin origin, long offset)
	{
		log.Seek(origin, offset);
	}
}
