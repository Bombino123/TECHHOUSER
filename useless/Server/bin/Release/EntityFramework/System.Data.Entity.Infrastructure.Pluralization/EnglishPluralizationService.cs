using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Design.PluralizationServices;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Data.Entity.Infrastructure.Pluralization;

public sealed class EnglishPluralizationService : IPluralizationService
{
	private readonly BidirectionalDictionary<string, string> _userDictionary;

	private readonly StringBidirectionalDictionary _irregularPluralsPluralizationService;

	private readonly StringBidirectionalDictionary _assimilatedClassicalInflectionPluralizationService;

	private readonly StringBidirectionalDictionary _oSuffixPluralizationService;

	private readonly StringBidirectionalDictionary _classicalInflectionPluralizationService;

	private readonly StringBidirectionalDictionary _irregularVerbPluralizationService;

	private readonly StringBidirectionalDictionary _wordsEndingWithSePluralizationService;

	private readonly StringBidirectionalDictionary _wordsEndingWithSisPluralizationService;

	private readonly List<string> _knownSingluarWords;

	private readonly List<string> _knownPluralWords;

	private readonly CultureInfo _culture = new CultureInfo("en-US");

	private readonly string[] _uninflectiveSuffixes = new string[7] { "fish", "ois", "sheep", "deer", "pos", "itis", "ism" };

	private readonly string[] _uninflectiveWords = new string[84]
	{
		"bison", "flounder", "pliers", "bream", "gallows", "proceedings", "breeches", "graffiti", "rabies", "britches",
		"headquarters", "salmon", "carp", "herpes", "scissors", "chassis", "high-jinks", "sea-bass", "clippers", "homework",
		"series", "cod", "innings", "shears", "contretemps", "jackanapes", "species", "corps", "mackerel", "swine",
		"debris", "measles", "trout", "diabetes", "mews", "tuna", "djinn", "mumps", "whiting", "eland",
		"news", "wildebeest", "elk", "pincers", "police", "hair", "ice", "chaos", "milk", "cotton",
		"corn", "millet", "hay", "pneumonoultramicroscopicsilicovolcanoconiosis", "information", "rice", "tobacco", "aircraft", "rabies", "scabies",
		"diabetes", "traffic", "cotton", "corn", "millet", "rice", "hay", "hemp", "tobacco", "cabbage",
		"okra", "broccoli", "asparagus", "lettuce", "beef", "pork", "venison", "bison", "mutton", "cattle",
		"offspring", "molasses", "shambles", "shingles"
	};

	private readonly Dictionary<string, string> _irregularVerbList = new Dictionary<string, string>
	{
		{ "am", "are" },
		{ "are", "are" },
		{ "is", "are" },
		{ "was", "were" },
		{ "were", "were" },
		{ "has", "have" },
		{ "have", "have" }
	};

	private readonly List<string> _pronounList = new List<string>
	{
		"I", "we", "you", "he", "she", "they", "it", "me", "us", "him",
		"her", "them", "myself", "ourselves", "yourself", "himself", "herself", "itself", "oneself", "oneselves",
		"my", "our", "your", "his", "their", "its", "mine", "yours", "hers", "theirs",
		"this", "that", "these", "those", "all", "another", "any", "anybody", "anyone", "anything",
		"both", "each", "other", "either", "everyone", "everybody", "everything", "most", "much", "nothing",
		"nobody", "none", "one", "others", "some", "somebody", "someone", "something", "what", "whatever",
		"which", "whichever", "who", "whoever", "whom", "whomever", "whose"
	};

