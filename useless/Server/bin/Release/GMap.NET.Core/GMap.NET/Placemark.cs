namespace GMap.NET;

public struct Placemark
{
	public int Accuracy;

	public string ThoroughfareName;

	public string LocalityName;

	public string PostalCodeNumber;

	public string CountryName;

	public string AdministrativeAreaName;

	public string DistrictName;

	public string SubAdministrativeAreaName;

	public string Neighborhood;

	public string StreetNumber;

	public string StreetAddress;

	public string CountryNameCode;

	public string HouseNo;

	public long PlacemarkId;

	public string Address { get; internal set; }

	internal Placemark(string address)
	{
		Address = address;
		PlacemarkId = 0L;
		Accuracy = 0;
		HouseNo = string.Empty;
		ThoroughfareName = string.Empty;
		DistrictName = string.Empty;
		LocalityName = string.Empty;
		PostalCodeNumber = string.Empty;
		CountryName = string.Empty;
		CountryNameCode = string.Empty;
		AdministrativeAreaName = string.Empty;
		SubAdministrativeAreaName = string.Empty;
		Neighborhood = string.Empty;
		StreetNumber = string.Empty;
		StreetAddress = string.Empty;
	}
}
