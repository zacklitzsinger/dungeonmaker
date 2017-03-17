using System;
using UnityEngine;

[Flags]
public enum DamageType
{
    Generic = 1,
    Slash = 2,
    Explosive = 4,
    Ground = 8,
    Fall = 16,
}

public interface IDamageable {
    bool Heal(int amt);

    void FullHeal();

    int Damage(int dmg, GameObject source, Vector2 knockback, DamageType damageType = DamageType.Generic);
}
