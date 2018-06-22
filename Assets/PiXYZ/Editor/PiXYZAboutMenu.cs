using UnityEngine;
using System;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using PIXYZImportScript;
using PIXYZImportScript.AssemblyCSharp;
#endif

public class PiXYZAboutMenu : EditorWindow
{
    private static string website = "https://www.pixyz-software.com";
    [MenuItem("PiXYZ/About PiXYZ...", false, 54)]
    public static void Display()
    {
        PiXYZAboutMenu window = (PiXYZAboutMenu)EditorWindow.GetWindow(typeof(PiXYZAboutMenu), true, "About PiXYZ PLUGIN for Unity");
        window.position = new Rect((Screen.currentResolution.width - window.position.width) / 2,
            (Screen.currentResolution.height - window.position.height) / 2,
            430.0f,
            600.0f);
        window.maxSize = new Vector2(window.position.width, window.position.height);
        window.minSize = new Vector2(window.position.width, window.position.height);

        window.Show();
        PiXYZ4UnityWrapper.initialize();
    }

    [MenuItem("PiXYZ/Get A Sample Model", false, 23)]
    public static void GetSample()
    {
        Application.OpenURL(website + "/download/");
    }

    [MenuItem("PiXYZ/Open Plugin Documentation", false, 22)]
    public static void OpenTutorialPDF()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Application.OpenURL(Application.dataPath+ "/PiXYZ/Resources/[DOC]-PiXYZ-PLUGIN-for-Unity_2018_1.pdf") ;
            string text = "A PDF documentation will be open because you are currently offline (no internet connection). If you wish to access an up-to-date online documentation, please connect here: https://pixyz-software.com/documentations/PiXYZ4Unity/ ";
            if (!EditorPrefs.GetBool("PiXYZ.DoNotShowAgainDocumentationPopup", false))
                EditorPrefs.SetBool("PiXYZ.DoNotShowAgainDocumentationPopup", EditorUtility.DisplayDialog("Internet not reachable", text, "Do not show again", "Ok"));
        }
        else
        {
            Application.OpenURL(PiXYZ4UnityWrapper.getProductDocumentationURL());
        }
    }

    public static void showLicenseInfos(bool center = true)
    {
        EditorGUILayout.BeginVertical();
        {
            if(center)
                GUILayout.FlexibleSpace();
            if (PiXYZ4UnityWrapper.checkLicense())
            {
                String[] names;
                String[] values;
                if (PiXYZ4UnityWrapper.isFloatingLicense())
                {
                    string server; int port;
                    PiXYZ4UnityWrapper.getLicenseServer(out server, out port);
                    names = new String[]{
                        "License",
                        "",
                        "Server address",
                        "Port"
                    };
                    values = new String[] {
                        "Floating",
                        "",
                        server,
                        port.ToString()
                    };
                }
                else
                {
                    names = new String[]{
                        "Start date",
                        "End date",
                        "Company name",
                        "Name",
                        "E-mail"
                    };
                    values = new String[] {
                        PiXYZ4UnityWrapper.getCurrentLicenseStartDate(),
                        PiXYZ4UnityWrapper.getCurrentLicenseEndDate().Length == 0 ? "Perpetual" : PiXYZ4UnityWrapper.getCurrentLicenseEndDate(),
                        PiXYZ4UnityWrapper.getCurrentLicenseCompany(),
                        PiXYZ4UnityWrapper.getCurrentLicenseName(),
                        PiXYZ4UnityWrapper.getCurrentLicenseEmail(),
                    };
                }
                GUIStyle bold = new GUIStyle(EditorStyles.boldLabel);
                GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.fontSize = 10;
                bold.alignment = TextAnchor.MiddleLeft;
                bold.fontSize = 10;
                PiXYZUtils.beginGroupBox("License informations");
                for (int i = 0; i < names.Length; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(names[i].Length > 0 ? names[i] + ": " : "", labelStyle, GUILayout.Width((int)(Screen.width * 0.28)));
                    EditorGUILayout.LabelField(values[i], bold);
                    EditorGUILayout.EndHorizontal();
                }
                PiXYZUtils.endGroupBox();
            }
            else
            {
                GUIStyle boldRed = new GUIStyle(EditorStyles.boldLabel);
                boldRed.alignment = TextAnchor.MiddleCenter;
                boldRed.fontSize = 18;
                boldRed.wordWrap = true;
                PiXYZUtils.beginGroupBox("");
                {
                    EditorGUILayout.LabelField("");
                    EditorGUILayout.LabelField("Your license is inexistant or invalid.", boldRed);
                    EditorGUILayout.LabelField("");
                }
                PiXYZUtils.endGroupBox();
            }
            if (center)
                GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndVertical();
    }

    void OnGUI()
    {

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Height(Screen.width - 40));
        EditorGUILayout.LabelField("");
        EditorGUILayout.EndVertical();
        Rect rectangle = GUILayoutUtility.GetLastRect();
        EditorGUILayout.EndHorizontal();
        {
            rectangle.y = (rectangle.height - rectangle.width) / 2;
            rectangle.height = rectangle.width;
            GUI.DrawTexture(rectangle, Resources.Load("Icon/pixyz_banner", typeof(Texture2D)) as Texture2D);
            //PiXYZUtils.drawGroupBox(rectangle, "test");
        }
        showLicenseInfos(false);
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();
        {
            GUIStyle centeredBold = new GUIStyle(EditorStyles.boldLabel);
            centeredBold.alignment = TextAnchor.UpperCenter;
            EditorGUILayout.LabelField("Plugin version: " + PiXYZ4UnityWrapper.getVersion(), centeredBold);

        }
        {
            GUIStyle boldRich = new GUIStyle(EditorStyles.boldLabel);
            boldRich.alignment = TextAnchor.MiddleCenter;
            boldRich.normal.textColor = Color.blue;
            //GUI.Label(new Rect(0, Screen.height * 3 / 4 - 10, Screen.width, Screen.height / 4), "Click to see Terms & Conditions", boldRich);
            string str = "Click to see Terms & Conditions";
            TextGenerationSettings settings = new TextGenerationSettings();
            settings.fontSize = boldRich.fontSize;
            settings.fontStyle = boldRich.fontStyle;
            settings.font = boldRich.font;
            settings.color = boldRich.normal.textColor;
            settings.pivot = Vector2.zero;
            if (GUILayout.Button(str, boldRich))
            {
                Application.OpenURL("https://www.pixyz-software.com/general-and-products-terms-and-conditions/");
            }
            TextGenerator a = new TextGenerator();
            Rect buttonRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
            Rect underlineRect = new Rect(buttonRect);
            underlineRect.width = a.GetPreferredWidth(str, settings);
            underlineRect.x = Screen.width / 2 - underlineRect.width / 2;
            underlineRect.y += underlineRect.height - 2;
            underlineRect.height = 1;
            PiXYZUtils.GUIDrawRect(underlineRect, Color.blue);

        }
        {
            GUIStyle italic = new GUIStyle();
            italic.fontStyle = FontStyle.Italic;
            italic.alignment = TextAnchor.MiddleCenter;
            italic.fontSize = 10;
            italic.wordWrap = true;
            EditorGUILayout.LabelField("PiXYZ Software solutions are edited by Metaverse Technologies France", italic);
        }
        EditorGUILayout.EndVertical();
    }
}
