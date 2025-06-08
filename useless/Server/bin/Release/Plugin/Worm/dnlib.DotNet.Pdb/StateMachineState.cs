using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public enum StateMachineState
{
	FirstResumableAsyncIteratorState = -4,
	InitialAsyncIteratorState = -3,
	FirstIteratorFinalizeState = -3,
	FinishedState = -2,
	NotStartedOrRunningState = -1,
	FirstUnusedState = 0,
	FirstResumableAsyncState = 0,
	InitialIteratorState = 0,
	FirstResumableIteratorState = 1
}
