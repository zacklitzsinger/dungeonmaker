using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Health : MonoBehaviour {

    public int health;
    public int invulnFrames;
    public int remInvulnFrames;

    public int Damage(int dmg)
    {
        if (remInvulnFrames > 0)
            dmg = 0;
        health -= dmg;
        if (health <= 0)
            Destroy(gameObject);
        if (dmg > 0)
            remInvulnFrames = invulnFrames;
        return dmg;
    }

    void FixedUpdate()
    {
        if (remInvulnFrames > 0)
            remInvulnFrames--;
    }
}
