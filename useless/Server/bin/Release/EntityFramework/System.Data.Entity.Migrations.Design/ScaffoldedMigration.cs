using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Design;

[Serializable]
public class ScaffoldedMigration
{
	private string _migrationId;

	private string _userCode;

	private string _designerCode;

	private string _language;

	private string _directory;

	private readonly Dictionary<string, object> _resources = new Dictionary<string, object>();

	public string MigrationId
	{
		get
		{
			return _migrationId;
		}
		set
		{
			Check.NotEmpty(value, "value");
			_migrationId = value;
		}
	}

	public string UserCode
	{
		get
		{
			return _userCode;
		}
		set
		{
			Check.NotEmpty(value, "value");
			_userCode = value;
		}
	}

	public string DesignerCode
	{
		get
		{
			return _designerCode;
		}
		set
		{
			Check.NotEmpty(value, "value");
			_designerCode = value;
		}
	}

	public string Language
	{
		get
		{
			return _language;
		}
		set
		{
			Check.NotEmpty(value, "value");
			_language = value;
		}
	}

	public string Directory
	{
		get
		{
			return _directory;
		}
		set
		{
			Check.NotEmpty(value, "value");
			_directory = value;
		}
	}

	public IDictionary<string, object> Resources => _resources;

	public bool IsRescaffold { get; set; }
}
