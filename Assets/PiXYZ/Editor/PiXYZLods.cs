using Assets.PiXYZ.Editor;
using PIXYZImportScript;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class PiXYZLods
{
    public static string tt = "";
    public static bool reset = false;
    PiXYZLODSlider slider = null;

    public PiXYZLods()
    {
        slider = new PiXYZLODSlider();
    }

    int computeDepth(Transform transform)
    {
        int depth = 0;
        for (int i = 0; i < transform.childCount; ++i)
            depth = Math.Max(depth, computeDepth(transform.GetChild(i)));
        return depth + 1;
    }

    public void printLoDSlider(SerializedObject serializedObject, string prefix, int winId, bool showLODMode = true, GameObject gameObject = null)
    {
        bool isInspector = serializedObject.FindProperty("isInspector") != null ? serializedObject.FindProperty("isInspector").boolValue : false;
        SerializedProperty serializedProperty = serializedObject.FindProperty(prefix);

        tt = "Use the button “Add LOD level” to add multiple LODs (or Level of Detail, see documentation for more information).\n\nFor each LOD(up to 5, including LOD 0), choose a Quality preset.\n\nUse the horizontal bar to set screen size percentage for each LOD.\n\nRight-click in the horizontal bar to add / remove a new LOD.";
        PiXYZUtils.beginGroupBox("LODs Mesh Quality", tooltip: tt);
        {
            int currentLodIndex = -1;
            tt = "To insert or remove a LoD, right-click on the row";

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.Space();
                bool changed = false || reset;
                if (changed)
                    reset = false;
                if (showLODMode)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20);
                        float width = GUI.skin.label.CalcSize(new GUIContent("Quality")).x;
                        GUILayout.Label("Mode", GUILayout.Width(width));
                        List<string> propertyNames = new List<string>(3);
                        propertyNames.Add("LOD Group put on root object");
                        propertyNames.Add("LOD Groups put on the parent of each mesh");
                        List<int> intValue = new List<int>(3);
                        intValue.Add(1);
                        intValue.Add(2);
                        width = (float)Math.Truncate(Screen.width * 0.6);
                        Rect rect = EditorGUILayout.GetControlRect();
                        rect.width = width;
                        GUILayout.FlexibleSpace();
                        int originalValue = serializedProperty.FindPropertyRelative("lodsMode").intValue;
                        serializedProperty.FindPropertyRelative("lodsMode").intValue = EditorGUI.IntPopup(rect, originalValue, propertyNames.ToArray(), intValue.ToArray());
                        if (isInspector && gameObject != null && originalValue != serializedProperty.FindPropertyRelative("lodsMode").intValue)
                        {
                            if (originalValue == 1)
                            {
                                LODGroup lodGroup = gameObject.GetComponent<LODGroup>();
                                Dictionary<LODGroup, Dictionary<float, List<Renderer>>> finalLods = new Dictionary<LODGroup, Dictionary<float, List<Renderer>>>();
                                foreach (LOD lod in lodGroup.GetLODs())
                                {
                                    foreach (Renderer renderer in lod.renderers)
                                    {
                                        LODGroup parentLODGroup = renderer.transform.parent.GetComponent<LODGroup>();
                                        if (parentLODGroup == null)
                                            parentLODGroup = renderer.transform.parent.gameObject.AddComponent<LODGroup>();
                                        if (!finalLods.ContainsKey(parentLODGroup))
                                            finalLods.Add(parentLODGroup, new Dictionary<float, List<Renderer>>());
                                        if (!finalLods[parentLODGroup].ContainsKey(lod.screenRelativeTransitionHeight))
                                            finalLods[parentLODGroup].Add(lod.screenRelativeTransitionHeight, new List<Renderer>());
                                        finalLods[parentLODGroup][lod.screenRelativeTransitionHeight].Add(renderer);
                                    }
                                }
                                UnityEngine.Object.DestroyImmediate(lodGroup);
                                foreach (var groupPair in finalLods)
                                {
                                    List<LOD> lods = new List<LOD>();
                                    foreach (var pair in groupPair.Value)
                                    {
                                        lods.Add(new LOD(pair.Key, pair.Value.ToArray()));
                                    }
                                    lods.Sort(delegate (LOD x, LOD y)
                                    {
                                        if (x.screenRelativeTransitionHeight < y.screenRelativeTransitionHeight) return 1;
                                        else if (x.screenRelativeTransitionHeight == y.screenRelativeTransitionHeight) return 0;
                                        else return -1;
                                    });
                                    groupPair.Key.SetLODs(lods.ToArray());
                                }
                            }
                            else
                            {
                                Dictionary<float, List<Renderer>> newLods = new Dictionary<float, List<Renderer>>();
                                foreach (LODGroup lodGroup in gameObject.GetComponentsInChildren<LODGroup>())
                                {
                                    foreach (LOD lod in lodGroup.GetLODs())
                                    {
                                        if (!newLods.ContainsKey(lod.screenRelativeTransitionHeight))
                                            newLods.Add(lod.screenRelativeTransitionHeight, new List<Renderer>());
                                        newLods[lod.screenRelativeTransitionHeight].AddRange(lod.renderers);
                                    }
                                    UnityEngine.Object.DestroyImmediate(lodGroup);
                                }
                                LODGroup parentLODGroup = gameObject.AddComponent<LODGroup>();
                                List<LOD> lods = new List<LOD>();
                                foreach (KeyValuePair<float, List<Renderer>> pair in newLods)
                                {
                                    lods.Add(new LOD(pair.Key, pair.Value.ToArray()));
                                }
                                lods.Sort(delegate (LOD x, LOD y)
                                {
                                    if (x.screenRelativeTransitionHeight < y.screenRelativeTransitionHeight) return 1;
                                    else if (x.screenRelativeTransitionHeight == y.screenRelativeTransitionHeight) return 0;
                                    else return -1;
                                });
                                parentLODGroup.SetLODs(lods.ToArray());
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    slider.show(serializedObject, gameObject);
                    GUILayout.Space(20);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
                if (!isInspector)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (!serializedProperty.FindPropertyRelative("useLods").boolValue)
                            if (GUILayout.Button(new GUIContent("Activate LODs")))
                                serializedProperty.FindPropertyRelative("useLods").boolValue = true;
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.Space(10);
            }
            EditorGUILayout.EndVertical();

            currentLodIndex = serializedProperty.FindPropertyRelative("lodCurrentIndex").intValue;

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                if (EditorWindow.focusedWindow != null)
                    EditorWindow.focusedWindow.Repaint();
            }

            if (!isInspector)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        if (serializedProperty.FindPropertyRelative("useLods").boolValue)
                            printLoDSettings(currentLodIndex, serializedObject, prefix + "." + PiXYZLODSettings.serializePrefix);
                        serializedObject.ApplyModifiedProperties();
                        GUILayout.Space(10);
                    }
                    GUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("Propagate Materials from LOD0", "After applying a new material to one (or multiple) LOD0, use this button to propagate the material assignment to all the other LODs.")))
                    {
                        if (serializedProperty.FindPropertyRelative("lodsMode").intValue == 2)
                            foreach (var lodGroup in gameObject.GetComponentsInChildren<LODGroup>())
                            {
                                for (int i = 1; i < lodGroup.lodCount; ++i)
                                //foreach(LOD lod in lodGroup.GetLODs())
                                {
                                    if (lodGroup.GetLODs()[0].renderers.Length != lodGroup.GetLODs()[i].renderers.Length)
                                        Debug.Log("The number of renderers on each LOD is not equal, can't synchronize !");
                                    else
                                        for (int j = 0; j < lodGroup.GetLODs()[0].renderers.Length; ++j)
                                        {
                                            Renderer renderer = lodGroup.GetLODs()[i].renderers[j].gameObject.GetComponent<Renderer>();
                                            renderer.sharedMaterial = lodGroup.GetLODs()[0].renderers[j].sharedMaterial;
                                            renderer.sharedMaterials = lodGroup.GetLODs()[0].renderers[j].sharedMaterials;
                                        }
                                }
                            }
                        else
                        {
                            LODGroup lodGroup = gameObject.GetComponent<LODGroup>();
                            for (int i = 1; i < lodGroup.lodCount; ++i)
                            //foreach(LOD lod in lodGroup.GetLODs())
                            {
                                if (lodGroup.GetLODs()[0].renderers.Length != lodGroup.GetLODs()[i].renderers.Length)
                                    Debug.Log("The number of renderers on each LOD is not equal, can't synchronize !");
                                else
                                    for (int j = 0; j < lodGroup.GetLODs()[0].renderers.Length; ++j)
                                    {
                                        Renderer renderer = lodGroup.GetLODs()[i].renderers[j].gameObject.GetComponent<Renderer>();
                                        renderer.sharedMaterial = lodGroup.GetLODs()[0].renderers[j].sharedMaterial;
                                        renderer.sharedMaterials = lodGroup.GetLODs()[0].renderers[j].sharedMaterials;
                                    }
                            }
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
        }
        PiXYZUtils.endGroupBox();
    }

    public static void printLoDSettings(int index, SerializedObject serializedObject, string prefix = "settings.lodSettings", bool showTessellation = true)
    {
        int winId = serializedObject.FindProperty("windowId").intValue;
        string tt = "";
        serializedObject.Update();
        SerializedProperty lodProperties = serializedObject.FindProperty(prefix);
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical();
            {
                tt = PiXYZUtils.getTooltipText<PiXYZLODSettings>("preset");

                for (int i = 0; i < lodProperties.arraySize; i++)
                {
                    if (lodProperties.GetArrayElementAtIndex(i).FindPropertyRelative("preset").intValue > 4)
                    {
                        int j = lodProperties.arraySize - 1;
                        while (j >= i)
                        {
                            removeLod(j, serializedObject, "settings");
                            j--;
                        }
                        break;
                    }
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(new GUIContent("LOD " + i, tt));
                        EditorGUILayout.PropertyField(lodProperties.GetArrayElementAtIndex(i).FindPropertyRelative("preset"), GUIContent.none, GUILayout.ExpandWidth(true));
                        for (int j = i; j < lodProperties.arraySize - 1; j++)
                            while (lodProperties.GetArrayElementAtIndex(j).FindPropertyRelative("preset").intValue >= lodProperties.GetArrayElementAtIndex(j + 1).FindPropertyRelative("preset").intValue)
                            {
                                lodProperties.GetArrayElementAtIndex(j + 1).FindPropertyRelative("preset").intValue++;
                            }

                        if (GUILayout.Button(new GUIContent("-"), GUILayout.MaxHeight(13)))
                            if (lodProperties.arraySize == 1)
                            {
                                if (EditorPrefs.GetBool("PiXYZ.ShowPopupLods", true))
                                {
                                    if (EditorUtility.DisplayDialog("PiXYZImporter", "If you remove the last LOD, the functionality will be disabled.", "Continue", "Cancel"))
                                    {
                                        serializedObject.FindProperty("settings.useLods").boolValue = false;
                                        EditorPrefs.SetBool("PiXYZ.ShowPopupLods", false);
                                    }

                                }
                                else
                                    serializedObject.FindProperty("settings.useLods").boolValue = false;
                            }
                            else
                                removeLod(i, serializedObject, "settings");

                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (lodProperties.GetArrayElementAtIndex(lodProperties.arraySize - 1).FindPropertyRelative("preset").intValue < 4)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(new GUIContent("Add LOD level")))
                            addLod(lodProperties.arraySize, serializedObject, lodProperties.arraySize - 1, 1f - lodProperties.arraySize * 0.2f, "settings");
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    private static float lowMovePercent = 0.2f;
    private static float getWidthFromGUI(float totalWidth, float percentWidth, float totalPercent)
    {
        float specialWidth = 0;
        if (totalPercent > 0.99f)
            specialWidth = Math.Min(totalPercent - 0.99f, percentWidth);
        return ((totalWidth * (1f - lowMovePercent)) * (percentWidth - specialWidth) + (totalWidth * lowMovePercent) * (specialWidth / 0.01f));
    }

    private static float getPercentFromGUI(float percentWidth, float totalPercent)
    {
        float newTotal = totalPercent + percentWidth;
        float specialPercent = Math.Max(0, newTotal - (1f - lowMovePercent));
        float relativePercent = (percentWidth - specialPercent) / (1f - lowMovePercent);
        specialPercent = specialPercent / lowMovePercent;
        return relativePercent * 0.99f + specialPercent * 0.01f;
    }

    static public void removeLod(int index, SerializedObject serializedObject, string prefix)
    {
        SerializedProperty serializedProperty = serializedObject.FindProperty(prefix);
        PiXYZLoDSettingsEditor.removeAt(index, serializedObject, prefix + "." + PiXYZLODSettings.serializePrefix);
        serializedProperty.FindPropertyRelative("lodSettingCount").intValue = serializedProperty.FindPropertyRelative("lodSettings").arraySize;
        serializedProperty.FindPropertyRelative("lodCurrentIndex").intValue = Math.Max(0,
            Math.Min(serializedProperty.FindPropertyRelative("lodCurrentIndex").intValue,
            serializedProperty.FindPropertyRelative("lodSettingCount").intValue - 1));
        serializedObject.ApplyModifiedProperties();
        PiXYZSettingsEditor.saveEditorPref(serializedObject, prefix);
    }

    static public void addLod(int index, SerializedObject serializedObject, int insertModel, float insertStartLod, string prefix)
    {
        SerializedProperty serializedProperty = serializedObject.FindProperty(prefix);
        PiXYZLoDSettingsEditor.insertAt(index, serializedObject, insertModel);
        serializedProperty.FindPropertyRelative("lodCurrentIndex").intValue = index;
        serializedProperty.FindPropertyRelative("lodSettingCount").intValue = serializedProperty.FindPropertyRelative("lodSettings").arraySize;
        if (insertStartLod >= 0f)
            serializedProperty.FindPropertyRelative("lodSettings").GetArrayElementAtIndex(insertModel).FindPropertyRelative("startLod").floatValue = insertStartLod;
        serializedObject.ApplyModifiedProperties();
        PiXYZSettingsEditor.saveEditorPref(serializedObject, prefix);
    }


    class SettingsMenuCallbackObject : object
    {
        public SerializedObject serializedObject;
        public int index;
        public int insertModel;
        public float insertStartLod;
        public string prefix;
        public SettingsMenuCallbackObject(SerializedObject _serializedObject, int _index, int _insertModel, float _insertStartLod, string _prefix = PiXYZSettings.serializePrefix) :
            this(_serializedObject, _index, _prefix)
        {
            insertModel = _insertModel;
            insertStartLod = _insertStartLod;
        }
        public SettingsMenuCallbackObject(SerializedObject _serializedObject, int _index, string _prefix = PiXYZSettings.serializePrefix)
        {
            serializedObject = _serializedObject;
            index = _index;
            prefix = _prefix;
            insertModel = 0;
            insertStartLod = -1f;
        }

        public static void selectItem(object Object)
        {
            SettingsMenuCallbackObject callbackObject = (SettingsMenuCallbackObject)Object;
            SerializedProperty serializedProperty = callbackObject.serializedObject.FindProperty(callbackObject.prefix);
            serializedProperty.FindPropertyRelative("lodCurrentIndex").intValue = callbackObject.index;
            callbackObject.serializedObject.ApplyModifiedProperties();
            PiXYZSettingsEditor.saveEditorPref(callbackObject.serializedObject, callbackObject.prefix);
        }
        public static void insertItem(object Object)
        {
            SettingsMenuCallbackObject callbackObject = (SettingsMenuCallbackObject)Object;
            addLod(callbackObject.index, callbackObject.serializedObject, callbackObject.insertModel, callbackObject.insertStartLod, callbackObject.prefix);
        }
        public static void removeItem(object Object)
        {
            SettingsMenuCallbackObject callbackObject = (SettingsMenuCallbackObject)Object;
            removeLod(callbackObject.index, callbackObject.serializedObject, callbackObject.prefix);
        }
    }
}
