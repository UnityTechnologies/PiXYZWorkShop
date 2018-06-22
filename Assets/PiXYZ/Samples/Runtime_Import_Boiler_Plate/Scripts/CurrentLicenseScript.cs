using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PIXYZImportScript;

public class CurrentLicenseScript : MonoBehaviour
{
    public Text infoText;
    public Text detailText;

    // Use this for initialization
    void Start () {
        PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.initialize();
	}
	
	// Update is called once per frame
	void Update () {
		if(infoText != null)
        {
            if (PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.checkLicense())
            {
                detailText.text = "";
                infoText.text = "";
                infoText.color = Color.black;
                infoText.fontSize = 14;
                infoText.alignment = TextAnchor.MiddleLeft;
                string[] names;
                string[] values;
                if (PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.isFloatingLicense())
                {
                    string server; int port;
                    PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getLicenseServer(out server, out port);
                    names = new string[]{
                        "License",
                        "",
                        "Server address",
                        "Port"
                    };
                    values = new string[] {
                        "Floating",
                        "",
                        server,
                        port.ToString()
                    };
                }
                else
                {
                    names = new string[]{
                        "License version",
                        "Start date",
                        "End date",
                        "Company name",
                        "Name",
                        "E-mail"
                    };
                    values = new string[] {
                        PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getCurrentLicenseVersion(),
                        PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getCurrentLicenseStartDate(),
                        PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getCurrentLicenseEndDate().Length == 0 ? "Perpetual" : PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getCurrentLicenseEndDate(),
                        PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getCurrentLicenseCompany(),
                        PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getCurrentLicenseName(),
                        PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getCurrentLicenseEmail(),
                    };
                }

                for (int i = 0; i < names.Length; ++i)
                {
                    infoText.text += names[i] + (names[i].Length > 0 ? ":\n" : "\n");
                    detailText.text += values[i] + "\n";
                }
            }
            else
            {
                infoText.text = "Your license is inexistant or invalid.";
                infoText.color = Color.red;
                infoText.fontSize = 18;
                infoText.alignment = TextAnchor.MiddleCenter;
            }
        }
	}
}
