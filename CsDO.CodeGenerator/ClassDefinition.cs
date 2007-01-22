using System;
using System.Collections.Generic;
using System.Text;

namespace CsDO.CodeGenerator
{
    public class ClassDefinition
    {
        public ClassDefinition(string table)
        {
            this.table = table;
        }

        private string table;
        public string Table
        {
            get { return table; }
        }

        private string alias = null;
        public string Alias
        {
            get { return alias; }
            set { alias = value; }
        }

        private List<FieldDefinition> columns = new List<FieldDefinition>();
        public List<FieldDefinition> Columns
        {
            get { return columns; }
        }

        public override string ToString()
        {
            return String.IsNullOrEmpty(alias) ? table : alias;
        }
    }
}