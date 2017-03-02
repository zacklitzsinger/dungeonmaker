using System;
using UnityEngine;

public class ObjectMasterList : MonoBehaviour {

    public static ObjectMasterList main;

    public GameObject[] options;

	void Awake () {
        main = this;
        Array.Sort(options, (a, b) => { return a.name.CompareTo(b.name); });
	}
}
