using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour {

    public int health;
    public int invulnFrames;
    [ReadOnly] public int remInvulnFrames;

    public int Damage(int dmg)
    {
        if (remInvulnFrames > 0)
            dmg = 0;
        health -= dmg;
        if (health <= 0)
            Destroy(gameObject);
        if (dmg > 0)
        {
            remInvulnFrames = invulnFrames;
            if (invulnFrames > 0)
                StartCoroutine(Flash(GetComponent<SpriteRenderer>(), invulnFrames));
        }
        return dmg;
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
