using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineUI : MonoBehaviour
{
    public RectTransform onlineCreds;
    public RectTransform onlineWin;

    public InputField username;
    public InputField password;

    public Text LicenseTitles;
    public Text LicenseDetails;

    public Dropdown licenseList;

    public Button installBtn;

    public ErrorWinScript errorWindow;

    private bool connected = false;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        onlineCreds.gameObject.SetActive(!connected);
        onlineWin.gameObject.SetActive(connected);
	}

    public void connect()
    {
        connected = username.text.Length != 0 && password.text.Length != 0 && PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.connectToLicenseServer(username.text, password.text);
        if(connected)
            populateLicenseList();
    }

    public void refresh()
    {
        connected = PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.connectToLicenseServer(username.text, password.text);
        if (connected)
            populateLicenseList();
        else
            errorWindow.popWithText("Credentials error.");
    }

    private void populateLicenseList()
    {
        int index = licenseList.value;
        licenseList.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.availableLicensesCount(); ++i)
        {
            string option = "License " + (i + 1) + ": " + PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.licenseProduct(i) + "  [" + PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.licenseValidity(i) + "]";
            if (PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.licenseOnMachine(i))
                option += "  (installed)";
            options.Add(option);
        }
        licenseList.AddOptions(options);
        switchLicense(index);
    }

    public void switchLicense(int index)
    {
        int usedIndex = index;
        int daysRemaining = Math.Max(0, (Convert.ToDateTime(PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.licenseValidity(usedIndex)) - DateTime.Now).Days + 1);
        string remainingTextColor = daysRemaining > 185 ? "green" : daysRemaining > 92 ? "orange" : "red";
        bool installed = PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.licenseOnMachine(usedIndex);
        string productName = PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.licenseProduct(usedIndex);
        string validity = PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.licenseValidity(usedIndex)
            + "   (<color='" + remainingTextColor + "'><b>" + daysRemaining + "</b> Day" + (daysRemaining > 1 ? "s" : "") + " remaining</color>)";
        string licenseUse = "" + PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.licenseInUse(usedIndex) + " / " + PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.licenseCount(usedIndex);
        string currentlyInstalled = installed ? "<color='green'>true</color>" : "false";

        LicenseTitles.text = "Product name: \n";
        LicenseTitles.text += "Validity: \n";
        LicenseTitles.text += "License use: \n";
        LicenseTitles.text += "Currently installed: \n";

        LicenseDetails.text = productName + "\n";
        LicenseDetails.text += validity + "\n";
        LicenseDetails.text += licenseUse + "\n";
        LicenseDetails.text += currentlyInstalled + "\n";

        installBtn.gameObject.SetActive(!installed);
    }

    public void installLicense()
    {
        if(!PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.requestLicense(username.text, password.text, licenseList.value))
            errorWindow.popWithText("An error occured while installing the license: \n" + PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getLastError());
    }

    public void releaseLicense()
    {
        if (!PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.releaseLicense(username.text, password.text, licenseList.value))
        {
            errorWindow.popWithText("An error has occured while releasing the license: \n" + PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getLastError());
            PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.requestLicense(username.text, password.text, licenseList.value);
        }
    }
}
