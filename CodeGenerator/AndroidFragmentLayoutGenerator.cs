using System.Collections.Generic;
using System.Text;

namespace CodeGenerator
{
    public class AndroidFragmentLayoutGenerator : Generator
    {
        public AndroidFragmentLayoutGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "xml";
            fileNamePrefix = "fragment_";
        }

        internal override string FileName(string tableName)
        {
            return $"{_destinationFolder}{fileNamePrefix}{Library.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals(tableName)}{fileNameSuffix}.{fileSuffix}";
        }
        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine(StandardConstraintLayoutStart(table.Name));

            if (table.Columns.Count > 1)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (i == 0)
                    {
                        classText.AppendLine(CreateInputField(table.Columns[i], null, table.Columns[i + 1]));
                    }
                    else if (i == table.Columns.Count - 1)
                    {
                        classText.AppendLine(CreateInputField(table.Columns[i], table.Columns[i - 1], null));
                    }
                    else
                    {
                        classText.AppendLine(CreateInputField(table.Columns[i], table.Columns[i - 1], table.Columns[i + 1]));
                    }
                }
            }
            else
            {
                classText.AppendLine(CreateInputField(null, table.Columns[0], null));
            }

            classText.AppendLine("\t\t<com.google.android.material.button.MaterialButton");
            classText.AppendLine($"\t\t\tandroid:id=\"@+id/save{table.Name}\"");
            classText.AppendLine("\t\t\tandroid:layout_width=\"wrap_content\"");
            classText.AppendLine("\t\t\tandroid:layout_height=\"wrap_content\"");
            classText.AppendLine("\t\t\tandroid:layout_gravity=\"start\"");
            //classText.AppendLine("\t\t\tandroid:layout_marginStart=\"48dp\"");
            //classText.AppendLine("\t\t\tandroid:layout_marginTop=\"16dp\"");
            //classText.AppendLine("\t\t\tandroid:layout_marginEnd=\"48dp\"");
            //classText.AppendLine("\t\t\tandroid:layout_marginBottom=\"64dp\"");
            classText.AppendLine("\t\t\tandroid:enabled=\"false\"");
            classText.AppendLine($"\t\t\tandroid:text=\"@string/{table.Name.ToLower()}_save\"");
            Library.WriteToKotlinStringsFile($"{table.Name.ToLower()}_save", $"Save {table.Name}", _destinationFolder);
            classText.AppendLine($"\t\t\tapp:layout_constraintBottom_toBottomOf=\"parent\"");
            classText.AppendLine($"\t\t\tapp:layout_constraintEnd_toEndOf=\"parent\"");
            classText.AppendLine($"\t\t\tapp:layout_constraintStart_toStartOf=\"parent\"");
            classText.AppendLine($"\t\t\tapp:layout_constraintTop_toBottomOf=\"@+id/{table.Columns[table.Columns.Count - 1].Name.Decapitalise()}\"");
            classText.AppendLine($"\t\t\tapp:layout_constraintVertical_bias=\"0.2\" />");

            classText.AppendLine($"\t<ProgressBar");
            classText.AppendLine($"\t\t\tandroid:id=\"@+id/loading\"");
            classText.AppendLine($"\t\t\tandroid:layout_width=\"wrap_content\"");
            classText.AppendLine($"\t\t\tandroid:layout_height=\"wrap_content\"");
            classText.AppendLine($"\t\t\tandroid:layout_gravity=\"center\"");
            classText.AppendLine($"\t\t\tandroid:layout_marginStart=\"32dp\"");
            classText.AppendLine($"\t\t\tandroid:layout_marginTop=\"64dp\"");
            classText.AppendLine($"\t\t\tandroid:layout_marginEnd=\"32dp\"");
            classText.AppendLine($"\t\t\tandroid:layout_marginBottom=\"64dp\"");
            classText.AppendLine($"\t\t\tandroid:visibility=\"gone\"");
            classText.AppendLine($"\t\t\tapp:layout_constraintBottom_toBottomOf=\"parent\"");
            classText.AppendLine($"\t\t\tapp:layout_constraintEnd_toEndOf=\"@+id/{table.Columns[table.Columns.Count - 1].Name.Decapitalise()}\"");
            classText.AppendLine($"\t\t\tapp:layout_constraintStart_toStartOf=\"@+id/{table.Columns[table.Columns.Count - 1].Name.Decapitalise()}\"");
            classText.AppendLine($"\t\t\tapp:layout_constraintTop_toTopOf=\"parent\"");
            classText.AppendLine($"\t\t\tapp:layout_constraintVertical_bias=\"0.3\" /> ");


            classText.AppendLine(StandardConstraintLayoutFinish());
        }

        private string StandardConstraintLayoutStart(string tableName)
        {
            StringBuilder standardConstraintStart = new StringBuilder();

            standardConstraintStart.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            standardConstraintStart.AppendLine("<androidx.constraintlayout.widget.ConstraintLayout xmlns:android=\"http://schemas.android.com/apk/res/android\"");
            standardConstraintStart.AppendLine("\tandroid:layout_width=\"match_parent\"");
            standardConstraintStart.AppendLine("\tandroid:layout_height=\"match_parent\"");
            standardConstraintStart.AppendLine("\txmlns:tools=\"http://schemas.android.com/tools\"");
            standardConstraintStart.AppendLine("\txmlns:app=\"http://schemas.android.com/apk/res-auto\"");
            standardConstraintStart.AppendLine($"\ttools:context=\".ui.{tableName.Decapitalise()}.{tableName}Fragment\">");

            return standardConstraintStart.ToString();
        }

        private string StandardConstraintLayoutFinish()
        {
            return "</androidx.constraintlayout.widget.ConstraintLayout>";
        }

        private string NestedScrollLayoutStart(string tableName)
        {
            StringBuilder nestedScrollLayoutStart = new StringBuilder();

            nestedScrollLayoutStart.AppendLine("<?xml version =\"1.0\" encoding=\"utf-8\"?>");
            nestedScrollLayoutStart.AppendLine("<androidx.core.widget.NestedScrollView android:layout_width=\"match_parent\"");
            nestedScrollLayoutStart.AppendLine("\tandroid:layout_height=\"wrap_content\"");
            nestedScrollLayoutStart.AppendLine("\txmlns:android=\"http://schemas.android.com/apk/res/android\">");
            nestedScrollLayoutStart.AppendLine("\t<androidx.constraintlayout.widget.ConstraintLayout xmlns:android=\"http://schemas.android.com/apk/res/android\"");
            nestedScrollLayoutStart.AppendLine("\t\txmlns:app=\"http://schemas.android.com/apk/res-auto\"");
            nestedScrollLayoutStart.AppendLine("\t\txmlns:tools=\"http://schemas.android.com/tools\"");
            nestedScrollLayoutStart.AppendLine("\t\tandroid:layout_width=\"match_parent\"");
            nestedScrollLayoutStart.AppendLine($"\t\ttools:context=\".ui.{tableName.Decapitalise()}.{tableName}Fragment\">");
            nestedScrollLayoutStart.AppendLine("\t\tandroid:scrollbars=\"vertical\" android:layout_height=\"wrap_content\">");

            return nestedScrollLayoutStart.ToString();
        }

        private string NestedScrollLayoutFinish()
        {
            return "</androidx.constraintlayout.widget.ConstraintLayout></androidx.core.widget.NestedScrollView>";
        }
        private string CreateInputField(SQLTableColumn column, SQLTableColumn previousColumn, SQLTableColumn nextColumn)
        {
            StringBuilder inputFieldCode = new StringBuilder();

            string previousControl = previousColumn == null ? "\t\tapp:layout_constraintTop_toTopOf=\"parent\"" : $"\t\tapp:layout_constraintTop_toBottomOf=\"@+id/{Library.LowerFirstCharacter(previousColumn.Name)}\"";
            string nextControl = nextColumn == null ? $"\t\tapp:layout_constraintBottom_toTopOf=\"@+id/save{column.TableName}\" >" : $"\t\tapp:layout_constraintBottom_toTopOf=\"@+id/{Library.LowerFirstCharacter(nextColumn.Name)}\" >";

            if (column.cSharpDataType == "DateTime")
            {

                inputFieldCode.AppendLine("\t<DatePicker");
                inputFieldCode.AppendLine($"\t\tandroid:id=\"@+id/{column.Name.Decapitalise()}\"");
                inputFieldCode.AppendLine("\t\tandroid:layout_width=\"wrap_content\"");
                inputFieldCode.AppendLine("\t\tandroid:layout_height=\"wrap_content\"");
                inputFieldCode.AppendLine("\t\tandroid:datePickerMode=\"spinner\"");
                inputFieldCode.AppendLine("\t\tandroid:layoutMode=\"opticalBounds\"");
                inputFieldCode.AppendLine("\t\tandroid:calendarViewShown=\"false\"");
                inputFieldCode.AppendLine("\t\tapp:layout_constraintEnd_toEndOf=\"parent\"");
                inputFieldCode.AppendLine("\t\tapp:layout_constraintStart_toStartOf=\"parent\"");
                inputFieldCode.AppendLine(previousControl);
                inputFieldCode.AppendLine(nextControl);

            }
            else
            {
                inputFieldCode.AppendLine("\t<com.google.android.material.textfield.TextInputLayout");
                inputFieldCode.AppendLine($"\t\tandroid:id=\"@+id/{Library.LowerFirstCharacter(column.Name)}\"");
                inputFieldCode.AppendLine("\t\tapp:shapeAppearanceOverlay=\"@style/RoundedTextBox\"");
                inputFieldCode.AppendLine("\t\tandroid:layout_width=\"0dp\"");
                inputFieldCode.AppendLine("\t\tandroid:layout_height=\"wrap_content\"");
                inputFieldCode.AppendLine("\t\tandroid:layout_marginStart=\"@dimen/fragment_horizontal_margin\"");
                inputFieldCode.AppendLine("\t\tandroid:layout_marginEnd=\"@dimen/fragment_horizontal_margin\"");
                inputFieldCode.AppendLine($"\t\tandroid:hint=\"{column.Name}\"");
                inputFieldCode.AppendLine("\t\tapp:layout_constraintEnd_toEndOf=\"parent\"");
                inputFieldCode.AppendLine("\t\tapp:layout_constraintStart_toStartOf=\"parent\"");
                inputFieldCode.AppendLine(previousControl);
                inputFieldCode.AppendLine(nextControl);
                inputFieldCode.AppendLine("\t\t<com.google.android.material.textfield.TextInputEditText");
                inputFieldCode.AppendLine("\t\t\tandroid:inputType=\"text\"");
                inputFieldCode.AppendLine("\t\t\tandroid:selectAllOnFocus=\"true\"");
                inputFieldCode.AppendLine("\t\t\tandroid:layout_width=\"match_parent\"");
                inputFieldCode.AppendLine("\t\t\tandroid:layout_height=\"wrap_content\"");
                inputFieldCode.AppendLine($"\t\t\tandroid:maxLength=\"{column.MaximumLength}\" />");
                inputFieldCode.AppendLine("\t</com.google.android.material.textfield.TextInputLayout>");
            }
            return inputFieldCode.ToString();
            
        }
    }
}
