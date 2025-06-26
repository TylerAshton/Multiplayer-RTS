using UnityEngine;

public class VFXScaler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ScaleParticles(0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ScaleParticles(float scaleFactor)
    {
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            /*main.startSizeMultiplier *= scaleFactor;
            main.startSpeedMultiplier *= scaleFactor;
            main.startLifetimeMultiplier *= scaleFactor;*/
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }

        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }
}
