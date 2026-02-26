using System.IO;

namespace CodeGenerator
{
    public class SwiftResultStateGenerator
    {
        public string FileName => "ResultState.swift";

        public void Generate(string destinationFile)
        {
            string resultStateFile = @"enum ResultState<T> {
    case idle
    case loading
    case success(T)
    case failure(Error)
}";

            TextWriter writer = File.CreateText(destinationFile + FileName);

            writer.Write(resultStateFile.ToString());

            writer.Close();
        }


    }
}
