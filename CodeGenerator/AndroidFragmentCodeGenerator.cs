using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerator
{
    public class AndroidFragmentCodeGenerator : Generator
    {
        public AndroidFragmentCodeGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = "kt";
            fileNameSuffix = "Fragment";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            classText.AppendLine($"package com.{_nameSpace}.ui.{table.Name.Decapitalise()}");
            classText.Append(Environment.NewLine);

            classText.AppendLine("import android.content.Context");
            classText.AppendLine("import android.content.SharedPreferences");
            classText.AppendLine("import android.os.Bundle");
            classText.AppendLine("import android.text.Editable");
            classText.AppendLine("import android.text.TextWatcher");
            classText.AppendLine("import android.view.LayoutInflater");
            classText.AppendLine("import android.view.View");
            classText.AppendLine("import android.view.ViewGroup");
            classText.AppendLine("import android.widget.Toast");
            classText.AppendLine("import androidx.annotation.StringRes");
            classText.AppendLine("import androidx.fragment.app.Fragment");
            classText.AppendLine("import androidx.lifecycle.Observer");
            classText.AppendLine("import androidx.lifecycle.ViewModelProvider");
            classText.AppendLine($"import com.{_nameSpace}.R");
            classText.AppendLine($"import com.{_nameSpace}.data.model.{table.Name}");
            classText.AppendLine($"import com.{_nameSpace}.databinding.Fragment{table.Name}Binding");
            classText.AppendLine($"import com.{_nameSpace}.preferenceFileName");

            classText.Append(Environment.NewLine);

            classText.AppendLine($"class {table.Name}Fragment : Fragment() {{");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tprivate lateinit var {table.Name.Decapitalise()}ViewModel: {table.Name}ViewModel");
            classText.AppendLine($"\tprivate var _binding: Fragment{table.Name}Binding? = null");
            classText.Append(Environment.NewLine);

            classText.AppendLine("// This property is only valid between onCreateView and onDestroyView.");
            classText.AppendLine("\tprivate val binding get() = _binding!!");
            classText.Append(Environment.NewLine);

            classText.AppendLine("\toverride fun onCreateView(");
            classText.AppendLine("\t\tinflater: LayoutInflater,");
            classText.AppendLine("\t\tcontainer: ViewGroup?,");
            classText.AppendLine("\t\tsavedInstanceState: Bundle?");
            classText.AppendLine("\t): View? {");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine($"\t\t_binding = Fragment{table.Name}Binding.inflate(inflater, container, false)");
            classText.AppendLine("\t\treturn binding.root");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine("\t}");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine("\toverride fun onViewCreated(view: View, savedInstanceState: Bundle?) {");
            classText.AppendLine("\t\tsuper.onViewCreated(view, savedInstanceState)");
            classText.AppendLine($"\t\t{table.Name.Decapitalise()}ViewModel = ViewModelProvider(requireActivity(), {table.Name}ViewModelFactory())");
            classText.AppendLine($"\t\t\t.get({table.Name}ViewModel::class.java)");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine(Library.TableColumnsCode(table, FragmentBinding, includePrimaryKey: false, appendCommas: false, singleLine: false));
            classText.Append(Environment.NewLine);
            classText.AppendLine(Library.TableColumnsCode(table, DateChooserDisplayAndSetOfDateValue, includePrimaryKey: false, appendCommas: false, singleLine: false));

            classText.AppendLine($"\t\tval save{table.Name}Button = binding.save{table.Name}");
            classText.AppendLine("\t\tval loadingProgressBar = binding.loading");
            classText.AppendLine(Environment.NewLine);


            classText.AppendLine($"\t\t{table.Name.Decapitalise()}ViewModel.{table.Name.Decapitalise()}FormState.observe(viewLifecycleOwner,");
            classText.AppendLine($"\t\t\tObserver {{ {table.Name.Decapitalise()}FormState ->");
            classText.AppendLine($"\t\t\t\tif ({table.Name.Decapitalise()}FormState == null) {{");
            classText.AppendLine("\t\t\t\t\treturn@Observer");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine($"\t\t\t\tsave{table.Name}Button.isEnabled = {table.Name.Decapitalise()}FormState.isDataValid");

            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey && !column.Nullable)
                {
                    classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}FormState.{column.Name.Decapitalise()}Error?.let {{");
                    classText.AppendLine($"\t\t\t\t\t{column.Name.Decapitalise()}EditText.error = getString(it)");
                    classText.AppendLine("\t\t\t\t}");
                }
            }
            classText.AppendLine("\t\t\t})");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\t\t{table.Name.Decapitalise()}ViewModel.{table.Name.Decapitalise()}Result.observe(viewLifecycleOwner,");
            classText.AppendLine($"\t\t\tObserver {{ {table.Name.Decapitalise()}Result ->");
            classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}Result ?: return@Observer");
            classText.AppendLine("\t\t\t\tloadingProgressBar.visibility = View.GONE");
            classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}Result.error?.let {{");
            classText.AppendLine($"\t\t\t\t\tshowSave{table.Name}Failed(it)");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}Result.success?.let {{");
            classText.AppendLine($"\t\t\t\t\t\tupdateUiWithSaved{table.Name}(it)");


            classText.AppendLine("\t\t\t\t\t\t}");
            classText.AppendLine("\t\t\t\t})");
            classText.Append(Environment.NewLine);

            classText.AppendLine("\t\tval afterTextChangedListener = object : TextWatcher {");
            classText.AppendLine("\t\t\toverride fun beforeTextChanged(s: CharSequence, start: Int, count: Int, after: Int) {");
            classText.AppendLine("\t\t\t\t// ignore");
            classText.AppendLine("\t\t\t}");

            classText.AppendLine("\t\t\toverride fun onTextChanged(s: CharSequence, start: Int, before: Int, count: Int) {");
            classText.AppendLine("\t\t\t\t// ignore");
            classText.AppendLine("\t\t\t}");

            classText.AppendLine("\t\t\toverride fun afterTextChanged(s: Editable) {");
            classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}ViewModel.{table.Name.Decapitalise()}DataChanged(");

            classText.AppendLine(Library.TableColumnsCode(table, ReadControlsForKotlin, includePrimaryKey: false, appendCommas: true, singleLine: false));

            classText.AppendLine("\t\t\t\t\t)");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t}");
            classText.AppendLine(Library.TableColumnsCode(table, TextEditorTextChangedListener, includePrimaryKey: false, appendCommas: false, singleLine: false));
            classText.AppendLine(Environment.NewLine);
            classText.AppendLine($"\t\tsave{table.Name}Button.setOnClickListener {{");
            classText.AppendLine("\t\t\tloadingProgressBar.visibility = View.VISIBLE");
            classText.AppendLine($"\t\t\t{table.Name.Decapitalise()}ViewModel.save{table.Name}(");
            classText.AppendLine(Library.TableColumnsCode(table, ReadControlsForKotlin, includePrimaryKey: false, appendCommas: true, singleLine: false));
            classText.AppendLine("\t\t\t)");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t}");

            classText.AppendLine($"\t\tprivate fun updateUiWithSaved{table.Name}(model: {table.Name}) {{");
            classText.AppendLine($"\t\t\tval {table.Name.Decapitalise()}SavedMessage: String = getString(R.string.{table.Name.ToLower()}_saved_message)");
            Library.WriteToKotlinStringsFile($"{table.Name.ToLower()}_saved_message", $"The {table.Name} has been successfully saved.", _destinationFolder);
            classText.AppendLine("\t\t\tval appContext = context?.applicationContext ?: return");
            classText.AppendLine($"\t\t\tToast.makeText(appContext, {table.Name.Decapitalise()}SavedMessage, Toast.LENGTH_LONG).show()");
            classText.AppendLine("\t\t}");

            classText.AppendLine($"\t\tprivate fun showSave{table.Name}Failed(@StringRes errorString: Int) {{");
            classText.AppendLine("\t\t\tval appContext = context?.applicationContext ?: return");
            classText.AppendLine("\t\t\tToast.makeText(appContext, errorString, Toast.LENGTH_LONG).show()");
            classText.AppendLine("\t\t}");

            classText.AppendLine("\t\toverride fun onDestroyView() {");
            classText.AppendLine("\t\t\tsuper.onDestroyView()");
            classText.AppendLine("\t\t\t_binding = null");
            classText.AppendLine("\t\t}");

            classText.AppendLine("\t\tprivate fun awaitSharedPreference(sharedPreferenceName: String) : String {");

            classText.AppendLine("\t\t\tval tenthOfSecond: Long = 100");
            classText.AppendLine("\t\t\tvar sharedPrefs: SharedPreferences? = activity?.applicationContext?.getSharedPreferences(");
            classText.AppendLine("\t\t\t\tpreferenceFileName, Context.MODE_PRIVATE");
            classText.AppendLine("\t\t\t)");

            classText.AppendLine("\t\t\twhile(sharedPrefs?.getString(sharedPreferenceName, null) == null) {");
            classText.AppendLine("\t\t\t\tThread.sleep(tenthOfSecond)");
            classText.AppendLine("\t\t\tsharedPrefs =");
            classText.AppendLine("\t\t\t\tactivity?.applicationContext?.getSharedPreferences(preferenceFileName, Context.MODE_PRIVATE)!!");
            classText.AppendLine("\t\t}");

            classText.AppendLine("\t\treturn sharedPrefs.getString(sharedPreferenceName, null)!!");
            classText.AppendLine("\t}");
            classText.AppendLine("}");

        }

        private string FragmentBinding(SQLTableColumn column)
        {
            if (column.DataType == SQLDataTypes.dateTime)
            {
                return $"val {column.Name.Decapitalise()}Switch = binding.{column.KotlinDateSwitchField}\r\nval {column.Name.Decapitalise()}EditText = binding.{column.KotlinDateTextField}\r\nval {column.Name.Decapitalise()}InputLayout = binding.{column.KotlinDateLabelField}";
            }
            else
            {
                return $"\t\tval {column.Name.Decapitalise()}EditText = binding.{column.Name.Decapitalise()}";
            }
        }

        private string DateChooserDisplayAndSetOfDateValue(SQLTableColumn column)
        {
            if (column.DataType == SQLDataTypes.dateTime)
            {
                StringBuilder chooserText = new StringBuilder();

                if (column.Nullable)
                {
                    chooserText.AppendLine($"\t\t{column.KotlinDateSwitchField}.setOnCheckedChangeListener {{ _,isChecked ->");
                    chooserText.AppendLine($"\t\t\t{column.KotlinDateLabelField}.visibility = if (isChecked) View.VISIBLE else View.GONE");

                    chooserText.AppendLine($"\t\t}}");
                    chooserText.Append(Environment.NewLine);
                }
                chooserText.AppendLine($"\t\t{column.KotlinDateLabelField}.setEndIconOnClickListener({{");
                chooserText.AppendLine($"\t\t\tshowDatePicker(null, \"Pick start date\" ) {{ millis ->");
                chooserText.AppendLine($"\t\t\t\t{column.Name.Decapitalise()}Date = Date(millis)");
                chooserText.AppendLine($"\t\t\t\tval sdf = SimpleDateFormat(\"dd MMM yyyy\", Locale.getDefault())");
                chooserText.AppendLine($"\t\t\t\tval date = {column.Name.Decapitalise()}Date?.let {{ it1 -> sdf.format(it1) }}");
                chooserText.AppendLine($"\t\t\t\t{column.KotlinDateTextField}.setText(date)");
                chooserText.AppendLine("\t\t\t}");
                chooserText.Append("\t\t})");
                chooserText.Append(Environment.NewLine);

                return chooserText.ToString();
            }
            else { return ""; }
        }

        private string ReadControlsForKotlin(SQLTableColumn column)
        {
            if (column.DataType == SQLDataTypes.dateTime)
                return $"{column.Name.Decapitalise()}.getDate()";
            else
                return $"{column.Name.Decapitalise()}EditText.editText?.text.toString()";
        }

        private string TextEditorTextChangedListener(SQLTableColumn column)
        {
            if (column.DataType == SQLDataTypes.dateTime)
                return "something different";
            else
                return $"{column.Name.Decapitalise()}EditText.editText?.addTextChangedListener(afterTextChangedListener)";
        }
    }
}
