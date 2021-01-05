using System.Collections.Generic;

namespace CodeGenerator
{
    public class AndroidViewModelGenerator : Generator
    {
        public AndroidViewModelGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            filePrefix = "ViewModel";
        }
        internal override void GenerateFilePerTable(SQLTable table)
        {
            string className = table.Name;
            string objectName = Library.LowerFirstCharacter(table.Name);

            classText.AppendLine($"package com.example.{Library.LowerFirstCharacter(_nameSpace)}");

            classText.AppendLine($"import androidx.lifecycle.* ");
            classText.AppendLine($"import kotlinx.coroutines.launch");

            classText.AppendLine($"class {className}ViewModel (private val repository: {_nameSpace}Repository) : ViewModel(){{");


            classText.AppendLine($"\tval all{className}s:LiveData<List<{className}>> = repository.all{className}s.asLiveData()");

            classText.AppendLine($"\tfun insert({objectName}: {className}) = viewModelScope.launch{{");
            classText.AppendLine($"\t\trepository.insert({objectName})");
            classText.AppendLine($"\t}}");
            classText.AppendLine($"}}");

            classText.AppendLine($"class {className}ViewModelFactory(private val repository: {_nameSpace}Repository) : ViewModelProvider.Factory {{");
            classText.AppendLine($"\toverride fun<T : ViewModel> create(modelClass: Class<T>): T {{");
            classText.AppendLine($"\t\tif (modelClass.isAssignableFrom({className}ViewModel::class.java)) {{");
            classText.AppendLine($"\t\t\t@Suppress(\"UNCHECKED_CAST\")");
            classText.AppendLine($"\t\t\treturn {className}ViewModel(repository) as T");
            classText.AppendLine($"\t\t}}");
            classText.AppendLine($"\t\tthrow IllegalArgumentException(\"Unknown ViewModel class\")");
            classText.AppendLine($"\t}}");
            classText.AppendLine($"}}");
        }
    }
}
