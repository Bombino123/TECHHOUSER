namespace SharpDX.Multimedia;

public enum WaveFormatEncoding : short
{
	Unknown = 0,
	Adpcm = 2,
	IeeeFloat = 3,
	Vselp = 4,
	IbmCvsd = 5,
	Alaw = 6,
	Mulaw = 7,
	Dts = 8,
	Drm = 9,
	Wmavoice9 = 10,
	Wmavoice10 = 11,
	OkiAdpcm = 16,
	DviAdpcm = 17,
	ImaAdpcm = 17,
	MediaspaceAdpcm = 18,
	SierraAdpcm = 19,
	G723Adpcm = 20,
	Digistd = 21,
	Digifix = 22,
	DialogicOkiAdpcm = 23,
	MediavisionAdpcm = 24,
	CuCodec = 25,
	HpDynVoice = 26,
	YamahaAdpcm = 32,
	Sonarc = 33,
	DspgroupTruespeech = 34,
	Echosc1 = 35,
	AudiofileAf36 = 36,
	Aptx = 37,
	AudiofileAf10 = 38,
	Prosody1612 = 39,
	Lrc = 40,
	DolbyAc2 = 48,
	DefaultGsm610 = 49,
	Msnaudio = 50,
	AntexAdpcme = 51,
	ControlResVqlpc = 52,
	Digireal = 53,
	Digiadpcm = 54,
	ControlResCr10 = 55,
	NmsVbxadpcm = 56,
	CsImaadpcm = 57,
	Echosc3 = 58,
	RockwellAdpcm = 59,
	RockwellDigitalk = 60,
	Xebec = 61,
	G721Adpcm = 64,
	G728Celp = 65,
	Msg723 = 66,
	IntelG7231 = 67,
	IntelG729 = 68,
	SharpG726 = 69,
	Mpeg = 80,
	Rt24 = 82,
	Pac = 83,
	Mpeglayer3 = 85,
	LucentG723 = 89,
	Cirrus = 96,
	Espcm = 97,
	Voxware = 98,
	CanopusAtrac = 99,
	G726Adpcm = 100,
	G722Adpcm = 101,
	Dsat = 102,
	DsatDisplay = 103,
	VoxwareByteAligned = 105,
	VoxwareAc8 = 112,
	VoxwareAc10 = 113,
	VoxwareAc16 = 114,
	VoxwareAc20 = 115,
	VoxwareRt24 = 116,
	VoxwareRt29 = 117,
	VoxwareRt29hw = 118,
	VoxwareVr12 = 119,
	VoxwareVr18 = 120,
	VoxwareTq40 = 121,
	VoxwareSc3 = 122,
	VoxwareSc31 = 123,
	Softsound = 128,
	VoxwareTq60 = 129,
	Msrt24 = 130,
	G729A = 131,
	MviMvi2 = 132,
	DfG726 = 133,
	DfGsm610 = 134,
	Isiaudio = 136,
	Onlive = 137,
	MultitudeFtSx20 = 138,
	InfocomItsG721Adpcm = 139,
	ConvediaG729 = 140,
	Congruency = 141,
	Sbc24 = 145,
	DolbyAc3Spdif = 146,
	MediasonicG723 = 147,
	Prosody8kbps = 148,
	ZyxelAdpcm = 151,
	PhilipsLpcbb = 152,
	Packed = 153,
	MaldenPhonytalk = 160,
	RacalRecorderGsm = 161,
	RacalRecorderG720A = 162,
	RacalRecorderG7231 = 163,
	RacalRecorderTetraAcelp = 164,
	NecAac = 176,
	RawAac1 = 255,
	RhetorexAdpcm = 256,
	Irat = 257,
	VivoG723 = 273,
	VivoSiren = 274,
	PhilipsCelp = 288,
	PhilipsGrundig = 289,
	DigitalG723 = 291,
	SanyoLdAdpcm = 293,
	SiprolabAceplnet = 304,
	SiprolabAcelp4800 = 305,
	SiprolabAcelp8v3 = 306,
	SiprolabG729 = 307,
	SiprolabG729A = 308,
	SiprolabKelvin = 309,
	VoiceageAmr = 310,
	G726ADPCM = 320,
	DictaphoneCelp68 = 321,
	DictaphoneCelp54 = 322,
	QualcommPurevoice = 336,
	QualcommHalfrate = 337,
	Tubgsm = 341,
	Msaudio1 = 352,
	Wmaudio2 = 353,
	Wmaudio3 = 354,
	WmaudioLossless = 355,
	Wmaspdif = 356,
	UnisysNapAdpcm = 368,
	UnisysNapUlaw = 369,
	UnisysNapAlaw = 370,
	UnisysNap16k = 371,
	SycomAcmSyc008 = 372,
	SycomAcmSyc701G726L = 373,
	SycomAcmSyc701Celp54 = 374,
	SycomAcmSyc701Celp68 = 375,
	KnowledgeAdventureAdpcm = 376,
	FraunhoferIisMpeg2Aac = 384,
	DtsDs = 400,
	CreativeAdpcm = 512,
	CreativeFastspeech8 = 514,
	CreativeFastspeech10 = 515,
	UherAdpcm = 528,
	UleadDvAudio = 533,
	UleadDvAudio1 = 534,
	Quarterdeck = 544,
	IlinkVc = 560,
	RawSport = 576,
	EsstAc3 = 577,
	GenericPassthru = 585,
	IpiHsx = 592,
	IpiRpelp = 593,
	Cs2 = 608,
	SonyScx = 624,
	SonyScy = 625,
	SonyAtrac3 = 626,
	SonySpc = 627,
	TelumAudio = 640,
	TelumIaAudio = 641,
	NorcomVoiceSystemsAdpcm = 645,
	FmTownsSnd = 768,
	Micronas = 848,
	MicronasCelp833 = 849,
	BtvDigital = 1024,
	IntelMusicCoder = 1025,
	IndeoAudio = 1026,
	QdesignMusic = 1104,
	On2Vp7Audio = 1280,
	On2Vp6Audio = 1281,
	VmeVmpcm = 1664,
	Tpc = 1665,
	LightwaveLossless = 2222,
	Oligsm = 4096,
	Oliadpcm = 4097,
	Olicelp = 4098,
	Olisbc = 4099,
	Oliopr = 4100,
	LhCodec = 4352,
	LhCodecCelp = 4353,
	LhCodecSbc8 = 4354,
	LhCodecSbc12 = 4355,
	LhCodecSbc16 = 4356,
	Norris = 5120,
	Isiaudio2 = 5121,
	SoundspaceMusicompress = 5376,
	MpegAdtsAac = 5632,
	MpegRawAac = 5633,
	MpegLoas = 5634,
	NokiaMpegAdtsAac = 5640,
	NokiaMpegRawAac = 5641,
	VodafoneMpegAdtsAac = 5642,
	VodafoneMpegRawAac = 5643,
	MpegHeaac = 5648,
	VoxwareRt24Speech = 6172,
	SonicfoundryLossless = 6513,
	InningsTelecomAdpcm = 6521,
	LucentSx8300p = 7175,
	LucentSx5363s = 7180,
	Cuseeme = 7939,
	NtcsoftAlf2cmAcm = 8132,
	Dvm = 8192,
	Dts2 = 8193,
	Makeavis = 13075,
	DivioMpeg4Aac = 16707,
	NokiaAdaptiveMultirate = 16897,
	DivioG726 = 16963,
	LeadSpeech = 17228,
	LeadVorbis = 22092,
	WavpackAudio = 22358,
	Alac = 27745,
	OggVorbisMode1 = 26447,
	OggVorbisMode2 = 26448,
	OggVorbisMode3 = 26449,
	OggVorbisMode1Plus = 26479,
	OggVorbisMode2Plus = 26480,
	OggVorbisMode3Plus = 26481,
	Tag3COMNbx = 28672,
	Opus = 28751,
	FaadAac = 28781,
	AmrNb = 29537,
	AmrWb = 29538,
	AmrWp = 29539,
	GsmAmrCbr = 31265,
	GsmAmrVbrSid = 31266,
	ComverseInfosysG7231 = -24320,
	ComverseInfosysAvqsbc = -24319,
	ComverseInfosysSbc = -24318,
	SymbolG729A = -24317,
	VoiceageAmrWb = -24316,
	IngenientG726 = -24315,
	Mpeg4Aac = -24314,
	EncoreG726 = -24313,
	ZollAsao = -24312,
	SpeexVoice = -24311,
	VianixMasc = -24310,
	Wm9SpectrumAnalyzer = -24309,
	WmfSpectrumAnayzer = -24308,
	Gsm610 = -24307,
	Gsm620 = -24306,
	Gsm660 = -24305,
	Gsm690 = -24304,
	GsmAdaptiveMultirateWb = -24303,
	PolycomG722 = -24302,
	PolycomG728 = -24301,
	PolycomG729A = -24300,
	PolycomSiren = -24299,
	GlobalIpIlbc = -24298,
	RadiotimeTimeShiftRadio = -24297,
	NiceAca = -24296,
	NiceAdpcm = -24295,
	VocordG721 = -24294,
	VocordG726 = -24293,
	VocordG7221 = -24292,
	VocordG728 = -24291,
	VocordG729 = -24290,
	VocordG729A = -24289,
	VocordG7231 = -24288,
	VocordLbc = -24287,
	NiceG728 = -24286,
	FraceTelecomG729 = -24285,
	Codian = -24284,
	Flac = -3668,
	Extensible = -2,
	Development = -1,
	Pcm = 1
}
