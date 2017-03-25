using UnityEngine;

public class CleanupParticle : MonoBehaviour {

    ParticleSystem ps;

	// Use this for initialization
	void Start () {
        ps = GetComponentInChildren<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!ps.IsAlive())
            Destroy(gameObject);
	}
}
