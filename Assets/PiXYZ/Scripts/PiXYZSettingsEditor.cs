using PIXYZImportScript;
using System;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[System.Serializable]
public class PiXYZSettingsEditor : PIXYZImportScript.PiXYZSettings
{
#if UNITY_EDITOR
    public void getEditorPref()
    {
        string version = InternalEditorUtility.GetFullUnityVersion();
        version = version.Substring(0, version.LastIndexOf('.'));
        originalFilename = "";
        orient = EditorPrefs.GetBool("PiXYZ.Orient", false);
        mapUV = EditorPrefs.GetBool("PiXYZ.MapUV", false);
        mapUV3dSize = EditorPrefs.GetFloat("PiXYZ.MapUV3dSize", 100.0f);
        scaleFactor = EditorPrefs.GetFloat("PiXYZ.ScaleFactor", 0.001f);
        isRightHanded = EditorPrefs.GetBool("PiXYZ.IsRightHanded", true);
        isZUp = EditorPrefs.GetBool("PiXYZ.IsZUp", true);
        treeProcess = (TreeProcessType)EditorPrefs.GetInt("PiXYZ.TreeProcess", 0);
        lodCurrentIndex = EditorPrefs.GetInt("PiXYZ.LODCurrentIndex", 0);
        lodSettingCount = EditorPrefs.GetInt("PiXYZ.LODSettingCount", 1);
        useLods = EditorPrefs.GetBool("PiXYZ.UseLods", false);
        lodsMode = (LODsMode)EditorPrefs.GetInt("PiXYZ.LODsMode", 2);
        lodSettings = new List<PiXYZLODSettings>();
        for (int i = 0; i < lodSettingCount; ++i)
        {
            PiXYZLoDSettingsEditor lod = new PiXYZLoDSettingsEditor();
            lod.index = i;
            lod.getEditorPref();
            lodSettings.Add(lod);
        }
        splitTo16BytesIndex = EditorPrefs.GetBool("PiXYZ.SplitTo16BytesIndex", false);
        useMergeFinalAssemblies = EditorPrefs.GetBool("PiXYZ.UseMergeFinalAssemblies", false);
    }

    public void factoryReset()
    {
        EditorPrefs.DeleteKey("PiXYZ.Orient");
        EditorPrefs.DeleteKey("PiXYZ.MapUV");
        EditorPrefs.DeleteKey("PiXYZ.MapUV3dSize");
        EditorPrefs.DeleteKey("PiXYZ.ScaleFactor");
        EditorPrefs.DeleteKey("PiXYZ.IsRightHanded");
        EditorPrefs.DeleteKey("PiXYZ.IsZUp");
        EditorPrefs.DeleteKey("PiXYZ.TreeProcess");
        EditorPrefs.DeleteKey("PiXYZ.LODCurrentIndex");
        EditorPrefs.DeleteKey("PiXYZ.LODSettingCount");
        EditorPrefs.DeleteKey("PiXYZ.UseLods");
        EditorPrefs.DeleteKey("PiXYZ.LODsMode");
        lodSettings = new List<PiXYZLODSettings>();
        EditorPrefs.DeleteKey("PiXYZ.SplitTo16BytesIndex");
        EditorPrefs.DeleteKey("PiXYZ.UseMergeFinalAssemblies");
        EditorPrefs.DeleteKey("PiXYZ.ShowPopupLods");
        EditorPrefs.DeleteKey("PiXYZ.AutoUpdate");
        EditorPrefs.SetBool("PiXYZ.ShowPopupLods", true);
        EditorPrefs.SetBool("PiXYZ.AutoUpdate", true);
        EditorPrefs.SetBool("PiXYZ.DoNotShowAgainDocumentationPopup", false);
        PiXYZLoDSettingsEditor.factoryReset();
        getEditorPref();
    }

