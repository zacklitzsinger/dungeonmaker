﻿using UnityEngine;

public class HeartPickup : MonoBehaviour {

    public int amount;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponentInParent<Health>();
            if (health.currentHealth < health.maxHealth)
            {
                health.Heal(amount);
                Destroy(gameObject);
            }
        }
    }

}
