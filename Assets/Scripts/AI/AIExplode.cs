using UnityEngine;

[RequireComponent(typeof(Explosive))]
public class AIExplode : AIBehavior
{
    void OnEnable()
    {
        Health health = GetComponent<Health>();
        health.Die();
    }

}
