using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gust : MonoBehaviour {

    public int aliveFrames;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.GetComponentInParent<ObjectData>().type == ObjectType.Wall)
            GetComponent<Wind>().size = 0;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (aliveFrames-- <= 0)
            Destroy(gameObject);
	}
}
