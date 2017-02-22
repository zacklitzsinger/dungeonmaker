using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour {

    public float percentage = 0;
    public Image foreground;
	
	// Update is called once per frame
	void Update () {
        foreground.fillAmount = percentage;
	}
}