	private readonly Dictionary<string, string> _irregularPluralsList = new Dictionary<string, string>
	{
		{ "brother", "brothers" },
		{ "child", "children" },
		{ "cow", "cows" },
		{ "ephemeris", "ephemerides" },
		{ "genie", "genies" },
		{ "money", "moneys" },
		{ "mongoose", "mongooses" },
		{ "mythos", "mythoi" },
		{ "octopus", "octopuses" },
		{ "ox", "oxen" },
		{ "soliloquy", "soliloquies" },
		{ "trilby", "trilbys" },
		{ "crisis", "crises" },
		{ "synopsis", "synopses" },
		{ "rose", "roses" },
		{ "gas", "gases" },
		{ "bus", "buses" },
		{ "axis", "axes" },
		{ "memo", "memos" },
		{ "casino", "casinos" },
		{ "silo", "silos" },
		{ "stereo", "stereos" },
		{ "studio", "studios" },
		{ "lens", "lenses" },
		{ "alias", "aliases" },
		{ "pie", "pies" },
		{ "corpus", "corpora" },
		{ "viscus", "viscera" },
		{ "hippopotamus", "hippopotami" },
		{ "trace", "traces" },
		{ "person", "people" },
		{ "chilli", "chillies" },
		{ "analysis", "analyses" },
		{ "basis", "bases" },
		{ "neurosis", "neuroses" },
		{ "oasis", "oases" },
		{ "synthesis", "syntheses" },
		{ "thesis", "theses" },
		{ "pneumonoultramicroscopicsilicovolcanoconiosis", "pneumonoultramicroscopicsilicovolcanoconioses" },
		{ "status", "statuses" },
		{ "prospectus", "prospectuses" },
		{ "change", "changes" },
		{ "lie", "lies" },
		{ "calorie", "calories" },
		{ "freebie", "freebies" },
		{ "case", "cases" },
		{ "house", "houses" },
		{ "valve", "valves" },
		{ "cloth", "clothes" }
	};

	private readonly Dictionary<string, string> _assimilatedClassicalInflectionList = new Dictionary<string, string>
	{
		{ "alumna", "alumnae" },
		{ "alga", "algae" },
		{ "vertebra", "vertebrae" },
		{ "codex", "codices" },
		{ "murex", "murices" },
		{ "silex", "silices" },
		{ "aphelion", "aphelia" },
		{ "hyperbaton", "hyperbata" },
		{ "perihelion", "perihelia" },
		{ "asyndeton", "asyndeta" },
		{ "noumenon", "noumena" },
		{ "phenomenon", "phenomena" },
		{ "criterion", "criteria" },
		{ "organon", "organa" },
		{ "prolegomenon", "prolegomena" },
		{ "agendum", "agenda" },
		{ "datum", "data" },
		{ "extremum", "extrema" },
		{ "bacterium", "bacteria" },
		{ "desideratum", "desiderata" },
		{ "stratum", "strata" },
		{ "candelabrum", "candelabra" },
		{ "erratum", "errata" },
		{ "ovum", "ova" },
		{ "forum", "fora" },
		{ "addendum", "addenda" },
		{ "stadium", "stadia" },
		{ "automaton", "automata" },
		{ "polyhedron", "polyhedra" }
	};

	private readonly Dictionary<string, string> _oSuffixList = new Dictionary<string, string>
	{
		{ "albino", "albinos" },
		{ "generalissimo", "generalissimos" },
		{ "manifesto", "manifestos" },
		{ "archipelago", "archipelagos" },
		{ "ghetto", "ghettos" },
		{ "medico", "medicos" },
		{ "armadillo", "armadillos" },
		{ "guano", "guanos" },
		{ "octavo", "octavos" },
		{ "commando", "commandos" },
		{ "inferno", "infernos" },
		{ "photo", "photos" },
		{ "ditto", "dittos" },
		{ "jumbo", "jumbos" },
		{ "pro", "pros" },
		{ "dynamo", "dynamos" },
		{ "lingo", "lingos" },
		{ "quarto", "quartos" },
		{ "embryo", "embryos" },
		{ "lumbago", "lumbagos" },
		{ "rhino", "rhinos" },
		{ "fiasco", "fiascos" },
		{ "magneto", "magnetos" },
		{ "stylo", "stylos" }
	};

