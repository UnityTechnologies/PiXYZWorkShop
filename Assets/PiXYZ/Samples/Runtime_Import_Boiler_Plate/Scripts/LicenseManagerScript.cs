using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using PIXYZImportScript;

public class LicenseManagerScript : MonoBehaviour {

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void GenerateActivationCode()
    {
        SaveFileDialog save = new SaveFileDialog();
        save.InitialDirectory = UnityEngine.Application.dataPath;
        save.FileName = "PiXYZ_activationCode.bin";
        save.Filter = "Binary file | *.bin";
        if (save.ShowDialog() == DialogResult.OK)
        {
            PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.generateActivationCode(save.FileName);
        }
    }

    public void InstalLicense()
    {
        OpenFileDialog open = new OpenFileDialog();
        open.InitialDirectory = UnityEngine.Application.dataPath;
        open.Filter = "Binary file|*.bin|License file|*.lic";
        if (open.ShowDialog() == DialogResult.OK)
        {
            string path = open.FileName;
            if (path.ToLower().EndsWith(".bin") || path.ToLower().EndsWith(".lic"))
            {
                PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.installActivationCode(path);
            }
        }
    }

    public void GenerateReleaseCode()
    {
        SaveFileDialog save = new SaveFileDialog();
        save.InitialDirectory = UnityEngine.Application.dataPath;
        save.FileName = "PiXYZ_activationCode.bin";
        save.Filter = "Binary file | *.bin";
        if (save.ShowDialog() == DialogResult.OK)
        {
            PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.generateDeactivationCode(save.FileName);
        }
    }
}
