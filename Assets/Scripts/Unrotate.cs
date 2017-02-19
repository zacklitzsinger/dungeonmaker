using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unrotate : MonoBehaviour {

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
