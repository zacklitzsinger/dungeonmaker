using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour {

    public int maxHealth;
    public int invulnFrames;
    [ReadOnly]
    public int currentHealth;
    [ReadOnly] public int remInvulnFrames;
    public Checkpoint deathRespawnPoint;

    Rigidbody2D rb2d;
    Player player;

    void Start()
    {
        currentHealth = maxHealth;
        rb2d = GetComponentInParent<Rigidbody2D>();
        player = GetComponentInParent<Player>();
    }

    public int Damage(int dmg, bool fall = false)
    {
        if (remInvulnFrames > 0)
            dmg = 0;
        currentHealth -= dmg;
        if (dmg > 0)
        {
            remInvulnFrames = invulnFrames;
            if (invulnFrames > 0)
                StartCoroutine(Flash(GetComponentInParent<SpriteRenderer>(), invulnFrames));
        }
        if (currentHealth <= 0)
        {
            if (deathRespawnPoint != null)
                Respawn();
            else
                Destroy(rb2d.gameObject);
        } else if (fall && player)
        {
            RespawnAtRoomEntrance();
        }
        return dmg;
    }

    public void Knockback(Vector2 motion)
    {
        rb2d.AddForce(motion);
    }

    void Respawn()
    {
        transform.parent.position = deathRespawnPoint.transform.position;
        currentHealth = maxHealth;
        rb2d.velocity = Vector2.zero;
    }

    void RespawnAtRoomEntrance()
    {
        transform.parent.position = player.roomEntrance;
        rb2d.velocity = Vector2.zero;
    }

    public void SetDeathRespawnPoint(Checkpoint checkpoint)
    {
        if (deathRespawnPoint != null)
            deathRespawnPoint.active = false;
        deathRespawnPoint = checkpoint;
    }

    void FixedUpdate()
    {
        if (remInvulnFrames > 0)
            remInvulnFrames--;
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
