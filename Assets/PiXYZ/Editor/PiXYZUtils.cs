
using System;
using System.Collections.Generic;
using PIXYZImportScript;
using UnityEngine;
using System.Reflection;

using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;

class PiXYZUtils
{
    PiXYZLods pixyzLods = null;

    public PiXYZUtils()
    {
        pixyzLods = new PiXYZLods();
    }

    public void OnEnable()
    {
    }

    public void GUISettings(SerializedObject serializedObject, string fileExt = "", GameObject gameObject = null)
    {
        printSettingsGUI(serializedObject, "settings", fileExt, gameObject);
        serializedObject.ApplyModifiedProperties();
    }

    public static void saveEditorPref(SerializedObject serializedObject)
    {
        PiXYZSettingsEditor.saveEditorPref(serializedObject, "settings");
    }

    public static bool isPiXYZExt(string ext)
    {
        return ".pxz" == ext;
    }

    static Dictionary<int, AnimBool> UvAnim = new Dictionary<int, AnimBool>();
    static Dictionary<int, AnimBool> lodAnim = new Dictionary<int, AnimBool>();
    static Dictionary<int, bool> advancedToggle = new Dictionary<int, bool>();
    static Dictionary<int, Vector2> g_scrollViewPosition = new Dictionary<int, Vector2>();
    public static int lastFocusedId = 0;

