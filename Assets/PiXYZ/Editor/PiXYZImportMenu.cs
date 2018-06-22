using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Callbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
#if UNITY_EDITOR
using PIXYZImportScript;
using PIXYZImportScript.AssemblyCSharp;
#endif

[ExecuteInEditMode]
public class PiXYZImportMenu : EditorWindow
{
    public class ImportSettings : ScriptableObject
    {
        public PiXYZSettingsEditor settings = new PiXYZSettingsEditor();
        public int windowId;

        void OnEnable()
        {
            settings.getEditorPref();
            windowId = 0;
        }
    }
    //[SerializeField]
    //public PiXYZSettings settings;
    public ImportSettings importSettings;

    public SerializedObject serializedObject;
    PiXYZUtils utils;
    public static string pluginName;
    public static string pluginPath;

    public string selectedFile = "";
    public bool isFileNameValid;

    public static bool importing = false;
    static CoroutineScheduler coroutineScheduler = null;

    static PiXYZ4UnityLoader loader = null;
    static int instanceCount = 0;
    static Texture2D saveIconNormal;
    static Texture2D saveIconHover;
    static Texture2D saveIconActive;
    static Texture2D resetIconNormal;
    static Texture2D resetIconHover;
    static Texture2D resetIconActive;
    static Texture2D resetFactoryIconNormal;
    static Texture2D resetFactoryIconHover;
    static Texture2D resetFactoryIconActive;

