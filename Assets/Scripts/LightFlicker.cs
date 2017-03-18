using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour {

    public float maxIntensity;
    public float minIntensity;
    public float frequency;
    new Light light;

    void Start()
    {
        light = GetComponent<Light>();
    }

    void FixedUpdate()
    {
        light.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.Sin(Time.frameCount * frequency) / 2 + 0.5f);
    }

}
