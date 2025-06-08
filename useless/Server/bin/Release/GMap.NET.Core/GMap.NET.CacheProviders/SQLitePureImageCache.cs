#define TRACE
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using GMap.NET.MapProviders;
using GMap.NET.Properties;

namespace GMap.NET.CacheProviders;

public class SQLitePureImageCache : PureImageCache
{
	private static int _ping;

	private string _cache;

	private string _dir;

	private string _db;

	private bool _created;

	private static readonly string singleSqlSelect;

	private static readonly string singleSqlInsert;

	private static readonly string singleSqlInsertLast;

	private string _connectionString;

	private readonly List<string> _attachedCaches = new List<string>();

	private string _finnalSqlSelect = singleSqlSelect;

	private string _attachSqlQuery = string.Empty;

	private string _detachSqlQuery = string.Empty;

	private int _preAllocationPing;

	public string GtileCache { get; private set; }

	public string CacheLocation
	{
		get
		{
			return _cache;
		}
		set
		{
			_cache = value;
			string text = Path.Combine(_cache, "TileDBv5");
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			GtileCache = text + directorySeparatorChar;
			string gtileCache = GtileCache;
			string languageStr = GMapProvider.LanguageStr;
			directorySeparatorChar = Path.DirectorySeparatorChar;
			_dir = gtileCache + languageStr + directorySeparatorChar;
			if (!Directory.Exists(_dir))
			{
				Directory.CreateDirectory(_dir);
			}
			SQLiteConnection.ClearAllPools();
			_db = _dir + "Data.gmdb";
			if (!File.Exists(_db))
			{
				_created = CreateEmptyDB(_db);
			}
			else
			{
				_created = AlterDBAddTimeColumn(_db);
			}
			CheckPreAllocation();
			_connectionString = $"Data Source=\"{_db}\";Page Size=32768;Pooling=True";
			_attachedCaches.Clear();
			RebuildFinnalSelect();
			string[] files = Directory.GetFiles(_dir, "*.gmdb", SearchOption.AllDirectories);
			foreach (string text2 in files)
			{
				if (text2 != _db)
				{
					Attach(text2);
				}
			}
		}
	}

	static SQLitePureImageCache()
	{
		singleSqlSelect = "SELECT Tile FROM main.TilesData WHERE id = (SELECT id FROM main.Tiles WHERE X={0} AND Y={1} AND Zoom={2} AND Type={3})";
		singleSqlInsert = "INSERT INTO main.Tiles(X, Y, Zoom, Type, CacheTime) VALUES(@p1, @p2, @p3, @p4, @p5)";
		singleSqlInsertLast = "INSERT INTO main.TilesData(id, Tile) VALUES((SELECT last_insert_rowid()), @p1)";
		AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
	}

	private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
	{
		return null;
	}

	public static void Ping()
	{
		if (++_ping == 1)
		{
			Trace.WriteLine("SQLiteVersion: " + SQLiteConnection.SQLiteVersion + " | " + SQLiteConnection.SQLiteSourceId + " | " + SQLiteConnection.DefineConstants);
		}
	}

	private void CheckPreAllocation()
	{
		byte[] array = new byte[2];
		byte[] array2 = new byte[4];
		lock (this)
		{
			using FileStream fileStream = File.Open(_db, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			fileStream.Seek(16L, SeekOrigin.Begin);
			fileStream.Lock(16L, 2L);
			fileStream.Read(array, 0, 2);
			fileStream.Unlock(16L, 2L);
			fileStream.Seek(36L, SeekOrigin.Begin);
			fileStream.Lock(36L, 4L);
			fileStream.Read(array2, 0, 4);
			fileStream.Unlock(36L, 4L);
			fileStream.Close();
		}
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse((Array)array);
			Array.Reverse((Array)array2);
		}
		ushort num = BitConverter.ToUInt16(array, 0);
		uint num2 = BitConverter.ToUInt32(array2, 0);
		double num3 = (double)(num * num2) / 1048576.0;
		int addSizeInMBytes = 32;
		int num4 = 4;
		if (num3 <= (double)num4)
		{
			PreAllocateDB(_db, addSizeInMBytes);
		}
	}

