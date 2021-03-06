﻿using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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

        internal static string LowerFirstCharacterAndAddUnderscoreToFurtherCapitals(string stringToBeDecapitalised)
        {

            string newFirstCharacter = stringToBeDecapitalised.Substring(0, 1).ToLower();
            string potentialReturn = newFirstCharacter + stringToBeDecapitalised.Substring(1);

            IEnumerable<char> capitalLetters = potentialReturn.Where(c => c >= 'A' && c <= 'Z');

            foreach(char capital in capitalLetters)
            {
               potentialReturn = potentialReturn.Replace(capital.ToString(), "_" + capital.ToString().ToLower());
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
        internal const string doubleData = "double";
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
    internal class htmlFormValueType
    {
        internal const string text = "text";
        internal const string number = "number";
        internal const string checkbox = "checkbox";
        internal const string colour = "color";
        internal const string date = "date";
        internal const string datetimeLocal = "datetime-local";
        internal const string email = "email";
        internal const string file = "file";
        internal const string hidden = "hidden";
        internal const string time = "time";
    }

    internal class kotlinDataTypes
    {
        internal const string strings = "String";
        internal const string date = "Date";
        internal const string doubleNum = "Double";
        internal const string integer = "Int";
        internal const string boolean = "Boolean";
        internal const string floatNum = "Float";
    }

    internal class iosDataTypes
    {
        internal const string strings = "String";
        internal const string date = "Date";
        internal const string doubleNum = "Double";
        internal const string integer = "Int32";
        internal const string boolean = "Bool";
        internal const string floatNum = "Float";
    }

    internal class sqlLiteStorageDataTypes
    {
        internal const string nullStore = "NULL";
        internal const string intStore = "INTEGER";
        internal const string floatStore = "REAL";
        internal const string textStore = "TEXT";
        internal const string blobStore = "BLOB";
    }

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
