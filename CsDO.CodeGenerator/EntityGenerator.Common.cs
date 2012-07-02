using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Windows.Forms;
using System.IO;

namespace CsDO.CodeGenerator
{
    public partial class EntityGenerator
    {
        private string namespaceName = "Application.Persist";

        #region Comment Manipulation
        public enum Documetation
        {
            None, Remarks, Summary, Example,
            Exception, Param, Permission,
            Returns, SeeAlso, Include
        };

        private CodeCommentStatementCollection InsertComments(string[] text)
        {
            return InsertDocumentation(Documetation.None, text);
        }

        private CodeCommentStatementCollection InsertDocumentation(Documetation type, string[] text)
        {
            CodeCommentStatementCollection comments = new CodeCommentStatementCollection();

            switch (type)
            {
                case Documetation.Remarks:
                    comments.Add(new CodeCommentStatement("<remarks>", true));
                    break;
                case Documetation.Summary:
                    comments.Add(new CodeCommentStatement("<summary>", true));
                    break;
                case Documetation.Example:
                    comments.Add(new CodeCommentStatement("<example>", true));
                    break;
                case Documetation.Exception:
                    comments.Add(new CodeCommentStatement("<exception>", true));
                    break;
                case Documetation.Param:
                    comments.Add(new CodeCommentStatement("<param>", true));
                    break;
                case Documetation.Permission:
                    comments.Add(new CodeCommentStatement("<permission>", true));
                    break;
                case Documetation.Returns:
                    comments.Add(new CodeCommentStatement("<returns>", true));
                    break;
                case Documetation.SeeAlso:
                    foreach (string comment in text)
                        comments.Add(new CodeCommentStatement("<seealso cref=\"" + comment + "\" />", true));
                    return comments;
                case Documetation.Include:
                    comments.Add(new CodeCommentStatement("<include>", true));
                    break;
                default:
                    break;
            }

            foreach (string comment in text)
                comments.Add(new CodeCommentStatement(comment, (null != type)));

            switch (type)
            {
                case Documetation.Remarks:
                    comments.Add(new CodeCommentStatement("</remarks>", true));
                    break;
                case Documetation.Summary:
                    comments.Add(new CodeCommentStatement("</summary>", true));
                    break;
                case Documetation.Example:
                    comments.Add(new CodeCommentStatement("</example>", true));
                    break;
                case Documetation.Exception:
                    comments.Add(new CodeCommentStatement("</exception>", true));
                    break;
                case Documetation.Param:
                    comments.Add(new CodeCommentStatement("</param>", true));
                    break;
                case Documetation.Permission:
                    comments.Add(new CodeCommentStatement("</permission>", true));
                    break;
                case Documetation.Returns:
                    comments.Add(new CodeCommentStatement("</returns>", true));
                    break;
                case Documetation.Include:
                    comments.Add(new CodeCommentStatement("</include>", true));
                    break;
                default:
                    break;
            }

            return comments;
        } 
        #endregion

        private CodeTypeDeclaration CreateClass(ClassDefinition table)
        {
            CodeTypeDeclaration type = new CodeTypeDeclaration();

            type.Name = table.ToString();
            type.IsClass = true;
            type.IsPartial = true;
            type.BaseTypes.Add(new CodeTypeReference(typeof(CsDO.Lib.DataObject)));
            type.CustomAttributes.Add(new CodeAttributeDeclaration("Serializable",
                    new CodeAttributeArgument[] { }));
            if (!String.IsNullOrEmpty(table.Alias) && !table.Table.Equals(table.Alias))
            {
                type.CustomAttributes.Add(new CodeAttributeDeclaration("Table",
                    new CodeAttributeArgument[] {
                            new CodeAttributeArgument(new CodePrimitiveExpression(table.Table))
                        }));
            }

            #region class comments
            type.Comments.AddRange(InsertDocumentation(Documetation.Remarks, new string[]
                {
                    "Persistence class that maps table '" + table + "'",
                    "Warning: Each property maps a column, use the attribute",
                    "Column to mark properties that should not be persisted."
                }));
            type.Comments.AddRange(InsertDocumentation(Documetation.SeeAlso, new string[] { "Column" }));
            #endregion         

            return type;
        }

