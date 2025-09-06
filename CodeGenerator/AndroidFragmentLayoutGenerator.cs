using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CodeGenerator
{
    public class AndroidFragmentLayoutGenerator : Generator
    {
        bool dimensFileCreated = false;
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
            if(!dimensFileCreated)
            {
                Library.WriteToKotlinDimensFile("fragment_horizontal_margin", "16dp", _destinationFolder);
                dimensFileCreated = true;
            }

            classText.AppendLine(StandardConstraintLayoutStart(table.Name));

            List<SQLTableColumn> columnsWithoutPrimaryKey = table.Columns.Where(co => !co.PrimaryKey).ToList();

            if (columnsWithoutPrimaryKey.Count > 1)
            {
                for (int i = 0; i < columnsWithoutPrimaryKey.Count; i++)
                {
                    if (i == 0)
                    {
                        classText.AppendLine(CreateInputField(columnsWithoutPrimaryKey[i], null, columnsWithoutPrimaryKey[i + 1]));
                    }
                    else if (i == columnsWithoutPrimaryKey.Count - 1)
                    {
                        classText.AppendLine(CreateInputField(columnsWithoutPrimaryKey[i], columnsWithoutPrimaryKey[i - 1], null));
                    }
                    else
                    {
                        classText.AppendLine(CreateInputField(columnsWithoutPrimaryKey[i], columnsWithoutPrimaryKey[i - 1], columnsWithoutPrimaryKey[i + 1]));
                    }
                }
            }
            else
            {
                classText.AppendLine(CreateInputField(columnsWithoutPrimaryKey[0], null, null));
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
            classText.AppendLine($"\t\t\tapp:layout_constraintTop_toBottomOf=\"@+id/{table.Columns[table.Columns.Count - 1].KotlinFragmentNameForPreviousField}\"");
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
            classText.AppendLine($"\t\t\tapp:layout_constraintEnd_toEndOf=\"@+id/{table.Columns[table.Columns.Count - 1].KotlinFragmentNameForPreviousField}\"");
            classText.AppendLine($"\t\t\tapp:layout_constraintStart_toStartOf=\"@+id/{table.Columns[table.Columns.Count - 1].KotlinFragmentNameForNextField}\"");
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

            string previousControl = previousColumn == null ? "\t\tapp:layout_constraintTop_toTopOf=\"parent\"" : $"\t\tapp:layout_constraintTop_toBottomOf=\"@+id/{previousColumn.KotlinFragmentNameForPreviousField}\"";
            string nextControl = nextColumn == null ? $"\t\tapp:layout_constraintBottom_toTopOf=\"@+id/save{column.TableName}\"" : $"\t\tapp:layout_constraintBottom_toTopOf=\"@+id/{nextColumn.KotlinFragmentNameForNextField}\"";

            if (column.cSharpDataType == "DateTime")
            {
                if (column.Nullable)
                {
                    inputFieldCode.AppendLine("\t<com.google.android.material.materialswitch.MaterialSwitch");
                    inputFieldCode.AppendLine($"\t\tandroid:id=\"@+id/{column.KotlinDateSwitchField}\"");
                    inputFieldCode.AppendLine($"\t\tandroid:layout_width=\"wrap_content\"");
                    inputFieldCode.AppendLine($"\t\tandroid:layout_height=\"wrap_content\"");
                    string dateFieldKey = $"{column.TableName.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_{column.Name.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_field";
                    inputFieldCode.AppendLine($"\t\tandroid:text=\"@string/{dateFieldKey}\"");
                    Library.WriteToKotlinStringsFile(dateFieldKey, $"{column.Name} Date", _destinationFolder);
                    inputFieldCode.AppendLine($"\t\tapp:layout_constraintBottom_toTopOf=\"@id/{column.Name.Decapitalise()}DateInputLayout\"");
                    inputFieldCode.AppendLine($"\t\tapp:layout_constraintEnd_toEndOf=\"parent\"");
                    inputFieldCode.AppendLine($"\t\tapp:layout_constraintStart_toStartOf=\"parent\"");
                    inputFieldCode.Append(previousControl);
                    inputFieldCode.AppendLine(" />");
                    inputFieldCode.Append(Environment.NewLine);
                }

                inputFieldCode.AppendLine("\t<com.google.android.material.textfield.TextInputLayout");
                inputFieldCode.AppendLine($"\t\tandroid:id=\"@+id/{column.KotlinDateLabelField}\"");
                inputFieldCode.AppendLine($"\t\tandroid:layout_width=\"match_parent\"");
                inputFieldCode.AppendLine($"\t\tandroid:layout_height=\"wrap_content\"");
                inputFieldCode.AppendLine($"\t\tapp:layout_constraintEnd_toEndOf=\"parent\"");
                inputFieldCode.AppendLine($"\t\tapp:layout_constraintStart_toStartOf=\"parent\"");
                inputFieldCode.AppendLine(column.Nullable ? $"\t\tapp:layout_constraintTop_toBottomOf=\"@id/{column.KotlinDateSwitchField}\"": previousControl);
                inputFieldCode.AppendLine(nextControl);
                inputFieldCode.AppendLine($"\t\tandroid:visibility=\"{(column.Nullable ? "gone" : "visible")}\"");
                inputFieldCode.AppendLine($"\t\tapp:endIconMode=\"custom\"");
                inputFieldCode.AppendLine($"\t\tapp:endIconDrawable=\"@drawable/calendar_month_24px\"> <!-- use a calendar icon -->");

                inputFieldCode.AppendLine($"\t\t<com.google.android.material.textfield.TextInputEditText");
                inputFieldCode.AppendLine($"\t\t\tandroid:id=\"@+id/{column.KotlinDateTextField}\"");
                inputFieldCode.AppendLine($"\t\t\tandroid:layout_width=\"match_parent\"");
                inputFieldCode.AppendLine($"\t\t\tandroid:layout_height=\"wrap_content\"");
                inputFieldCode.AppendLine($"\t\t\tandroid:focusable=\"false\"");
                inputFieldCode.AppendLine($"\t\t\tandroid:clickable=\"true\"");
                string selectDateHintKey = $"{column.TableName.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_date_{column.Name.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_hint";
                inputFieldCode.AppendLine($"\t\t\tandroid:hint=\"@string/{selectDateHintKey}\" />");
                Library.WriteToKotlinStringsFile(selectDateHintKey, $"Select {column.Name} date", _destinationFolder);
                inputFieldCode.AppendLine("\t</com.google.android.material.textfield.TextInputLayout>");

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
                string textHintKey = $"{column.TableName.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_{column.Name.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_title";
                inputFieldCode.AppendLine($"\t\tandroid:hint=\"@string/{textHintKey}\"");
                Library.WriteToKotlinStringsFile(textHintKey, column.Name, _destinationFolder);
                inputFieldCode.AppendLine("\t\tapp:layout_constraintEnd_toEndOf=\"parent\"");
                inputFieldCode.AppendLine("\t\tapp:layout_constraintStart_toStartOf=\"parent\"");
                inputFieldCode.AppendLine(previousControl);
                inputFieldCode.Append(nextControl);
                inputFieldCode.AppendLine(" >");
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
