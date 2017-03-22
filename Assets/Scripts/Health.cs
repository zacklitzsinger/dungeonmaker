using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Health : MonoBehaviour, ICustomSerializable, IDamageable
{
    [PlayerEditable("Invulnerable")]
    [ReadOnly]
    public bool invulnerableOverride = false; // Invulnerability, set by some effects

    [EnumFlag]
    public DamageType vulnerableTo = DamageType.Generic | DamageType.Slash | DamageType.Explosive | DamageType.Ground;

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
    public ParticleSystem deathParticles;
    public AudioClip hitSound;
    public AudioClip deathSound;

    public delegate void OnDamagedDelegate(GameObject source);
    public event OnDamagedDelegate onDamaged;

    public delegate void OnDeathDelegate(GameObject source);
    public event OnDeathDelegate onDeath;

    ObjectData data;
    Rigidbody2D rb2d;
    Player player;
    SpriteRenderer spriteRenderer;
    Shield shield;

    void Start()
    {
        currentHealth = maxHealth;
        data = GetComponentInParent<ObjectData>();
        rb2d = GetComponentInParent<Rigidbody2D>();
        player = GetComponentInParent<Player>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        shield = GetComponentInChildren<Shield>(true);
    }

    public bool Heal(int amt)
    {
        if (amt == 0 || currentHealth >= maxHealth)
            return false;
        currentHealth = Mathf.Min(maxHealth, amt + currentHealth);
        return true;
    }

    public void FullHeal()
    {
        Heal(maxHealth - currentHealth);
    }

    public void Die()
    {
        Damage(currentHealth, gameObject, Vector2.zero);
    }

    public int Damage(int dmg, GameObject source, Vector2 knockback, DamageType damageType = DamageType.Generic)
    {
        if (remInvulnFrames > 0 || invulnerableOverride || (damageType | vulnerableTo) != vulnerableTo || currentHealth <= 0)
        {
            return 0;
        }
        if (shield && shield.enabled && knockback.magnitude > 0)
        {
            if (shield.TryBlock(knockback.normalized, source.GetComponentInParent<IActionQueue>()))
                return 0;
        }
        if (currentHealth > 0)
        {
            currentHealth -= dmg;
            remInvulnFrames = invulnFrames;
            if (invulnFrames > 0 && spriteRenderer)
                StartCoroutine(Flash(spriteRenderer, invulnFrames));
            if (damageParticles && knockback.magnitude > 0 && (damageType | DamageType.Fall) != damageType)
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
            if (deathParticles)
                Instantiate(deathParticles, transform.position, Quaternion.identity);
            if (deathSound)
                Camera.main.GetComponent<AudioSource>().PlayOneShot(deathSound);
            if (onDeath != null)
                onDeath(source);
            if (deathRespawnPoint != null)
                Respawn();
            else
            {
                if (Random.value <= itemChanceDropOnDeath && itemChoices.Count > 0)
                {
                    GameObject itemChoice = itemChoices[UnityEngine.Random.Range(0, itemChoices.Count)];
                    LevelEditor.main.CreateObjectAtGrid(transform.position, itemChoice);
                }
                data.gameObject.SetActive(false);
            }
        }
        else if ((damageType & DamageType.Fall) == DamageType.Fall && player)
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

    public void Serialize(BinaryWriter bw)
    {
        ObjectSerializer.Serialize(bw, this);
    }

    public void Deserialize(BinaryReader br)
    {
        ObjectSerializer.Deserialize(br, this);
    }
}
