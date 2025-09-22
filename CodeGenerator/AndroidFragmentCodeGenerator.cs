using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerator
{
    public class AndroidFragmentCodeGenerator : Generator
    {
        string dateDisplayFormatStringKey = "date_display_format";

        /*
         * If there are any date or date time parameters, the following two functions will need to be added to the library:
         * 
         * fun showDatePicker(
            initialDate: Long? = null,
            title: String = "Select a date",
            fragment: Fragment,
            onDateSelected: (LocalDate) -> Unit
        ) {
            val picker = MaterialDatePicker.Builder.datePicker()
                .setTitleText(title)
                .setSelection(initialDate ?: MaterialDatePicker.todayInUtcMilliseconds())
                .build()

            picker.addOnPositiveButtonClickListener { millis ->
                onDateSelected(Instant.ofEpochMilli(millis).atZone(ZoneId.systemDefault()).toLocalDate())
            }

            fragment.activity?.let { picker.show(it.supportFragmentManager, "MATERIAL_DATE_PICKER") }
        }

        fun showDateTimePicker(initialDateTime: Long? = null, dateTitle: String, timeTitle: String, fragment: Fragment, onDateTimeSelected: (LocalDateTime) -> Unit) {
            val datePicker = MaterialDatePicker.Builder.datePicker()
                .setTitleText(dateTitle)
                .setSelection(MaterialDatePicker.todayInUtcMilliseconds())
                .build()

            fragment.activity?.supportFragmentManager?.let { datePicker.show(it,"DATE_PICKER") }


            // Temporary store picked date
            var pickedDate: LocalDate? = null

            datePicker.addOnPositiveButtonClickListener { dateMillis ->
                pickedDate = Instant.ofEpochMilli(dateMillis)
                    .atZone(ZoneId.systemDefault())
                    .toLocalDate()
            }

            // When DatePicker is dismissed → show TimePicker if a date was chosen
            datePicker.addOnDismissListener {
                pickedDate?.let { date ->
                    val timePicker = MaterialTimePicker.Builder()
                        .setTitleText(timeTitle)
                        .setTimeFormat(TimeFormat.CLOCK_24H) // or CLOCK_12H
                        .setHour(12)
                        .setMinute(0)
                        .build()

                    fragment.activity?.let { timePicker.show(it.supportFragmentManager, "TIME_PICKER") }

                    timePicker.addOnPositiveButtonClickListener {
                        val pickedTime = LocalTime.of(timePicker.hour, timePicker.minute)
                        val pickedDateTime = LocalDateTime.of(date, pickedTime)
                        onDateTimeSelected(pickedDateTime)
                    }
                }
            }
        }
         * */
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
            classText.Append(table.ContainsDateColumn ? $"import kotlinx.coroutines.flow.collectLatest{Environment.NewLine}" : "");
            classText.AppendLine("import kotlinx.coroutines.launch");
            classText.AppendLine("import kotlinx.coroutines.withContext");
            classText.Append(table.ContainsDateColumn ? $"import java.time.format.DateTimeFormatter{Environment.NewLine}" : "");

            classText.Append(Environment.NewLine);

            classText.AppendLine($"class {table.Name}Fragment : Fragment() {{");
            classText.Append(Environment.NewLine);

            classText.AppendLine($"\tprivate lateinit var {table.Name.Decapitalise()}ViewModel: {table.Name}ViewModel");
            classText.AppendLine($"\tprivate var _binding: Fragment{table.Name}Binding? = null");
            classText.Append(Environment.NewLine);

            if (table.ContainsDateColumn)
            {
                string dateAndTimeDisplayFormatStringKey = "date_and_time_display_format";
                Library.WriteToKotlinStringsFile(dateDisplayFormatStringKey, "dd MMM yyyy", _destinationFolder);
                Library.WriteToKotlinStringsFile(dateAndTimeDisplayFormatStringKey, "d MMM yyyy hh:mm:ss", _destinationFolder);

                classText.AppendLine($"private lateinit var dateTimeFormatter: DateTimeFormatter");
            }

            classText.AppendLine("\tprivate val binding get() = _binding!!");
            classText.Append(Environment.NewLine);

            classText.AppendLine("\toverride fun onCreateView(");
            classText.AppendLine("\t\tinflater: LayoutInflater,");
            classText.AppendLine("\t\tcontainer: ViewGroup?,");
            classText.AppendLine("\t\tsavedInstanceState: Bundle?");
            classText.AppendLine("\t): View? {");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine("dateTimeFormatter = DateTimeFormatter.ofPattern(getString(R.string.{ dateAndTimeDisplayFormatStringKey}))");
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
            classText.Append(Environment.NewLine);

            classText.AppendLine(Library.TableColumnsCode(table, FragmentBinding, includePrimaryKey: false, appendCommas: false, singleLine: false));

            classText.AppendLine($"\t\tval save{table.Name}Button = binding.save{table.Name}");
            classText.AppendLine("\t\tval loadingProgressBar = binding.loading");
            classText.Append(Environment.NewLine);
            classText.AppendLine(Library.TableColumnsCode(table, DateChooserDisplayAndSetOfDateValue, includePrimaryKey: false, appendCommas: false, singleLine: false));


            classText.AppendLine($"\t\t{table.Name.Decapitalise()}ViewModel.{table.Name.Decapitalise()}FormState.observe(viewLifecycleOwner,");
            classText.AppendLine($"\t\t\tObserver {{ {table.Name.Decapitalise()}FormState ->");
            classText.AppendLine($"\t\t\t\tif ({table.Name.Decapitalise()}FormState == null) {{");
            classText.AppendLine("\t\t\t\t\treturn@Observer");
            classText.AppendLine("\t\t\t\t}");

            foreach (SQLTableColumn column in table.Columns)
            {
                if (column.IsToBeValidated && column.kotlinDataType == kotlinDataTypes.strings)
                {
                    classText.AppendLine($"\t\t\t\t{column.Name.Decapitalise()}EditText.error = {table.Name.Decapitalise()}FormState.{column.Name.Decapitalise()}Error?.let {{ getString(it) }}");
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
            classText.AppendLine($"\t\t\t\t\tupdateUiWithSaved{table.Name}(it)");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine("\t\t\t})");
            classText.Append(Environment.NewLine);

            classText.AppendLine("//functions for date and time values");
            classText.AppendLine(Library.TableColumnsCode(table.Columns.Where(col => col.kotlinDataType == kotlinDataTypes.date), DateViewModelListener, includePrimaryKey: false, appendCommas: false, singleLine: true));
            classText.Append(Environment.NewLine);

            classText.AppendLine("//functions for date values");
            classText.AppendLine(Library.TableColumnsCode(table.Columns.Where(col => col.kotlinDataType == kotlinDataTypes.date), DateModelListener, includePrimaryKey: false, appendCommas: false, singleLine: true));

            classText.AppendLine(Library.TableColumnsCode(table, ValueChangedListener, includePrimaryKey: false, appendCommas: false, singleLine: false));            
            classText.AppendLine(Library.TableColumnsCode(table, TextEditorTextChangedListener, includePrimaryKey: false, appendCommas: false, singleLine: false));
            classText.AppendLine(Environment.NewLine);
            classText.AppendLine(Library.TableColumnsCode(table.Columns.Where(co => co.IsToBeValidated), OnFocusListener, includePrimaryKey: false, appendCommas: false, singleLine: false));
            classText.AppendLine($"\t\tsave{table.Name}Button.setOnClickListener {{");
            classText.AppendLine("\t\t\tloadingProgressBar.visibility = View.VISIBLE");
            classText.AppendLine($"\t\t\t{table.Name.Decapitalise()}ViewModel.save{table.Name}()");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t}");

            classText.AppendLine($"\t\tprivate fun updateUiWithSaved{table.Name}(result: String) {{");
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
                    chooserText.AppendLine($"\t\t\t{column.TableName.Decapitalise()}ViewModel.set{column.Name}Enabled(isChecked)");

                    chooserText.AppendLine($"\t\t}}");
                    chooserText.Append(Environment.NewLine);
                }
                chooserText.AppendLine("//Use these functions if you require a Date");
                chooserText.AppendLine($"\t\t{column.KotlinDateLabelField}.setEndIconOnClickListener({{");
                chooserText.AppendLine(SetUpDatePicker(column));
                chooserText.AppendLine("\t\t})");
                chooserText.AppendLine($"\t\t{column.KotlinDateTextField}.setOnClickListener({{");
                chooserText.AppendLine(SetUpDatePicker(column));
                chooserText.AppendLine("\t\t})");

                chooserText.AppendLine("//Use these functions if you require a DateTime");
                chooserText.AppendLine($"\t\t{column.KotlinDateLabelField}.setEndIconOnClickListener({{");
                chooserText.AppendLine(SetUpDateTimePicker(column));
                chooserText.AppendLine("\t\t})");
                chooserText.AppendLine($"\t\t{column.KotlinDateTextField}.setOnClickListener({{");
                chooserText.AppendLine(SetUpDateTimePicker(column));
                chooserText.AppendLine("\t\t})");

                return chooserText.ToString();
            }
            else { return ""; }
        }

        private string SetUpDatePicker(SQLTableColumn column)
        {
            StringBuilder setUpDatePickerFunction = new StringBuilder();
            setUpDatePickerFunction.AppendLine($"\t\t\tLibrary.showDatePicker(null, getString(R.string.{column.TableName.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_date_{column.Name.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_hint)), this {{ selectedDate ->");
            setUpDatePickerFunction.AppendLine($"\t\t\t{column.TableName.Decapitalise()}ViewModel.update{column.Name}(selectedDate)");

            setUpDatePickerFunction.AppendLine("\t\t\t}");
            return setUpDatePickerFunction.ToString();
        }

        private string SetUpDateTimePicker(SQLTableColumn column)
        {
            StringBuilder setUpDateTimePickerFunction = new StringBuilder();
            setUpDateTimePickerFunction.AppendLine($"\t\t\tLibrary.showDateTimePicker(null, getString(R.string.{column.TableName.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_date_{column.Name.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_hint)), getString(R.string.{column.TableName.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_time_{column.Name.LowerFirstCharacterAndAddUnderscoreToFurtherCapitals()}_hint)), this {{ selectedDate ->");
            setUpDateTimePickerFunction.AppendLine($"\t\t\t\t{column.TableName.Decapitalise()}ViewModel.update{column.Name}(selectedDate)");
            setUpDateTimePickerFunction.AppendLine("\t\t\t}");
            return setUpDateTimePickerFunction.ToString();
        }

        private string ReadControlsForKotlin(SQLTableColumn column)
        {
            if (column.DataType == SQLDataTypes.dateTime)
                return $"{column.Name.Decapitalise()}Date";
            else
                return $"{column.Name.Decapitalise()}EditText.editText?.text.toString()";
        }

        private string TextEditorTextChangedListener(SQLTableColumn column)
        {
            if (column.DataType != SQLDataTypes.dateTime)
                return $"{column.Name.Decapitalise()}EditText.editText?.addTextChangedListener(after{column.Name}ChangedListener)";
            else
                return "";
        }

        private string DateVariablesAtFragmentLevel(SQLTableColumn column)
        {
            if(column.Nullable)
            {
                return $"private var {column.Name.Decapitalise()}Date: Date? = null";
            }
            else
            {
                return $"private lateinit var {column.Name.Decapitalise()}Date: Date";
            }
        }

        private string ValueChangedListener(SQLTableColumn column)
        {
            if (column.kotlinDataType == kotlinDataTypes.date)
            {
                return "";
            }
            else
            {
                StringBuilder textChangedHandler = new StringBuilder();

                textChangedHandler.AppendLine($"val after{column.Name}ChangedListener = object : TextWatcher {{");
                textChangedHandler.AppendLine("\t\t\toverride fun beforeTextChanged(s: CharSequence, start: Int, count: Int, after: Int) {");
                textChangedHandler.AppendLine("\t\t\t\t// ignore");
                textChangedHandler.AppendLine("\t\t\t}");
                textChangedHandler.AppendLine("\t\t\toverride fun onTextChanged(s: CharSequence, start: Int, before: Int, count: Int) {");
                textChangedHandler.AppendLine("\t\t\t\t// ignore");
                textChangedHandler.AppendLine("\t\t\t}");
                textChangedHandler.AppendLine("\t\t\toverride fun afterTextChanged(s: Editable) {");
                textChangedHandler.AppendLine($"\t\t\t\t{column.TableName.Decapitalise()}ViewModel.update{column.Name}({column.Name.Decapitalise()}EditText.editText?.text.toString())");
                textChangedHandler.AppendLine($"\r\n\t\t\t\tsave{column.TableName}Button.isEnabled = {column.TableName.Decapitalise()}ViewModel.validateAll()");
                textChangedHandler.AppendLine("\t\t\t}\r\n\t\t}");

                return textChangedHandler.ToString();
            }
        }

        private string OnFocusListener(SQLTableColumn column)
        {
            StringBuilder onFocusListener = new StringBuilder();

            onFocusListener.AppendLine($"\t{column.Name.Decapitalise()}EditText.editText?.setOnFocusChangeListener {{ _, hasFocus ->");
            onFocusListener.AppendLine("\t\tif (!hasFocus) {");
            onFocusListener.AppendLine($"\t\t\t{column.TableName.Decapitalise()}ViewModel.validateField(");
            onFocusListener.AppendLine($"\t\t\t\t{column.TableName}FormField.{column.Name},");
            onFocusListener.AppendLine($"\t\t\t\t{column.Name.Decapitalise()}EditText.editText?.text.toString(),");
            onFocusListener.AppendLine("\t\t\t\ttouched = true");
            onFocusListener.AppendLine("\t\t\t)");
            onFocusListener.AppendLine("\t\t}");
            onFocusListener.AppendLine("\t}");

            return onFocusListener.ToString();
        }

        private string DateViewModelListener(SQLTableColumn column) {
            StringBuilder dateViewModelListener = new StringBuilder();
            dateViewModelListener.AppendLine("\t\t\tlifecycleScope.launchWhenStarted {");
            dateViewModelListener.AppendLine($"\t\t\t\t{column.TableName.Decapitalise()}ViewModel.{column.Name.Decapitalise()}.collectLatest {{ {column.Name.Decapitalise()} ->");
            dateViewModelListener.AppendLine($"\t\t\t\t\tif({column.Name.Decapitalise()} == null) {{");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}Switch.isChecked = false");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}DateInputLayout.visibility = View.GONE");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}EditText.setText(\"\")");
            dateViewModelListener.AppendLine($"\t\t\t\t\t}} else {{");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}Switch.isChecked = true");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}DateInputLayout.visibility = View.VISIBLE");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}EditText.setText({column.Name.Decapitalise()}.format(dateTimeFormatter))");
            dateViewModelListener.AppendLine($"\t\t\t\t\t}}");
            dateViewModelListener.AppendLine($"\t\t\t\t}}");
            dateViewModelListener.AppendLine($"\t\t\t}}");

            return dateViewModelListener.ToString();
        }

        private string DateModelListener(SQLTableColumn column)
        {
            StringBuilder dateViewModelListener = new StringBuilder();
            dateViewModelListener.AppendLine("\t\t\tlifecycleScope.launchWhenStarted {");
            dateViewModelListener.AppendLine($"\t\t\t\t{column.TableName.Decapitalise()}ViewModel.{column.Name.Decapitalise()}.collectLatest {{ {column.Name.Decapitalise()} ->");
            dateViewModelListener.AppendLine($"\t\t\t\t\tif({column.Name.Decapitalise()} == null) {{");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}Switch.isChecked = false");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}DateInputLayout.visibility = View.GONE");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}EditText.setText(\"\")");
            dateViewModelListener.AppendLine($"\t\t\t\t\t}} else {{");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}Switch.isChecked = true");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}DateInputLayout.visibility = View.VISIBLE");
            dateViewModelListener.AppendLine($"val formatted{column.Name} = {column.Name.Decapitalise()}.format(DateTimeFormatter.ofPattern(getString(R.string.{dateDisplayFormatStringKey})))");
            dateViewModelListener.AppendLine($"\t\t\t\t\t\t{column.Name.Decapitalise()}EditText.setText(formatted{column.Name})");
            dateViewModelListener.AppendLine($"\t\t\t\t\t}}");
            dateViewModelListener.AppendLine($"\t\t\t\t}}");
            dateViewModelListener.AppendLine($"\t\t\t}}");

            return dateViewModelListener.ToString();
        }
    }
}
