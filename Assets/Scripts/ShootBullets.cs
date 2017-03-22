using System.Collections.Generic;
using UnityEngine;

public class ShootBullets : AIBehavior {

    public int freqency;
    private int remFrames;
    public List<float> angles = new List<float>();
    public Bullet bulletPrefab;

    void OnEnable()
    {
        remFrames = freqency;
    }

    void FixedUpdate()
    {
        if (remFrames-- <= 0)
        {
            foreach(float angle in angles)
            {
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * transform.up;
                Bullet bullet = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, dir));
                bullet.friendly = false;
            }
            remFrames = freqency;
        }
    }
}
