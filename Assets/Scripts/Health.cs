using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{

    public int maxHealth;
    public int invulnFrames;
    [ReadOnly]
    public int currentHealth;
    [ReadOnly]
    public int remInvulnFrames;
    [ReadOnly]
    public Checkpoint deathRespawnPoint;

    public ParticleSystem damageParticles;
    public AudioClip hitSound;
    public AudioClip deathSound;

    Rigidbody2D rb2d;
    Player player;

    void Start()
    {
        currentHealth = maxHealth;
        rb2d = GetComponentInParent<Rigidbody2D>();
        player = GetComponentInParent<Player>();
    }

    public int Damage(int dmg, Vector2 knockback, bool fall = false)
    {
        if (remInvulnFrames > 0)
            dmg = 0;
        currentHealth -= dmg;
        if (dmg > 0)
        {
            remInvulnFrames = invulnFrames;
            if (invulnFrames > 0)
                StartCoroutine(Flash(GetComponentInChildren<SpriteRenderer>(), invulnFrames));
            if (damageParticles && knockback.magnitude > 0)
            {
                Instantiate(damageParticles, transform.position, Quaternion.LookRotation(knockback, Vector3.forward));
                rb2d.AddForce(knockback);
            }
            Camera.main.GetComponent<AudioSource>().PlayOneShot(hitSound);
            LevelEditor.main.HitPause(4);
        }
        if (currentHealth <= 0)
        {
            if (deathSound)
                Camera.main.GetComponent<AudioSource>().PlayOneShot(deathSound);
            if (deathRespawnPoint != null)
                Respawn();
            else
                GetComponentInParent<ObjectData>().gameObject.SetActive(false);
        }
        else if (fall && player)
        {
            RespawnAtRoomEntrance();
        }
        return dmg;
    }

    void Respawn()
    {
        player.transform.position = deathRespawnPoint.transform.position;
        currentHealth = maxHealth;
        rb2d.velocity = Vector2.zero;
    }

    void RespawnAtRoomEntrance()
    {
        player.transform.position = player.roomEntrance;
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
            float alpha = Mathf.Sin(count / 1.5f) / 2 + 0.5f;
            c = r.color;
            c.a = alpha;
            r.color = c;
        }
        c = r.color;
        c.a = 1f;
        r.color = c;
    }
}
