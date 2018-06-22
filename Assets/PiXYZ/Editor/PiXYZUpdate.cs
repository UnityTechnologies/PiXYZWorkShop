using UnityEngine;
using System;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using PIXYZImportScript;
using PIXYZImportScript.AssemblyCSharp;
#endif

//[InitializeOnLoad]
public class PiXYZUpdate : EditorWindow
{
    private static string version = null;
    private static string link = null;
    private static bool updateNeeded = false;
    private static bool _automaticUpdate = false;
    private static string errorMessage = "";
    public static PiXYZImportMenu _pixyzImport = null;

    [MenuItem("PiXYZ/Check For Update", false, 52)]
    public static void Display()
    {
        checkForUpdate(false);
        createWindow();
    }

    public static void createWindow()
    {
        PiXYZUpdate window = (PiXYZUpdate)EditorWindow.GetWindow(typeof(PiXYZUpdate), true, "Check For Update");
        window.CenterOnMainWin();
        window.maxSize = new Vector2(window.position.width, window.position.height);
        window.minSize = new Vector2(window.position.width, window.position.height);

        window.ShowPopup();
        //window.coroutineScheduler.StartCoroutine(window.GetUpdatePageContent());
    }

    public static void checkForUpdate(bool automaticUpdate = true, PiXYZImportMenu pixyzImport = null)
    {
        try
        {
            _automaticUpdate = automaticUpdate;
            _pixyzImport = pixyzImport;
            updateNeeded = PiXYZ4UnityWrapper.checkForUpdate(out version, out link, automaticUpdate);
            if (updateNeeded)
                createWindow();
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
            return;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            {
                GUILayout.FlexibleSpace();

                if (updateNeeded)
                {
                    EditorGUILayout.LabelField("A new version is available : " + version, EditorStyles.wordWrappedLabel);
                    GUILayout.Space(20);
                    if (_automaticUpdate)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            EditorPrefs.SetBool("PiXYZ.AutoUpdate", !EditorGUILayout.Toggle("Do not show Again", !EditorPrefs.GetBool("PiXYZ.AutoUpdate")));
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();
                    }


                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Download"))
                    {
                        Application.OpenURL(link);
                        this.Close();
                    }
                    if (GUILayout.Button("Later"))
                    {
                        this.Close();
                    }
                    GUILayout.EndHorizontal();
                }
                else if (errorMessage == "")
                {
                    EditorGUILayout.LabelField("Your version is up to date", EditorStyles.wordWrappedLabel);
                    GUILayout.Space(20);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Close"))
                    {
                        this.Close();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField(errorMessage, EditorStyles.wordWrappedLabel);
                    GUILayout.Space(20);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Retry"))
                    {
                        errorMessage = "";
                        checkForUpdate();
                    }

                    if (GUILayout.Button("Close"))
                    {
                        this.Close();
                        errorMessage = "";
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();
    }
}