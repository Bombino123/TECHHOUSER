using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.Utilities;
using System.Data.SqlTypes;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.SqlServer;

internal sealed class SqlSpatialDataReader : DbSpatialDataReader
{
	private static readonly Lazy<Func<BinaryReader, object>> _sqlGeographyFromBinaryReader = new Lazy<Func<BinaryReader, object>>(() => CreateBinaryReadDelegate(SqlTypesAssemblyLoader.DefaultInstance.GetSqlTypesAssembly().SqlGeographyType), isThreadSafe: true);

	private static readonly Lazy<Func<BinaryReader, object>> _sqlGeometryFromBinaryReader = new Lazy<Func<BinaryReader, object>>(() => CreateBinaryReadDelegate(SqlTypesAssemblyLoader.DefaultInstance.GetSqlTypesAssembly().SqlGeometryType), isThreadSafe: true);

	private const string GeometrySqlType = "sys.geometry";

	private const string GeographySqlType = "sys.geography";

	private readonly DbSpatialServices _spatialServices;

	private readonly SqlDataReaderWrapper _reader;

	private readonly bool[] _geographyColumns;

	private readonly bool[] _geometryColumns;

	internal SqlSpatialDataReader(DbSpatialServices spatialServices, SqlDataReaderWrapper underlyingReader)
	{
		_spatialServices = spatialServices;
		_reader = underlyingReader;
		int fieldCount = _reader.FieldCount;
		_geographyColumns = new bool[fieldCount];
		_geometryColumns = new bool[fieldCount];
		for (int i = 0; i < _reader.FieldCount; i++)
		{
			string dataTypeName = _reader.GetDataTypeName(i);
			if (dataTypeName.EndsWith("sys.geography", StringComparison.Ordinal))
			{
				_geographyColumns[i] = true;
			}
			else if (dataTypeName.EndsWith("sys.geometry", StringComparison.Ordinal))
			{
				_geometryColumns[i] = true;
			}
		}
	}

	public override DbGeography GetGeography(int ordinal)
	{
		EnsureGeographyColumn(ordinal);
		SqlBytes sqlBytes = _reader.GetSqlBytes(ordinal);
		object obj = _sqlGeographyFromBinaryReader.Value(new BinaryReader(sqlBytes.Stream));
		return _spatialServices.GeographyFromProviderValue(obj);
	}

	public override DbGeometry GetGeometry(int ordinal)
	{
		EnsureGeometryColumn(ordinal);
		SqlBytes sqlBytes = _reader.GetSqlBytes(ordinal);
		object obj = _sqlGeometryFromBinaryReader.Value(new BinaryReader(sqlBytes.Stream));
		return _spatialServices.GeometryFromProviderValue(obj);
	}

	public override bool IsGeographyColumn(int ordinal)
	{
		return _geographyColumns[ordinal];
	}

	public override bool IsGeometryColumn(int ordinal)
	{
		return _geometryColumns[ordinal];
	}

	private void EnsureGeographyColumn(int ordinal)
	{
		if (!((DbSpatialDataReader)this).IsGeographyColumn(ordinal))
		{
			throw new InvalidDataException(Strings.SqlProvider_InvalidGeographyColumn(_reader.GetDataTypeName(ordinal)));
		}
	}

	private void EnsureGeometryColumn(int ordinal)
	{
		if (!((DbSpatialDataReader)this).IsGeometryColumn(ordinal))
		{
			throw new InvalidDataException(Strings.SqlProvider_InvalidGeometryColumn(_reader.GetDataTypeName(ordinal)));
		}
	}

	private static Func<BinaryReader, object> CreateBinaryReadDelegate(Type spatialType)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(BinaryReader));
		ParameterExpression parameterExpression2 = Expression.Variable(spatialType);
		MethodInfo publicInstanceMethod = spatialType.GetPublicInstanceMethod("Read", typeof(BinaryReader));
		return Expression.Lambda<Func<BinaryReader, object>>(Expression.Block(new ParameterExpression[1] { parameterExpression2 }, Expression.Assign(parameterExpression2, Expression.New(spatialType)), Expression.Call(parameterExpression2, publicInstanceMethod, parameterExpression), parameterExpression2), new ParameterExpression[1] { parameterExpression }).Compile();
	}
}
