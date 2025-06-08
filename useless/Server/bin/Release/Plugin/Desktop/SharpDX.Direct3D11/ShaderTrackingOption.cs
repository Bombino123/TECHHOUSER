namespace SharpDX.Direct3D11;

public enum ShaderTrackingOption
{
	Ignore = 0,
	TrackUninitialized = 1,
	TrackRaw = 2,
	TrackWar = 4,
	TrackWaw = 8,
	AllowSame = 16,
	TrackAtomicConsistency = 32,
	TrackRawAcrossThreadgroups = 64,
	TrackWarAcrossThreadgroups = 128,
	TrackWawAcrossThreadgroups = 256,
	TrackAtomicConsistencyAcrossThreadgroups = 512,
	UnorderedAccessViewSpecificFlags = 960,
	AllHazards = 1006,
	AllHazardsAllowingSame = 1022,
	AllOptions = 1023
}
