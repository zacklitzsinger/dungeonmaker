using UnityEngine;

[RequireComponent(typeof(BasicAI))]
public class AIOnDamaged : MonoBehaviour {

    public MonoBehaviour state;

	// Use this for initialization
	void Start () {
        Health health = GetComponent<Health>();
        BasicAI ai = GetComponent<BasicAI>();
        // When damaged by the player, they should become the focus. This code can't be in the behaviors
        // because they are in various states of enabled/disabled.
        health.onDamaged += (go) =>
        {
            ai.SetCurrentState(state);
            if (go.CompareTag("Player") && state as IAttack != null)
            {
                (state as IAttack).SetTarget(go.transform);
                ai.SetCurrentState(state);
            }
        };
    }
}