	private readonly Dictionary<string, string> _classicalInflectionList = new Dictionary<string, string>
	{
		{ "stamen", "stamina" },
		{ "foramen", "foramina" },
		{ "lumen", "lumina" },
		{ "anathema", "anathemata" },
		{ "enema", "enemata" },
		{ "oedema", "oedemata" },
		{ "bema", "bemata" },
		{ "enigma", "enigmata" },
		{ "sarcoma", "sarcomata" },
		{ "carcinoma", "carcinomata" },
		{ "gumma", "gummata" },
		{ "schema", "schemata" },
		{ "charisma", "charismata" },
		{ "lemma", "lemmata" },
		{ "soma", "somata" },
		{ "diploma", "diplomata" },
		{ "lymphoma", "lymphomata" },
		{ "stigma", "stigmata" },
		{ "dogma", "dogmata" },
		{ "magma", "magmata" },
		{ "stoma", "stomata" },
		{ "drama", "dramata" },
		{ "melisma", "melismata" },
		{ "trauma", "traumata" },
		{ "edema", "edemata" },
		{ "miasma", "miasmata" },
		{ "abscissa", "abscissae" },
		{ "formula", "formulae" },
		{ "medusa", "medusae" },
		{ "amoeba", "amoebae" },
		{ "hydra", "hydrae" },
		{ "nebula", "nebulae" },
		{ "antenna", "antennae" },
		{ "hyperbola", "hyperbolae" },
		{ "nova", "novae" },
		{ "aurora", "aurorae" },
		{ "lacuna", "lacunae" },
		{ "parabola", "parabolae" },
		{ "apex", "apices" },
		{ "latex", "latices" },
		{ "vertex", "vertices" },
		{ "cortex", "cortices" },
		{ "pontifex", "pontifices" },
		{ "vortex", "vortices" },
		{ "index", "indices" },
		{ "simplex", "simplices" },
		{ "iris", "irides" },
		{ "clitoris", "clitorides" },
		{ "alto", "alti" },
		{ "contralto", "contralti" },
		{ "soprano", "soprani" },
		{ "basso", "bassi" },
		{ "crescendo", "crescendi" },
		{ "tempo", "tempi" },
		{ "canto", "canti" },
		{ "solo", "soli" },
		{ "aquarium", "aquaria" },
		{ "interregnum", "interregna" },
		{ "quantum", "quanta" },
		{ "compendium", "compendia" },
		{ "lustrum", "lustra" },
		{ "rostrum", "rostra" },
		{ "consortium", "consortia" },
		{ "maximum", "maxima" },
		{ "spectrum", "spectra" },
		{ "cranium", "crania" },
		{ "medium", "media" },
		{ "speculum", "specula" },
		{ "curriculum", "curricula" },
		{ "memorandum", "memoranda" },
		{ "stadium", "stadia" },
		{ "dictum", "dicta" },
		{ "millenium", "millenia" },
		{ "trapezium", "trapezia" },
		{ "emporium", "emporia" },
		{ "minimum", "minima" },
		{ "ultimatum", "ultimata" },
		{ "enconium", "enconia" },
		{ "momentum", "momenta" },
		{ "vacuum", "vacua" },
		{ "gymnasium", "gymnasia" },
		{ "optimum", "optima" },
		{ "velum", "vela" },
		{ "honorarium", "honoraria" },
		{ "phylum", "phyla" },
		{ "focus", "foci" },
		{ "nimbus", "nimbi" },
		{ "succubus", "succubi" },
		{ "fungus", "fungi" },
		{ "nucleolus", "nucleoli" },
		{ "torus", "tori" },
		{ "genius", "genii" },
		{ "radius", "radii" },
		{ "umbilicus", "umbilici" },
		{ "incubus", "incubi" },
		{ "stylus", "styli" },
		{ "uterus", "uteri" },
		{ "stimulus", "stimuli" },
		{ "apparatus", "apparatus" },
		{ "impetus", "impetus" },
		{ "prospectus", "prospectus" },
		{ "cantus", "cantus" },
		{ "nexus", "nexus" },
		{ "sinus", "sinus" },
		{ "coitus", "coitus" },
		{ "plexus", "plexus" },
		{ "status", "status" },
		{ "hiatus", "hiatus" },
		{ "afreet", "afreeti" },
		{ "afrit", "afriti" },
		{ "efreet", "efreeti" },
		{ "cherub", "cherubim" },
		{ "goy", "goyim" },
		{ "seraph", "seraphim" },
		{ "alumnus", "alumni" }
	};

	private readonly List<string> _knownConflictingPluralList = new List<string> { "they", "them", "their", "have", "were", "yourself", "are" };

