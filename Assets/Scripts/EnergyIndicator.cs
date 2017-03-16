using UnityEngine;
using UnityEngine.UI;

public class EnergyIndicator : MonoBehaviour {

    public float percentage = 0;
    public float limitPercentage = 1;
    public Image foreground;
    public Image limit;
	
	// Update is called once per frame
	void Update () {
        foreground.fillAmount = percentage;
        limit.fillAmount = 1 - limitPercentage;
	}
}
