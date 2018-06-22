using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorWinScript : MonoBehaviour {

    public Text errorString;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void popWithText(string text)
    {
        errorString.text = text;
        gameObject.SetActive(true);
    }
}
