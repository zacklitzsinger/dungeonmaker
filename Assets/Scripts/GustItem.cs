using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GustItem : MonoBehaviour {

    public GameObject prefabGust;
    public int cooldownFrames = 90;
    [ReadOnly]
    public int remFrames = 0;
    public float force = 1200f;

    void Start()
    {
        prefabGust = Resources.Load<GameObject>("Gust");
    }

	// Update is called once per frame
	void FixedUpdate () {
        if (remFrames > 0)
        {
            remFrames--;
            return;
        }
        if (Input.GetButtonDown("Use item"))
            Activate(gameObject);
	}

    void Activate(GameObject gameObject)
    {
        if (remFrames > 0)
            return;
        remFrames = cooldownFrames;
        Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position).normalized;
        GameObject gust = Instantiate(prefabGust, (Vector2)transform.position + direction * 2, Quaternion.LookRotation(Vector3.forward, direction));
        gust.GetComponent<Rigidbody2D>().AddForce(direction * force);
        gameObject.GetComponent<Rigidbody2D>().AddForce(-direction * force);
    }
}