	public static bool CreateEmptyDB(string file)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		bool result = true;
		try
		{
			string directoryName = Path.GetDirectoryName(file);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			SQLiteConnection val = new SQLiteConnection();
			try
			{
				((DbConnection)(object)val).ConnectionString = $"Data Source=\"{file}\";FailIfMissing=False;Page Size=32768";
				((DbConnection)(object)val).Open();
				using (DbTransaction dbTransaction = val.BeginTransaction())
				{
					try
					{
						using (DbCommand dbCommand = val.CreateCommand())
						{
							dbCommand.Transaction = dbTransaction;
							dbCommand.CommandText = Resources.CreateTileDb;
							dbCommand.ExecuteNonQuery();
						}
						dbTransaction.Commit();
					}
					catch (Exception)
					{
						dbTransaction.Rollback();
						result = false;
					}
				}
				((DbConnection)(object)val).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception)
		{
			result = false;
		}
		return result;
	}

	public static bool PreAllocateDB(string file, int addSizeInMBytes)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		bool result = true;
		try
		{
			SQLiteConnection val = new SQLiteConnection();
			try
			{
				((DbConnection)(object)val).ConnectionString = $"Data Source=\"{file}\";FailIfMissing=False;Page Size=32768";
				((DbConnection)(object)val).Open();
				using (DbTransaction dbTransaction = val.BeginTransaction())
				{
					try
					{
						using (DbCommand dbCommand = val.CreateCommand())
						{
							dbCommand.Transaction = dbTransaction;
							dbCommand.CommandText = $"create table large (a); insert into large values (zeroblob({addSizeInMBytes * 1024 * 1024})); drop table large;";
							dbCommand.ExecuteNonQuery();
						}
						dbTransaction.Commit();
					}
					catch (Exception)
					{
						dbTransaction.Rollback();
						result = false;
					}
				}
				((DbConnection)(object)val).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception)
		{
			result = false;
		}
		return result;
	}

