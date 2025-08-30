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
            classText.AppendLine(StandardConstraintLayoutStart());

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
            
            classText.AppendLine(StandardConstraintLayoutFinish());
        }


        private string StandardConstraintLayoutStart()
        {
            StringBuilder standardConstraintStart = new StringBuilder();

            standardConstraintStart.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            standardConstraintStart.AppendLine("<androidx.constraintlayout.widget.ConstraintLayout xmlns:android=\"http://schemas.android.com/apk/res/android\"");
            standardConstraintStart.AppendLine("\tandroid:layout_width=\"match_parent\"");
            standardConstraintStart.AppendLine("\tandroid:layout_height=\"match_parent\"");
            standardConstraintStart.AppendLine("\txmlns:tools=\"http://schemas.android.com/tools\"");
            standardConstraintStart.AppendLine("\txmlns:app=\"http://schemas.android.com/apk/res-auto\"");
            standardConstraintStart.AppendLine("\ttools:context=\".ui.oneOffScanCode.OneOffScanCodeFragment\">");

            return standardConstraintStart.ToString();
        }

        private string StandardConstraintLayoutFinish()
        {
            return "</androidx.constraintlayout.widget.ConstraintLayout>";
        }

        private string NestedScrollLayoutStart()
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
            nestedScrollLayoutStart.AppendLine("\t\ttools:context=\".ui.message.MessageFragment\"");
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
            inputFieldCode.AppendLine(previousColumn == null ? "\t\tapp:layout_constraintTop_toTopOf=\"parent\"" : $"\t\tapp:layout_constraintTop_toBottomOf=\"@+id/{Library.LowerFirstCharacter(previousColumn.Name)}\"");
            inputFieldCode.AppendLine(nextColumn == null ? "\t\tapp:layout_constraintBottom_toBottomOf=\"parent\" >" : $"\t\tapp:layout_constraintBottom_toTopOf=\"@+id/{Library.LowerFirstCharacter(nextColumn.Name)}\" >");
            inputFieldCode.AppendLine("\t\t<com.google.android.material.textfield.TextInputEditText");
            inputFieldCode.AppendLine("\t\t\tandroid:inputType=\"text\"");
            inputFieldCode.AppendLine("\t\t\tandroid:selectAllOnFocus=\"true\"");
            inputFieldCode.AppendLine("\t\t\tandroid:layout_width=\"match_parent\"");
            inputFieldCode.AppendLine("\t\t\tandroid:layout_height=\"wrap_content\"");
            inputFieldCode.AppendLine($"\t\t\tandroid:maxLength=\"{column.MaximumLength}\" />");
            inputFieldCode.AppendLine("\t</com.google.android.material.textfield.TextInputLayout>");

            return inputFieldCode.ToString();
            
        }
    }
}
