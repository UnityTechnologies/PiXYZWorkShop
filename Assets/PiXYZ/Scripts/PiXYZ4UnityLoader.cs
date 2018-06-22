using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using PIXYZImportScript.AssemblyCSharp;
using PIXYZImportScript;

public class PiXYZ4UnityLoader : ScriptableObject
{
    private int m_PartsCount;
    private int m_PolyCount;
    private int m_PartsToLoadCount;

    List<UnityEngine.Object> m_loadedObject = new List<UnityEngine.Object>();

    public string getErrorMessage()
    {
        return errorMsg;
    }
    private float _scale;
    private bool _mirrorX = false;
    private Quaternion _rotation = new Quaternion();

    private object _handle = new object();
    private LODsMode _lodsMode = LODsMode.NONE;

    private volatile string _errorMsg = "";
    public volatile float _progress;
    public volatile string _progressStatus;
    public GameObject lastImportedObject;
    private Dictionary<uint, Mesh> _meshes = new Dictionary<uint, Mesh>();
    private Dictionary<uint, List<Material>> _meshesMaterials = new Dictionary<uint, List<Material>>();
    private string errorMsg
    {
        get
        {
            string tmp;
            lock (_handle)
            {
                tmp = _errorMsg;
            }
            return tmp;
        }
        set
        {
            lock (_handle)
            {
                _errorMsg = value;
            }
        }
    }
    public float progress
    {
        get
        {
            float tmp;
            lock (_handle)
            {
                tmp = _progress;
            }
            return tmp;
        }
        set
        {
            lock (_handle)
            {
                _progress = value;
            }
        }
    }
    public string progressStatus
    {
        get
        {
            string tmp;
            lock (_handle)
            {
                tmp = _progressStatus;
            }
            return tmp;
        }
        set
        {
            lock (_handle)
            {
                _progressStatus = value;
            }
        }
    }

    public List<UnityEngine.Object> loadedObject
    {
        get
        {
            List<UnityEngine.Object> tmp;
            lock (_handle)
            {
                tmp = m_loadedObject;
            }
            return tmp;
        }
        set
        {
            lock (_handle)
            {
                m_loadedObject = value;
            }
        }
    }

    public void setSourceCoordinatesSystem(bool rightHanded, bool zUp, float scaleFactor)
    {
        _scale = scaleFactor;
        //PiXYZ4UnityWrapper.configureScale(scaleFactor);
        _mirrorX = rightHanded;

        if (zUp)
        {
            _rotation.eulerAngles = new Vector3(-90, 0, 0);
        }
    }

    public void configure(bool orient, double mapUV3dSize, TreeProcessType treeProcess, LODsMode lodsMode, List<PiXYZLODSettings> lods, bool support32BytesIndex, bool useMergeFinalAssemblies)
    {
        _lodsMode = lodsMode;
        PiXYZ4UnityWrapper.configure(orient, mapUV3dSize, treeProcess, lodsMode, lods, support32BytesIndex, useMergeFinalAssemblies);
    }

    public bool loadFile(string filePath)
    {
        return loadFile(filePath, false, null);
    }

    private void importThread(string filePath, out int assembly)
    {
        errorMsg = "";
        progress = 0;
        progressStatus = "Initializing PiXYZ";
        try
        {
            PiXYZ4UnityWrapper.initialize();

            progressStatus = "Importing file in PiXYZ";
            progress += 0.05f;
            m_PartsToLoadCount = PiXYZ4UnityWrapper.importFile(filePath, out assembly);
        }
        catch (Exception e)
        {
            assembly = -1;
            errorMsg = e.Message;
            return;
        }
    }