    void OnEnable()
    {
        instanceCount++;

        saveIconNormal = Resources.Load("icon/save_32_Roll") as Texture2D;
        saveIconHover = Resources.Load("icon/save_32_Roll1") as Texture2D;
        saveIconActive = Resources.Load("icon/save_32_Roll2") as Texture2D;
        resetIconNormal = Resources.Load("icon/reset_32_Roll") as Texture2D;
        resetIconHover = Resources.Load("icon/reset_32_Roll1") as Texture2D;
        resetIconActive = Resources.Load("icon/reset_32_Roll2") as Texture2D;
        resetFactoryIconNormal = Resources.Load("icon/resetUsine_32_Roll") as Texture2D;
        resetFactoryIconHover = Resources.Load("icon/resetUsine_32_Roll1") as Texture2D;
        resetFactoryIconActive = Resources.Load("icon/resetUsine_32_Roll2") as Texture2D;

        EditorApplication.update += UpdateCoroutine;
        importSettings = ScriptableObject.CreateInstance<ImportSettings>();// new ImportSettings();
        coroutineScheduler = coroutineScheduler == null ? ScriptableObject.CreateInstance<CoroutineScheduler>() : coroutineScheduler;
        importSettings.windowId = GetInstanceID();

        serializedObject = new SerializedObject(importSettings);
        utils = new PiXYZUtils();
        //selectedFile = "";
        //isFileNameValid = false;

        pluginName = "PiXYZ4Unity.dll";
        pluginPath = Path.Combine(Application.dataPath, "PiXYZ/Plugins");

#if UNITY_EDITOR_WIN    //On windows, copy .dll to project root
        try
        {
            string[] lPluginFiles = Directory.GetFiles(pluginPath);

            foreach (string lFile in lPluginFiles)
            {
                if (Path.GetExtension(lFile) == ".dll" && Path.GetFileName(lFile) != "PiXYZ4Unity.dll")
                {
                    string lExportFile = Path.Combine(Application.dataPath + "/..", Path.GetFileName(lFile));
                    if (!File.Exists(lExportFile))
                        File.Copy(lFile, lExportFile);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
#endif
    }

    public void OnDestroy()
    {
        instanceCount--;
    }

    [MenuItem("PiXYZ/Add Runtime CAD Import Sample", false, 21)]
    public static void AddRuntimeImportPrefab()
    {
        //Display success PopUp
        EditorUtility.DisplayDialog("PiXYZ Runtime Sample", "This will instanciate PiXYZ_Runtime_Import_Boiler_Plate prefab into your scene", "ok");
        Instantiate(Resources.Load("PiXYZ_Runtime_Import_Boiler_Plate", typeof(GameObject)));
    }


    [MenuItem("PiXYZ/Import CAD", true, 1)]
    static bool IsDisplayable()
    {
        return instanceCount == 0 && !importing;
    }

    [MenuItem("PiXYZ/Import CAD", false, 1)]
    public static void Display()
    {
        // Get existing open window or if none, make a new one:
        EditorWindow window = (PiXYZImportMenu)EditorWindow.GetWindow(typeof(PiXYZImportMenu), true, "PiXYZ CAD Import Settings");
        window.minSize = new Vector2(430.0f, 100.0f);
        window.maxSize = new Vector2(430.0f, 110);

        window.CenterOnMainWin();

        window.Show();
    }

    static int ticks = 0;
    static void UpdateCoroutine()
    {
        ++ticks;
        if (ticks > 20)
        {
            ticks = 0;
            if (loader != null)
            {
                if (coroutineScheduler.HasCoroutines())
                {
                    if (Time.frameCount == lastFrame)
                        ++frameSkip;
                    else
                        lastFrame = Time.frameCount;
                    coroutineScheduler.UpdateAllCoroutines((Time.frameCount + frameSkip), Time.time);
                    if (loader != null)
                        PiXYZUtils.displayProgressBar("PiXYZ Import", loader.progressStatus, loader.progress);

                    return;
                }
            }
        }
    }

    static int lastFrame = 0;
    static int frameSkip = 0;
    void OnGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.MaxWidth(Screen.width));
        {
            GUILayout.Space(5);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            {
                EditorGUI.BeginDisabledGroup(coroutineScheduler.HasCoroutines());
                {
                    GUILayout.Space(20);
                    PiXYZUtils.beginGroupBox("Importing File");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.TextField("File Name", Path.GetFileName(selectedFile));
                            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
                            btnStyle.margin.top = 0;
                            if (GUILayout.Button(new GUIContent("... ", "Open browser and choose the import to import"), btnStyle, GUILayout.Width(25)))
                                OnFileSelectionClicked();
                            GUILayout.Space(40);
                        }
                        EditorGUILayout.EndHorizontal();

                    }
                    PiXYZUtils.endGroupBox();
                    GUI.enabled = isFileNameValid && !coroutineScheduler.HasCoroutines();
                    if (isFileNameValid)
                    {
                        string ext = Path.GetExtension(selectedFile);
                        utils.GUISettings(serializedObject, ext);
                        if (maxSize.y == 110.0f ||
                                          (maxSize.y > 300.0f && PiXYZUtils.isPiXYZExt(ext)) ||
                                          (maxSize.y < 300.0f && !PiXYZUtils.isPiXYZExt(ext)))
                        {
                            minSize = new Vector2(430.0f, !PiXYZUtils.isPiXYZExt(ext) ? 660.0f : 240.0f);
                            maxSize = new Vector2(430.0f, !PiXYZUtils.isPiXYZExt(ext) ? 700.0f : 260.0f);
                            this.CenterOnMainWin();
                        }

                    }
                    else if (!isFileNameValid && maxSize.y > 110.0f)
                    {
                        minSize = new Vector2(430.0f, 100.0f);
                        maxSize = new Vector2(430.0f, 110.0f);
                        this.CenterOnMainWin();
                    }
                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginHorizontal(GUILayout.Width(Screen.width / 2));
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Import", GUILayout.Width(80)))
                                OnImportClicked();
                            GUI.enabled = true;
                            GUILayout.Space(15);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(15);
                            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
                                OnCancelClicked();
                            GUILayout.FlexibleSpace();
                            if (isFileNameValid && !PiXYZUtils.isPiXYZExt(selectedFile))
                            {
                                GUIStyle bs = new GUIStyle(GUI.skin.button);
                                bs.normal.background = saveIconNormal;// Resources.Load("icon/save_32_Roll") as Texture2D;
                                bs.hover.background = saveIconHover;// Resources.Load("icon/save_32_Roll1") as Texture2D;
                                bs.active.background = saveIconActive;// Resources.Load("icon/Save_32_Roll2") as Texture2D;
                                bs.border = new RectOffset(0, 0, 0, 0);
                                bs.margin = new RectOffset(0, 0, 0, 0);
                                bs.overflow = new RectOffset(0, 0, 0, 0);
                                if (GUILayout.Button("", bs, GUILayout.Width(24), GUILayout.Height(24)))
                                {
                                    PiXYZUtils.saveEditorPref(serializedObject);
                                }
                                bs.normal.background = resetIconNormal;// Resources.Load("icon/Reset_32_Roll") as Texture2D;
                                bs.hover.background = resetIconHover;// Resources.Load("icon/Reset_32_Roll1") as Texture2D;
                                bs.active.background = resetIconActive;// Resources.Load("icon/Reset_32_Roll2") as Texture2D;
                                if (GUILayout.Button("", bs, GUILayout.Width(24), GUILayout.Height(24)))
                                {
                                    importSettings = ScriptableObject.CreateInstance<ImportSettings>();
                                    serializedObject = new SerializedObject(importSettings);
                                    importSettings.windowId = GetInstanceID();
                                    PiXYZLods.reset = true;
                                }
                                bs.normal.background = resetFactoryIconNormal;// Resources.Load("icon/ResetUsine_32_Roll") as Texture2D;
                                bs.hover.background = resetFactoryIconHover;// Resources.Load("icon/ResetUsine_32_Roll1") as Texture2D;
                                bs.active.background = resetFactoryIconActive;// Resources.Load("icon/ResetUsine_32_Roll2") as Texture2D;
                                if (GUILayout.Button("", bs, GUILayout.Width(24), GUILayout.Height(24)))
                                {
                                    importSettings.settings.factoryReset();

                                    serializedObject = new SerializedObject(importSettings);
                                    importSettings.windowId = GetInstanceID();
                                    PiXYZLods.reset = true;
                                }
                                GUILayout.Space(10);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(10);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndHorizontal();
    }

    public void OnFileSelectionClicked()
    {
        string[] extensions = { "All PiXYZ files","pxz,fbx,igs,iges,stp,step,stpz,stepz,ifc,u3d,CATProduct,CATPart,cgr,CATShape,model,session,sldasm,sldprt,prt,asm*,prt*,neu,neu*,xas,xas*,xpr,xpr*,asm,par,pwd,psm,ipt,iam,ipj,sat,sab,"/* ptx,xyz,*/ + " vda,3dm,3dxml,wrl,vrml,dae,stl,"/* e57,pts,*/ + " jt,x_t,x_b,p_t,p_b,xmt,xmt_txt,xmt_bin,plmxml,obj,csb,wire,skp,pdf,prc,3ds,dwg,dxf",
                             "FBX files", "fbx",
                             "IGES files", "igs,iges",
                             "STEP files", "stp,step,stepz",
                             "IFC files", "ifc",
                             "U3D files", "u3d",
                             "CATIA files", "CATProduct,CATPart,cgr,CATShape",
                             "SolidWorks files", "sldasm,sldprt",
                             "Creo files", "prt,asm*,prt*,neu,neu*,xas,xas*,xpr,xpr*",
                             "SolidEdge", "asm,par,pwd,psm",
                             "ACIS SAT files", "sat,sab",
                             "VDA-FS files", "vda",
                             "Rhino files", "3dm",
                             "3dxml files", "3dxml",
                             "VRML files", "wrl,vrml",
                             "COLLADA files", "dae",
                             "Stereolithography files", "stl",
                             "JT files", "jt",
                             "Parasolid files", "x_t,x_b,p_t,p_b,xmt,xmt_txt,xmt_bin",
                             "PLMXML files", "plmxml",
                             "OBJ files", "obj",
                             "CSB files", "csb",
                             "Alias files", "wire",
                             "Sketchup files", "skp",
                             "Pdf files", "pdf",
                             "Prc files", "prc",
                             "3DS files", "3ds",
                             "AutoCAD files", "dwg,dxf"
        };

        try
        {
            PiXYZ4UnityWrapper.initialize();
        }
        catch (Exception)
        {
            if (EditorUtility.DisplayDialog("Invalid license", "Your license is inexistant or invalid.", "Open license manager", "Close"))
            {
                PiXYZLicenseManager.Init();
            }
            return;
        }

        string file = EditorUtility.OpenFilePanelWithFilters("Select File", "", extensions);

        if (file.Length != 0)
        {
            isFileNameValid = true;
            selectedFile = file;
        }
        else if (selectedFile.Length == 0)
        {
            isFileNameValid = false;
        }
        if (EditorPrefs.GetBool("PiXYZ.AutoUpdate", true))
            PiXYZUpdate.checkForUpdate(pixyzImport: this);
    }

    public void OnImportClicked()
    {
        //Checks if Folder Assets/3DModels exist, otherwise create it
        if (!AssetDatabase.IsValidFolder("Assets/3DModels"))
        {
            AssetDatabase.CreateFolder("Assets", "3DModels");
        }

        //Import CAD Model in Assets/PiXYZ/3DModels
        this.Close();
        importing = true;
        loader = ScriptableObject.CreateInstance<PiXYZ4UnityLoader>();
        coroutineScheduler.StartCoroutine(ImportModel(OnImportFinished));
    }

    public void OnImportFinished()
    {
        PiXYZUtils.clearProgressBar();
        //Display success PopUp
        if (loader.getErrorMessage().Length > 0)
            EditorUtility.DisplayDialog("PiXYZImporter", "File import failed.\n\nReasons : " + loader.getErrorMessage(), "ok");
        else
            EditorUtility.DisplayDialog("PiXYZImporter", "File successfully imported in Scene.\nPrefab created in Assets/3DModels.", "ok");
        loader = null;
        frameSkip = 0;
        //window.Close();
        DestroyImmediate(this);
        importing = false;
        isFileNameValid = false;
    }

    public void OnCancelClicked()
    {
        if (coroutineScheduler != null)
            coroutineScheduler.StopAllCoroutines();
        if (loader != null)
            PiXYZUtils.clearProgressBar();
        this.Close();
    }

    public IEnumerator ImportModel(Action callback)
    {
        GameObject gameObject = null;
        UnityEngine.Object prefab;
        importSettings.settings.prefabName = Path.GetFileNameWithoutExtension(selectedFile) + "_" + Path.GetRandomFileName();
        prefab = PrefabUtility.CreateEmptyPrefab("Assets/3DModels/" + importSettings.settings.prefabName + ".prefab");

        var method = loader.GetType().GetMethod("setSourceCoordinatesSystem",
            new Type[] { importSettings.settings.isRightHanded.GetType(), importSettings.settings.isZUp.GetType(), importSettings.settings.scaleFactor.GetType() }
        );
        method.Invoke(loader, new object[] { importSettings.settings.isRightHanded, importSettings.settings.isZUp, importSettings.settings.scaleFactor });
        double mapUV3dSize = importSettings.settings.mapUV ? importSettings.settings.mapUV3dSize : -1;
        loader.configure(importSettings.settings.orient, mapUV3dSize, importSettings.settings.treeProcess, importSettings.settings.useLods ? importSettings.settings.lodsMode : LODsMode.NONE, importSettings.settings.lodSettings, !importSettings.settings.splitTo16BytesIndex, importSettings.settings.useMergeFinalAssemblies);

        CoroutineNode coco = coroutineScheduler.StartCoroutine(loader.loadFileRuntime(gameObject, selectedFile, true, prefab));
        yield return coco;
        while (!coco.finished) ;

        if ((loader.getErrorMessage().Length > 0))
        {
            Debug.Log("Failure");
            if (callback != null) callback();
            yield break;
        }
        else
            Debug.Log("Success");

#if UNITY_EDITOR
        foreach (Material material in loader.materials.Values)
        {
            loader.loadedObject.Add(material);
            String[] textTypes = { "_MainTex", "_BumpMap", "_Cube", "_LightMap" };
            foreach (string textType in textTypes)
                if (material.HasProperty(textType) && material.GetTexture(textType))
                    loader.loadedObject.Add(material.GetTexture(textType));
        }
        foreach (UnityEngine.Object obj in loader.loadedObject)
        {
            AssetDatabase.AddObjectToAsset(obj, prefab);
        }
#endif
        GameObject importedNode;
        if ((importSettings.settings.treeProcess == TreeProcessType.MERGE || importSettings.settings.treeProcess == TreeProcessType.MERGE_BY_MATERIAL) && !importSettings.settings.useLods)
        {
            importedNode = loader.lastImportedObject.transform.GetChild(0).gameObject;
            loader.lastImportedObject.transform.GetChild(0).transform.SetParent(null);
            importedNode.name = loader.lastImportedObject.name;
            DestroyImmediate(loader.lastImportedObject);
        }
        else
            importedNode = loader.lastImportedObject;
        importSettings.settings.originalFilename = selectedFile;

        importedNode.AddComponent<PiXYZImportSettings>();
        importedNode.GetComponent<PiXYZImportSettings>().settings = importSettings.settings;   //Copy import settings to object inspector
        importedNode.GetComponent<PiXYZImportSettings>().windowId = this.GetInstanceID();
        importedNode.transform.SetParent(null);
        PrefabUtility.ReplacePrefab(importedNode, prefab, ReplacePrefabOptions.ConnectToPrefab);
        if (callback != null) callback();
    }

    //[PreferenceItem("PiXYZ")]
    //static void PreferencesGUI()
    //{
    //    ImportSettings import = ScriptableObject.CreateInstance<ImportSettings>();// new ImportSettings();

    //    if (!EditorWindow.mouseOverWindow || EditorWindow.focusedWindow.GetInstanceID() == EditorWindow.mouseOverWindow.GetInstanceID())
    //        import.windowId = EditorWindow.focusedWindow.GetInstanceID();

    //SerializedObject serializedObject = new SerializedObject(import);

    //PiXYZUtils.GUISettings(serializedObject);

    // Save the preferences
    //if (GUI.changed)
    //{
    //    PiXYZUtils.saveEditorPref(serializedObject);
    //}
    //}

#if UNITY_2017
    class PreProcessor : IPreprocessBuild
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            PlayerSettings.SetApiCompatibilityLevel(BuildPipeline.GetBuildTargetGroup(target), ApiCompatibilityLevel.NET_2_0);
        }
    }
#elif UNITY_2018_1_OR_NEWER
    class PreProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            PlayerSettings.SetApiCompatibilityLevel(report.summary.platformGroup, ApiCompatibilityLevel.NET_2_0);
        }
    }
    
