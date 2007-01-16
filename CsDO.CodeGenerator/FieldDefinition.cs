using System;
using System.Collections.Generic;
using System.Text;

namespace CsDO.CodeGenerator
{
  public class FieldDefinition
  {
    public FieldDefinition(string fieldName, Type type)
    {
      this.propertyName = fieldName;
      this.fieldName = "_" + fieldName;
      this.type = type;
    }
    
    private Type type;
    public Type Type
    {
      get { return type; }
    }

    private bool primaryKey = false;
    public bool PrimaryKey
    {
        get { return primaryKey; }
        set { primaryKey = value; }
    }

    private bool foreignKey = false;
    public bool ForeignKey
    {
        get { return foreignKey; }
        set { foreignKey = value; }
    }

    private string fieldName;
    public string FieldName
    {
      get { return fieldName; }
    }

    private string propertyName;
    public string PropertyName
    {
        get { return propertyName; }
        set { propertyName = value; } 
    }
  }
}