    void printSettingsGUI(SerializedObject serializedObject, string prefix = PiXYZSettings.serializePrefix, string fileExt = "", GameObject gameObject = null)
    {
        serializedObject.Update();

        int winId = serializedObject.FindProperty("windowId").intValue;
        bool isInspector = serializedObject.FindProperty("isInspector") != null ? serializedObject.FindProperty("isInspector").boolValue : false;
        string tt = "";
        Vector2 scrollViewPosition;

        int focusId = EditorWindow.focusedWindow != null ? EditorWindow.focusedWindow.GetInstanceID() : lastFocusedId;
        if (winId != focusId)
            winId = 0;
        if (!g_scrollViewPosition.ContainsKey(winId))   //winId = 0 is shared
        {
            scrollViewPosition = new Vector2(0, 0);
            g_scrollViewPosition[winId] = scrollViewPosition;
        }
        else
        {
            scrollViewPosition = g_scrollViewPosition[winId];
        }
        if (!UvAnim.ContainsKey(winId) && winId != 0)
        {
            UvAnim[winId] = new AnimBool(false);
            UvAnim[winId].valueChanged.AddListener(EditorWindow.focusedWindow.Repaint);
        }
        if (!lodAnim.ContainsKey(winId) && winId != 0)
        {
            lodAnim[winId] = new AnimBool(false);
            lodAnim[winId].valueChanged.AddListener(EditorWindow.focusedWindow.Repaint);
        }
        if (!advancedToggle.ContainsKey(winId))
        {
            advancedToggle[winId] = false;
        }

        SerializedProperty serializedProperty = serializedObject.FindProperty(prefix);

        int lastShown = 0;
        scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, GUILayout.MaxHeight(Screen.height - 30));
        {
            g_scrollViewPosition[winId] = scrollViewPosition;
            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Space(10);
                tt = "Use the following settings to adapt the imported model’s units/transforms to Unity3D’s units/coordinate system (meters/left-handed).\n\nDefault settings change a millimeters / Z-up axis scene to Unity configuration.";
                beginGroupBox("Coordinate System", isInspector, tt);
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    {
                        tt = PiXYZUtils.getTooltipText<PiXYZSettings>("scaleFactor");
                        EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("scaleFactor"), new GUIContent("Scale", tt));
                        GUILayout.Space(10);
                    }
                    EditorGUILayout.EndHorizontal();
                    tt = PiXYZUtils.getTooltipText<PiXYZSettings>("isRightHanded");
                    EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("isRightHanded"), new GUIContent("Right Handed", tt));
                    tt = PiXYZUtils.getTooltipText<PiXYZSettings>("isZUp");
                    EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("isZUp"), new GUIContent("Z-up", tt));
                    EditorGUILayout.Space();
                }
                endGroupBox();

                if (!isPiXYZExt(fileExt))
                {
                    GUILayout.Space(10);
                    tt = "Choose one of the following mode to optimize the imported model’s hierarchy (or tree)\n\n\nNone: No modification of the hierarchy (default mode)\n\nClean-up intermediary nodes: Compresses the hierarchy by removing empty nodes, or any node containing only one sub-node.\n\nTransfer all objects under root: Simplifies the hierarchy by transferring all imported 3D objects (or GameObject) under the root node.";
                    beginGroupBox("Hierarchy Optimization", isInspector, tooltip: tt);
                    {
                        tt = PiXYZUtils.getTooltipText<PiXYZSettings>("treeProcess");
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(20);
                            float width = GUI.skin.label.CalcSize(new GUIContent("Quality")).x;
                            GUILayout.Label("Mode", GUILayout.Width(width));
                            List<string> propertyNames = new List<string>(3);
                            propertyNames.Add("None");
                            propertyNames.Add("Clean-up intermediary nodes");
                            propertyNames.Add("Transfer all objects under root");
                            propertyNames.Add("Merge all objects");
                            propertyNames.Add("Merge objects by material");
                            List<int> intValue = new List<int>(3);
                            intValue.Add(0);
                            intValue.Add(1);
                            intValue.Add(2);
                            intValue.Add(3);
                            intValue.Add(4);
                            width = (float)Math.Truncate(Screen.width * 0.6);
                            Rect rect = EditorGUILayout.GetControlRect();
                            rect.width = width;
                            GUILayout.FlexibleSpace();
                            serializedProperty.FindPropertyRelative("treeProcess").intValue = EditorGUI.IntPopup(rect, serializedProperty.FindPropertyRelative("treeProcess").intValue, propertyNames.ToArray(), intValue.ToArray());
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                    }
                    endGroupBox();
                    GUILayout.Space(10);
                    if (!serializedObject.FindProperty("settings.useLods").boolValue)
                    {
                        tt = "Choose the quality level (preset) for the imported model.\nQuality defines the density of the mesh that PiXYZ creates.\nDepending if you import a CAD model (exact geometry) or a mesh model (tessellated geometry), PiXYZ will either perform a Tessellation or a Decimation on the model (see documentation for more information and presets details).";
                        beginGroupBox("Mesh Quality", isInspector, tooltip: tt);
                        {
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginHorizontal();
                            {
                                GUILayout.Space(20);
                                tt = PiXYZUtils.getTooltipText<PiXYZLODSettings>("preset");
                                GUILayout.Label(new GUIContent("Quality", tt));
                                int width = (int)(Math.Truncate(Screen.width * 0.5));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("settings.lodSettings").GetArrayElementAtIndex(0).FindPropertyRelative("preset"), GUIContent.none, GUILayout.Width(width));
                                EditorGUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(5);
                                    GUILayout.Label("Use LODs");
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("settings.useLods"), GUIContent.none, GUILayout.Width(40));
                                }
                                EditorGUILayout.EndHorizontal();
                                GUILayout.FlexibleSpace();
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                        }
                        endGroupBox();
                    }
                    else
                    {
                        pixyzLods.printLoDSlider(serializedObject, prefix, winId, serializedProperty.FindPropertyRelative("treeProcess").intValue<3, gameObject);
                    }
                    GUILayout.Space(10);
                    beginGroupBox("Post Process", isInspector, tooltip: "Generate UV : Use this setting to add a new primary UV set (channel #0). Set the size of the projection box used to create UVs.\n\nCaution: PiXYZ will override the existing UV set, do not Use this setting if you wish to preserve the UVs embedded in the imported model.\n\nOrient… : Use this setting for PiXYZ to perform a unification of all triangles orientation.\n\nCaution: Do not Use this setting if the imported model is a mesh (tessellated geometry) and is already correctly oriented.");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            tt = PiXYZUtils.getTooltipText<PiXYZSettings>("mapUV");
                            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("mapUV"), GUIContent.none, true, GUILayout.Width(40));
                            GUILayout.Label(new GUIContent("Generate UV (size)", tt));
                            //GUILayout.FlexibleSpace();
                            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("mapUV3dSize"), GUIContent.none, true, GUILayout.Width(100));
                            GUILayout.Label(new GUIContent("millimeters", tt));
                            serializedProperty.FindPropertyRelative("mapUV3dSize").floatValue = Mathf.Clamp(serializedProperty.FindPropertyRelative("mapUV3dSize").floatValue, 1.0f, 1000f);
                            GUILayout.Space(10);
                        }
                        EditorGUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        {
                            tt = PiXYZUtils.getTooltipText<PiXYZSettings>("orient");
                            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("orient"), GUIContent.none, true, GUILayout.Width(40));
                            GUILayout.Label(new GUIContent("Orient normals of adjacent faces consistently", tt), GUILayout.ExpandWidth(true));
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                    }
                    endGroupBox();

                    GUILayout.Space(10);
                    bool before = advancedToggle[winId];
                    advancedToggle[winId] = EditorGUILayout.Foldout(advancedToggle[winId], "Advanced", true);
                    if (advancedToggle[winId])
                    {

                        if (!before)
                            g_scrollViewPosition[winId] = new Vector2(0, Screen.height);
                        GUILayout.Space(10);
                        EditorGUI.indentLevel++;
                        string version = InternalEditorUtility.GetFullUnityVersion();
                        version = version.Substring(0, version.LastIndexOf('.'));
                        if (float.Parse(version) >= 2017.3)    //Cannot change before 2017.3
                        {
                            EditorGUI.BeginDisabledGroup(isInspector);
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(10);
                                    tt = PiXYZUtils.getTooltipText<PiXYZSettings>("useMergeFinalAssemblies");
                                    serializedProperty.FindPropertyRelative("useMergeFinalAssemblies").boolValue = EditorGUILayout.Toggle(serializedProperty.FindPropertyRelative("useMergeFinalAssemblies").boolValue, GUILayout.Width(40));
                                    GUILayout.Label(new GUIContent("Stitch unconnected surfaces", tt));
                                    GUILayout.FlexibleSpace();
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(10);
                                    tt = PiXYZUtils.getTooltipText<PiXYZSettings>("splitTo16BytesIndex");
                                    serializedProperty.FindPropertyRelative("splitTo16BytesIndex").boolValue = EditorGUILayout.Toggle(serializedProperty.FindPropertyRelative("splitTo16BytesIndex").boolValue, GUILayout.Width(40));
                                    GUILayout.Label(new GUIContent("Split to limit vertex count per mesh", tt));
                                    GUILayout.FlexibleSpace();
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUI.EndDisabledGroup();

                        }
                        EditorGUILayout.Space();
                    }
                    EditorGUILayout.EndFadeGroup();
                    Rect boxRect = GUILayoutUtility.GetLastRect();
                    lastShown = (int)(boxRect.y + boxRect.height);
                }
            }
            EditorGUILayout.EndVertical();
        }
        GUILayout.EndScrollView();
        Rect scrollRect = GUILayoutUtility.GetLastRect();
        int gradientHeight = 15;
        //up
        if (scrollViewPosition.y > 0f)
            PiXYZUtils.gradientBox(new Rect(scrollRect.x, scrollRect.y, scrollRect.width, gradientHeight), new Vector2(0.5f, 1f));
        //down
        if (scrollViewPosition.y < (lastShown - scrollRect.height))
            PiXYZUtils.gradientBox(new Rect(scrollRect.x, scrollRect.y + scrollRect.height - gradientHeight, scrollRect.width, gradientHeight), new Vector2(0.5f, 0f));
    }

    public static string getTooltipText<T>(string field)
    {
        FieldInfo fieldInfo = typeof(T).GetField(field);
        TooltipAttribute toolTip = fieldInfo != null ? GetTooltip(fieldInfo, true) : null;
        return toolTip != null ? toolTip.tooltip : "";
    }
    private static TooltipAttribute GetTooltip(FieldInfo field, bool inherit)
    {
        TooltipAttribute[] attributes = field.GetCustomAttributes(typeof(TooltipAttribute), inherit) as TooltipAttribute[];

        return attributes.Length > 0 ? attributes[0] : null;
    }

    private static Texture2D _staticRectTexture;
    private static GUIStyle _staticRectStyle;
    // Note that this function is only meant to be called from OnGUI() functions.
    public static void GUIDrawRect(Rect position, Color color, int borderThikness = 0, string title = "")
    {
        Color tmp = color;
        tmp.a = 1;
        GUIDrawRect(position, color, tmp, borderThikness, title);
    }

    public static void GUIDrawRect(Rect position, Color color, Color borderColor, int borderThikness = 0, string text = "", TextAnchor rectTextAnchor = TextAnchor.MiddleCenter)
    {
        if (_staticRectTexture == null)
        {
            _staticRectTexture = new Texture2D(1, 1);
        }

        if (_staticRectStyle == null)
        {
            _staticRectStyle = new GUIStyle();
        }
        _staticRectTexture.SetPixel(0, 0, color);
        _staticRectTexture.Apply();

        if (color != Color.white && color != Color.gray)
            _staticRectStyle.normal.textColor = Color.white;
        _staticRectStyle.clipping = TextClipping.Clip;
        _staticRectStyle.border = new RectOffset(-borderThikness, -borderThikness, -borderThikness, -borderThikness);
        _staticRectStyle.normal.background = _staticRectTexture;
        _staticRectStyle.alignment = rectTextAnchor;
        _staticRectStyle.fontSize = 10;

        Rect contentRect = new Rect(position.x + borderThikness, position.y + borderThikness, position.width - borderThikness * 2, position.height - borderThikness * 2);
        GUI.Box(contentRect, new GUIContent(text), _staticRectStyle);

        if (borderThikness > 0)
        {
            _staticRectTexture.SetPixel(0, 0, borderColor);
            _staticRectTexture.Apply();
            GUI.DrawTexture(new Rect(position.x, position.y, position.width, borderThikness), _staticRectTexture);
            GUI.DrawTexture(new Rect(position.x, position.y + position.height - borderThikness, position.width, borderThikness), _staticRectTexture);
            GUI.DrawTexture(new Rect(position.x, position.y, borderThikness, position.height), _staticRectTexture);
            GUI.DrawTexture(new Rect(position.x + position.width - borderThikness, position.y, borderThikness, position.height), _staticRectTexture);
        }
    }



    public static void displayProgressBar(string title, string info, float progress)
    {
#if UNITY_EDITOR || (PIXYZ_DLL && !PIXYZ_RUNTIME)
        EditorUtility.DisplayProgressBar(title, info, progress);
#endif
    }

    public static void clearProgressBar()
    {
#if UNITY_EDITOR || (PIXYZ_DLL && !PIXYZ_RUNTIME)
        EditorUtility.ClearProgressBar();
#endif
    }

    public enum ColorType { Highlight, Active }
    public static Color getColor(ColorType type)
    {
        Color c = Color.black;
        const float LightGray = 0.76f;
        const float LightGrayPro = 0.40f;
        const float LighterGray = 0.87f;
        const float LighterGrayPro = 0.19f;
        switch (type)
        {
            case ColorType.Highlight:
                c = EditorGUIUtility.isProSkin ? new Color(LightGrayPro, LightGrayPro, LightGrayPro) : new Color(LightGray, LightGray, LightGray);
                break;
            case ColorType.Active:
                c = EditorGUIUtility.isProSkin ? new Color(LighterGrayPro, LighterGrayPro, LighterGrayPro) : new Color(LighterGray, LighterGray, LighterGray);
                break;
        }
        return c;
    }

    public static int Tabs(string[] options, int selected)
    {
        bool clicked = false;
        return Tabs(options, selected, ref clicked);
    }
    public static int Tabs(string[] options, int selected, ref bool clicked)
    {
#if UNITY_EDITOR || (PIXYZ_DLL && !PIXYZ_RUNTIME)
        if (selected < 0) selected = 0;
        const float StartSpace = 10;
        int newSelected = selected;
        bool useProSkin = EditorGUIUtility.isProSkin;

        GUILayout.Space(StartSpace);
        Color storeColor = GUI.backgroundColor;
        Color highlightCol = getColor(ColorType.Highlight);
        Color activeCol = getColor(ColorType.Active);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.padding.bottom = 8;
        buttonStyle.normal = GUI.skin.box.normal;
        buttonStyle.normal.textColor = GUI.skin.button.normal.textColor;

        Rect[] optionRect = new Rect[options.Length];

        GUILayout.BeginHorizontal();
        {
            //Create a row of buttons
            var line = new Texture2D(1, 1);
            line.SetPixel(0, 0, useProSkin ? Color.white : Color.black);
            line.Apply();
            for (int i = 0; i < options.Length; ++i)
            {
                if (i == selected)
                    GUI.backgroundColor = activeCol;
                else
                    GUI.backgroundColor = highlightCol;
                if (GUILayout.Button(options[i], buttonStyle))
                {
                    newSelected = i; //Tab click
                    clicked = true;
                }
                Rect lastRect = GUILayoutUtility.GetLastRect();
                optionRect[i] = new Rect(lastRect);
                //right
                GUI.DrawTexture(new Rect(
                        optionRect[i].x - (selected == i ? 1 : 0),
                        optionRect[i].y,
                        1 + (selected == i ? 1 : 0),
                        optionRect[i].height
                    ),
                    line);
                //top
                GUI.DrawTexture(new Rect(
                        optionRect[i].x,
                        optionRect[i].y,
                        optionRect[i].width,
                        1 + (selected == i ? 1 : 0)),
                    line);
                //left
                GUI.DrawTexture(new Rect(
                        optionRect[i].x + optionRect[i].width - 1,
                        optionRect[i].y,
                        1 + (selected == i ? 1 : 0),
                        optionRect[i].height
                    ),
                    line);
            }
            //bottom line
            GUI.DrawTexture(new Rect(0, optionRect[0].y + optionRect[0].height /*buttonStyle.lineHeight + buttonStyle.border.top + buttonStyle.margin.top + StartSpace*/, Screen.width, 1), line);
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, useProSkin ? activeCol : highlightCol);
            texture.Apply();

            //remove bottom tab
            GUI.DrawTexture(new Rect(
                        optionRect[selected].x + 1,
                        optionRect[selected].y + optionRect[selected].height,
                        optionRect[selected].width - 2,
                        1
                    ),
                    texture);
            //vlean extra bottom lines
            //GUI.DrawTexture(new Rect(0, optionRect[0].y + optionRect[0].height + 1 /*buttonStyle.lineHeight + buttonStyle.border.top + buttonStyle.margin.top + StartSpace + 1*/, Screen.width, 4), texture);
        }
        GUILayout.EndHorizontal();
        //Restore color
        GUI.backgroundColor = storeColor;

        return newSelected;
