using System.Collections.Generic;
using UnityEngine;

public class ShootBullets : MonoBehaviour {

    public int freqency;
    private int remFrames;
    public List<Vector2> directions = new List<Vector2>();
    public Bullet bulletPrefab;

    void Start()
    {
        remFrames = freqency;
    }

    void FixedUpdate()
    {
        if (!LevelEditor.main.currentRoom.Contains(transform.position.ToGrid()))
            return;
        if (remFrames-- <= 0)
        {
            foreach(Vector2 dir in directions)
            {
                Bullet bullet = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, dir));
                bullet.friendly = false;
            }
            remFrames = freqency;
        }
    }
}