	private static bool AlterDBAddTimeColumn(string file)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		bool result = true;
		try
		{
			if (File.Exists(file))
			{
				SQLiteConnection val = new SQLiteConnection();
				try
				{
					((DbConnection)(object)val).ConnectionString = $"Data Source=\"{file}\";FailIfMissing=False;Page Size=32768;Pooling=True";
					((DbConnection)(object)val).Open();
					using (DbTransaction dbTransaction = val.BeginTransaction())
					{
						bool? flag;
						try
						{
							using DbCommand dbCommand = new SQLiteCommand("SELECT CacheTime FROM Tiles", val);
							dbCommand.Transaction = dbTransaction;
							using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
							{
								dbDataReader.Close();
							}
							flag = false;
						}
						catch (Exception ex)
						{
							if (!ex.Message.Contains("no such column: CacheTime"))
							{
								throw ex;
							}
							flag = true;
						}
						try
						{
							if (flag.HasValue && flag.Value)
							{
								using (DbCommand dbCommand2 = val.CreateCommand())
								{
									dbCommand2.Transaction = dbTransaction;
									dbCommand2.CommandText = "ALTER TABLE Tiles ADD CacheTime DATETIME";
									dbCommand2.ExecuteNonQuery();
								}
								dbTransaction.Commit();
							}
						}
						catch (Exception)
						{
							dbTransaction.Rollback();
							result = false;
						}
					}
					((DbConnection)(object)val).Close();
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else
			{
				result = false;
			}
		}
		catch (Exception)
		{
			result = false;
		}
		return result;
	}

	public static bool VacuumDb(string file)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		bool result = true;
		try
		{
			SQLiteConnection val = new SQLiteConnection();
			try
			{
				((DbConnection)(object)val).ConnectionString = $"Data Source=\"{file}\";FailIfMissing=True;Page Size=32768";
				((DbConnection)(object)val).Open();
				using (DbCommand dbCommand = val.CreateCommand())
				{
					dbCommand.CommandText = "vacuum;";
					dbCommand.ExecuteNonQuery();
				}
				((DbConnection)(object)val).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception)
		{
			result = false;
		}
		return result;
	}

	public static bool ExportMapDataToDB(string sourceFile, string destFile)
	{
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		bool flag = true;
		try
		{
			if (!File.Exists(destFile))
			{
				flag = CreateEmptyDB(destFile);
			}
			if (flag)
			{
				SQLiteConnection val = new SQLiteConnection();
				try
				{
					((DbConnection)(object)val).ConnectionString = $"Data Source=\"{sourceFile}\";Page Size=32768";
					((DbConnection)(object)val).Open();
					if (((DbConnection)(object)val).State == ConnectionState.Open)
					{
						SQLiteConnection val2 = new SQLiteConnection();
						try
						{
							((DbConnection)(object)val2).ConnectionString = $"Data Source=\"{destFile}\";Page Size=32768";
							((DbConnection)(object)val2).Open();
							if (((DbConnection)(object)val2).State == ConnectionState.Open)
							{
								SQLiteCommand val3 = new SQLiteCommand($"ATTACH DATABASE \"{sourceFile}\" AS Source", val2);
								try
								{
									((DbCommand)(object)val3).ExecuteNonQuery();
								}
								finally
								{
									((IDisposable)val3)?.Dispose();
								}
								SQLiteTransaction val4 = val2.BeginTransaction();
								try
								{
									List<long> list = new List<long>();
									SQLiteCommand val5 = new SQLiteCommand("SELECT id, X, Y, Zoom, Type FROM Tiles;", val);
									try
									{
										SQLiteDataReader val6 = val5.ExecuteReader();
										try
										{
											while (((DbDataReader)(object)val6).Read())
											{
												long @int = ((DbDataReader)(object)val6).GetInt64(0);
												SQLiteCommand val7 = new SQLiteCommand($"SELECT id FROM Tiles WHERE X={((DbDataReader)(object)val6).GetInt32(1)} AND Y={((DbDataReader)(object)val6).GetInt32(2)} AND Zoom={((DbDataReader)(object)val6).GetInt32(3)} AND Type={((DbDataReader)(object)val6).GetInt32(4)};", val2);
												try
												{
													SQLiteDataReader val8 = val7.ExecuteReader();
													try
													{
														if (!((DbDataReader)(object)val8).Read())
														{
															list.Add(@int);
														}
													}
													finally
													{
														((IDisposable)val8)?.Dispose();
													}
												}
												finally
												{
													((IDisposable)val7)?.Dispose();
												}
											}
										}
										finally
										{
											((IDisposable)val6)?.Dispose();
										}
									}
									finally
									{
										((IDisposable)val5)?.Dispose();
									}
									foreach (long item in list)
									{
										SQLiteCommand val9 = new SQLiteCommand(string.Format("INSERT INTO Tiles(X, Y, Zoom, Type, CacheTime) SELECT X, Y, Zoom, Type, CacheTime FROM Source.Tiles WHERE id={0}; INSERT INTO TilesData(id, Tile) Values((SELECT last_insert_rowid()), (SELECT Tile FROM Source.TilesData WHERE id={0}));", item), val2);
										try
										{
											val9.Transaction = val4;
											((DbCommand)(object)val9).ExecuteNonQuery();
										}
										finally
										{
											((IDisposable)val9)?.Dispose();
										}
									}
									list.Clear();
									((DbTransaction)(object)val4).Commit();
								}
								catch (Exception)
								{
									((DbTransaction)(object)val4).Rollback();
									flag = false;
								}
								finally
								{
									((IDisposable)val4)?.Dispose();
								}
								SQLiteCommand val10 = new SQLiteCommand("DETACH DATABASE Source;", val2);
								try
								{
									((DbCommand)(object)val10).ExecuteNonQuery();
								}
								finally
								{
									((IDisposable)val10)?.Dispose();
								}
							}
						}
						finally
						{
							((IDisposable)val2)?.Dispose();
						}
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		catch (Exception)
		{
			flag = false;
		}
		return flag;
	}

	private void RebuildFinnalSelect()
	{
		_finnalSqlSelect = null;
		_finnalSqlSelect = singleSqlSelect;
		_attachSqlQuery = null;
		_attachSqlQuery = string.Empty;
		_detachSqlQuery = null;
		_detachSqlQuery = string.Empty;
		int num = 1;
		foreach (string attachedCache in _attachedCaches)
		{
			_finnalSqlSelect += string.Format("\nUNION SELECT Tile FROM db{0}.TilesData WHERE id = (SELECT id FROM db{0}.Tiles WHERE X={{0}} AND Y={{1}} AND Zoom={{2}} AND Type={{3}})", num);
			_attachSqlQuery += $"\nATTACH '{attachedCache}' as db{num};";
			_detachSqlQuery += $"\nDETACH DATABASE db{num};";
			num++;
		}
	}

	public void Attach(string db)
	{
		if (!_attachedCaches.Contains(db))
		{
			_attachedCaches.Add(db);
			RebuildFinnalSelect();
		}
	}

	public void Detach(string db)
	{
		if (_attachedCaches.Contains(db))
		{
			_attachedCaches.Remove(db);
			RebuildFinnalSelect();
		}
	}

	bool PureImageCache.PutImageToCache(byte[] tile, int type, GPoint pos, int zoom)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		bool result = true;
		if (_created)
		{
			try
			{
				SQLiteConnection val = new SQLiteConnection();
				try
				{
					((DbConnection)(object)val).ConnectionString = _connectionString;
					((DbConnection)(object)val).Open();
					using (DbTransaction dbTransaction = val.BeginTransaction())
					{
						try
						{
							using (DbCommand dbCommand = val.CreateCommand())
							{
								dbCommand.Transaction = dbTransaction;
								dbCommand.CommandText = singleSqlInsert;
								dbCommand.Parameters.Add((object)new SQLiteParameter("@p1", (object)pos.X));
								dbCommand.Parameters.Add((object)new SQLiteParameter("@p2", (object)pos.Y));
								dbCommand.Parameters.Add((object)new SQLiteParameter("@p3", (object)zoom));
								dbCommand.Parameters.Add((object)new SQLiteParameter("@p4", (object)type));
								dbCommand.Parameters.Add((object)new SQLiteParameter("@p5", (object)DateTime.Now));
								dbCommand.ExecuteNonQuery();
							}
							using (DbCommand dbCommand2 = val.CreateCommand())
							{
								dbCommand2.Transaction = dbTransaction;
								dbCommand2.CommandText = singleSqlInsertLast;
								dbCommand2.Parameters.Add((object)new SQLiteParameter("@p1", (object)tile));
								dbCommand2.ExecuteNonQuery();
							}
							dbTransaction.Commit();
						}
						catch (Exception)
						{
							dbTransaction.Rollback();
							result = false;
						}
					}
					((DbConnection)(object)val).Close();
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				if (Interlocked.Increment(ref _preAllocationPing) % 22 == 0)
				{
					CheckPreAllocation();
				}
			}
			catch (Exception)
			{
				result = false;
			}
		}
		return result;
	}

	PureImage PureImageCache.GetImageFromCache(int type, GPoint pos, int zoom)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		PureImage result = null;
		try
		{
			SQLiteConnection val = new SQLiteConnection();
			try
			{
				((DbConnection)(object)val).ConnectionString = _connectionString;
				((DbConnection)(object)val).Open();
				if (!string.IsNullOrEmpty(_attachSqlQuery))
				{
					using DbCommand dbCommand = val.CreateCommand();
					dbCommand.CommandText = _attachSqlQuery;
					dbCommand.ExecuteNonQuery();
				}
				using (DbCommand dbCommand2 = val.CreateCommand())
				{
					dbCommand2.CommandText = string.Format(_finnalSqlSelect, pos.X, pos.Y, zoom, type);
					using DbDataReader dbDataReader = dbCommand2.ExecuteReader(CommandBehavior.SequentialAccess);
					if (dbDataReader.Read())
					{
						byte[] array = new byte[dbDataReader.GetBytes(0, 0L, null, 0, 0)];
						dbDataReader.GetBytes(0, 0L, array, 0, array.Length);
						if (GMapProvider.TileImageProxy != null)
						{
							result = GMapProvider.TileImageProxy.FromArray(array);
						}
					}
					dbDataReader.Close();
				}
				if (!string.IsNullOrEmpty(_detachSqlQuery))
				{
					using DbCommand dbCommand3 = val.CreateCommand();
					dbCommand3.CommandText = _detachSqlQuery;
					dbCommand3.ExecuteNonQuery();
				}
				((DbConnection)(object)val).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception)
		{
			result = null;
		}
		return result;
	}

	int PureImageCache.DeleteOlderThan(DateTime date, int? type)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		int result = 0;
		try
		{
			SQLiteConnection val = new SQLiteConnection();
			try
			{
				((DbConnection)(object)val).ConnectionString = _connectionString;
				((DbConnection)(object)val).Open();
				using DbCommand dbCommand = val.CreateCommand();
				dbCommand.CommandText = string.Format("DELETE FROM Tiles WHERE CacheTime is not NULL and CacheTime < datetime('{0}')", date.ToString("s"));
				if (type.HasValue)
				{
					string commandText = dbCommand.CommandText;
					int? num = type;
					dbCommand.CommandText = commandText + " and Type = " + num;
				}
				result = dbCommand.ExecuteNonQuery();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception)
		{
		}
		return result;
	}
}