#else
        return -1;
#endif
    }

    public static void endTabs()
    {
#if UNITY_EDITOR || (PIXYZ_DLL && !PIXYZ_RUNTIME)
        GUILayout.Label(GUIContent.none, GUILayout.Height(10));
        Rect lastRect = GUILayoutUtility.GetLastRect();
        Texture2D line = new Texture2D(1, 1);
        line.SetPixel(0, 0, EditorGUIUtility.isProSkin ? Color.white : Color.black);
        line.Apply();
        //bottom line
        GUI.DrawTexture(new Rect(0, lastRect.y, Screen.width, 1), line);
#endif
    }

    public static void gradientBox(Rect rectangle, Vector2 maskCenter)
    {
        Texture2D mask = new Texture2D((int)rectangle.width, (int)rectangle.height, TextureFormat.RGBA32, true);
        float color = 0f;

        for (int y = 0; y < mask.height; ++y)
        {
            for (int x = 0; x < mask.width; ++x)
            {

                float distFromCenter = Vector2.Distance(maskCenter, new Vector2((float)x / (float)mask.width, (float)y / (float)mask.height));
                float maskPixel = (0.5f - distFromCenter) * 0.5f;
                mask.SetPixel(x, y, new Color(color, color, color, maskPixel));
            }
        }
        mask.Apply();
        GUI.DrawTexture(rectangle, mask);
    }

    //static Color defaultColor = GUI.color;
    static string groupBoxTitle = "";
    static string groupBoxTooltip = "";
    public static void beginGroupBox(String title, bool condition = false, string tooltip = "")
    {
        EditorGUI.indentLevel++;
        GUIStyle myStyle = new GUIStyle(GUI.skin.label);
        myStyle.margin = new RectOffset(5, 5, 5, 5);
        groupBoxTitle = title;
        groupBoxTooltip = tooltip;
        EditorGUI.BeginDisabledGroup(condition);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(myStyle);
        GUILayout.Space(10);
    }

    public static void endGroupBox()
    {
        GUILayout.Space(10);
        EditorGUILayout.EndVertical();
        Rect rectangle = GUILayoutUtility.GetLastRect();
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
        drawGroupBox(rectangle, groupBoxTitle, groupBoxTooltip);
        EditorGUI.indentLevel--;
    }

    public static void drawGroupBox(Rect rectangle, string title, string tooltip = "")
    {
        bool useProSkin = EditorGUIUtility.isProSkin;
        Color highlightCol = getColor(ColorType.Highlight);
        Color activeCol = getColor(ColorType.Active);
        int borderWidth = 2;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Normal;
        style.normal.textColor = Color.Lerp(style.normal.textColor, useProSkin ? Color.black : Color.white, 0.3f);
        Vector2 size = style.CalcSize(new GUIContent(title));
        GUIDrawRect(rectangle, new Color(0f, 0f, 0f, 0f), Color.gray, borderWidth);

        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, useProSkin ? activeCol : highlightCol);
        texture.Apply();

        GUI.DrawTexture(new Rect(
                    rectangle.x + rectangle.width / 2 - size.x / 2 - 1 - (tooltip != "" ? 8 : 0),
                    rectangle.y,
                    size.x + 2 + (tooltip != "" ? 14 : 0),
                    borderWidth
                ),
                texture);

        GUI.Label(new Rect(
                    rectangle.x + rectangle.width / 2 - size.x / 2 - (tooltip != "" ? 8 : 0),
                    rectangle.y - size.y / 2,
                    size.x,
                    size.y
                ), new GUIContent(title), style);
        if (tooltip != "")
        {
            Texture2D icon = Resources.Load("icon/info") as Texture2D;
            GUIStyle helpStyle = new GUIStyle(GUI.skin.button);
            helpStyle.normal.background = icon;
            helpStyle.border = new RectOffset(0, 0, 0, 0);
            helpStyle.margin = new RectOffset(0, 0, 0, 0);
            helpStyle.overflow = new RectOffset(0, 0, 0, 0);
            GUI.Label(new Rect(
                        rectangle.x + rectangle.width / 2 + size.x / 2 - 6,
                        rectangle.y - 5,
                        10,
                        10
                    ), new GUIContent("", tooltip), helpStyle);
        }
    }
}
