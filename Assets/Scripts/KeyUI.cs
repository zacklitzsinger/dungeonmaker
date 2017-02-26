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
                Destroy(transform.GetChild(0).gameObject);
        }
    }

    public GameObject keyImage;
}