    public static void saveEditorPref(SerializedObject serializedObject, string prefix = PiXYZSettings.serializePrefix)
    {
        SerializedProperty serializedProperty = serializedObject.FindProperty(prefix);
        EditorPrefs.SetBool("PiXYZ.Orient", serializedProperty.FindPropertyRelative("orient").boolValue);
        EditorPrefs.SetBool("PiXYZ.MapUV", serializedProperty.FindPropertyRelative("mapUV").boolValue);
        EditorPrefs.SetFloat("PiXYZ.MapUV3dSize", serializedProperty.FindPropertyRelative("mapUV3dSize").floatValue);
        EditorPrefs.SetFloat("PiXYZ.ScaleFactor", serializedProperty.FindPropertyRelative("scaleFactor").floatValue);
        EditorPrefs.SetBool("PiXYZ.IsRightHanded", serializedProperty.FindPropertyRelative("isRightHanded").boolValue);
        EditorPrefs.SetBool("PiXYZ.IsZUp", serializedProperty.FindPropertyRelative("isZUp").boolValue);
        EditorPrefs.SetInt("PiXYZ.TreeProcess", serializedProperty.FindPropertyRelative("treeProcess").intValue);
        EditorPrefs.SetInt("PiXYZ.LODCurrentIndex", serializedProperty.FindPropertyRelative("lodCurrentIndex").intValue);
        EditorPrefs.SetBool("PiXYZ.UseLods", serializedProperty.FindPropertyRelative("useLods").boolValue);
        EditorPrefs.SetInt("PiXYZ.LODsMode", serializedProperty.FindPropertyRelative("lodsMode").intValue);
        EditorPrefs.SetInt("PiXYZ.LODSettingCount", serializedProperty.FindPropertyRelative("lodSettingCount").intValue);
        for (int i = 0; i < serializedProperty.FindPropertyRelative("lodSettingCount").intValue; ++i)
        {
            PiXYZLoDSettingsEditor.saveEditorPref(i, serializedObject, prefix + "." + PiXYZLODSettings.serializePrefix);
        }
        EditorPrefs.SetBool("PiXYZ.UseMergeFinalAssemblies", serializedProperty.FindPropertyRelative("useMergeFinalAssemblies").boolValue);
        EditorPrefs.SetBool("PiXYZ.SplitTo16BytesIndex", serializedProperty.FindPropertyRelative("splitTo16BytesIndex").boolValue);
    }
#endif
}

