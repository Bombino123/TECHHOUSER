using System.Xml.Linq;

namespace System.Data.Entity.Migrations.Infrastructure;

internal class VersionedModel
{
	private readonly XDocument _model;

	private readonly string _version;

	public XDocument Model => _model;

	public string Version => _version;

	public VersionedModel(XDocument model, string version = null)
	{
		_model = model;
		_version = version;
	}
}
