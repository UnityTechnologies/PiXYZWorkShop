using UnityEngine;
using PIXYZImportScript;

[System.Serializable]
public class PiXYZImportSettings : MonoBehaviour
{
    [SerializeField]
    public PiXYZSettings settings = new PiXYZSettings();
    public int windowId = 0;
    public bool isInspector = false;
}
