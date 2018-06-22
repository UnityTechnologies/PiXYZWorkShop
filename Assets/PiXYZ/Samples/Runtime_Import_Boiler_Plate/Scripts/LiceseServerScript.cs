using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LiceseServerScript : MonoBehaviour {

    public InputField address;
    public InputField port;

    public ErrorWinScript errorWindow;

    public UnityEvent onSuccess;

    // Use this for initialization
    void Start () {
		
	}

    private void OnEnable()
    {
        string savedAddress;
        int savedPort;
        PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getLicenseServer(out savedAddress, out savedPort);
        if (address.text == "") address.text = savedAddress;
        if (port.text == "") port.text = savedPort.ToString();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void apply()
    {
        if (!PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.configureLicenseServer(address.text, int.Parse(port.text)))
            errorWindow.popWithText(PIXYZImportScript.AssemblyCSharp.PiXYZ4UnityWrapper.getLastError());
        else
            onSuccess.Invoke();
    }
}
