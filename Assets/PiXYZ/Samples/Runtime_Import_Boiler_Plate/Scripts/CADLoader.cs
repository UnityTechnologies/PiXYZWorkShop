using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using PIXYZImportScript.AssemblyCSharp;
using PIXYZImportScript;
using System.Xml;

public class CADLoader : MonoBehaviour
{
  public GameObject sceneRoot;

  [HideInInspector]
    public string cadFileName = "";
    public List<string> multiFileName;
    public string tubFileName = "";
    public string cadFolderName = "";
    public int cadFolderStartIndex = -1;
    public int cadFolderStopIndex = -1;

    CoroutineScheduler coroutineScheduler;
  public CoroutineScheduler CoroutineScheduler { get { return coroutineScheduler; } }
  PiXYZ4UnityLoader loader = null;

  public delegate void ImportStartedHandler();
  public delegate void ImportEndedHandler();
  public delegate void ModelImportedHandler(int p_Index, GameObject p_Model);
  public event ImportStartedHandler ImportStarted;
  public event ImportEndedHandler ImportEnded;


  public string ProgressStatus { get { if (loader != null) return loader.progressStatus; else return ""; } }
  public float Progress { get { if (loader != null) return loader.progress; else return 0; } }
  private bool isImporting = false;
  public bool IsImporting { get { return isImporting; } }

  private float cadImportBeginTime = 0;
  private float cadImportTiming = 0;
  public float CadImportTiming { get { return cadImportTiming; } }

    public bool CheckLicense()
    {
      try
      {
        PiXYZ4UnityWrapper.initialize();
      }
      catch (Exception)
      {
        Debug.Log("License not found");
        return false;
        
      }

      PiXYZ4UnityWrapper.clear();
      return true;
    }

    bool CheckCADLoader()
  {

        if (isImporting)
    {
      Debug.Log("CAD Loader is already at work. Try later.");
      return false;
    }

    if (sceneRoot == null)
    {
      Debug.Log("CAD Loader root scene is not specified.");
      return false;
    }

    if (cadFileName.Length == 0 || !File.Exists(cadFileName))
    {
      Debug.Log("CAD Loader file path is not correct.");
      return false;
    }

    return true;
  }

  void Update()
  {
    if (coroutineScheduler != null && coroutineScheduler.HasCoroutines())
      coroutineScheduler.UpdateAllCoroutines(Time.frameCount, Time.time);
  }

    public void LoadCAD(bool p_IsLastOne, int p_Index, int meshQuality, float scale, bool zUp, bool rightHanded)
    {
        if (!CheckCADLoader())
            return;

        isImporting = true;
        if (p_Index <= 1)
            cadImportBeginTime = Time.realtimeSinceStartup;

        if (ImportStarted != null)
            ImportStarted();

        coroutineScheduler = ScriptableObject.CreateInstance<CoroutineScheduler>();
        loader = ScriptableObject.CreateInstance<PiXYZ4UnityLoader>();

        // LOAD CAD ===============
        coroutineScheduler.StartCoroutine(ImportModel(cadFileName, sceneRoot, p_IsLastOne, p_Index, meshQuality, scale, zUp, rightHanded));
    }

    Bounds getSize(GameObject obj)
    {
        MeshRenderer[] tab = obj.GetComponentsInChildren<MeshRenderer>();
        bool first = true;

        Bounds bounds = new Bounds();
        foreach (MeshRenderer child in tab)
        {
            if (first)
            {
                first = false;
                bounds = child.bounds;
            }
            else
                bounds.Encapsulate(child.bounds);
        }
        return bounds;
    }

  IEnumerator ImportModel(string filePath, GameObject parentGameObject, bool p_IsLastOne, int p_Index, int meshQuality, float scale, bool zUp, bool rightHanded)
  {
        if (!File.Exists(filePath))
        {
            Debug.Log("CAD FILE HAS BEEN DELETED  " + filePath);
        }


    GameObject modelGameObject = new GameObject(Path.GetFileNameWithoutExtension(filePath));

    modelGameObject.transform.parent = parentGameObject.transform;

  
    bool orient = true;
    float mapUV3dSize = 100.0f;
    int treeProcessType = 0;
 


    List<PiXYZLODSettings> lodSettingsList = new List<PiXYZLODSettings>();
    lodSettingsList.Add(GetDefaultLODSettings(meshQuality));


    loader.setSourceCoordinatesSystem(rightHanded, zUp, scale);
    loader.configure(orient, mapUV3dSize, (TreeProcessType)treeProcessType, LODsMode.NONE, lodSettingsList, true, true);

    yield return null;

    CoroutineNode _routine = coroutineScheduler.StartCoroutine(loader.loadFileRuntime(modelGameObject, filePath, false, null));
    yield return _routine;
    while (!_routine.finished) ;

    loader = null;

    //add Unity Colliders to every GO that has a mesh
    //CreateUnityColliders(modelGameObject);

    //set the model as static by default
    modelGameObject.isStatic = true;

    isImporting = false;
    cadImportTiming = Time.realtimeSinceStartup - cadImportBeginTime;

    Bounds tmp = getSize(parentGameObject.transform.GetChild(0).gameObject);
    modelGameObject.transform.localPosition = new Vector3(0 - tmp.center.x, (0.5f - tmp.center.y) - (tmp.size.y / 2), 0 - tmp.center.z);
        

    if (ImportEnded != null && p_IsLastOne == true)
        ImportEnded();
    Debug.Log("CAD loaded");

    coroutineScheduler.StopAllCoroutines();
    yield break;
  }


  void CreateUnityColliders(GameObject go)
  {
    // process node
    MeshFilter filter = go.GetComponent<MeshFilter>();
    MeshCollider collider = go.GetComponent<MeshCollider>();
    if (collider == null && filter != null)
    {
      if (filter.sharedMesh.GetTopology(0) == MeshTopology.Triangles)
      {
        if (filter.sharedMesh.vertexCount == 0)
          Debug.LogWarning("Mesh " + filter.sharedMesh.name + " has no vertice. Consider removing its MeshFilter or MeshRenderer.");
        else
          go.AddComponent<MeshCollider>();
      }
    }
    // process children
    foreach (Transform child in go.transform)
      CreateUnityColliders(child.gameObject);
  }


  PiXYZLODSettings GetDefaultLODSettings(int lodQuality)
  {
    PiXYZLODSettings lodSettings = new PiXYZLODSettings();

    lodSettings.index = -1;
    lodSettings.preset = (MeshQualityPresets)lodQuality;
    lodSettings.startLod = 0.01f;

    return lodSettings;
  }
}
