using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.PiXYZ.Editor
{
    class PiXYZLODSlider
    {
        float currentValue = 0.5f;
        int selectedLOD = 0;
        int grabbing = -1;
        GUIStyle sliderStyle;
        GUIStyle thumbStyle;
        Color[] normalColors = {
                    new Color ( 0.235f, 0.274f, 0.105f, 1f ),     //
                    new Color ( 0.180f, 0.216f, 0.263f, 1f ),     //
                    new Color ( 0.157f, 0.251f, 0.282f, 1f ),     //
                    new Color ( 0.251f, 0.145f, 0.106f, 1f ),     //
                    new Color ( 0.208f, 0.180f, 0.247f, 1f ),     //
                    new Color ( 0.314f, 0f, 0f, 1f ),     //
                    };
        Color[] highlightColors = {
                    new Color ( 0.380f, 0.490f, 0.019f, 1f ),     //
                    new Color ( 0.219f, 0.322f, 0.458f, 1f ),     //
                    new Color ( 0.165f, 0.419f, 0.517f, 1f ),     //
                    new Color ( 0.420f, 0.125f, 0.024f, 1f ),     //
                    new Color ( 0.302f, 0.227f, 0.416f, 1f ),     //
                    new Color ( 0.243f, 0.373f, 0.588f, 1f ),     //
                    };

        public void show(SerializedObject serializedObject, GameObject gameObject=null)
        {
            GUIStyle sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            GUIStyle thumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
            Texture2D tex = new Texture2D(2, 300);
            var fillColorArray = tex.GetPixels();

            for (var i = 0; i < fillColorArray.Length; ++i)
            {
                fillColorArray[i] = Color.black;
            }

            tex.SetPixels(fillColorArray);

            tex.Apply();
            sliderStyle.normal.background = null;
            thumbStyle.fixedWidth = 1f;
            thumbStyle.fixedHeight = 1f;
            thumbStyle.normal.background = null;

            int maxLod = serializedObject.FindProperty("settings").FindPropertyRelative("lodSettingCount").intValue;
            Rect sliderRect = EditorGUILayout.GetControlRect();
            sliderRect.height = 30;
            Rect labelRect = new Rect(sliderRect);
            float lodValue = 1f;
            float nextLodValue = PiXYZLoDSettingsEditor.getIndexProperty(0, serializedObject, "startLod", "settings.lodSettings").floatValue;
            float start = 0;
            for (int i = 0; i < maxLod; i++)
            {
                labelRect.width = sliderRect.width * (lodValue - nextLodValue);
                labelRect.position = new Vector2(sliderRect.position.x + start, sliderRect.position.y);
                if (Event.current.type == EventType.MouseUp && nextLodValue < currentValue && currentValue < lodValue)
                    selectedLOD = i;
                if (i < maxLod)
                    PiXYZUtils.GUIDrawRect(labelRect,
                                    selectedLOD == i ?
                                    highlightColors[i] : normalColors[i],
                                    highlightColors[highlightColors.Length-1],
                                    selectedLOD == i ? 2 : 0,
                                    " LOD " + i + "\n " + (Math.Round(lodValue * 100)) + "%",
                                    TextAnchor.MiddleLeft);
                Rect movePos = new Rect(labelRect.x + labelRect.width - 5, labelRect.y, 10, labelRect.height);
                EditorGUIUtility.AddCursorRect(movePos, MouseCursor.ResizeHorizontal);
                if (movePos.Contains(Event.current.mousePosition) && (Event.current.type == EventType.MouseDown && Event.current.button == 0))
                    grabbing = i;
                else if (grabbing!= -1 && Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    if (gameObject != null)
                    {
                        foreach (LODGroup lodGroup in gameObject.GetComponentsInChildren<LODGroup>())
                        {
                            LOD[] lods = lodGroup.GetLODs();
                            lods[grabbing].screenRelativeTransitionHeight = PiXYZLoDSettingsEditor.getIndexProperty(grabbing, serializedObject, "startLod", "settings.lodSettings").floatValue;
                            lodGroup.SetLODs(lods);
                        }
                    }
                    grabbing = -1;
                }
                lodValue = nextLodValue;
                nextLodValue = i < maxLod - 1 ? PiXYZLoDSettingsEditor.getIndexProperty(i + 1, serializedObject, "startLod", "settings.lodSettings").floatValue : -1;
                start += labelRect.width;
            }
            labelRect.width = sliderRect.width-start;
            labelRect.position = new Vector2(sliderRect.position.x + start, sliderRect.position.y);
            PiXYZUtils.GUIDrawRect(labelRect,
                                    normalColors[normalColors.Length - 1],
                                    highlightColors[highlightColors.Length - 1],
                                    0,
                                    " Culled\n " + (Math.Round(lodValue * 100)) + "%",
                                    TextAnchor.MiddleLeft);
            currentValue = GUI.Slider(sliderRect, currentValue, 0, 1, 0, sliderStyle, thumbStyle, true, 0);
            if (grabbing != -1)
            {
                if (grabbing == 2 && currentValue < 0.01f)
                    return;
                else if(grabbing==0 && currentValue>0.99f)
                    return;
                else if (grabbing > 0 && currentValue > PiXYZLoDSettingsEditor.getIndexProperty(grabbing - 1, serializedObject, "startLod", "settings.lodSettings").floatValue - 0.01f)
                    return;
                else if (grabbing < maxLod - 1 && currentValue < PiXYZLoDSettingsEditor.getIndexProperty(grabbing + 1, serializedObject, "startLod", "settings.lodSettings").floatValue + 0.01f)
                    return;
                PiXYZLoDSettingsEditor.getIndexProperty(grabbing, serializedObject, "startLod", "settings.lodSettings").floatValue = currentValue<=0f?1f: currentValue;
            }
        }
    }
}