	private readonly Dictionary<string, string> _wordsEndingWithSeList = new Dictionary<string, string>
	{
		{ "house", "houses" },
		{ "case", "cases" },
		{ "enterprise", "enterprises" },
		{ "purchase", "purchases" },
		{ "surprise", "surprises" },
		{ "release", "releases" },
		{ "disease", "diseases" },
		{ "promise", "promises" },
		{ "refuse", "refuses" },
		{ "whose", "whoses" },
		{ "phase", "phases" },
		{ "noise", "noises" },
		{ "nurse", "nurses" },
		{ "rose", "roses" },
		{ "franchise", "franchises" },
		{ "supervise", "supervises" },
		{ "farmhouse", "farmhouses" },
		{ "suitcase", "suitcases" },
		{ "recourse", "recourses" },
		{ "impulse", "impulses" },
		{ "license", "licenses" },
		{ "diocese", "dioceses" },
		{ "excise", "excises" },
		{ "demise", "demises" },
		{ "blouse", "blouses" },
		{ "bruise", "bruises" },
		{ "misuse", "misuses" },
		{ "curse", "curses" },
		{ "prose", "proses" },
		{ "purse", "purses" },
		{ "goose", "gooses" },
		{ "tease", "teases" },
		{ "poise", "poises" },
		{ "vase", "vases" },
		{ "fuse", "fuses" },
		{ "muse", "muses" },
		{ "slaughterhouse", "slaughterhouses" },
		{ "clearinghouse", "clearinghouses" },
		{ "endonuclease", "endonucleases" },
		{ "steeplechase", "steeplechases" },
		{ "metamorphose", "metamorphoses" },
		{ "intercourse", "intercourses" },
		{ "commonsense", "commonsenses" },
		{ "intersperse", "intersperses" },
		{ "merchandise", "merchandises" },
		{ "phosphatase", "phosphatases" },
		{ "summerhouse", "summerhouses" },
		{ "watercourse", "watercourses" },
		{ "catchphrase", "catchphrases" },
		{ "compromise", "compromises" },
		{ "greenhouse", "greenhouses" },
		{ "lighthouse", "lighthouses" },
		{ "paraphrase", "paraphrases" },
		{ "mayonnaise", "mayonnaises" },
		{ "racecourse", "racecourses" },
		{ "apocalypse", "apocalypses" },
		{ "courthouse", "courthouses" },
		{ "powerhouse", "powerhouses" },
		{ "storehouse", "storehouses" },
		{ "glasshouse", "glasshouses" },
		{ "hypotenuse", "hypotenuses" },
		{ "peroxidase", "peroxidases" },
		{ "pillowcase", "pillowcases" },
		{ "roundhouse", "roundhouses" },
		{ "streetwise", "streetwises" },
		{ "expertise", "expertises" },
		{ "discourse", "discourses" },
		{ "warehouse", "warehouses" },
		{ "staircase", "staircases" },
		{ "workhouse", "workhouses" },
		{ "briefcase", "briefcases" },
		{ "clubhouse", "clubhouses" },
		{ "clockwise", "clockwises" },
		{ "concourse", "concourses" },
		{ "playhouse", "playhouses" },
		{ "turquoise", "turquoises" },
		{ "boathouse", "boathouses" },
		{ "cellulose", "celluloses" },
		{ "epitomise", "epitomises" },
		{ "gatehouse", "gatehouses" },
		{ "grandiose", "grandioses" },
		{ "menopause", "menopauses" },
		{ "penthouse", "penthouses" },
		{ "racehorse", "racehorses" },
		{ "transpose", "transposes" },
		{ "almshouse", "almshouses" },
		{ "customise", "customises" },
		{ "footloose", "footlooses" },
		{ "galvanise", "galvanises" },
		{ "princesse", "princesses" },
		{ "universe", "universes" },
		{ "workhorse", "workhorses" }
	};

	private readonly Dictionary<string, string> _wordsEndingWithSisList = new Dictionary<string, string>
	{
		{ "analysis", "analyses" },
		{ "crisis", "crises" },
		{ "basis", "bases" },
		{ "atherosclerosis", "atheroscleroses" },
		{ "electrophoresis", "electrophoreses" },
		{ "psychoanalysis", "psychoanalyses" },
		{ "photosynthesis", "photosyntheses" },
		{ "amniocentesis", "amniocenteses" },
		{ "metamorphosis", "metamorphoses" },
		{ "toxoplasmosis", "toxoplasmoses" },
		{ "endometriosis", "endometrioses" },
		{ "tuberculosis", "tuberculoses" },
		{ "pathogenesis", "pathogeneses" },
		{ "osteoporosis", "osteoporoses" },
		{ "parenthesis", "parentheses" },
		{ "anastomosis", "anastomoses" },
		{ "peristalsis", "peristalses" },
		{ "hypothesis", "hypotheses" },
		{ "antithesis", "antitheses" },
		{ "apotheosis", "apotheoses" },
		{ "thrombosis", "thromboses" },
		{ "diagnosis", "diagnoses" },
		{ "synthesis", "syntheses" },
		{ "paralysis", "paralyses" },
		{ "prognosis", "prognoses" },
		{ "cirrhosis", "cirrhoses" },
		{ "sclerosis", "scleroses" },
		{ "psychosis", "psychoses" },
		{ "apoptosis", "apoptoses" },
		{ "symbiosis", "symbioses" }
	};

