using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour {

    private int amount;
    public int Amount
    {
        get { return amount; }
        set
        {
            amount = value;
            for (int i = transform.childCount; i < amount; i++)
                Instantiate(healthImage, transform);
            for (int i = transform.childCount; i > amount; i--)
                Destroy(transform.GetChild(0).gameObject);
        }
    }

    public GameObject healthImage;
}