        #region Field Manipulation
        private void WriteFields(List<FieldDefinition> list,
          CodeTypeDeclaration type)
        {
            foreach (FieldDefinition definition in list)
                type.Members.Add(WriteField(definition));
        }

        private CodeTypeMember WriteField(FieldDefinition definition)
        {
            CodeMemberField field = null;

            if (definition.Type.IsValueType)
            {
                //CodeTypeReference t2 = new CodeTypeReference(typeof(System.Nullable));
                //t2.TypeArguments.Add(type);
                //field = new CodeMemberField(t2, fieldName);
                if (definition.ForeignKey)
                    field = new CodeMemberField(definition.ForeignKeyType, definition.FieldName);
                else
                    field = new CodeMemberField(definition.Type, definition.FieldName);
            }
            else
            {
                if (definition.ForeignKey)
                    field = new CodeMemberField(definition.ForeignKeyType, definition.FieldName);
                else
                    field = new CodeMemberField(definition.Type, definition.FieldName);
            }

            field.Attributes = MemberAttributes.Private;

            return field;
        }
        
        #endregion

        #region Property Manipulation
        private void WriteProperties(List<FieldDefinition> list,
          CodeTypeDeclaration type)
        {
            foreach (FieldDefinition definition in list)
                type.Members.Add(WriteProperty(definition));
        }

        private CodeMemberProperty WriteProperty(FieldDefinition definition)
        {
            CodeMemberProperty property = null;

            Type type = definition.Type;

            if (type.IsValueType)
            {
                //CodeTypeReference t2 = new CodeTypeReference(typeof(System.Nullable));
                //t2.TypeArguments.Add(type);
                //property = new CodeMemberProperty();
                //property.Type = t2;
                property = new CodeMemberProperty();
                if (definition.ForeignKey)
                    property.Type = new CodeTypeReference(definition.ForeignKeyType);
                else
                    property.Type = new CodeTypeReference(definition.Type);
            }
            else
            {
                property = new CodeMemberProperty();
                if (definition.ForeignKey)
                    property.Type = new CodeTypeReference(definition.ForeignKeyType);
                else
                    property.Type = new CodeTypeReference(definition.Type);
            }

            property.Name = definition.PropertyName;
            property.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            if (property.Name != definition.ColumnName)
            {
                property.CustomAttributes.Add(new CodeAttributeDeclaration("Column",
                    new CodeAttributeArgument[] {
                        new CodeAttributeArgument(new CodePrimitiveExpression(definition.ColumnName))
                    }));
            }

            #region property comments
            property.Comments.AddRange(InsertDocumentation(Documetation.Summary, new string[]
                {
                    "Property that that maps column '" + definition.ColumnName + "'."
                }));
            #endregion 

            property.GetStatements.Add(
                new CodeMethodReturnStatement(
                    new CodeFieldReferenceExpression(null, definition.FieldName)));

            property.SetStatements.Add(
              new CodeAssignStatement(
                new CodeFieldReferenceExpression(null, definition.FieldName),
                    new CodeArgumentReferenceExpression("value")));

            if (definition.PrimaryKey)
            {
                property.CustomAttributes.Add(new CodeAttributeDeclaration("PrimaryKey",
                    new CodeAttributeArgument[] { }));
            }

            return property;
        } 
        #endregion

        private void WriteConstructor(List<FieldDefinition> list,
          CodeTypeDeclaration type)
        {
            if (list.Count == 0) return;

            // define initialization constructor
            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            foreach (FieldDefinition definition in list)
            {
                //assign parameters
                ctor.Parameters.Add(
                  new CodeParameterDeclarationExpression(
                  definition.Type, definition.FieldName));

                // do fields assignments
                ctor.Statements.Add(
                  new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                      new CodeThisReferenceExpression(), definition.FieldName),
                    new CodeArgumentReferenceExpression(definition.FieldName)));
            }

            type.Members.Add(ctor);
        }
    }
}