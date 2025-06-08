namespace SharpDX.Direct3D11;

public enum BusType
{
	TypeOther = 0,
	TypePci = 1,
	TypePcix = 2,
	TypePciexpress = 3,
	TypeAgp = 4,
	ImplModifierInsideOfChipset = 65536,
	ImplModifierTracksOnMotherBoardToChip = 131072,
	ImplModifierTracksOnMotherBoardToSocket = 196608,
	ImplModifierDaughterBoardConnector = 262144,
	ImplModifierDaughterBoardConnectorInsideOfNuae = 327680,
	ImplModifierNonStandard = int.MinValue
}
