using UnityEngine;

public class Energy : MonoBehaviour, IDamageable, IRespawnable
{

    private float current;
    public float Current { get { return current; } set { current = Mathf.Clamp(value, 0, limit); } }
    private float limit;
    public float Limit { get { return limit; } set { limit = Mathf.Clamp(value, 0, max); } }
    public float max;
    public int framesUntilRechargeBegins;
    public float rechargeRate;
    public float damageToEnergy; // How much energy to take off per point of damage.
    public float damageToLimit; // How much energy limit to reduce per point of damage
    [EnumFlag]
    public DamageType vulnerableTo = DamageType.Generic | DamageType.Slash | DamageType.Explosive | DamageType.Ground;
    [ReadOnly]
    public int framesSinceEnergyUsed;

    [ReadOnly]
    public int framesSinceLastPulse;
    public float pulseThreshold;

    [ReadOnly]
    public EnergyIndicator indicator;

    [ReadOnly]
    public Checkpoint deathRespawnPoint;

    public ParticleSystem damageParticles;
    public ParticleSystem deathParticles;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip pulseSound;

    ObjectData data;
    Rigidbody2D rb2d;


    void Start()
    {
        Current = Limit = max;
        data = GetComponentInParent<ObjectData>();
        rb2d = GetComponentInParent<Rigidbody2D>();
    }

    /// <summary>
    /// Uses up a certain amount of energy. Returns actual energy consumed.
    /// </summary>
    public float UseEnergy(float amt)
    {
        framesSinceEnergyUsed = 0;
        float previous = Current;
        Current -= amt;
        return (previous - Current);
    }

    void FixedUpdate()
    {
        if (pulseSound)
        {
            framesSinceLastPulse++;
            if (current/max < pulseThreshold && framesSinceLastPulse >= Mathf.FloorToInt(current / max * 50 + 8))
            {
                framesSinceLastPulse = 0;
                AudioSource.PlayClipAtPoint(pulseSound, transform.position);
            }
        }

        if (Current < max)
            framesSinceEnergyUsed++;
        Current += rechargeRate * Mathf.Max(0, (framesSinceEnergyUsed - framesUntilRechargeBegins) / 60f);
        if (indicator)
        {
            indicator.percentage = current / max;
            indicator.limitPercentage = limit / max;
        }
    }

    public bool Heal(int amt)
    {
        if (Limit == max)
            return false;
        Limit += amt * damageToLimit;
        return true;
    }

    public void FullHeal()
    {
        Limit = max;
    }

    public int Damage(int dmg, GameObject source, Vector2 knockback, DamageType damageType = DamageType.Generic)
    {
        if ((damageType | vulnerableTo) != vulnerableTo)
            dmg = 0;
        float actualEnergyDamage = ConvertDamageToEnergy(dmg);
        framesSinceEnergyUsed = 0;
        if (actualEnergyDamage > 0)
        {
            CheckDeath();
        }
        return dmg;
    }

    void CheckDeath()
    {
        if (Limit > 0)
            return;
        if (deathParticles)
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        if (deathSound)
            Camera.main.GetComponent<AudioSource>().PlayOneShot(deathSound);
        if (deathRespawnPoint != null)
            Respawn();
        else
        {
            data.gameObject.SetActive(false);
        }
    }

    float ConvertDamageToEnergy(int dmg)
    {
        float energy = dmg * damageToEnergy;
        float excess = Mathf.Max(0, energy - Current);
        float excessEnergyDamage = excess * damageToLimit / damageToEnergy;
        Current -= energy - excess;
        Limit -= excessEnergyDamage;
        return excessEnergyDamage;
    }

    public void SetDeathRespawnPoint(Checkpoint checkpoint)
    {
        if (deathRespawnPoint != null)
            deathRespawnPoint.active = false;
        deathRespawnPoint = checkpoint;
    }

    void Respawn()
    {
        data.transform.position = deathRespawnPoint.transform.position;
        FullHeal();
        rb2d.velocity = Vector2.zero;
    }
}
