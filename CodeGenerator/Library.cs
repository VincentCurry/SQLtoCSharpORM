using System;

namespace CodeGenerator
{
    class Library
    {


        internal static string LowerFirstCharacter(string stringToBeLowered)
        {
            string newFirstCharacter = stringToBeLowered.Substring(0, 1).ToLower();
            string potentialReturn = newFirstCharacter + stringToBeLowered.Substring(1);

            string[] CSharpKeywords = { "abstract", "event", "new", "struct", "as", "explicit", "null", "switch", "base", "extern", "object", "this", "bool", "false", "operator", "throw", "break", "finally", "out", "true", "byte", "fixed", "override", "try", "case", "float", "params", "typeof", "catch", "for", "private", "uint", "char", "foreach", "protected", "ulong", "checked", "goto", "public", "unchecked", "class", "if", "readonly", "unsafe", "const", "implicit", "ref", "ushort", "continue", "in", "return", "using", "decimal", "int", "sbyte", "virtual", "default", "interface", "sealed", "volatile", "delegate", "internal", "short", "void", "do", "is", "sizeof", "while", "double", "lock", "stackalloc", "else", "long", "static", "enum", "namespace", "string" };

            foreach(string keyword in CSharpKeywords)
            {
                if(potentialReturn==keyword)
                    return "@" + potentialReturn;
            }

            return potentialReturn;
        }
    }

    internal class SQLDataTypes
    {
        internal const string uniqueIdentifier = "uniqueidentifier";
        internal const string intData = "int";
        internal const string varChar = "nvarchar";
        internal const string bit = "bit";
        internal const string dateTime = "datetime";
        internal const string decimalData = "decimal";
        internal const string varBinary = "varbinary";
        internal const string binary = "binary";
        internal const string floatData = "float";
        internal const string ncharData = "nchar";
        internal const string image = "image";
        internal const string charType = "char";
        internal const string timeType = "time";
        internal const string moneyType = "money";
    }

    /*internal class DotNetSqlDataTypes
    {
        internal const string uniqueIdentifier = "UniqueIdentifier";
        internal const string intData = "Int";
        internal const string varChar = "nvarchar";
        internal const string bit = "Bit";
        internal const string dateTime = "datetime";
        internal const string decimalData = "decimal";
        internal const string varBinary = "varbinary";
    }*/
  

    public class SQLDBTypeNotSupported : Exception
    {
        string dataType;

        public SQLDBTypeNotSupported(string dataType)
        {
            this.dataType = dataType;
        }

        public override string Message
        {
            get
            {
                return dataType + " data type not supported";
            }
        }
    }
}
