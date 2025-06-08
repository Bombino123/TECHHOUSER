using System.Collections.Generic;

namespace SMBLibrary.Server;

internal class OpenSearch
{
	public List<QueryDirectoryFileInformation> Entries;

	public int EnumerationLocation;

	public OpenSearch(List<QueryDirectoryFileInformation> entries, int enumerationLocation)
	{
		Entries = entries;
		EnumerationLocation = enumerationLocation;
	}
}