    public IEnumerator loadFileRuntime(GameObject rootObject, string filePath, bool editor, UnityEngine.Object prefab)
    {
        if (editor)
            loadedObject.Clear();

        int assembly = -1;
        PiXYZ4UnityWrapper.setResourcesFolder(Application.dataPath + "/PiXYZ/Resources/");
        Thread _thread = new Thread(() => importThread(filePath, out assembly));

        m_PartsCount = 0;
        m_PolyCount = 0;

        _thread.Start();

        while (_thread.IsAlive)
        {
            progress = progress >= 0.05f ? 0.05f + (float)(PiXYZ4UnityWrapper.getProgress() * 0.45f) : progress;
            progressStatus = PiXYZ4UnityWrapper.getProgressStatus();
            yield return null;
        }

        if (getErrorMessage() != "")
        {
            yield break;
        }

        materials = new Dictionary<int, Material>();

        PiXYZ4UnityWrapper.ScenePath root = null;
        if (assembly > 0)
        {
            int[] path = new int[2];
            path[0] = PiXYZ4UnityWrapper.getSceneRoot();
            path[1] = assembly;
            root = new PiXYZ4UnityWrapper.ScenePath(path);
        }
        else
        {
            root = new PiXYZ4UnityWrapper.ScenePath(PiXYZ4UnityWrapper.getSceneRoot());
        }

        //PiXYZ4UnityWrapper.removeSymmetryMatrices(root.node());  //used to fix odd negative scale
        Dictionary<int, List<Renderer>> renderers = new Dictionary<int, List<Renderer>>();
        Dictionary<int, double> lodToThreshold = new Dictionary<int, double>();

        loadSubTree(null, root, 0, editor, prefab, false, -1, ref renderers, ref lodToThreshold);
        
        foreach (KeyValuePair<int, Material> kvpair in PiXYZ4UnityWrapper.getCreatedMaterials())
            if (!materials.ContainsKey(kvpair.Key))
                materials.Add(kvpair.Key, kvpair.Value);
        PiXYZ4UnityWrapper.clear();
    }


    public bool loadFile(string filePath, bool editor, UnityEngine.Object prefab)
    {
        if (editor)
            loadedObject.Clear();

        m_PartsCount = 0;
        m_PolyCount = 0;

        progressStatus = "Initializing PiXYZ";
        progress = 0.0f;

        try
        {
            errorMsg = "";
            PiXYZ4UnityWrapper.setResourcesFolder(Application.dataPath + "\\PiXYZ\\Resources\\");
            PiXYZ4UnityWrapper.initialize();

            progressStatus = "Importing file in PiXYZ";
            progress += 0.05f;
            int assembly = -1;
            m_PartsToLoadCount = PiXYZ4UnityWrapper.importFile(filePath, out assembly);

            progress = 0.5f;

            materials = new Dictionary<int, Material>();

            PiXYZ4UnityWrapper.ScenePath root = null;
            if (assembly > 0)
            {
                int[] path = new int[2];
                path[0] = PiXYZ4UnityWrapper.getSceneRoot();
                path[1] = assembly;
                root = new PiXYZ4UnityWrapper.ScenePath(path);
            }
            else
            {
                root = new PiXYZ4UnityWrapper.ScenePath(PiXYZ4UnityWrapper.getSceneRoot());
            }
            progressStatus = "Importing into Unity";

            //PiXYZ4UnityWrapper.removeSymmetryMatrices(root.node());  //used to fix odd negative scale
            Dictionary<int, List<Renderer>> renderers = new Dictionary<int, List<Renderer>>();
            Dictionary<int, double> lodToThreshold = new Dictionary<int, double>();
            loadSubTree(null, root, 0, editor, prefab, false, -1, ref renderers, ref lodToThreshold);

            progress = 1.0f;
            progressStatus = "Finalizing";
        }
        catch (Exception e)
        {
            PiXYZ4UnityWrapper.clear();
            Debug.LogException(e);
            return false;
        }

        PiXYZ4UnityWrapper.clear();
        return true;
    }