	public EnglishPluralizationService()
	{
		_userDictionary = new BidirectionalDictionary<string, string>();
		_irregularPluralsPluralizationService = new StringBidirectionalDictionary(_irregularPluralsList);
		_assimilatedClassicalInflectionPluralizationService = new StringBidirectionalDictionary(_assimilatedClassicalInflectionList);
		_oSuffixPluralizationService = new StringBidirectionalDictionary(_oSuffixList);
		_classicalInflectionPluralizationService = new StringBidirectionalDictionary(_classicalInflectionList);
		_wordsEndingWithSePluralizationService = new StringBidirectionalDictionary(_wordsEndingWithSeList);
		_wordsEndingWithSisPluralizationService = new StringBidirectionalDictionary(_wordsEndingWithSisList);
		_irregularVerbPluralizationService = new StringBidirectionalDictionary(_irregularVerbList);
		_knownSingluarWords = new List<string>(_irregularPluralsList.Keys.Concat(_assimilatedClassicalInflectionList.Keys).Concat(_oSuffixList.Keys).Concat(_classicalInflectionList.Keys)
			.Concat(_irregularVerbList.Keys)
			.Concat(_uninflectiveWords)
			.Except(_knownConflictingPluralList));
		_knownPluralWords = new List<string>(_irregularPluralsList.Values.Concat(_assimilatedClassicalInflectionList.Values).Concat(_oSuffixList.Values).Concat(_classicalInflectionList.Values)
			.Concat(_irregularVerbList.Values)
			.Concat(_uninflectiveWords));
	}

	public EnglishPluralizationService(IEnumerable<CustomPluralizationEntry> userDictionaryEntries)
		: this()
	{
		Check.NotNull(userDictionaryEntries, "userDictionaryEntries");
		userDictionaryEntries.Each(delegate(CustomPluralizationEntry entry)
		{
			_userDictionary.AddValue(entry.Singular, entry.Plural);
		});
	}

	public string Pluralize(string word)
	{
		return Capitalize(word, InternalPluralize);
	}

