using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour {

    public int remainingFrames;

	// Use this for initialization
	void Start () {
		
	}
	
	void Update () {
        if (remainingFrames > 0)
            remainingFrames--;
        else
            Destroy(gameObject);
	}
}
