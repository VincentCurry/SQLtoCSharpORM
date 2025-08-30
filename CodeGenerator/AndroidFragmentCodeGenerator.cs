using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    public class AndroidFragmentCodeGenerator : Generator
    {
        public AndroidFragmentCodeGenerator(List<SQLTable> tables, string destinationFolder, string nameSpace) : base(tables, destinationFolder, nameSpace)
        {
            fileSuffix = ".kt";
            fileNameSuffix = "Fragment";
        }

        internal override void GenerateFilePerTable(SQLTable table)
        {
            string loweredClassName = Library.LowerFirstCharacter(table.Name);
            classText.AppendLine("package com.receiptsandrewards.merchant.ui.{table.Name.Decapitalise()}");

            classText.AppendLine("import android.content.Context");
            classText.AppendLine("import android.content.SharedPreferences");
            classText.AppendLine("import android.os.Bundle");
            classText.AppendLine("import android.text.Editable");
            classText.AppendLine("import android.text.TextWatcher");
            classText.AppendLine("import android.view.LayoutInflater");
            classText.AppendLine("import android.view.View");
            classText.AppendLine("import android.view.ViewGroup");
            classText.AppendLine("import android.view.inputmethod.EditorInfo");
            classText.AppendLine("import android.widget.Toast");
            classText.AppendLine("import androidx.annotation.StringRes");
            classText.AppendLine("import androidx.fragment.app.Fragment");
            classText.AppendLine("import androidx.lifecycle.Observer");
            classText.AppendLine("import androidx.lifecycle.ViewModelProvider");
            classText.AppendLine("import com.receiptsandrewards.merchant.R");
            classText.AppendLine("import com.receiptsandrewards.merchant.data.SessionManager");
            classText.AppendLine("import com.receiptsandrewards.merchant.databinding.FragmentLoginBinding");
            classText.AppendLine("import com.receiptsandrewards.merchant.preferenceFileName");
            classText.AppendLine("import com.receiptsandrewards.merchant.preferenceFirebaseToken");
            classText.AppendLine("import com.receiptsandrewards.merchant.preferenceUniqueId");
            /*classText.AppendLine("import com.receiptsandrewards.merchant.ui.oneOffScanCode.OneOffScanCodeFragment");
            classText.AppendLine("import com.receiptsandrewards.merchant.ui.oneOffScanCode.OneOffScanCodeViewModel");
            classText.AppendLine("import com.receiptsandrewards.merchant.ui.oneOffScanCode.OneOffScancodeViewModelFactory");*/

            classText.AppendLine(Environment.NewLine);

            classText.AppendLine($"class {table.Name}Fragment : Fragment() {{");
            classText.AppendLine(Environment.NewLine);

            classText.AppendLine($"\tprivate lateinit var {table.Name.Decapitalise()}ViewModel: {table.Name}ViewModel");
            classText.AppendLine($"\tprivate var _binding: Fragment{table.Name}Binding? = null");
            classText.AppendLine(Environment.NewLine);

            // This property is only valid between onCreateView and
            // onDestroyView.
            classText.AppendLine("\tprivate val binding get() = _binding!!");
            classText.AppendLine(Environment.NewLine);

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

            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey)
                {
                    classText.AppendLine($"\t\tval {column.Name.Decapitalise()}EditText = binding.{column.Name.Decapitalise()}");
                }
            }
            classText.AppendLine("\t\tval saveButton = binding.save");
            classText.AppendLine("\t\tval loadingProgressBar = binding.loading");
            classText.AppendLine(Environment.NewLine);


            classText.AppendLine($"\t\t{table.Name.Decapitalise()}ViewModel.{table.Name.Decapitalise()}FormState.observe(viewLifecycleOwner,");
            classText.AppendLine($"\t\t\tObserver {{ {table.Name.Decapitalise()}FormState ->");
            classText.AppendLine($"\t\t\t\tif ({table.Name.Decapitalise()}FormState == null) {{");
            classText.AppendLine("\t\t\t\t\treturn@Observer");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine($"\t\t\t\tsaveButton.isEnabled = {table.Name.Decapitalise()}FormState.isDataValid");

            foreach (SQLTableColumn column in table.Columns)
            {
                if (!column.PrimaryKey && (!column.Nullable || column.cSharpDataType == "string"))
                {
                    classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}FormState.{column.Name.Decapitalise()}Error?.let {{");
                    classText.AppendLine($"\t\t\t\t\t{column.Name.Decapitalise()}EditText.error = getString(it)");
                    classText.AppendLine("\t\t\t\t}");
                }
            }
            classText.AppendLine("\t\t\t})");

            classText.AppendLine($"\t\t{table.Name.Decapitalise()}ViewModel.{table.Name.Decapitalise()}Result.observe(viewLifecycleOwner,");
            classText.AppendLine($"\t\t\tObserver {{ {table.Name.Decapitalise()}Result ->");
            classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}Result ?: return@Observer");
            classText.AppendLine("\t\t\t\tloadingProgressBar.visibility = View.GONE");
            classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}Result.error?.let {{");
            classText.AppendLine("\t\t\t\t\tshowSaveFailed(it)");
            classText.AppendLine("\t\t\t\t}");
            classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}Result.success?.let {{");

            /*classText.AppendLine("\t\t\t\t\tif(SessionManager.isLoggedIn()) {");
            classText.AppendLine("\t\t\t\t\t\tval oneOffScanCodeViewModel = ViewModelProvider(requireActivity(), OneOffScancodeViewModelFactory()).get(OneOffScanCodeViewModel::class.java)");
            classText.AppendLine("\t\t\t\t\t\t\tval deviceId: String = awaitSharedPreference(preferenceUniqueId)");
            classText.AppendLine("\t\t\t\t\t\t\tval firebaseToken: String =  awaitSharedPreference(preferenceFirebaseToken)");
            classText.AppendLine("\t\t\t\t\t\t\tSessionManager.loggedInUser?.defaultLoyaltyOfferId?.let {");
            classText.AppendLine("\t\t\t\t\t\t\t\toneOffScanCodeViewModel.getOneOffScanCode(");
            classText.AppendLine("\t\t\t\t\t\t\t\t\tit, 1, deviceId, firebaseToken");
            classText.AppendLine("\t\t\t\t\t\t\t\t)");
            classText.AppendLine("\t\t\t\t\t\t\t}");*/
            classText.AppendLine("\\\t\t\t\t\t\tupdateUiWithUser(it)");

            //classText.AppendLine("\t\t\t\t\t\t\tSessionManager.currentBusiness?.copyBusinessIconLocally(requireActivity().applicationContext)");

            classText.AppendLine("\t\t\t\t\t\t\tactivity?.supportFragmentManager?.beginTransaction()?.apply {");
            classText.AppendLine("\t\t\t\t\t\t\t\treplace(R.id.flFragment, OneOffScanCodeFragment())");
            classText.AppendLine("\t\t\t\t\t\t\t\taddToBackStack(null)");
            classText.AppendLine("\t\t\t\t\t\t\t\tcommit()");
            classText.AppendLine("\t\t\t\t\t\t\t}");
            classText.AppendLine("\t\t\t\t\t\t}");
            classText.AppendLine("\t\t\t\t\t}");
            classText.AppendLine("\t\t\t\t})");

            classText.AppendLine("\t\tval afterTextChangedListener = object : TextWatcher {");
            classText.AppendLine("\t\t\toverride fun beforeTextChanged(s: CharSequence, start: Int, count: Int, after: Int) {");
            classText.AppendLine("\t\t\t\t// ignore");
            classText.AppendLine("\t\t\t}");

            classText.AppendLine("\t\t\toverride fun onTextChanged(s: CharSequence, start: Int, before: Int, count: Int) {");
            classText.AppendLine("\t\t\t\t// ignore");
            classText.AppendLine("\t\t\t}");

            classText.AppendLine("\t\t\toverride fun afterTextChanged(s: Editable) {");
            classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}ViewModel.{table.Name.Decapitalise()}DataChanged(");
            bool firstSaveParameter = true;
            foreach (SQLTableColumn column in table.Columns)
            {
                if (!firstSaveParameter)
                    classText.AppendLine(",");

                firstSaveParameter = false;

                classText.Append($"\t\t\t\t\t{column.Name.Decapitalise()}EditText.editText?.text.toString()");
            }
            classText.AppendLine("\t\t\t\t\t)");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t}");
            foreach (SQLTableColumn column in table.Columns)
            {
                classText.AppendLine($"\t\t{column.Name.Decapitalise()}EditText.editText?.addTextChangedListener(afterTextChangedListener)");
            }
            /*classText.AppendLine("\t\tpasswordEditText.editText?.setOnEditorActionListener { _, actionId, _ ->");
            classText.AppendLine("\t\t\tif (actionId == EditorInfo.IME_ACTION_DONE) {");
            classText.AppendLine($"\t\t\t\t{table.Name.Decapitalise()}ViewModel.save(");

            bool firstSaveParameter = true;
            foreach (SQLTableColumn column in table.Columns)
            {
                if (!firstSaveParameter)
                    classText.AppendLine(",");

                firstSaveParameter = false;

                classText.Append($"\t\t\t\t\t{column.Name.Decapitalise()}EditText.editText?.text.toString()");
            }
            classText.AppendLine();
            classText.AppendLine("\t\t\t\t)");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t\tfalse");
            classText.AppendLine("\t\t}");*/
            classText.AppendLine(Environment.NewLine);
            classText.AppendLine("\t\tsaveButton.setOnClickListener {");
            classText.AppendLine("\t\t\tloadingProgressBar.visibility = View.VISIBLE");
            classText.AppendLine($"\t\t\t{table.Name.Decapitalise()}ViewModel.login(");
            classText.AppendLine("\t\t\t\tusernameEditText.editText?.text.toString(),");
            classText.AppendLine("\t\t\t\tpasswordEditText.editText?.text.toString()");
            classText.AppendLine("\t\t\t)");
            classText.AppendLine("\t\t\t}");
            classText.AppendLine("\t\t}");

            classText.AppendLine("\t\tprivate fun updateUiWithUser(model: LoggedInUserView) {");
            classText.AppendLine("\t\t\tval welcome = getString(R.string.welcome) + model.displayName");
            classText.AppendLine("\t\t\t// TODO : initiate successful logged in experience");
            classText.AppendLine("\t\t\tval appContext = context?.applicationContext ?: return");
            classText.AppendLine("\t\t\tToast.makeText(appContext, welcome, Toast.LENGTH_LONG).show()");
            classText.AppendLine("\t\t}");

            classText.AppendLine("\t\tprivate fun showLoginFailed(@StringRes errorString: Int) {");
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
    }
}