    private Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 translate;
        translate.x = matrix.m03;
        translate.y = matrix.m13;
        translate.z = matrix.m23;
        return translate;
    }

    private Quaternion QuaternionFromMatrix(ref Matrix4x4 matrix)
    {
        float s = 0.0f;
        float[] q = new float[4];
        q[0] = q[1] = q[2] = q[3] = 0;
        float trace = matrix.m00 + matrix.m11 + matrix.m22;
        if (trace > 0.000001f)
        {
            s = (float)Math.Sqrt(trace + 1.0);
            q[3] = s * 0.5f;
            s = 0.5f / s;
            q[0] = (matrix.m21 - matrix.m12) * s;
            q[1] = (matrix.m02 - matrix.m20) * s;
            q[2] = (matrix.m10 - matrix.m01) * s;
        }
        else
        {
            int i = 0, j = 0, k = 0;
            if (matrix.m11 > matrix.m00)
                i = 1;
            if (matrix.m22 > matrix[i, i])
                i = 2;
            j = (i + 1) % 3;
            k = (j + 1) % 3;
            s = (float)Math.Sqrt(matrix[i, i] - (matrix[j, j] + matrix[k, k]) + 1.0);
            q[i] = s * 0.5f;
            s = 0.5f / s;
            q[3] = (matrix[k, j] - matrix[j, k]) * s;
            q[j] = (matrix[j, i] + matrix[i, j]) * s;
            q[k] = (matrix[k, i] + matrix[i, k]) * s;
        }
        return new Quaternion(q[0], q[1], q[2], q[3]);
    }

    private Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix, Vector3 scale)
    {
        Matrix4x4 scale_m = Matrix4x4.identity;
        scale_m.m00 = scale.x;
        scale_m.m11 = scale.y;
        scale_m.m22 = scale.z;

        Matrix4x4 copy = matrix;
        copy.m03 = 0.0f;
        copy.m13 = 0.0f;
        copy.m23 = 0.0f;

        Matrix4x4 rotation = copy * scale_m.inverse;
        return QuaternionFromMatrix(ref rotation);
    }

    private float ExtractDeterminant(ref Matrix4x4 matrix)
    {
        float det = matrix.m00 * (matrix.m11 * matrix.m22 - matrix.m12 * matrix.m21) -
                    matrix.m10 * (matrix.m01 * matrix.m22 - matrix.m02 * matrix.m21) +
                    matrix.m20 * (matrix.m01 * matrix.m12 - matrix.m02 * matrix.m11);
        return det;
    }

    private Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = matrix.MultiplyVector(new Vector3(1, 0, 0)).magnitude;
        scale.y = matrix.MultiplyVector(new Vector3(0, 1, 0)).magnitude;
        scale.z = matrix.MultiplyVector(new Vector3(0, 0, 1)).magnitude;

        if (ExtractDeterminant(ref matrix) < 0)
            scale = -scale;

        return scale;
    }

    private void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
    {
        transform.localScale = ExtractScaleFromMatrix(ref matrix);
        transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
        transform.localRotation = ExtractRotationFromMatrix(ref matrix, transform.localScale);
    }

    bool isPart(int node)
    {
        if (PiXYZ4UnityWrapper.getSceneNodeType(node) != PiXYZ4UnityWrapper.SceneNodeType.COMPONENT)
            return false;
        return (PiXYZ4UnityWrapper.getComponentType(node) == PiXYZ4UnityWrapper.ComponentType.PART);
    }

    //!isPart && !isAssembly => Light or camera
    bool isAssembly(int node)
    {
        if (PiXYZ4UnityWrapper.getSceneNodeType(node) != PiXYZ4UnityWrapper.SceneNodeType.COMPONENT)
            return true;
        return PiXYZ4UnityWrapper.getComponentType(node) == PiXYZ4UnityWrapper.ComponentType.ASSEMBLY;
    }

    //HOOPS workaround
    bool isHidden(PiXYZ4UnityWrapper.ScenePath subTree)
    {
        return PiXYZ4UnityWrapper.isHidden(subTree);
    }

    void loadSubTree(GameObject parent, PiXYZ4UnityWrapper.ScenePath subTree, int currentMateriaId, bool editor, UnityEngine.Object prefab, bool importLines, int curLOD, ref Dictionary<int, List<Renderer>> lodToRenderers, ref Dictionary<int, double> lodToThreshold, bool isMaterialOverride = false)
    {
        if (!isPart(subTree.node()) && !isAssembly(subTree.node()) || isHidden(subTree))  //Last part is HOOPS workaround.
            return; //no light or camera
        GameObject obj = new GameObject();

        Transform parentTransform = parent != null ? parent.transform : null;
        obj.name = PiXYZ4UnityWrapper.getNodeName(subTree.node());
        obj.transform.parent = parentTransform;
        Matrix4x4 matrix = PiXYZ4UnityWrapper.getLocalMatrix(subTree.node());

        int parentInstance = subTree.parentInstance();
        if (parentInstance > 0)
        {
            Matrix4x4 parentMatrix = PiXYZ4UnityWrapper.getLocalMatrix(parentInstance);
            matrix = parentMatrix * matrix;
        }
        SetTransformFromMatrix(obj.transform, ref matrix);
        if (parent == null)  //apply mirror and rotation once
        {
            lastImportedObject = obj;
            obj.transform.localScale *= _scale;
            if (_mirrorX)
                obj.transform.localScale = new Vector3(-obj.transform.localScale.x, obj.transform.localScale.y, obj.transform.localScale.z);
            if (_rotation.eulerAngles.sqrMagnitude > 0)
                obj.transform.localRotation = _rotation;
        }

        if (!isMaterialOverride || isAssembly(subTree.node()) || currentMateriaId == 0)
        {
            int materialId = PiXYZ4UnityWrapper.getOccurrenceMaterial(subTree);
            if (materialId > 0)
                currentMateriaId = materialId;
        }

        int lodCount = 0;
        bool isLodGroup = PiXYZ4UnityWrapper.isLODGroup(subTree.node(), out lodCount);
        if (_lodsMode != LODsMode.NONE)
        {
            if (isLodGroup)
            {
                lodToRenderers.Clear();
                lodToThreshold.Clear();
            }

            if (PiXYZ4UnityWrapper.isLOD(subTree.node()))
            {
                double threshold;
                curLOD = PiXYZ4UnityWrapper.getLODNumber(subTree.node(), out threshold);
                if (!lodToThreshold.ContainsKey(curLOD))
                    lodToThreshold.Add(curLOD, threshold);
            }
        }
        if (isPart(subTree.node()))
        {
            loadPart(obj, subTree.node(), currentMateriaId, editor, prefab, importLines, curLOD, ref lodToRenderers);
        }
        else if (isAssembly(subTree.node()))
        {
            foreach (PiXYZ4UnityWrapper.ScenePath child in PiXYZ4UnityWrapper.getChildren(subTree))
            {
                try
                {
                    loadSubTree(obj, child, currentMateriaId, editor, prefab, importLines, curLOD, ref lodToRenderers, ref lodToThreshold, isMaterialOverride || isAssembly(subTree.node()));
                }
                catch (KeyNotFoundException knf)
                {
                    Debug.LogError("Key not found: " + knf.ToString());
                }
            }
        }

        if (_lodsMode != LODsMode.NONE && isLodGroup)
        {
            LOD[] lods = new LOD[lodCount];
            const double e = 0.00001;
            double minThreshold = 1;
            for (int iLOD = 0; iLOD < lodCount; ++iLOD)
            {
                double threshold = Math.Min(lodToThreshold[iLOD], minThreshold);
                if (!lodToRenderers.ContainsKey(iLOD))
                {
                    Debug.LogError("Key '" + iLOD + "' not found.");
                    continue;
                }
                Renderer[] renderers = new Renderer[lodToRenderers[iLOD].Count];
                for (int iRenderer = 0; iRenderer < lodToRenderers[iLOD].Count; ++iRenderer)
                {
                    renderers[iRenderer] = lodToRenderers[iLOD][iRenderer];
                }
                lods.SetValue(new LOD((float)threshold, renderers), iLOD);
                minThreshold = threshold - e;
            }
            LODGroup lodGroup = obj.AddComponent<LODGroup>();
            lodGroup.SetLODs(lods);
            lodToRenderers.Clear();
            lodToThreshold.Clear();
        }
    }

    private Material lineMaterial;

    void createLineMaterial(bool editor, UnityEngine.Object prefab)
    {
        if (!lineMaterial)
        {
            Material mat = (Material)Resources.Load("PatchLineMaterial", typeof(Material));
            lineMaterial = UnityEngine.Object.Instantiate(mat);
            if (editor)
                loadedObject.Add(lineMaterial);
        }
    }

    public Dictionary<int, Material> materials = new Dictionary<int, Material>();

    Material getMaterial(int id, bool editor, UnityEngine.Object prefab)
    {
        Material material;
        if (materials.TryGetValue(id, out material))
            return material;

        bool cloned;
        material = PiXYZ4UnityWrapper.getMaterial(id, out cloned);

        materials.Add(id, material);
        //AssetDatabase.AddObjectToAsset(material, prefab);
        return material;
    }

    void loadPart(GameObject obj, int part, int currentMaterialId, bool editor, UnityEngine.Object prefab, bool importLines, int curLOD, ref Dictionary<int, List<Renderer>> lodToRenderers)
    {
        uint tessRep = (uint)PiXYZ4UnityWrapper.getPartActiveShape(part);
        if (tessRep < 0)
            return;
        int tessellation = PiXYZ4UnityWrapper.getTessellatedShapeTessellation(tessRep);
        if (tessellation < 0)
        {
            errorMsg = "No tessellation found for part : " + part + "(tessellation id : " + tessellation + ")";
            return;
        }

        ++m_PartsCount;
        progress = 0.5f + ((float)m_PartsCount / (float)m_PartsToLoadCount) * 0.5f;
        progressStatus = "Loading part " + m_PartsCount + "/" + m_PartsToLoadCount;

        obj.name = PiXYZ4UnityWrapper.getNodeName(part);
        if (obj.name.Length == 0)
            obj.name = "Mesh";

        MeshRenderer renderer = null;
        List<Material> materials = null;
        Material material = null;
        Mesh mesh;
        bool cloned = false;
        try
        {
            if (currentMaterialId != 0)
                material = getMaterial(currentMaterialId, editor, prefab);
            if (_meshes.TryGetValue((uint)tessellation, out mesh))
            {
                cloned = true;
                _meshesMaterials.TryGetValue((uint)tessellation, out materials);
            }
            else
            {
                mesh = PiXYZ4UnityWrapper.getTriangleTessellation((uint)tessellation, material == null, out materials);
                _meshes.Add((uint)tessellation, mesh);
                _meshesMaterials.Add((uint)tessellation, materials);
            }

            if (mesh != null)
            {
                mesh.name = tessRep.ToString() + "_" + tessellation.ToString();
                renderer = obj.AddComponent<MeshRenderer>();
                m_PolyCount += mesh.triangles.Length;
                if (editor)
                {
                    if (materials != null)
                        renderer.sharedMaterials = materials.ToArray();
                    else
                        renderer.sharedMaterial = material;
                }
                else
                {
                    if (materials != null)
                        renderer.materials = materials.ToArray();
                    else
                        renderer.material = material;
                }
            }

            if (renderer)
            {
                MeshFilter filter = obj.AddComponent<MeshFilter>();
                if (editor)
                {
                    if (!cloned)
                        loadedObject.Add(mesh);
                    filter.sharedMesh = mesh;
                }
                else
                {
                    filter.mesh = mesh;
                }

                PiXYZ4UnityWrapper.afterPartUsed(part);
            }
        }
        catch (Exception e)
        {
            string msg = "Load part failed on part \"" + part.ToString() + "\": " + e.Message;
            Debug.LogError(msg);
        }

        if (curLOD >= 0 && (lodToRenderers != null))
        {
            if (!lodToRenderers.ContainsKey(curLOD))
            {
                lodToRenderers.Add(curLOD, new List<Renderer>());
            }
            lodToRenderers[curLOD].Add(renderer);
        }
    }

    public void GetCounts(out int p_PartsCount, out int p_PolyCount)
    {
        p_PartsCount = m_PartsCount;
        p_PolyCount = m_PolyCount;
    }
}