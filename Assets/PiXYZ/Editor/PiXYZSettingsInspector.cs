using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using PIXYZImportScript;
#if UNITY_EDITOR
using UnityEditor;
using PIXYZImportScript.AssemblyCSharp;
#endif

[CanEditMultipleObjects]
[CustomEditor(typeof(PiXYZImportSettings))]
public class PiXYZSettingsInspector : Editor
{
    public bool importing = false;
    CoroutineScheduler coroutineScheduler;
    PiXYZ4UnityLoader loader = null;
    PiXYZUtils utils = new PiXYZUtils();

    void OnEnable()
    {
        coroutineScheduler = ScriptableObject.CreateInstance<CoroutineScheduler>();
    }

    public override void OnInspectorGUI()
    {
        PiXYZImportSettings importSettings = target as PiXYZImportSettings;
        importSettings.windowId = GetInstanceID();
        importSettings.isInspector = true;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        GUI.enabled = false;

        if (System.IO.File.Exists(importSettings.settings.originalFilename))
        {
            EditorGUILayout.TextField(new GUIContent("Original File"), importSettings.settings.originalFilename);
        }
        else
        {
            GUIStyle redFont = new GUIStyle(EditorStyles.textField);
            redFont.normal.textColor = Color.red;
            EditorGUILayout.TextField("Original File", "Original File Missing", redFont);
        }
        if (importSettings.settings.originalFilename == "")
            return;
        GUI.enabled = true;
        EditorGUILayout.Space();

        utils.GUISettings(serializedObject, gameObject:importSettings.gameObject);

        EditorGUILayout.Space();
        //EditorGUILayout.BeginHorizontal();
        //GUILayout.FlexibleSpace();
        //string tooltips = "Click to apply new settings.";
        //if (GUILayout.Button(new GUIContent("Apply", tooltips), GUILayout.Width(150.0f)))
        //    if(EditorUtility.DisplayDialog("Warning!", "Caution! This action will completely re-import the model, all modifications will be lost, including material assignation.\n\nAre you sure you want to apply changes?", "Yes", "No"))
        //        OnApplyClicked();
        //GUILayout.FlexibleSpace();
        //EditorGUILayout.EndHorizontal();
        //EditorGUILayout.Space();

        EditorGUILayout.EndVertical();
        EditorUtility.SetDirty(importSettings);

    }

    public void OnApplyClicked()
    {
        PiXYZImportSettings importSettings = target as PiXYZImportSettings;
        //Checks if file is still present
        if (!System.IO.File.Exists(importSettings.settings.originalFilename))
        {
            EditorUtility.DisplayDialog("PiXYZImporter", "Cannot reimport file.\n\nOriginal file " + Path.GetFileName(importSettings.settings.originalFilename) + " does not exist anymore in " + Path.GetDirectoryName(importSettings.settings.originalFilename), "ok");
            return;
        }

        //Checks if Folder Assets/3DModels exist, otherwise create it
        if (!AssetDatabase.IsValidFolder("Assets/3DModels"))
        {
            AssetDatabase.CreateFolder("Assets", "3DModels");
        }

        //Import CAD Model in Assets/PiXYZ/3DModels
        serializedObject.ApplyModifiedProperties();
        loader = ScriptableObject.CreateInstance<PiXYZ4UnityLoader>();
        importing = true;
        coroutineScheduler.StartCoroutine(ImportModel());
    }

    public IEnumerator ImportModel()
    {
        PiXYZImportSettings settings = target as PiXYZImportSettings;

        GameObject gameObject = settings.gameObject;    //Find the gameobject the script is attached to

        Vector3 eulerAngles = new Vector3() + gameObject.transform.rotation.eulerAngles;
        Vector3 scale = new Vector3() + gameObject.transform.lossyScale;

        if (gameObject.transform.childCount > 0)
        {
            foreach (Transform child in gameObject.transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        UnityEngine.Object prefab;
        prefab = PrefabUtility.CreateEmptyPrefab("Assets/3DModels/" + settings.settings.prefabName + ".prefab");

        loader = new PiXYZ4UnityLoader();

        loader.setSourceCoordinatesSystem(settings.settings.isRightHanded, settings.settings.isZUp, settings.settings.scaleFactor);
        double mapUV3dSize = settings.settings.mapUV ? settings.settings.mapUV3dSize : -1;
        loader.configure(settings.settings.orient, mapUV3dSize, settings.settings.treeProcess, settings.settings.useLods ? settings.settings.lodsMode : LODsMode.NONE, settings.settings.lodSettings, !settings.settings.splitTo16BytesIndex, settings.settings.useMergeFinalAssemblies);

        CoroutineNode coco = coroutineScheduler.StartCoroutine(loader.loadFileRuntime(gameObject, settings.settings.originalFilename, true, prefab));
        yield return coco;
        PiXYZUtils.clearProgressBar();
        if (loader.getErrorMessage().Length > 0)
            Debug.LogError("PiXYZAssetImporter: loader.loadfile failed");
        else
            Debug.Log("Success");

#if UNITY_EDITOR
        foreach (UnityEngine.Object obj in loader.loadedObject)
            AssetDatabase.AddObjectToAsset(obj, prefab);
#endif

        gameObject.transform.Rotate(-gameObject.transform.rotation.eulerAngles);
        gameObject.transform.Rotate(eulerAngles);
        gameObject.transform.localScale = scale;
        PrefabUtility.ReplacePrefab(gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
        loader = null;
    }

}


