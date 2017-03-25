using System;
using UnityEngine;

public class ObjectMasterList : MonoBehaviour {

    public static ObjectMasterList main;

    public ObjectData[] options;

	void Awake () {
        main = this;
        Array.Sort(options, (a, b) => { return a.uiName.CompareTo(b.uiName); });
	}
}
