using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Xml.Serialization;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.ComponentModel.Design;
using System.Xml.Schema;
using System.Xml;

namespace FFXIVRuby
{
    [Serializable, XmlSchemaProvider("GetTypedDataSetSchema"), XmlRoot("FixedFormSentenceLibraryDataSet"), ToolboxItem(true), HelpKeyword("vs.data.DataSet"), DesignerCategory("code")]
    public class FixedFormSentenceLibraryDataSet : DataSet
    {
        // Fields
        private SchemaSerializationMode _schemaSerializationMode;
        private TabStringDataTable tableTabString;

        // Methods
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
        public FixedFormSentenceLibraryDataSet()
        {
            this._schemaSerializationMode = SchemaSerializationMode.IncludeSchema;
            base.BeginInit();
            this.InitClass();
            CollectionChangeEventHandler handler = new CollectionChangeEventHandler(this.SchemaChanged);
            base.Tables.CollectionChanged += handler;
            base.Relations.CollectionChanged += handler;
            base.EndInit();
        }

        [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected FixedFormSentenceLibraryDataSet(SerializationInfo info, StreamingContext context)
            : base(info, context, false)
        {
            this._schemaSerializationMode = SchemaSerializationMode.IncludeSchema;
            if (base.IsBinarySerialized(info, context)) {
                this.InitVars(false);
                CollectionChangeEventHandler handler = new CollectionChangeEventHandler(this.SchemaChanged);
                this.Tables.CollectionChanged += handler;
                this.Relations.CollectionChanged += handler;
            }
            else {
                string s = (string)info.GetValue("XmlSchema", typeof(string));
                if (base.DetermineSchemaSerializationMode(info, context) == SchemaSerializationMode.IncludeSchema) {
                    DataSet dataSet = new DataSet();
                    dataSet.ReadXmlSchema(new XmlTextReader(new StringReader(s)));
                    if (dataSet.Tables["TabString"] != null) {
                        base.Tables.Add(new TabStringDataTable(dataSet.Tables["TabString"]));
                    }
                    base.DataSetName = dataSet.DataSetName;
                    base.Prefix = dataSet.Prefix;
                    base.Namespace = dataSet.Namespace;
                    base.Locale = dataSet.Locale;
                    base.CaseSensitive = dataSet.CaseSensitive;
                    base.EnforceConstraints = dataSet.EnforceConstraints;
                    base.Merge(dataSet, false, MissingSchemaAction.Add);
                    this.InitVars();
                }
                else {
                    base.ReadXmlSchema(new XmlTextReader(new StringReader(s)));
                }
                base.GetSerializationData(info, context);
                CollectionChangeEventHandler handler2 = new CollectionChangeEventHandler(this.SchemaChanged);
                base.Tables.CollectionChanged += handler2;
                this.Relations.CollectionChanged += handler2;
            }
        }

        [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public override DataSet Clone()
        {
            FixedFormSentenceLibraryDataSet set = (FixedFormSentenceLibraryDataSet)base.Clone();
            set.InitVars();
            set.SchemaSerializationMode = this.SchemaSerializationMode;
            return set;
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
        protected override XmlSchema GetSchemaSerializable()
        {
            MemoryStream w = new MemoryStream();
            base.WriteXmlSchema(new XmlTextWriter(w, null));
            w.Position = 0L;
            return XmlSchema.Read(new XmlTextReader(w), null);
        }

        [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
        {
            FixedFormSentenceLibraryDataSet set = new FixedFormSentenceLibraryDataSet();
            XmlSchemaComplexType type = new XmlSchemaComplexType();
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            XmlSchemaAny item = new XmlSchemaAny();
            item.Namespace = set.Namespace;
            sequence.Items.Add(item);
            type.Particle = sequence;
            XmlSchema schemaSerializable = set.GetSchemaSerializable();
            if (xs.Contains(schemaSerializable.TargetNamespace)) {
                MemoryStream stream = new MemoryStream();
                MemoryStream stream2 = new MemoryStream();
                try {
                    XmlSchema current = null;
                    schemaSerializable.Write(stream);
                    var enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
                    while (enumerator.MoveNext()) {
                        current = (XmlSchema)enumerator.Current;
                        stream2.SetLength(0L);
                        current.Write(stream2);
                        if (stream.Length == stream2.Length) {
                            stream.Position = 0L;
                            stream2.Position = 0L;
                            while ((stream.Position != stream.Length) && (stream.ReadByte() == stream2.ReadByte())) {
                            }
                            if (stream.Position == stream.Length) {
                                return type;
                            }
                        }
                    }
                }
                finally {
                    if (stream != null) {
                        stream.Close();
                    }
                    if (stream2 != null) {
                        stream2.Close();
                    }
                }
            }
            xs.Add(schemaSerializable);
            return type;
        }

        [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        private void InitClass()
        {
            base.DataSetName = "FixedFormSentenceLibraryDataSet";
            base.Prefix = "";
            base.Namespace = "http://ff14.room301.net/FF14MacroEditor/TabStringLibrary.xsd";
            base.EnforceConstraints = true;
            this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
            this.tableTabString = new TabStringDataTable();
            base.Tables.Add(this.tableTabString);
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
        protected override void InitializeDerivedDataSet()
        {
            base.BeginInit();
            this.InitClass();
            base.EndInit();
        }

        [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        internal void InitVars()
        {
            this.InitVars(true);
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
        internal void InitVars(bool initTable)
        {
            this.tableTabString = (TabStringDataTable)base.Tables["TabString"];
            if (initTable && (this.tableTabString != null)) {
                this.tableTabString.InitVars();
            }
        }

        [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override void ReadXmlSerializable(XmlReader reader)
        {
            if (base.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema) {
                this.Reset();
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(reader);
                if (dataSet.Tables["TabString"] != null) {
                    base.Tables.Add(new TabStringDataTable(dataSet.Tables["TabString"]));
                }
                base.DataSetName = dataSet.DataSetName;
                base.Prefix = dataSet.Prefix;
                base.Namespace = dataSet.Namespace;
                base.Locale = dataSet.Locale;
                base.CaseSensitive = dataSet.CaseSensitive;
                base.EnforceConstraints = dataSet.EnforceConstraints;
                base.Merge(dataSet, false, MissingSchemaAction.Add);
                this.InitVars();
            }
            else {
                base.ReadXml(reader);
                this.InitVars();
            }
        }

        [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        private void SchemaChanged(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Remove) {
                this.InitVars();
            }
        }

        [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override bool ShouldSerializeRelations()
        {
            return false;
        }

        [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        protected override bool ShouldSerializeTables()
        {
            return false;
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
        private bool ShouldSerializeTabString()
        {
            return false;
        }

        // Properties
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DebuggerNonUserCode]
        public DataRelationCollection Relations
        {
            get
            {
                return base.Relations;
            }
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode, DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Browsable(true)]
        public override SchemaSerializationMode SchemaSerializationMode
        {
            get
            {
                return this._schemaSerializationMode;
            }
            set
            {
                this._schemaSerializationMode = value;
            }
        }

        [DebuggerNonUserCode, DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public DataTableCollection Tables
        {
            get
            {
                return base.Tables;
            }
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DebuggerNonUserCode, Browsable(false)]
        public TabStringDataTable TabString
        {
            get
            {
                return this.tableTabString;
            }
        }

        // Nested Types
        [Serializable, XmlSchemaProvider("GetTypedTableSchema")]
        public class TabStringDataTable : TypedTableBase<FixedFormSentenceLibraryDataSet.TabStringRow>
        {
            // Fields
            private DataColumn columnJapanese;
            private DataColumn columnTabCode;

            // Events
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event FixedFormSentenceLibraryDataSet.TabStringRowChangeEventHandler TabStringRowChanged;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event FixedFormSentenceLibraryDataSet.TabStringRowChangeEventHandler TabStringRowChanging;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event FixedFormSentenceLibraryDataSet.TabStringRowChangeEventHandler TabStringRowDeleted;

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public event FixedFormSentenceLibraryDataSet.TabStringRowChangeEventHandler TabStringRowDeleting;

            // Methods
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            public TabStringDataTable()
            {
                base.TableName = "TabString";
                this.BeginInit();
                this.InitClass();
                this.EndInit();
            }

            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            internal TabStringDataTable(DataTable table)
            {
                base.TableName = table.TableName;
                if (table.CaseSensitive != table.DataSet.CaseSensitive) {
                    base.CaseSensitive = table.CaseSensitive;
                }
                if (table.Locale.ToString() != table.DataSet.Locale.ToString()) {
                    base.Locale = table.Locale;
                }
                if (table.Namespace != table.DataSet.Namespace) {
                    base.Namespace = table.Namespace;
                }
                base.Prefix = table.Prefix;
                base.MinimumCapacity = table.MinimumCapacity;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            protected TabStringDataTable(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                this.InitVars();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            public void AddTabStringRow(FixedFormSentenceLibraryDataSet.TabStringRow row)
            {
                base.Rows.Add(row);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            public FixedFormSentenceLibraryDataSet.TabStringRow AddTabStringRow(string TabCode, string Japanese)
            {
                FixedFormSentenceLibraryDataSet.TabStringRow row = (FixedFormSentenceLibraryDataSet.TabStringRow)base.NewRow();
                row.ItemArray = new object[] { TabCode, Japanese };
                base.Rows.Add(row);
                return row;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            public override DataTable Clone()
            {
                FixedFormSentenceLibraryDataSet.TabStringDataTable table = (FixedFormSentenceLibraryDataSet.TabStringDataTable)base.Clone();
                table.InitVars();
                return table;
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            protected override DataTable CreateInstance()
            {
                return new FixedFormSentenceLibraryDataSet.TabStringDataTable();
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            public FixedFormSentenceLibraryDataSet.TabStringRow FindByTabCode(string TabCode)
            {
                return (FixedFormSentenceLibraryDataSet.TabStringRow)base.Rows.Find(new object[] { TabCode });
            }

            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override Type GetRowType()
            {
                return typeof(FixedFormSentenceLibraryDataSet.TabStringRow);
            }

            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
            {
                XmlSchemaComplexType type = new XmlSchemaComplexType();
                XmlSchemaSequence sequence = new XmlSchemaSequence();
                FixedFormSentenceLibraryDataSet set = new FixedFormSentenceLibraryDataSet();
                XmlSchemaAny item = new XmlSchemaAny();
                item.Namespace = "http://www.w3.org/2001/XMLSchema";
                item.MinOccurs = 0M;
                item.MaxOccurs = 79228162514264337593543950335M;
                item.ProcessContents = XmlSchemaContentProcessing.Lax;
                sequence.Items.Add(item);
                XmlSchemaAny any2 = new XmlSchemaAny();
                any2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
                any2.MinOccurs = 1M;
                any2.ProcessContents = XmlSchemaContentProcessing.Lax;
                sequence.Items.Add(any2);
                XmlSchemaAttribute attribute = new XmlSchemaAttribute();
                attribute.Name = "namespace";
                attribute.FixedValue = set.Namespace;
                type.Attributes.Add(attribute);
                XmlSchemaAttribute attribute2 = new XmlSchemaAttribute();
                attribute2.Name = "tableTypeName";
                attribute2.FixedValue = "TabStringDataTable";
                type.Attributes.Add(attribute2);
                type.Particle = sequence;
                XmlSchema schemaSerializable = set.GetSchemaSerializable();
                if (xs.Contains(schemaSerializable.TargetNamespace)) {
                    MemoryStream stream = new MemoryStream();
                    MemoryStream stream2 = new MemoryStream();
                    try {
                        XmlSchema current = null;
                        schemaSerializable.Write(stream);
                        var enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
                        while (enumerator.MoveNext()) {
                            current = (XmlSchema)enumerator.Current;
                            stream2.SetLength(0L);
                            current.Write(stream2);
                            if (stream.Length == stream2.Length) {
                                stream.Position = 0L;
                                stream2.Position = 0L;
                                while ((stream.Position != stream.Length) && (stream.ReadByte() == stream2.ReadByte())) {
                                }
                                if (stream.Position == stream.Length) {
                                    return type;
                                }
                            }
                        }
                    }
                    finally {
                        if (stream != null) {
                            stream.Close();
                        }
                        if (stream2 != null) {
                            stream2.Close();
                        }
                    }
                }
                xs.Add(schemaSerializable);
                return type;
            }

            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            private void InitClass()
            {
                this.columnTabCode = new DataColumn("TabCode", typeof(string), null, MappingType.Element);
                base.Columns.Add(this.columnTabCode);
                this.columnJapanese = new DataColumn("Japanese", typeof(string), null, MappingType.Element);
                base.Columns.Add(this.columnJapanese);
                base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[] { this.columnTabCode }, true));
                this.columnTabCode.AllowDBNull = false;
                this.columnTabCode.Unique = true;
                this.columnJapanese.AllowDBNull = false;
            }

            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            internal void InitVars()
            {
                this.columnTabCode = base.Columns["TabCode"];
                this.columnJapanese = base.Columns["Japanese"];
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
            {
                return new FixedFormSentenceLibraryDataSet.TabStringRow(builder);
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            public FixedFormSentenceLibraryDataSet.TabStringRow NewTabStringRow()
            {
                return (FixedFormSentenceLibraryDataSet.TabStringRow)base.NewRow();
            }

            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            protected override void OnRowChanged(DataRowChangeEventArgs e)
            {
                base.OnRowChanged(e);
                if (this.TabStringRowChanged != null) {
                    this.TabStringRowChanged(this, new FixedFormSentenceLibraryDataSet.TabStringRowChangeEvent((FixedFormSentenceLibraryDataSet.TabStringRow)e.Row, e.Action));
                }
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            protected override void OnRowChanging(DataRowChangeEventArgs e)
            {
                base.OnRowChanging(e);
                if (this.TabStringRowChanging != null) {
                    this.TabStringRowChanging(this, new FixedFormSentenceLibraryDataSet.TabStringRowChangeEvent((FixedFormSentenceLibraryDataSet.TabStringRow)e.Row, e.Action));
                }
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            protected override void OnRowDeleted(DataRowChangeEventArgs e)
            {
                base.OnRowDeleted(e);
                if (this.TabStringRowDeleted != null) {
                    this.TabStringRowDeleted(this, new FixedFormSentenceLibraryDataSet.TabStringRowChangeEvent((FixedFormSentenceLibraryDataSet.TabStringRow)e.Row, e.Action));
                }
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            protected override void OnRowDeleting(DataRowChangeEventArgs e)
            {
                base.OnRowDeleting(e);
                if (this.TabStringRowDeleting != null) {
                    this.TabStringRowDeleting(this, new FixedFormSentenceLibraryDataSet.TabStringRowChangeEvent((FixedFormSentenceLibraryDataSet.TabStringRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public void RemoveTabStringRow(FixedFormSentenceLibraryDataSet.TabStringRow row)
            {
                base.Rows.Remove(row);
            }

            // Properties
            [Browsable(false), DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public int Count
            {
                get
                {
                    return base.Rows.Count;
                }
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            public FixedFormSentenceLibraryDataSet.TabStringRow this[int index]
            {
                get
                {
                    return (FixedFormSentenceLibraryDataSet.TabStringRow)base.Rows[index];
                }
            }

            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataColumn JapaneseColumn
            {
                get
                {
                    return this.columnJapanese;
                }
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            public DataColumn TabCodeColumn
            {
                get
                {
                    return this.columnTabCode;
                }
            }
        }

        public class TabStringRow : DataRow
        {
            // Fields
            private FixedFormSentenceLibraryDataSet.TabStringDataTable tableTabString;

            // Methods
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            internal TabStringRow(DataRowBuilder rb)
                : base(rb)
            {
                this.tableTabString = (FixedFormSentenceLibraryDataSet.TabStringDataTable)base.Table;
            }

            // Properties
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            public string Japanese
            {
                get
                {
                    return (string)base[this.tableTabString.JapaneseColumn];
                }
                set
                {
                    base[this.tableTabString.JapaneseColumn] = value;
                }
            }

            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0"), DebuggerNonUserCode]
            public string TabCode
            {
                get
                {
                    return (string)base[this.tableTabString.TabCodeColumn];
                }
                set
                {
                    base[this.tableTabString.TabCodeColumn] = value;
                }
            }
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public class TabStringRowChangeEvent : EventArgs
        {
            // Fields
            private DataRowAction eventAction;
            private FixedFormSentenceLibraryDataSet.TabStringRow eventRow;

            // Methods
            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public TabStringRowChangeEvent(FixedFormSentenceLibraryDataSet.TabStringRow row, DataRowAction action)
            {
                this.eventRow = row;
                this.eventAction = action;
            }

            // Properties
            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public DataRowAction Action
            {
                get
                {
                    return this.eventAction;
                }
            }

            [DebuggerNonUserCode, GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
            public FixedFormSentenceLibraryDataSet.TabStringRow Row
            {
                get
                {
                    return this.eventRow;
                }
            }
        }


        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
        public delegate void TabStringRowChangeEventHandler(object sender, FixedFormSentenceLibraryDataSet.TabStringRowChangeEvent e);
    }

}
