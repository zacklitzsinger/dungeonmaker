using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyUI : MonoBehaviour {

    private int amount;
    public int Amount
    {
        get { return amount; }
        set
        {
            amount = value;
            for (int i = transform.childCount; i < amount; i++)
                Instantiate(keyImage, transform);
            for (int i = transform.childCount; i > amount; i--)
                Destroy(keyImage);
        }
    }

    public GameObject keyImage;
}