#endif

    //Called on build to copy .dll to built folder
    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        string buildName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
        string lBuildPath = Path.GetDirectoryName(pathToBuiltProject);
        if (pluginPath == null)
            pluginPath = Path.Combine(Application.dataPath, "PiXYZ/Plugins");
        if (target == BuildTarget.StandaloneWindows64)
        {
            string[] lPluginFiles = Directory.GetFiles(pluginPath);

            foreach (string lFile in lPluginFiles)
            {
                if (Path.GetExtension(lFile) == ".dll" &&
                    Path.GetFileName(lFile) != "PiXYZImportScrpts.dll" &&
                    Path.GetFileName(lFile) != "PiXYZ4Unity.dll")
                {
                    string lExportFile = Path.Combine(lBuildPath, Path.GetFileName(lFile));
                    if (File.Exists(lExportFile))
                        File.Delete(lExportFile);
                    File.Copy(lFile, lExportFile);
                }
            }

            // For runtime license installation
            File.Copy(pluginPath + "/PiXYZFinishInstall.exe", lBuildPath + "/" + buildName + "_Data/Plugins/PiXYZFinishInstall.exe");
            File.Copy(pluginPath + "/PiXYZ4Unity_LicenseRequest.exe", lBuildPath + "/" + buildName + "_Data/Plugins/PiXYZ4Unity_LicenseRequest.exe");
            File.Copy(pluginPath + "/PiXYZInstallLicense.exe", lBuildPath + "/" + buildName + "_Data/Plugins/PiXYZInstallLicense.exe");
        }
        if (pluginPath != null)
            pluginPath = Path.Combine(pluginPath, "3DH3P");
        if (target == BuildTarget.StandaloneWindows64)
        {
            string[] lPluginFiles = Directory.GetFiles(pluginPath);
            Directory.CreateDirectory(Path.Combine(lBuildPath, buildName + "_Data/Plugins/3DH3P"));

            foreach (string lFile in lPluginFiles)
            {
                if (Path.GetExtension(lFile) == ".dll")
                {
                    //string lExportFile = Path.Combine(lBuildPath, Path.GetFileName(lFile));
                    string lExportFile = Path.Combine(lBuildPath, buildName + "_Data/Plugins/3DH3P/" + Path.GetFileName(lFile));

                    if (File.Exists(lExportFile))
                        File.Delete(lExportFile);
                    File.Copy(lFile, lExportFile);
                }
            }
        }
    }
}

