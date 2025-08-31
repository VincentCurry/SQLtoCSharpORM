namespace CodeGenerator
{
    public static class StringExtensions
    {
        public static string Decapitalise(this string stringToBeDecapitalised)
        {
            return stringToBeDecapitalised.Substring(0, 1).ToLower() + stringToBeDecapitalised.Substring(1);
        }
    }
}