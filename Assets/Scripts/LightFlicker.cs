using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{

    public float maxIntensity;
    public float minIntensity;
    public float frequency;
    public bool active;
    Light light;
    [ReadOnly]
    public float modifier = 1f;

    void OnDisable()
    {
        if (light)
            light.enabled = false;
    }

    void Start()
    {
        light = GetComponent<Light>();
        light.intensity = minIntensity / 2 + maxIntensity / 2;
    }

    void FixedUpdate()
    {
        float targetModifier = active ? 1 : 0;
        modifier = Mathf.Lerp(modifier, targetModifier, 0.03f);
        if (Mathf.Abs(targetModifier - modifier) < 0.03f)
            modifier = targetModifier;
        light.enabled = (modifier > 0);
        light.intensity = Mathf.Lerp(minIntensity * modifier, maxIntensity * modifier, Mathf.Sin(Time.frameCount * frequency) / 2 + 0.5f);
    }

}
