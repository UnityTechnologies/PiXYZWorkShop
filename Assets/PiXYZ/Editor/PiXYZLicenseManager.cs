using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
using PIXYZImportScript.AssemblyCSharp;
#endif

public class PiXYZLicenseManager : EditorWindow
{
    string[] options = new string[] { };
    int index = 0;
    //PiXYZCredentialsPopup popup;
    string username = "";
    string password = "";
    int selectedTab = 0;
    bool connected = false;
    AnimBool m_Informations;
    AnimBool showCredentials;
    AnimBool showOnlineTab;
    
    [MenuItem("PiXYZ/License Manager", false, 51)]
    public static void Init()
    {
        PiXYZLicenseManager window = (PiXYZLicenseManager)EditorWindow.GetWindow(typeof(PiXYZLicenseManager), true, "PiXYZ License manager");
        window.position = new Rect(10000.0f, 0, 450.0f, 300.0f); //out of screen right
        window.maxSize = new Vector2(window.position.width, window.position.height);
        window.minSize = new Vector2(window.position.width, window.position.height);
        window.CenterOnMainWin();
        window.Show();
        try
        {
            PiXYZ4UnityWrapper.initialize();
        }
        catch (Exception) {}
    }

    void OnEnable()
    {
    }

    void OnFocus()
    {
    }

    void onDestroy()
    {
    }

    void Awake()
    {
        m_Informations = new AnimBool(false);
        m_Informations.valueChanged.AddListener(Repaint);
        showCredentials = new AnimBool(true);
        showCredentials.valueChanged.AddListener(Repaint);
        showOnlineTab = new AnimBool(false);
        showOnlineTab.valueChanged.AddListener(Repaint);
    }

    void OnDestroy()
    {
        try
        {
            PiXYZ4UnityWrapper.clear();
        }
        catch (Exception) { }
    }

    bool doRequest = false;
    bool doRelease = false;
    void OnGUI()
    {
        string[] titles = { "Current license", "Online", "Offline", "License server", "Tokens" };
        selectedTab = PiXYZUtils.Tabs(titles, selectedTab);
        
        switch (selectedTab)
        {
            case 0: //current
                {
                    currentLicenseTab();
                    break;
                }
            case 1: //online
                {
                    showCredentials.target = !connected;
                    showOnlineTab.target = connected;
                    if (!connected)
                    {
                        if (EditorGUILayout.BeginFadeGroup(showCredentials.faded))
                        {
                            connected = creds();
                            EditorGUILayout.EndFadeGroup();
                        }
                    }
                    else
                    {
                        if (EditorGUILayout.BeginFadeGroup(showOnlineTab.faded))
                        {
                            onlineTab();
                            EditorGUILayout.EndFadeGroup();
                        }
                    }
                    break;
                }
            case 2: //offline
                {
                    offlineTab();
                    break;
                }
            case 3: //server
                {
                    licenseServer();
                    break;
                }
            case 4: //tokens
                {
                    tokens();
                    break;
                }
        }
        //Outter calls
        if(doRelease)
        {
            doRelease = false;
            if (PiXYZ4UnityWrapper.releaseLicense(username, password, index))
            {
                EditorUtility.DisplayDialog("Release complete", "The license release has been completed.", "Ok");
            }
            else
            {
                EditorUtility.DisplayDialog("Release failed", "An error has occured while releasing the license: " + PiXYZ4UnityWrapper.getLastError(), "Ok");
                PiXYZ4UnityWrapper.requestLicense(username, password, index);
            }
        }
        else if(doRequest)
        {
            doRequest = false;
            if (PiXYZ4UnityWrapper.requestLicense(username, password, index))
                EditorUtility.DisplayDialog("Installation complete", "The license installation has been completed.", "Ok");
            else
                EditorUtility.DisplayDialog("Installation failed", "An error occured while installing the license: " + PiXYZ4UnityWrapper.getLastError(), "Ok");
        }
    }

    void currentLicenseTab()
    {
        PiXYZAboutMenu.showLicenseInfos();
    }

