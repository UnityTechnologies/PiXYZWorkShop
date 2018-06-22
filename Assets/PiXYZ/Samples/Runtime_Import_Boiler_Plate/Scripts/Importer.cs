using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

public class Importer : MonoBehaviour {

    // Poppup && Canvas Obj
    public RectTransform Import_Popup;
    public RectTransform Menu_Bar;
    public RectTransform Loading_Popup;
    public RectTransform Error_Popup;
    public RectTransform m_ProgressBar;
    public RectTransform m_ProgressBarFull;
    public RectTransform finish_button;

    //Text field
    public Text Import_Info;
    public Text Import_Value;
    public Text tessel_text;
    public Text scale_text;
    public Toggle Z_UP;
    public Toggle Right_Handed;
    public Text Loading_text;
    public Dropdown MeshQualityDropdown;

    // Required
    public CADLoader cadLoader;
    public GameObject RootLoader;

    // Use this for initialization
    void Start()
    {
        cadLoader.sceneRoot = RootLoader;
        cadLoader.ImportEnded += CADImportEnded;
    }

    // Update is called once per frame
    void Update()
    {
        if (cadLoader != null && cadLoader.IsImporting)
        {
            setProgressBar(cadLoader.Progress);
        }
    }

    public void HideShowPopup()
    {
        bool state = false;
        if (Import_Popup != null && Import_Popup != null)
        {
            state = !Import_Popup.gameObject.activeSelf;
            Import_Popup.gameObject.SetActive(state);
            Menu_Bar.gameObject.SetActive(state);
        }
    }

    public void HideErrorPopup()
    {
        if (Error_Popup != null)
            Error_Popup.gameObject.SetActive(false);
    }

    public void HideLoadingPopup()
    {
        if (Error_Popup != null)
            Loading_Popup.gameObject.SetActive(false);
            Menu_Bar.gameObject.SetActive(false);
    }

    public void CADFileBrowse()
    {
        OpenFileDialog form = new OpenFileDialog();

        /// INITIAL LOCALISATION
        if (cadLoader.cadFileName.Length == 0)
            form.InitialDirectory = UnityEngine.Application.dataPath;
        else
            form.InitialDirectory = Path.GetDirectoryName(cadLoader.cadFileName);

        // FILTER
        //form.Filter = "CAD files (*.3dxml;*.igs;*.iges;*.CatProduct;*.CatPart;*.stp;*.step;*.sldasm;*.sldprt;*.fbx;*.ifc) | *.3dxml;*.igs;*.iges;*.CATProduct;*.CATPart;*.stp;*.step;*.sldasm;*.sldprt;*.fbx;*.ifc";

        // RESULT
        if (form.ShowDialog() == DialogResult.OK)
        {
            cadLoader.tubFileName = form.FileName;
            cadLoader.cadFileName = form.FileName;
            Import_Value.text = Path.GetFileNameWithoutExtension(cadLoader.cadFileName);
            Import_Info.enabled = false;
        }
    }
   

    public void CADFileImport_start()
    {
        string tesselation = "";
        string scale = "";
        bool zUp = true;
        bool rightHanded = true;
        int meshQuality = 1;

        if (tessel_text != null && tessel_text.text != "")
            tesselation = tessel_text.text;

        if (scale_text != null && scale_text.text != "")
            scale = scale_text.text;

        if (Z_UP != null)
        {
            zUp = Z_UP.isOn;
        }

        if (Right_Handed != null)
        {
                rightHanded = Right_Handed.isOn;
        }

        if (MeshQualityDropdown != null)
        {
            meshQuality = MeshQualityDropdown.value;
        }

        CADFileImport(meshQuality, scale, zUp, rightHanded);
    }

    public void CADFileImport(int meshQuality, string scale, bool zUp, bool rightHanded)
    {

        float my_scale = 0.001f;
        if (scale.Length > 0)
            my_scale = (float)Math.Abs(Convert.ToDouble(scale));


        if (cadLoader == null)
            return;

        Import_Popup.gameObject.SetActive(false);

        if (!cadLoader.CheckLicense())
        {
            Loading_Popup.gameObject.SetActive(true);
            Loading_text.text = "Invalid License";
            return;
        }


        if (cadLoader.tubFileName.Length == 0) // cadFileName
        {
            Debug.Log("Please specify a valid CAD file");
            Error_Popup.gameObject.SetActive(true);
            return;
        }

        Loading_Popup.gameObject.SetActive(true);
        Loading_text.text = "Loading 3D Model : " + cadLoader.tubFileName;
        m_ProgressBar.gameObject.SetActive(true);
        m_ProgressBarFull.gameObject.SetActive(true);
        finish_button.gameObject.SetActive(false);


        Debug.Log("Loading ...");

        cadLoader.cadFileName = cadLoader.tubFileName;
        cadLoader.LoadCAD(true, -1, meshQuality, my_scale, zUp, rightHanded);
    }

    private void CADImportEnded()
    {
        Loading_text.text = "Loading complete ! (in " + (int)(cadLoader.CadImportTiming) + " seconds)\n" + Path.GetFileName(cadLoader.cadFileName);
        m_ProgressBar.gameObject.SetActive(false);
        m_ProgressBarFull.gameObject.SetActive(false);
        finish_button.gameObject.SetActive(true);
    }
	
    void setProgressBar(float progress)
    {
        Vector3 scale = m_ProgressBar.localScale;
        scale.x = progress;
        m_ProgressBar.localScale = scale;
    }

}