//Import Model when drag and drop
public class PostProcessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string asset in importedAssets)
        {
            if (IsSupportedFile(asset))
            {
                PiXYZImportMenu import = (PiXYZImportMenu)EditorWindow.GetWindow(typeof(PiXYZImportMenu), true, "PiXYZ CAD import settings");
                import.position = new Rect(1000.0f, 400.0f, 430.0f, 480.0f);

                import.selectedFile = asset;
                import.isFileNameValid = true;

                import.Show();
            }
        }
        //AssetDatabase.SaveAssets ();
        //AssetDatabase.Refresh();
    }

    public static bool IsSupportedFile(string asset)
    {
        string[] supportedExtensions = new string[] { "*.pxz", /*"*.fbx*,*/ "*.igs", "*.iges", "*.stp", "*.step", "*.stpz", "*.stepz", "*.ifc", "*.u3d", "*.CATProduct", "*.CATPart", "*.cgr", "*.CATShape", "*.model", "*.session", "*.sldasm", "*.sldprt", "*.prt", "*.asm.*", "*.prt.*", "*.neu", "*.neu.*", "*.xas", "*.xas.*", "*.xpr", "*.xpr.*", "*.asm", "*.par", "*.pwd", "*.psm", "*.ipt", "*.iam", "*.ipj", "*.sat", "*.sab", "*.ptx", "*.xyz", "*.vda", "*.3dm", "*.3dxml", "*.wrl", "*.vrml", /*"*.dae",*/ "*.stl", "*.e57", "*.pts", "*.jt", "*.x_t", "*.x_b", "*.p_t", "*.p_b", "*.xmt", "*.xmt_txt", "*.xmt_bin", "*.plmxml", /*"*.obj",*/ "*.csb", "*.wire", /*"*.skp",*/ "*.pdf", "*.prc", "*.3ds", "*.dwg", "*.dxf" };

        foreach (string ext in supportedExtensions)
        {
            if (asset.EndsWith(ext, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}