    static bool waitEvent = true;
    static int lastIndex = -1;
    static int lastOptionLength = -1;
    void onlineTab()
    {
        if (lastIndex == -1)
            waitEvent = true;
        if (lastOptionLength != options.Length && Event.current.type != EventType.Layout)
            return;
        else if (lastOptionLength != options.Length && Event.current.type == EventType.Layout)
            lastOptionLength = options.Length;
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Select your license", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        {
            EditorGUI.BeginDisabledGroup(options.Length < 1);
            {
                if (Event.current.type == EventType.Layout) //Updates on layout event
                    lastIndex = index;
                EditorStyles.popup.richText = true;
                index = EditorGUILayout.Popup(index, options);
                EditorStyles.popup.richText = false;
                if (lastIndex != index)
                    waitEvent = true;
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(10);
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);
            if(GUILayout.Button("Refresh", buttonStyle, GUILayout.MaxWidth(Screen.width * 0.2f)))
            {
                PiXYZ4UnityWrapper.connectToLicenseServer(username, password);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        bool installed = false;
        if (index < options.Length)
        {
            string productName, validity, licenseUse, currentlyInstalled;
            int usedIndex = index;
            if(waitEvent)
            {
                if (Event.current.type != EventType.Layout)
                    usedIndex = lastIndex;
                else
                    waitEvent = false;
            }
            int daysRemaining = Math.Max(0, (Convert.ToDateTime(PiXYZ4UnityWrapper.licenseValidity(usedIndex)) - DateTime.Now).Days + 1);
            string remainingTextColor = daysRemaining > 185 ? "green" : daysRemaining > 92 ? "orange" : "red";
            installed = PiXYZ4UnityWrapper.licenseOnMachine(usedIndex);
            productName = PiXYZ4UnityWrapper.licenseProduct(usedIndex);
            validity = PiXYZ4UnityWrapper.licenseValidity(usedIndex)
                + "   (<color='" + remainingTextColor + "'><b>" + daysRemaining + "</b> Day" + (daysRemaining > 1 ? "s" : "") + " remaining</color>)";
            licenseUse = "" + PiXYZ4UnityWrapper.licenseInUse(usedIndex) + " / " + PiXYZ4UnityWrapper.licenseCount(usedIndex);
            currentlyInstalled = installed ? "<color='green'>true</color>" : "false";

            GUIStyle italic = new GUIStyle(GUI.skin.label);
            italic.fontStyle = FontStyle.Italic;
            EditorGUI.indentLevel = 1;
            EditorGUILayout.LabelField("License informations", italic);
            EditorGUI.indentLevel = 2;
            EditorStyles.label.richText = true;
            EditorGUILayout.LabelField("Product name: ", productName);
            EditorGUILayout.LabelField("Validity: ", validity);
            EditorGUILayout.LabelField("License use: ", licenseUse);
            EditorGUILayout.LabelField("Currently installed: ", currentlyInstalled);
            GUI.skin.label.richText = false;
            EditorGUI.indentLevel = 0;
        }
        else if (options.Length == 0)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = Color.red;
            GUILayout.BeginVertical();
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("No license available in your account.", labelStyle);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Space(30);
            }
            GUILayout.EndVertical();
        }

