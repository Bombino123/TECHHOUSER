using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using GMap.NET.MapProviders;

namespace GMap.NET.CacheProviders;

public class MsSQLPureImageCache : PureImageCache, IDisposable
{
	private string _connectionString = string.Empty;

	private SqlCommand _cmdInsert;

	private SqlCommand _cmdFetch;

	private SqlConnection _cnGet;

	private SqlConnection _cnSet;

	private bool _initialized;

	public string ConnectionString
	{
		get
		{
			return _connectionString;
		}
		set
		{
			if (_connectionString != value)
			{
				_connectionString = value;
				if (Initialized)
				{
					Dispose();
					Initialize();
				}
			}
		}
	}

	public bool Initialized
	{
		get
		{
			lock (this)
			{
				return _initialized;
			}
		}
		private set
		{
			lock (this)
			{
				_initialized = value;
			}
		}
	}

	public bool Initialize()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		lock (this)
		{
			if (!Initialized)
			{
				try
				{
					_cnGet = new SqlConnection(_connectionString);
					((DbConnection)(object)_cnGet).Open();
					_cnSet = new SqlConnection(_connectionString);
					((DbConnection)(object)_cnSet).Open();
					SqlCommand val = new SqlCommand("select object_id('GMapNETcache')", _cnGet);
					bool flag;
					try
					{
						object obj = ((DbCommand)(object)val).ExecuteScalar();
						flag = obj != null && obj != DBNull.Value;
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
					if (!flag)
					{
						SqlCommand val2 = new SqlCommand("CREATE TABLE [GMapNETcache] ( \n   [Type] [int]   NOT NULL, \n   [Zoom] [int]   NOT NULL, \n   [X]    [int]   NOT NULL, \n   [Y]    [int]   NOT NULL, \n   [Tile] [image] NOT NULL, \n   CONSTRAINT [PK_GMapNETcache] PRIMARY KEY CLUSTERED (Type, Zoom, X, Y) \n)", _cnGet);
						try
						{
							((DbCommand)(object)val2).ExecuteNonQuery();
						}
						finally
						{
							((IDisposable)val2)?.Dispose();
						}
					}
					_cmdFetch = new SqlCommand("SELECT [Tile] FROM [GMapNETcache] WITH (NOLOCK) WHERE [X]=@x AND [Y]=@y AND [Zoom]=@zoom AND [Type]=@type", _cnGet);
					_cmdFetch.Parameters.Add("@x", SqlDbType.Int);
					_cmdFetch.Parameters.Add("@y", SqlDbType.Int);
					_cmdFetch.Parameters.Add("@zoom", SqlDbType.Int);
					_cmdFetch.Parameters.Add("@type", SqlDbType.Int);
					((DbCommand)(object)_cmdFetch).Prepare();
					_cmdInsert = new SqlCommand("INSERT INTO [GMapNETcache] ( [X], [Y], [Zoom], [Type], [Tile] ) VALUES ( @x, @y, @zoom, @type, @tile )", _cnSet);
					_cmdInsert.Parameters.Add("@x", SqlDbType.Int);
					_cmdInsert.Parameters.Add("@y", SqlDbType.Int);
					_cmdInsert.Parameters.Add("@zoom", SqlDbType.Int);
					_cmdInsert.Parameters.Add("@type", SqlDbType.Int);
					_cmdInsert.Parameters.Add("@tile", SqlDbType.Image);
					Initialized = true;
				}
				catch (Exception)
				{
					_initialized = false;
				}
			}
			return Initialized;
		}
	}

	public void Dispose()
	{
		lock (_cmdInsert)
		{
			if (_cmdInsert != null)
			{
				((Component)(object)_cmdInsert).Dispose();
				_cmdInsert = null;
			}
			if (_cnSet != null)
			{
				((Component)(object)_cnSet).Dispose();
				_cnSet = null;
			}
		}
		lock (_cmdFetch)
		{
			if (_cmdFetch != null)
			{
				((Component)(object)_cmdFetch).Dispose();
				_cmdFetch = null;
			}
			if (_cnGet != null)
			{
				((Component)(object)_cnGet).Dispose();
				_cnGet = null;
			}
		}
		Initialized = false;
	}

	public bool PutImageToCache(byte[] tile, int type, GPoint pos, int zoom)
	{
		bool result = true;
		if (Initialize())
		{
			try
			{
				lock (_cmdInsert)
				{
					((DbParameter)(object)_cmdInsert.Parameters["@x"]).Value = pos.X;
					((DbParameter)(object)_cmdInsert.Parameters["@y"]).Value = pos.Y;
					((DbParameter)(object)_cmdInsert.Parameters["@zoom"]).Value = zoom;
					((DbParameter)(object)_cmdInsert.Parameters["@type"]).Value = type;
					((DbParameter)(object)_cmdInsert.Parameters["@tile"]).Value = tile;
					((DbCommand)(object)_cmdInsert).ExecuteNonQuery();
				}
			}
			catch (Exception)
			{
				result = false;
				Dispose();
			}
		}
		return result;
	}

	public PureImage GetImageFromCache(int type, GPoint pos, int zoom)
	{
		PureImage result = null;
		if (Initialize())
		{
			try
			{
				object obj;
				lock (_cmdFetch)
				{
					((DbParameter)(object)_cmdFetch.Parameters["@x"]).Value = pos.X;
					((DbParameter)(object)_cmdFetch.Parameters["@y"]).Value = pos.Y;
					((DbParameter)(object)_cmdFetch.Parameters["@zoom"]).Value = zoom;
					((DbParameter)(object)_cmdFetch.Parameters["@type"]).Value = type;
					obj = ((DbCommand)(object)_cmdFetch).ExecuteScalar();
				}
				if (obj != null && obj != DBNull.Value)
				{
					byte[] array = (byte[])obj;
					if (array != null && array.Length != 0 && GMapProvider.TileImageProxy != null)
					{
						result = GMapProvider.TileImageProxy.FromArray(array);
					}
				}
			}
			catch (Exception)
			{
				result = null;
				Dispose();
			}
		}
		return result;
	}

	int PureImageCache.DeleteOlderThan(DateTime date, int? type)
	{
		throw new NotImplementedException();
	}
}
