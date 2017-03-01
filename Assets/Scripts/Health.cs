using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [ReadOnly]
    public bool invulnerableOverride = false; // Invulnerability, set by some effects

    public int maxHealth;
    public int invulnFrames;
    [ReadOnly]
    public int currentHealth;
    [ReadOnly]
    public int remInvulnFrames;
    [ReadOnly]
    public Checkpoint deathRespawnPoint;

    public float itemChanceDropOnDeath;
    public List<GameObject> itemChoices = new List<GameObject>();

    public ParticleSystem damageParticles;
    public AudioClip hitSound;
    public AudioClip deathSound;

    public delegate void OnDamagedDelegate(GameObject source);
    public event OnDamagedDelegate onDamaged;

    Rigidbody2D rb2d;
    Player player;

    void Start()
    {
        currentHealth = maxHealth;
        rb2d = GetComponentInParent<Rigidbody2D>();
        player = GetComponentInParent<Player>();
    }

    public void Heal(int amt)
    {
        currentHealth = Mathf.Min(maxHealth, amt + currentHealth);
    }

    public int Damage(int dmg, GameObject source, Vector2 knockback, bool fall = false)
    {
        if (remInvulnFrames > 0 || invulnerableOverride)
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
            LevelEditor.main.HitPause(Constants.HIT_PAUSE);
            if (onDamaged != null)
                onDamaged(source);
        }
        if (currentHealth <= 0)
        {
            if (deathSound)
                Camera.main.GetComponent<AudioSource>().PlayOneShot(deathSound);
            if (deathRespawnPoint != null)
                Respawn();
            else
            {
                if (Random.value <= itemChanceDropOnDeath && itemChoices.Count > 0)
                {
                    GameObject itemChoice = itemChoices[Random.Range(0, itemChoices.Count)];
                    LevelEditor.main.CreateObjectAtGrid(transform.position, itemChoice);
                }
                GetComponentInParent<ObjectData>().gameObject.SetActive(false);
            }
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
