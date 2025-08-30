using System.Collections.Generic;

namespace CodeGenerator
{
    public class AndroidViewModelGenerator : GeneratorFromForeignKeys
    {
        public AndroidViewModelGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            fileNameSuffix = "ViewModel";
        }

        internal override void GenerateFilePerForeignKey(SQLForeignKeyRelation foreignKeyRelation, string className)
        {
            GenerateFile(className, Library.LowerFirstCharacter(className), false);
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            string className = table.Name;
            string objectName = Library.LowerFirstCharacter(table.Name);

            GenerateFile(className, objectName, true);
        }

        private void GenerateFile(string className, string objectName, bool includeInsert)
        { 
            classText.AppendLine($"package com.example.{Library.LowerFirstCharacter(_nameSpace)}.viewmodels");

            classText.AppendLine($"import androidx.lifecycle.* ");

            classText.AppendLine($"import com.example.{Library.LowerFirstCharacter(_nameSpace)}.entities.{className}");
            classText.AppendLine($"import com.example.{Library.LowerFirstCharacter(_nameSpace)}.database.{_nameSpace}Repository");

            if (includeInsert)
                classText.AppendLine($"import kotlinx.coroutines.launch");

            classText.AppendLine($"class {className}ViewModel (private val repository: {_nameSpace}Repository) : ViewModel(){{");


            classText.AppendLine($"\tval all{className}s:LiveData<List<{className}>> = repository.all{className}s.asLiveData()");

            if (includeInsert)
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