[System.Serializable]
public class PiXYZLoDSettingsEditor : PIXYZImportScript.PiXYZLODSettings
{
    public PiXYZLoDSettingsEditor() { }
    public PiXYZLoDSettingsEditor(PIXYZImportScript.PiXYZLODSettings other)
    {
        //string lodName = index >= 0 ? "lod" + other.index : "lodDefault";
        //useSag = other.useSag;
        //maxSag = other.maxSag;
        //useAngle = other.useAngle;
        //maxAngle = other.maxAngle;
        //useDecimation = other.useDecimation;
        //surfaceTol = other.surfaceTol;
        //useDecimNormalTol = other.useDecimNormalTol;
        //normalTol = other.normalTol;
        //useDecimUvTol = other.useDecimUvTol;
        //uvTol = other.uvTol;
        preset = other.preset;
        startLod = other.startLod;
    }
#if UNITY_EDITOR
    public void getEditorPref()
    {
        string lodName = index >= 0 ? "lod" + index : "lodDefault";
        //useSag = EditorPrefs.GetBool("PiXYZ." + lodName + ".UseSag", true);
        //maxSag = EditorPrefs.GetFloat("PiXYZ." + lodName + ".MaxSag", 0.2f);
        //useAngle = EditorPrefs.GetBool("PiXYZ." + lodName + ".UseAngle", false);
        //maxAngle = EditorPrefs.GetFloat("PiXYZ." + lodName + ".MaxAngle", 20f);
        //useDecimation = EditorPrefs.GetBool("PiXYZ." + lodName + ".UseDecimation", false);
        //surfaceTol = EditorPrefs.GetFloat("PiXYZ." + lodName + ".DecimationTolerance", 1f);
        //useDecimNormalTol = EditorPrefs.GetBool("PiXYZ." + lodName + ".UseNormalTol", true);
        //normalTol = EditorPrefs.GetFloat("PiXYZ." + lodName + ".NormalTol", 5f);
        //useDecimUvTol = EditorPrefs.GetBool("PiXYZ." + lodName + ".UseUVTol", true);
        //uvTol = EditorPrefs.GetFloat("PiXYZ." + lodName + ".UvTol", 0.01f);
        preset = (MeshQualityPresets)EditorPrefs.GetInt("PiXYZ." + lodName + ".Preset", 2);
        startLod = EditorPrefs.GetFloat("PiXYZ." + lodName + ".StartLod", 0.01f);// 0.25f - index * 0.12f);
    }
    public static void factoryReset()
    {
        for (int index = 0; index < 5; index++)
        {
            string lodName = index >= 0 ? "lod" + index : "lodDefault";
            if (EditorPrefs.HasKey("PiXYZ." + lodName + ".Preset"))
                EditorPrefs.DeleteKey("PiXYZ." + lodName + ".Preset");
            if (EditorPrefs.HasKey("PiXYZ." + lodName + ".StartLod"))
                EditorPrefs.DeleteKey("PiXYZ." + lodName + ".StartLod");
        }
    }
    public static void saveEditorPref(int index, SerializedObject serializedObject, string prefix = "settings." + PiXYZLODSettings.serializePrefix)
    {
        SerializedProperty lodProperties = serializedObject.FindProperty(prefix);
        SerializedProperty lodProperty = null;
        string lodName = index >= 0 ? "lod" + index : "lodDefault";
        if (lodProperties.isArray)
        {
            lodProperty = lodProperties.GetArrayElementAtIndex(index);
        }
        else
        {
            lodProperty = lodProperties;
        }
        if (lodProperty != null)
        {
            //EditorPrefs.SetBool("PiXYZ." + lodName + ".UseSag", lodProperty.FindPropertyRelative("useSag").boolValue);
            //EditorPrefs.SetFloat("PiXYZ." + lodName + ".MaxSag", lodProperty.FindPropertyRelative("maxSag").floatValue);
            //EditorPrefs.SetBool("PiXYZ." + lodName + ".UseAngle", lodProperty.FindPropertyRelative("useAngle").boolValue);
            //EditorPrefs.SetFloat("PiXYZ." + lodName + ".MaxAngle", lodProperty.FindPropertyRelative("maxAngle").floatValue);
            //EditorPrefs.SetBool("PiXYZ." + lodName + ".UseDecimation", lodProperty.FindPropertyRelative("useDecimation").boolValue);
            //EditorPrefs.SetFloat("PiXYZ." + lodName + ".DecimationTolerance", lodProperty.FindPropertyRelative("surfaceTol").floatValue);
            //EditorPrefs.SetBool("PiXYZ." + lodName + ".UseNormalTol", lodProperty.FindPropertyRelative("useDecimNormalTol").boolValue);
            //EditorPrefs.SetFloat("PiXYZ." + lodName + ".NormalTol", lodProperty.FindPropertyRelative("normalTol").floatValue);
            //EditorPrefs.SetBool("PiXYZ." + lodName + ".UseUVTol", lodProperty.FindPropertyRelative("useDecimUvTol").boolValue);
            //EditorPrefs.SetFloat("PiXYZ." + lodName + ".UvTol", lodProperty.FindPropertyRelative("uvTol").floatValue);
            EditorPrefs.SetInt("PiXYZ." + lodName + ".Preset", lodProperty.FindPropertyRelative("preset").enumValueIndex);
            EditorPrefs.SetFloat("PiXYZ." + lodName + ".StartLod", lodProperty.FindPropertyRelative("startLod").floatValue);
        }
    }
    public static void insertAt(int index, SerializedObject serializedObject, string prefix = "settings." + PiXYZLODSettings.serializePrefix)
    {
        insertAt(index, serializedObject, index - 1, prefix);
    }
    public static void insertAt(int index, SerializedObject serializedObject, int from, string prefix = "settings." + PiXYZLODSettings.serializePrefix)
    {
        SerializedProperty lodProperties = serializedObject.FindProperty(prefix);
        if (lodProperties.isArray)
        {
            SerializedProperty model = lodProperties.GetArrayElementAtIndex(Math.Max(0, Math.Min(from, lodProperties.arraySize - 1)));
            lodProperties.InsertArrayElementAtIndex(index);
            SerializedProperty lodProperty = lodProperties.GetArrayElementAtIndex(index);
            SerializedProperty end = model.GetEndProperty(true);
            while (model.Next(true) && lodProperty.Next(true) && !SerializedProperty.EqualContents(model, end))
            {
                if (model.name == "index")
                    continue;
                string propertyName = model.propertyType.ToString();
                switch (propertyName)
                {
                    case "Boolean":
                        propertyName = "Bool";
                        break;
                    case "Integer":
                        propertyName = "Int";
                        break;
                }
                propertyName = propertyName.ToLower() + "Value";
                PropertyInfo prop = typeof(SerializedProperty).GetProperty(propertyName);
                if (prop != null)
                    prop.SetValue(lodProperty, prop.GetValue(model, null), null);
            }
        }
    }

    public static void removeAt(int index, SerializedObject serializedObject, string prefix = "settings." + PiXYZLODSettings.serializePrefix)
    {
        SerializedProperty lodProperties = serializedObject.FindProperty(prefix);
        if (lodProperties.isArray && lodProperties.arraySize > index)
        {
            if (lodProperties.arraySize != 1)
            {
                if (lodProperties.arraySize - 1 == index)
                    lodProperties.GetArrayElementAtIndex(index - 1).FindPropertyRelative("startLod").floatValue = lodProperties.GetArrayElementAtIndex(index).FindPropertyRelative("startLod").floatValue;
                lodProperties.DeleteArrayElementAtIndex(index);
            }
        }
    }

    public static SerializedProperty getIndexProperty(int index, SerializedObject serializedObject, string property, string prefix = "settings." + PiXYZLODSettings.serializePrefix)
    {
        SerializedProperty lodProperties = serializedObject.FindProperty(prefix);
        if (lodProperties.isArray && lodProperties.arraySize > index)
        {
            SerializedProperty lodProperty = lodProperties.GetArrayElementAtIndex(index);
            return lodProperty.FindPropertyRelative(property);
        }
        return null;
    }
#endif
}