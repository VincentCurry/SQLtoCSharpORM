using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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