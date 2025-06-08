using System.Data.Entity.Migrations.Utilities;
using System.IO;
using System.Text;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class SqlWriter : IndentedTextWriter
{
	public SqlWriter(StringBuilder b)
		: base((TextWriter)new StringWriter(b, IndentedTextWriter.Culture))
	{
	}
}