	private string InternalPluralize(string word)
	{
		if (_userDictionary.ExistsInFirst(word))
		{
			return _userDictionary.GetSecondValue(word);
		}
		if (IsNoOpWord(word))
		{
			return word;
		}
		string prefixWord;
		string suffixWord = GetSuffixWord(word, out prefixWord);
		if (IsNoOpWord(suffixWord))
		{
			return prefixWord + suffixWord;
		}
		if (IsUninflective(suffixWord))
		{
			return prefixWord + suffixWord;
		}
		if (_knownPluralWords.Contains(suffixWord.ToLowerInvariant()) || IsPlural(suffixWord))
		{
			return prefixWord + suffixWord;
		}
		if (_irregularPluralsPluralizationService.ExistsInFirst(suffixWord))
		{
			return prefixWord + _irregularPluralsPluralizationService.GetSecondValue(suffixWord);
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "man" }, (string s) => s.Remove(s.Length - 2, 2) + "en", _culture, out var newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "louse", "mouse" }, (string s) => s.Remove(s.Length - 4, 4) + "ice", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "tooth" }, (string s) => s.Remove(s.Length - 4, 4) + "eeth", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "goose" }, (string s) => s.Remove(s.Length - 4, 4) + "eese", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "foot" }, (string s) => s.Remove(s.Length - 3, 3) + "eet", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "zoon" }, (string s) => s.Remove(s.Length - 3, 3) + "oa", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "cis", "sis", "xis" }, (string s) => s.Remove(s.Length - 2, 2) + "es", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (_assimilatedClassicalInflectionPluralizationService.ExistsInFirst(suffixWord))
		{
			return prefixWord + _assimilatedClassicalInflectionPluralizationService.GetSecondValue(suffixWord);
		}
		if (_classicalInflectionPluralizationService.ExistsInFirst(suffixWord))
		{
			return prefixWord + _classicalInflectionPluralizationService.GetSecondValue(suffixWord);
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "trix" }, (string s) => s.Remove(s.Length - 1, 1) + "ces", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "eau", "ieu" }, (string s) => s + "x", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "inx", "anx", "ynx" }, (string s) => s.Remove(s.Length - 1, 1) + "ges", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "ch", "sh", "ss" }, (string s) => s + "es", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "alf", "elf", "olf", "eaf", "arf" }, (string s) => (!s.EndsWith("deaf", ignoreCase: true, _culture)) ? (s.Remove(s.Length - 1, 1) + "ves") : s, _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "nife", "life", "wife" }, (string s) => s.Remove(s.Length - 2, 2) + "ves", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "ay", "ey", "iy", "oy", "uy" }, (string s) => s + "s", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (suffixWord.EndsWith("y", ignoreCase: true, _culture))
		{
			return prefixWord + suffixWord.Remove(suffixWord.Length - 1, 1) + "ies";
		}
		if (_oSuffixPluralizationService.ExistsInFirst(suffixWord))
		{
			return prefixWord + _oSuffixPluralizationService.GetSecondValue(suffixWord);
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "ao", "eo", "io", "oo", "uo" }, (string s) => s + "s", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (suffixWord.EndsWith("o", ignoreCase: true, _culture))
		{
			return prefixWord + suffixWord + "es";
		}
		if (suffixWord.EndsWith("x", ignoreCase: true, _culture))
		{
			return prefixWord + suffixWord + "es";
		}
		return prefixWord + suffixWord + "s";
	}

	public string Singularize(string word)
	{
		return Capitalize(word, InternalSingularize);
	}

	private string InternalSingularize(string word)
	{
		if (_userDictionary.ExistsInSecond(word))
		{
			return _userDictionary.GetFirstValue(word);
		}
		if (IsNoOpWord(word))
		{
			return word;
		}
		string prefixWord;
		string suffixWord = GetSuffixWord(word, out prefixWord);
		if (IsNoOpWord(suffixWord))
		{
			return prefixWord + suffixWord;
		}
		if (IsUninflective(suffixWord))
		{
			return prefixWord + suffixWord;
		}
		if (_knownSingluarWords.Contains(suffixWord.ToLowerInvariant()))
		{
			return prefixWord + suffixWord;
		}
		if (_irregularVerbPluralizationService.ExistsInSecond(suffixWord))
		{
			return prefixWord + _irregularVerbPluralizationService.GetFirstValue(suffixWord);
		}
		if (_irregularPluralsPluralizationService.ExistsInSecond(suffixWord))
		{
			return prefixWord + _irregularPluralsPluralizationService.GetFirstValue(suffixWord);
		}
		if (_wordsEndingWithSisPluralizationService.ExistsInSecond(suffixWord))
		{
			return prefixWord + _wordsEndingWithSisPluralizationService.GetFirstValue(suffixWord);
		}
		if (_wordsEndingWithSePluralizationService.ExistsInSecond(suffixWord))
		{
			return prefixWord + _wordsEndingWithSePluralizationService.GetFirstValue(suffixWord);
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "men" }, (string s) => s.Remove(s.Length - 2, 2) + "an", _culture, out var newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "lice", "mice" }, (string s) => s.Remove(s.Length - 3, 3) + "ouse", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "teeth" }, (string s) => s.Remove(s.Length - 4, 4) + "ooth", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "geese" }, (string s) => s.Remove(s.Length - 4, 4) + "oose", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "feet" }, (string s) => s.Remove(s.Length - 3, 3) + "oot", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "zoa" }, (string s) => s.Remove(s.Length - 2, 2) + "oon", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "ches", "shes", "sses" }, (string s) => s.Remove(s.Length - 2, 2), _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (_assimilatedClassicalInflectionPluralizationService.ExistsInSecond(suffixWord))
		{
			return prefixWord + _assimilatedClassicalInflectionPluralizationService.GetFirstValue(suffixWord);
		}
		if (_classicalInflectionPluralizationService.ExistsInSecond(suffixWord))
		{
			return prefixWord + _classicalInflectionPluralizationService.GetFirstValue(suffixWord);
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "trices" }, (string s) => s.Remove(s.Length - 3, 3) + "x", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "eaux", "ieux" }, (string s) => s.Remove(s.Length - 1, 1), _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "inges", "anges", "ynges" }, (string s) => s.Remove(s.Length - 3, 3) + "x", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "alves", "elves", "olves", "eaves", "arves" }, (string s) => s.Remove(s.Length - 3, 3) + "f", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "nives", "lives", "wives" }, (string s) => s.Remove(s.Length - 3, 3) + "fe", _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "ays", "eys", "iys", "oys", "uys" }, (string s) => s.Remove(s.Length - 1, 1), _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (suffixWord.EndsWith("ies", ignoreCase: true, _culture))
		{
			return prefixWord + suffixWord.Remove(suffixWord.Length - 3, 3) + "y";
		}
		if (_oSuffixPluralizationService.ExistsInSecond(suffixWord))
		{
			return prefixWord + _oSuffixPluralizationService.GetFirstValue(suffixWord);
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "aos", "eos", "ios", "oos", "uos" }, (string s) => suffixWord.Remove(suffixWord.Length - 1, 1), _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "ces" }, (string s) => s.Remove(s.Length - 1, 1), _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (PluralizationServiceUtil.TryInflectOnSuffixInWord(suffixWord, new List<string> { "ces", "ses", "xes" }, (string s) => s.Remove(s.Length - 2, 2), _culture, out newWord))
		{
			return prefixWord + newWord;
		}
		if (suffixWord.EndsWith("oes", ignoreCase: true, _culture))
		{
			return prefixWord + suffixWord.Remove(suffixWord.Length - 2, 2);
		}
		if (suffixWord.EndsWith("ss", ignoreCase: true, _culture))
		{
			return prefixWord + suffixWord;
		}
		if (suffixWord.EndsWith("s", ignoreCase: true, _culture))
		{
			return prefixWord + suffixWord.Remove(suffixWord.Length - 1, 1);
		}
		return prefixWord + suffixWord;
	}

	private bool IsPlural(string word)
	{
		if (_userDictionary.ExistsInSecond(word))
		{
			return true;
		}
		if (_userDictionary.ExistsInFirst(word))
		{
			return false;
		}
		if (IsUninflective(word) || _knownPluralWords.Contains(word.ToLower(_culture)))
		{
			return true;
		}
		return !Singularize(word).Equals(word);
	}

	private static string Capitalize(string word, Func<string, string> action)
	{
		string text = action(word);
		if (IsCapitalized(word))
		{
			if (text.Length == 0)
			{
				return text;
			}
			StringBuilder stringBuilder = new StringBuilder(text.Length);
			stringBuilder.Append(char.ToUpperInvariant(text[0]));
			stringBuilder.Append(text.Substring(1));
			return stringBuilder.ToString();
		}
		return text;
	}

	private static string GetSuffixWord(string word, out string prefixWord)
	{
		int num = word.LastIndexOf(' ');
		prefixWord = word.Substring(0, num + 1);
		return word.Substring(num + 1);
	}

	private static bool IsCapitalized(string word)
	{
		if (!string.IsNullOrEmpty(word))
		{
			return char.IsUpper(word, 0);
		}
		return false;
	}

	private static bool IsAlphabets(string word)
	{
		if (string.IsNullOrEmpty(word.Trim()) || !word.Equals(word.Trim()) || Regex.IsMatch(word, "[^a-zA-Z\\s]"))
		{
			return false;
		}
		return true;
	}

	private bool IsUninflective(string word)
	{
		if (PluralizationServiceUtil.DoesWordContainSuffix(word, _uninflectiveSuffixes, _culture) || (!word.ToLower(_culture).Equals(word) && word.EndsWith("ese", ignoreCase: false, _culture)) || _uninflectiveWords.Contains(word.ToLowerInvariant()))
		{
			return true;
		}
		return false;
	}

	private bool IsNoOpWord(string word)
	{
		if (!IsAlphabets(word) || word.Length <= 1 || _pronounList.Contains(word.ToLowerInvariant()))
		{
			return true;
		}
		return false;
	}
}