        EditorGUI.BeginDisabledGroup(index >= options.Length);
        {
            GUIStyle btnContainerStyle = new GUIStyle();
            btnContainerStyle.margin.right = 5;
            GUILayout.BeginArea(new Rect(position.width * 0.05f, position.height - 30, position.width * 0.90f, 30), btnContainerStyle);
            {
                GUILayout.BeginHorizontal();
                string installName = installed ? "Reinstall" : "Install";
                if (GUILayout.Button(installName))
                {
                    //Unity don't like calls to precompiled function while in layout
                    // => delayed call to outside of layouts
                    doRequest = true;
                }
                if (installed)
                {
                    GUILayout.Space(40);
                    if (GUILayout.Button("Release"))
                    {
                        if (EditorUtility.DisplayDialog("Warning", "Release (or uninstall) current license lets you install it on another computer. This action is available only once.\n\nAre you sure you want to release this license ?", "Yes", "No"))
                        {
                            //Unity don't like calls to precompiled function while in layout
                            // => delayed call to outside of layouts
                            doRelease = true;
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }
        EditorGUI.EndDisabledGroup();

        if (username.Length > 0 && password.Length > 0)
        {
            options = new string[PiXYZ4UnityWrapper.availableLicensesCount()];
            for (int i = 0; i < PiXYZ4UnityWrapper.availableLicensesCount(); ++i)
            {
                options[i] = "License " + (i + 1) + ": " + PiXYZ4UnityWrapper.licenseProduct(i) + "  [" +  PiXYZ4UnityWrapper.licenseValidity(i) + "]";
                if (PiXYZ4UnityWrapper.licenseOnMachine(i))
                    options[i] += "  (installed)";
            }
        }
    }

    void offlineTab()
    {
        float spacing = position.height * 0.15f;
        GUIStyle sheetStyle = new GUIStyle();
        sheetStyle.margin.right = 5;
        GUILayout.BeginArea(new Rect(position.width * 0.1f, position.height * 0.20f, position.width * 0.80f, position.height * 0.70f), sheetStyle);
        GUILayout.BeginVertical();
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.margin.right = 5;
        GUILayout.FlexibleSpace();
        GUILayout.Label("Generate an activation code and upload it on PiXYZ website");
        if (GUILayout.Button("Generate activation code", buttonStyle))
        {
            var path = EditorUtility.SaveFilePanel(
                 "Save activation code",
                 "",
                 "PiXYZ_activationCode.bin",
                 "Binary file;*.bin");

            if (path.Length != 0)
            {
                if (PiXYZ4UnityWrapper.generateActivationCode(path) == 0)
                    EditorUtility.DisplayDialog("Generation succeed", "The activation code has been successfully generated.", "Ok");
                else
                    EditorUtility.DisplayDialog("Generation failed", "An error occured while generating the file: " + PiXYZ4UnityWrapper.getLastError(), "Ok");
            }

        }
        GUILayout.Space(spacing);
        GUILayout.Label("Install a new license");
        if (GUILayout.Button("Install license", buttonStyle))
        {
            var path = EditorUtility.OpenFilePanel(
                    "Open installation code (*.bin) or license file (*.lic)",
                    "",
                    "Install file;*.bin;*.lic");
            if (path.Length != 0)
            {
                if (path.ToLower().EndsWith(".bin") || path.ToLower().EndsWith(".lic"))
                {
                    if (PiXYZ4UnityWrapper.installActivationCode(path) == 0)
                        EditorUtility.DisplayDialog("Installation succeed", "The installation code has been installed.", "Ok");
                    else
                        EditorUtility.DisplayDialog("Installation failed", "An error occured while installing: " + PiXYZ4UnityWrapper.getLastError(), "Ok");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "The file must be an installation code (bin file) or a license file (lic file)", "Ok");
                }
            }
        }
        GUILayout.Space(spacing);
        GUILayout.Label("Generate a release code and upload it on PiXYZ website");
        if (GUILayout.Button("Generate release code", buttonStyle))
        {
            if (EditorUtility.DisplayDialog("Warning", "Release (or uninstall) current license lets you install it on another computer. This action is available only once.\n\nAre you sure you want to release this license ?", "Yes", "No"))
            {
                var path = EditorUtility.SaveFilePanel(
                 "Save release code as BIN",
                 "",
                 "PiXYZ_releaseCode.bin",
                 "Binary file;*.bin");

                if (path.Length != 0)
                {
                    if (PiXYZ4UnityWrapper.generateDeactivationCode(path) == 0)
                        EditorUtility.DisplayDialog("Generation succeed", "The release code has been successfully generated.", "Ok");
                    else
                        EditorUtility.DisplayDialog("Generation failed", "An error occured while generating the file: " + PiXYZ4UnityWrapper.getLastError(), "Ok");
                }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    static string address = "";
    static int port = 0;
    void licenseServer()
    {
        float spacing = position.height * 0.15f;
        string savedAddress = "";
        int savedPort = 0;
        GUIStyle sheetStyle = new GUIStyle();
        sheetStyle.margin.right = 5;
        PiXYZ4UnityWrapper.getLicenseServer(out savedAddress, out savedPort);
        if (address == "") address = savedAddress;
        if (port == 0) port = savedPort;
        GUILayout.BeginArea(new Rect(position.width * 0.1f, position.height * 0.20f, position.width * 0.80f, position.height * 0.70f), sheetStyle);
        {
            GUILayout.BeginVertical();
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.margin.right = 5;
                GUILayout.FlexibleSpace();
                GUILayout.Label("Address");
                address = GUILayout.TextField(address);
                GUILayout.Space(spacing);
                GUILayout.Label("Port");
                var newPort = GUILayout.TextField(port.ToString());
                int temp;
                if (int.TryParse(newPort, out temp))
                {
                    port = Math.Max(0, temp);
                }
                GUILayout.FlexibleSpace();

                GUI.enabled = (address != savedAddress) || (port != savedPort);
                if (GUILayout.Button("Apply", buttonStyle))
                {
                    if (PiXYZ4UnityWrapper.configureLicenseServer(address, port))
                        EditorUtility.DisplayDialog("Success", "License server has been successfuly configured", "Ok");
                    else
                        EditorUtility.DisplayDialog("License server error", PiXYZ4UnityWrapper.getLastError(), "Ok");
                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndArea();
    }

    static Vector2 _scrollViewPosition = new Vector2(0, 0);
    static bool allSelected = false;
    static Dictionary<int, bool> validToken = new Dictionary<int, bool>();
    void tokens()
    {
        bool newAllSelected = GUILayout.Toggle(allSelected, "Select all");
        bool selectAll = newAllSelected && !allSelected;
        bool deselectAll = !newAllSelected && allSelected;
        allSelected = true;
        _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition, GUILayout.MaxHeight(Screen.height - 30));
        {
            int[] tokens = PiXYZ4UnityWrapper.getTokens();
            foreach(int token in tokens)
            {
                if (selectAll)
                    PiXYZ4UnityWrapper.addWantedToken(token);
                else if (deselectAll && !PiXYZ4UnityWrapper.isMandatoryToken(token))
                    PiXYZ4UnityWrapper.removeWantedToken(token);

                bool required = PiXYZ4UnityWrapper.isTokenRequired(token);
                if (required)
                {
                    var oldColor = GUI.backgroundColor;
                    bool valid = false;
                    if (!validToken.ContainsKey(token))
                        validToken[token] = PiXYZ4UnityWrapper.isTokenValid(token);
                    valid = validToken[token];
                    GUI.backgroundColor = valid ? Color.green : Color.red;

                    if (PiXYZ4UnityWrapper.isMandatoryToken(token))
                    {
                        GUILayout.Toggle(true, PiXYZ4UnityWrapper.getTokenName(token), "Button");
                    }
                    else if (!GUILayout.Toggle(true, PiXYZ4UnityWrapper.getTokenName(token), "Button"))
                    {
                        PiXYZ4UnityWrapper.removeWantedToken(token);
                        allSelected = false;
                    }

                    GUI.backgroundColor = oldColor;
                }
                else
                {
                    validToken.Remove(token);
                    allSelected = false;
                    PiXYZ4UnityWrapper.releaseToken(token);
                    if (GUILayout.Toggle(false, PiXYZ4UnityWrapper.getTokenName(token), "Button"))
                        PiXYZ4UnityWrapper.addWantedToken(token);
                }
            }
            /*GUILayout.Toggle(true, "Unity", "Button");
            for(int i = 0; i < 15; ++i)
            {
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = tokensSelected[i] ? (i % 7 == 0 ? Color.red : Color.green) : oldColor;               

                GUIStyle style = new GUIStyle("Button");
                tokensSelected[i] = GUILayout.Toggle(tokensSelected[i], "Token " + i, "Button");
                GUI.backgroundColor = oldColor;
            }*/
        }
        GUILayout.EndScrollView();
    }

    bool creds()
    {
        GUILayout.BeginArea(new Rect(position.width * 0.05f, position.height * 0.36f, position.width * 0.90f, position.height * 0.24f));
        GUILayout.BeginHorizontal();
        GUILayout.Label("Username: ");
        username = EditorGUILayout.TextField("", username, GUILayout.MaxWidth(position.width / 2));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Password: ");
        password = EditorGUILayout.PasswordField("", password, GUILayout.MaxWidth(position.width / 2));
        GUILayout.EndHorizontal();
        EditorGUI.BeginDisabledGroup(username.Length == 0 || password.Length == 0);
        if (GUILayout.Button("Connect"))
        {
            bool connected = username.Length != 0 && password.Length != 0 && PiXYZ4UnityWrapper.connectToLicenseServer(username, password);
            //PiXYZ4UnityWrapper.availableLicensesCount();
            if (connected)
            {
                return true;
            }
            else
            {
                m_Informations.target = true;
            }
        }
        EditorGUI.EndDisabledGroup();
        if (EditorGUILayout.BeginFadeGroup(m_Informations.faded))
        {
            EditorGUI.indentLevel++;
            GUIStyle a = new GUIStyle();
            a.normal.textColor = Color.red;
            GUILayout.Label("Credentials error", a);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFadeGroup();
        }
        GUILayout.EndArea();
        return false;
    }

    void setUsername(string uname)
    {
        username = uname;
    }

    void setPassword(string passwd)
    {
        password = passwd;
    }
    
}