using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class Dump : BasicOpVisitor, IDisposable
{
	internal class ColumnMapDumper : ColumnMapVisitor<Dump>
	{
		internal static ColumnMapDumper Instance = new ColumnMapDumper();

		private ColumnMapDumper()
		{
		}

		private void DumpCollection(CollectionColumnMap columnMap, Dump dumper)
		{
			if (columnMap.ForeignKeys.Length != 0)
			{
				using (new AutoXml(dumper, "foreignKeys"))
				{
					VisitList(columnMap.ForeignKeys, dumper);
				}
			}
			if (columnMap.Keys.Length != 0)
			{
				using (new AutoXml(dumper, "keys"))
				{
					VisitList(columnMap.Keys, dumper);
				}
			}
			using (new AutoXml(dumper, "element"))
			{
				columnMap.Element.Accept(this, dumper);
			}
		}

		private static Dictionary<string, object> GetAttributes(ColumnMap columnMap)
		{
			return new Dictionary<string, object> { 
			{
				"Type",
				columnMap.Type.ToString()
			} };
		}

		internal override void Visit(ComplexTypeColumnMap columnMap, Dump dumper)
		{
			using (new AutoXml(dumper, "ComplexType", GetAttributes(columnMap)))
			{
				if (columnMap.NullSentinel != null)
				{
					using (new AutoXml(dumper, "nullSentinel"))
					{
						columnMap.NullSentinel.Accept(this, dumper);
					}
				}
				VisitList(columnMap.Properties, dumper);
			}
		}

		internal override void Visit(DiscriminatedCollectionColumnMap columnMap, Dump dumper)
		{
			using (new AutoXml(dumper, "DiscriminatedCollection", GetAttributes(columnMap)))
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("Value", columnMap.DiscriminatorValue);
				using (new AutoXml(dumper, "discriminator", dictionary))
				{
					columnMap.Discriminator.Accept(this, dumper);
				}
				DumpCollection(columnMap, dumper);
			}
		}

		internal override void Visit(EntityColumnMap columnMap, Dump dumper)
		{
			using (new AutoXml(dumper, "Entity", GetAttributes(columnMap)))
			{
				using (new AutoXml(dumper, "entityIdentity"))
				{
					VisitEntityIdentity(columnMap.EntityIdentity, dumper);
				}
				VisitList(columnMap.Properties, dumper);
			}
		}

		internal override void Visit(SimplePolymorphicColumnMap columnMap, Dump dumper)
		{
			using (new AutoXml(dumper, "SimplePolymorphic", GetAttributes(columnMap)))
			{
				using (new AutoXml(dumper, "typeDiscriminator"))
				{
					columnMap.TypeDiscriminator.Accept(this, dumper);
				}
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				foreach (KeyValuePair<object, TypedColumnMap> typeChoice in columnMap.TypeChoices)
				{
					dictionary.Clear();
					dictionary.Add("DiscriminatorValue", typeChoice.Key);
					using (new AutoXml(dumper, "choice", dictionary))
					{
						typeChoice.Value.Accept(this, dumper);
					}
				}
				using (new AutoXml(dumper, "default"))
				{
					VisitList(columnMap.Properties, dumper);
				}
			}
		}

		internal override void Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, Dump dumper)
		{
			using (new AutoXml(dumper, "MultipleDiscriminatorPolymorphic", GetAttributes(columnMap)))
			{
				using (new AutoXml(dumper, "typeDiscriminators"))
				{
					VisitList(columnMap.TypeDiscriminators, dumper);
				}
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				foreach (KeyValuePair<EntityType, TypedColumnMap> typeChoice in columnMap.TypeChoices)
				{
					dictionary.Clear();
					dictionary.Add("EntityType", typeChoice.Key);
					using (new AutoXml(dumper, "choice", dictionary))
					{
						typeChoice.Value.Accept(this, dumper);
					}
				}
				using (new AutoXml(dumper, "default"))
				{
					VisitList(columnMap.Properties, dumper);
				}
			}
		}

		internal override void Visit(RecordColumnMap columnMap, Dump dumper)
		{
			using (new AutoXml(dumper, "Record", GetAttributes(columnMap)))
			{
				if (columnMap.NullSentinel != null)
				{
					using (new AutoXml(dumper, "nullSentinel"))
					{
						columnMap.NullSentinel.Accept(this, dumper);
					}
				}
				VisitList(columnMap.Properties, dumper);
			}
		}

		internal override void Visit(RefColumnMap columnMap, Dump dumper)
		{
			using (new AutoXml(dumper, "Ref", GetAttributes(columnMap)))
			{
				using (new AutoXml(dumper, "entityIdentity"))
				{
					VisitEntityIdentity(columnMap.EntityIdentity, dumper);
				}
			}
		}

		internal override void Visit(SimpleCollectionColumnMap columnMap, Dump dumper)
		{
			using (new AutoXml(dumper, "SimpleCollection", GetAttributes(columnMap)))
			{
				DumpCollection(columnMap, dumper);
			}
		}

		internal override void Visit(ScalarColumnMap columnMap, Dump dumper)
		{
			Dictionary<string, object> attributes = GetAttributes(columnMap);
			attributes.Add("CommandId", columnMap.CommandId);
			attributes.Add("ColumnPos", columnMap.ColumnPos);
			using (new AutoXml(dumper, "AssignedSimple", attributes))
			{
			}
		}

		internal override void Visit(VarRefColumnMap columnMap, Dump dumper)
		{
			Dictionary<string, object> attributes = GetAttributes(columnMap);
			attributes.Add("Var", columnMap.Var.Id);
			using (new AutoXml(dumper, "VarRef", attributes))
			{
			}
		}

		protected override void VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, Dump dumper)
		{
			using (new AutoXml(dumper, "DiscriminatedEntityIdentity"))
			{
				using (new AutoXml(dumper, "entitySetId"))
				{
					entityIdentity.EntitySetColumnMap.Accept(this, dumper);
				}
				if (entityIdentity.Keys.Length != 0)
				{
					using (new AutoXml(dumper, "keys"))
					{
						VisitList(entityIdentity.Keys, dumper);
						return;
					}
				}
			}
		}

		protected override void VisitEntityIdentity(SimpleEntityIdentity entityIdentity, Dump dumper)
		{
			using (new AutoXml(dumper, "SimpleEntityIdentity"))
			{
				if (entityIdentity.Keys.Length != 0)
				{
					using (new AutoXml(dumper, "keys"))
					{
						VisitList(entityIdentity.Keys, dumper);
						return;
					}
				}
			}
		}
	}

	internal struct AutoString : IDisposable
	{
		private readonly Dump _dumper;

		internal AutoString(Dump dumper, Op op)
		{
			_dumper = dumper;
			_dumper.WriteString(ToString(op.OpType));
			_dumper.BeginExpression();
		}

		public void Dispose()
		{
			try
			{
				_dumper.EndExpression();
			}
			catch (Exception e)
			{
				if (!e.IsCatchableExceptionType())
				{
					throw;
				}
			}
		}

		internal static string ToString(OpType op)
		{
			return op switch
			{
				OpType.Aggregate => "Aggregate", 
				OpType.And => "And", 
				OpType.Case => "Case", 
				OpType.Cast => "Cast", 
				OpType.Collect => "Collect", 
				OpType.Constant => "Constant", 
				OpType.ConstantPredicate => "ConstantPredicate", 
				OpType.CrossApply => "CrossApply", 
				OpType.CrossJoin => "CrossJoin", 
				OpType.Deref => "Deref", 
				OpType.Distinct => "Distinct", 
				OpType.Divide => "Divide", 
				OpType.Element => "Element", 
				OpType.EQ => "EQ", 
				OpType.Except => "Except", 
				OpType.Exists => "Exists", 
				OpType.Filter => "Filter", 
				OpType.FullOuterJoin => "FullOuterJoin", 
				OpType.Function => "Function", 
				OpType.GE => "GE", 
				OpType.GetEntityRef => "GetEntityRef", 
				OpType.GetRefKey => "GetRefKey", 
				OpType.GroupBy => "GroupBy", 
				OpType.GroupByInto => "GroupByInto", 
				OpType.GT => "GT", 
				OpType.In => "In", 
				OpType.InnerJoin => "InnerJoin", 
				OpType.InternalConstant => "InternalConstant", 
				OpType.Intersect => "Intersect", 
				OpType.IsNull => "IsNull", 
				OpType.IsOf => "IsOf", 
				OpType.LE => "LE", 
				OpType.Leaf => "Leaf", 
				OpType.LeftOuterJoin => "LeftOuterJoin", 
				OpType.Like => "Like", 
				OpType.LT => "LT", 
				OpType.Minus => "Minus", 
				OpType.Modulo => "Modulo", 
				OpType.Multiply => "Multiply", 
				OpType.MultiStreamNest => "MultiStreamNest", 
				OpType.Navigate => "Navigate", 
				OpType.NE => "NE", 
				OpType.NewEntity => "NewEntity", 
				OpType.NewInstance => "NewInstance", 
				OpType.DiscriminatedNewEntity => "DiscriminatedNewEntity", 
				OpType.NewMultiset => "NewMultiset", 
				OpType.NewRecord => "NewRecord", 
				OpType.Not => "Not", 
				OpType.Null => "Null", 
				OpType.NullSentinel => "NullSentinel", 
				OpType.Or => "Or", 
				OpType.OuterApply => "OuterApply", 
				OpType.PhysicalProject => "PhysicalProject", 
				OpType.Plus => "Plus", 
				OpType.Project => "Project", 
				OpType.Property => "Property", 
				OpType.Ref => "Ref", 
				OpType.RelProperty => "RelProperty", 
				OpType.ScanTable => "ScanTable", 
				OpType.ScanView => "ScanView", 
				OpType.SingleRow => "SingleRow", 
				OpType.SingleRowTable => "SingleRowTable", 
				OpType.SingleStreamNest => "SingleStreamNest", 
				OpType.SoftCast => "SoftCast", 
				OpType.Sort => "Sort", 
				OpType.Treat => "Treat", 
				OpType.UnaryMinus => "UnaryMinus", 
				OpType.UnionAll => "UnionAll", 
				OpType.Unnest => "Unnest", 
				OpType.VarDef => "VarDef", 
				OpType.VarDefList => "VarDefList", 
				OpType.VarRef => "VarRef", 
				OpType.ConstrainedSort => "ConstrainedSort", 
				_ => op.ToString(), 
			};
		}
	}

	internal struct AutoXml : IDisposable
	{
		private readonly string _nodeName;

		private readonly Dump _dumper;

		internal AutoXml(Dump dumper, Op op)
		{
			_dumper = dumper;
			_nodeName = AutoString.ToString(op.OpType);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (op.Type != null)
			{
				dictionary.Add("Type", op.Type.ToString());
			}
			_dumper.Begin(_nodeName, dictionary);
		}

		internal AutoXml(Dump dumper, Op op, Dictionary<string, object> attrs)
		{
			_dumper = dumper;
			_nodeName = AutoString.ToString(op.OpType);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (op.Type != null)
			{
				dictionary.Add("Type", op.Type.ToString());
			}
			foreach (KeyValuePair<string, object> attr in attrs)
			{
				dictionary.Add(attr.Key, attr.Value);
			}
			_dumper.Begin(_nodeName, dictionary);
		}

		internal AutoXml(Dump dumper, string nodeName)
			: this(dumper, nodeName, null)
		{
		}

		internal AutoXml(Dump dumper, string nodeName, Dictionary<string, object> attrs)
		{
			_dumper = dumper;
			_nodeName = nodeName;
			_dumper.Begin(_nodeName, attrs);
		}

		public void Dispose()
		{
			_dumper.End();
		}
	}

	private readonly XmlWriter _writer;

	internal static readonly Encoding DefaultEncoding = Encoding.UTF8;

	private Dump(Stream stream)
		: this(stream, DefaultEncoding)
	{
	}

	private Dump(Stream stream, Encoding encoding)
	{
		_writer = XmlWriter.Create(stream, new XmlWriterSettings
		{
			CheckCharacters = false,
			Indent = true,
			Encoding = encoding
		});
		_writer.WriteStartDocument(standalone: true);
	}

	internal static string ToXml(Command itree)
	{
		return ToXml(itree.Root);
	}

	internal static string ToXml(Node subtree)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (Dump dump = new Dump(memoryStream))
		{
			using (new AutoXml(dump, "nodes"))
			{
				dump.VisitNode(subtree);
			}
		}
		return DefaultEncoding.GetString(memoryStream.ToArray());
	}

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
		try
		{
			_writer.WriteEndDocument();
			_writer.Flush();
			_writer.Close();
		}
		catch (Exception e)
		{
			if (!e.IsCatchableExceptionType())
			{
				throw;
			}
		}
	}

	internal void Begin(string name, Dictionary<string, object> attrs)
	{
		_writer.WriteStartElement(name);
		if (attrs == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> attr in attrs)
		{
			_writer.WriteAttributeString(attr.Key, attr.Value.ToString());
		}
	}

	internal void BeginExpression()
	{
		WriteString("(");
	}

	internal void EndExpression()
	{
		WriteString(")");
	}

	internal void End()
	{
		_writer.WriteEndElement();
	}

	internal void WriteString(string value)
	{
		_writer.WriteString(value);
	}

	protected override void VisitDefault(Node n)
	{
		using (new AutoXml(this, n.Op))
		{
			base.VisitDefault(n);
		}
	}

	protected override void VisitScalarOpDefault(ScalarOp op, Node n)
	{
		using (new AutoString(this, op))
		{
			string value = string.Empty;
			foreach (Node child in n.Children)
			{
				WriteString(value);
				VisitNode(child);
				value = ",";
			}
		}
	}

	protected override void VisitJoinOp(JoinBaseOp op, Node n)
	{
		using (new AutoXml(this, op))
		{
			if (n.Children.Count > 2)
			{
				using (new AutoXml(this, "condition"))
				{
					VisitNode(n.Child2);
				}
			}
			using (new AutoXml(this, "input"))
			{
				VisitNode(n.Child0);
			}
			using (new AutoXml(this, "input"))
			{
				VisitNode(n.Child1);
			}
		}
	}

	public override void Visit(CaseOp op, Node n)
	{
		using (new AutoXml(this, op))
		{
			int num = 0;
			while (num < n.Children.Count)
			{
				if (num + 1 < n.Children.Count)
				{
					using (new AutoXml(this, "when"))
					{
						VisitNode(n.Children[num++]);
					}
					using (new AutoXml(this, "then"))
					{
						VisitNode(n.Children[num++]);
					}
				}
				else
				{
					using (new AutoXml(this, "else"))
					{
						VisitNode(n.Children[num++]);
					}
				}
			}
		}
	}

	public override void Visit(CollectOp op, Node n)
	{
		using (new AutoXml(this, op))
		{
			VisitChildren(n);
		}
	}

	protected override void VisitConstantOp(ConstantBaseOp op, Node n)
	{
		using (new AutoString(this, op))
		{
			if (op.Value == null)
			{
				WriteString("null");
			}
			else
			{
				WriteString("(");
				WriteString(op.Type.EdmType.FullName);
				WriteString(")");
				WriteString(string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { op.Value }));
			}
			VisitChildren(n);
		}
	}

	public override void Visit(DistinctOp op, Node n)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		StringBuilder stringBuilder = new StringBuilder();
		string value = string.Empty;
		foreach (Var key in op.Keys)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(key.Id);
			value = ",";
		}
		if (stringBuilder.Length != 0)
		{
			dictionary.Add("Keys", stringBuilder.ToString());
		}
		using (new AutoXml(this, op, dictionary))
		{
			VisitChildren(n);
		}
	}

	protected override void VisitGroupByOp(GroupByBaseOp op, Node n)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		StringBuilder stringBuilder = new StringBuilder();
		string value = string.Empty;
		foreach (Var key in op.Keys)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(key.Id);
			value = ",";
		}
		if (stringBuilder.Length != 0)
		{
			dictionary.Add("Keys", stringBuilder.ToString());
		}
		using (new AutoXml(this, op, dictionary))
		{
			using (new AutoXml(this, "outputs"))
			{
				foreach (Var output in op.Outputs)
				{
					DumpVar(output);
				}
			}
			VisitChildren(n);
		}
	}

	public override void Visit(IsOfOp op, Node n)
	{
		AutoXml autoXml = new AutoXml(this, op.IsOfOnly ? "IsOfOnly" : "IsOf");
		try
		{
			string value = string.Empty;
			foreach (Node child in n.Children)
			{
				WriteString(value);
				VisitNode(child);
				value = ",";
			}
		}
		finally
		{
			((IDisposable)autoXml).Dispose();
		}
	}

	protected override void VisitNestOp(NestBaseOp op, Node n)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		SingleStreamNestOp singleStreamNestOp = op as SingleStreamNestOp;
		if (singleStreamNestOp != null)
		{
			dictionary.Add("Discriminator", (singleStreamNestOp.Discriminator == null) ? "<null>" : singleStreamNestOp.Discriminator.ToString());
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (singleStreamNestOp != null)
		{
			stringBuilder.Length = 0;
			string value = string.Empty;
			foreach (Var key in singleStreamNestOp.Keys)
			{
				stringBuilder.Append(value);
				stringBuilder.Append(key.Id);
				value = ",";
			}
			if (stringBuilder.Length != 0)
			{
				dictionary.Add("Keys", stringBuilder.ToString());
			}
		}
		using (new AutoXml(this, op, dictionary))
		{
			using (new AutoXml(this, "outputs"))
			{
				foreach (Var output in op.Outputs)
				{
					DumpVar(output);
				}
			}
			foreach (CollectionInfo item in op.CollectionInfo)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				dictionary2.Add("CollectionVar", item.CollectionVar);
				if (item.DiscriminatorValue != null)
				{
					dictionary2.Add("DiscriminatorValue", item.DiscriminatorValue);
				}
				if (item.FlattenedElementVars.Count != 0)
				{
					dictionary2.Add("FlattenedElementVars", FormatVarList(stringBuilder, item.FlattenedElementVars));
				}
				if (item.Keys.Count != 0)
				{
					dictionary2.Add("Keys", item.Keys);
				}
				if (item.SortKeys.Count != 0)
				{
					dictionary2.Add("SortKeys", FormatVarList(stringBuilder, item.SortKeys));
				}
				using (new AutoXml(this, "collection", dictionary2))
				{
					item.ColumnMap.Accept(ColumnMapDumper.Instance, this);
				}
			}
			VisitChildren(n);
		}
	}

	private static string FormatVarList(StringBuilder sb, VarList varList)
	{
		sb.Length = 0;
		string value = string.Empty;
		foreach (Var var in varList)
		{
			sb.Append(value);
			sb.Append(var.Id);
			value = ",";
		}
		return sb.ToString();
	}

	private static string FormatVarList(StringBuilder sb, List<SortKey> varList)
	{
		sb.Length = 0;
		string value = string.Empty;
		foreach (SortKey var in varList)
		{
			sb.Append(value);
			sb.Append(var.Var.Id);
			value = ",";
		}
		return sb.ToString();
	}

	private void VisitNewOp(Op op, Node n)
	{
		using (new AutoXml(this, op))
		{
			foreach (Node child in n.Children)
			{
				using (new AutoXml(this, "argument", null))
				{
					VisitNode(child);
				}
			}
		}
	}

	public override void Visit(NewEntityOp op, Node n)
	{
		VisitNewOp(op, n);
	}

	public override void Visit(NewInstanceOp op, Node n)
	{
		VisitNewOp(op, n);
	}

	public override void Visit(DiscriminatedNewEntityOp op, Node n)
	{
		VisitNewOp(op, n);
	}

	public override void Visit(NewMultisetOp op, Node n)
	{
		VisitNewOp(op, n);
	}

	public override void Visit(NewRecordOp op, Node n)
	{
		VisitNewOp(op, n);
	}

	public override void Visit(PhysicalProjectOp op, Node n)
	{
		using (new AutoXml(this, op))
		{
			using (new AutoXml(this, "outputs"))
			{
				foreach (Var output in op.Outputs)
				{
					DumpVar(output);
				}
			}
			using (new AutoXml(this, "columnMap"))
			{
				op.ColumnMap.Accept(ColumnMapDumper.Instance, this);
			}
			using (new AutoXml(this, "input"))
			{
				VisitChildren(n);
			}
		}
	}

	public override void Visit(ProjectOp op, Node n)
	{
		using (new AutoXml(this, op))
		{
			using (new AutoXml(this, "outputs"))
			{
				foreach (Var output in op.Outputs)
				{
					DumpVar(output);
				}
			}
			VisitChildren(n);
		}
	}

	public override void Visit(PropertyOp op, Node n)
	{
		using (new AutoString(this, op))
		{
			VisitChildren(n);
			WriteString(".");
			WriteString(op.PropertyInfo.Name);
		}
	}

	public override void Visit(RelPropertyOp op, Node n)
	{
		using (new AutoString(this, op))
		{
			VisitChildren(n);
			WriteString(".NAVIGATE(");
			WriteString(op.PropertyInfo.Relationship.Name);
			WriteString(",");
			WriteString(op.PropertyInfo.FromEnd.Name);
			WriteString(",");
			WriteString(op.PropertyInfo.ToEnd.Name);
			WriteString(")");
		}
	}

	public override void Visit(ScanTableOp op, Node n)
	{
		using (new AutoXml(this, op))
		{
			DumpTable(op.Table);
			VisitChildren(n);
		}
	}

	public override void Visit(ScanViewOp op, Node n)
	{
		using (new AutoXml(this, op))
		{
			DumpTable(op.Table);
			VisitChildren(n);
		}
	}

	protected override void VisitSetOp(SetOp op, Node n)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (OpType.UnionAll == op.OpType)
		{
			UnionAllOp unionAllOp = (UnionAllOp)op;
			if (unionAllOp.BranchDiscriminator != null)
			{
				dictionary.Add("branchDiscriminator", unionAllOp.BranchDiscriminator);
			}
		}
		using (new AutoXml(this, op, dictionary))
		{
			using (new AutoXml(this, "outputs"))
			{
				foreach (Var output in op.Outputs)
				{
					DumpVar(output);
				}
			}
			int num = 0;
			foreach (Node child in n.Children)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				dictionary2.Add("VarMap", op.VarMap[num++].ToString());
				using (new AutoXml(this, "input", dictionary2))
				{
					VisitNode(child);
				}
			}
		}
	}

	public override void Visit(SortOp op, Node n)
	{
		using (new AutoXml(this, op))
		{
			base.Visit(op, n);
		}
	}

	public override void Visit(ConstrainedSortOp op, Node n)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("WithTies", op.WithTies);
		using (new AutoXml(this, op, dictionary))
		{
			base.Visit(op, n);
		}
	}

	protected override void VisitSortOp(SortBaseOp op, Node n)
	{
		using (new AutoXml(this, "keys"))
		{
			foreach (SortKey key in op.Keys)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("Var", key.Var);
				dictionary.Add("Ascending", key.AscendingSort);
				dictionary.Add("Collation", key.Collation);
				using (new AutoXml(this, "sortKey", dictionary))
				{
				}
			}
		}
		VisitChildren(n);
	}

	public override void Visit(UnnestOp op, Node n)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (op.Var != null)
		{
			dictionary.Add("Var", op.Var.Id);
		}
		using (new AutoXml(this, op, dictionary))
		{
			DumpTable(op.Table);
			VisitChildren(n);
		}
	}

	public override void Visit(VarDefOp op, Node n)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("Var", op.Var.Id);
		using (new AutoXml(this, op, dictionary))
		{
			VisitChildren(n);
		}
	}

	public override void Visit(VarRefOp op, Node n)
	{
		using (new AutoString(this, op))
		{
			VisitChildren(n);
			if (op.Type != null)
			{
				WriteString("Type=");
				WriteString(op.Type.ToString());
				WriteString(", ");
			}
			WriteString("Var=");
			WriteString(op.Var.Id.ToString(CultureInfo.InvariantCulture));
		}
	}

	private void DumpVar(Var v)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("Var", v.Id);
		if (v is ColumnVar columnVar)
		{
			dictionary.Add("Name", columnVar.ColumnMetadata.Name);
			dictionary.Add("Type", columnVar.ColumnMetadata.Type.ToString());
		}
		using (new AutoXml(this, v.GetType().Name, dictionary))
		{
		}
	}

	private void DumpVars(List<Var> vars)
	{
		foreach (Var var in vars)
		{
			DumpVar(var);
		}
	}

	private void DumpTable(Table table)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("Table", table.TableId);
		if (table.TableMetadata.Extent != null)
		{
			dictionary.Add("Extent", table.TableMetadata.Extent.Name);
		}
		using (new AutoXml(this, "Table", dictionary))
		{
			DumpVars(table.Columns);
		}
	}
}
