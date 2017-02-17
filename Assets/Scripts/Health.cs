using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour {

    public int maxHealth;
    public int invulnFrames;
    [ReadOnly]
    public int currentHealth;
    [ReadOnly] public int remInvulnFrames;
    public Checkpoint respawnPoint;

    Rigidbody2D rb2d;

    void Start()
    {
        currentHealth = maxHealth;
        rb2d = GetComponent<Rigidbody2D>();
    }

    public int Damage(int dmg)
    {
        if (remInvulnFrames > 0)
            dmg = 0;
        currentHealth -= dmg;
        if (dmg > 0)
        {
            remInvulnFrames = invulnFrames;
            if (invulnFrames > 0)
                StartCoroutine(Flash(GetComponent<SpriteRenderer>(), invulnFrames));
        }
        return dmg;
    }

    void Respawn()
    {
        transform.position = respawnPoint.transform.position;
        currentHealth = maxHealth;
        rb2d.velocity = Vector2.zero;
    }

    public void SetRespawnPoint(Checkpoint checkpoint)
    {
        if (respawnPoint != null)
            respawnPoint.active = false;
        respawnPoint = checkpoint;
    }

    void FixedUpdate()
    {
        if (remInvulnFrames > 0)
            remInvulnFrames--;

        if (currentHealth <= 0)
        {
            if (respawnPoint != null)
                Respawn();
            else
                Destroy(gameObject);
        }
    }


    IEnumerator Flash(SpriteRenderer r, int frameDuration)
    {
        Color c;
        int count = 0;
        while (count++ < frameDuration && r != null)
        {
            yield return new WaitForFixedUpdate();
            // Flicker alpha between 0 and 1
            float alpha = Mathf.Sin(count/1.5f)/2 + 0.5f;
            c = r.color;
            c.a = alpha;
            r.color = c;
        }
        c = r.color;
        c.a = 1f;
        r.color = c;
    }
}
