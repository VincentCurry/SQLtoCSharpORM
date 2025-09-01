using System.Collections.Generic;
using System.Linq;

namespace CodeGenerator
{
    public static class StringExtensions
    {
        public static string LowerFirstCharacter(this string stringToBeLowered)
        {
            string newFirstCharacter = stringToBeLowered.Substring(0, 1).ToLower();
            string potentialReturn = newFirstCharacter + stringToBeLowered.Substring(1);

            string[] CSharpKeywords = { "abstract", "event", "new", "struct", "as", "explicit", "null", "switch", "base", "extern", "object", "this", "bool", "false", "operator", "throw", "break", "finally", "out", "true", "byte", "fixed", "override", "try", "case", "float", "params", "typeof", "catch", "for", "private", "uint", "char", "foreach", "protected", "ulong", "checked", "goto", "public", "unchecked", "class", "if", "readonly", "unsafe", "const", "implicit", "ref", "ushort", "continue", "in", "return", "using", "decimal", "int", "sbyte", "virtual", "default", "interface", "sealed", "volatile", "delegate", "internal", "short", "void", "do", "is", "sizeof", "while", "double", "lock", "stackalloc", "else", "long", "static", "enum", "namespace", "string" };

            foreach (string keyword in CSharpKeywords)
            {
                if (potentialReturn == keyword)
                    return "@" + potentialReturn;
            }

            return potentialReturn;
        }


        public static string Decapitalise(this string stringToBeDecapitalised)
        {
            return stringToBeDecapitalised.Substring(0, 1).ToLower() + stringToBeDecapitalised.Substring(1);
        }

        public static string DecapitaliseAndUpdateSwiftKeywords(this string stringToBeLowered)
        {
            string potentialReturn = Decapitalise(stringToBeLowered);

            string[] swiftKeywords = { "default" };

            foreach (string keyword in swiftKeywords)
            {
                if (potentialReturn == keyword)
                    return "is" + potentialReturn;
            }

            return potentialReturn;
        }

        public static string LowerFirstCharacterAndAddUnderscoreToFurtherCapitals(this string stringToBeDecapitalised)
        {

            string newFirstCharacter = stringToBeDecapitalised.Substring(0, 1).ToLower();
            string potentialReturn = newFirstCharacter + stringToBeDecapitalised.Substring(1);

            IEnumerable<char> capitalLetters = potentialReturn.Where(c => c >= 'A' && c <= 'Z');

            foreach (char capital in capitalLetters)
            {
                potentialReturn = potentialReturn.Replace(capital.ToString(), "_" + capital.ToString().ToLower());
            }

            return potentialReturn;
        }
    }
}