using UnityEngine;

public class VFXScaler : MonoBehaviour
{

    /// <summary>
    /// Through the power of 2 lines this scales the particle systems within a GameObject.
    /// </summary>
    /// <param name="scaleFactor"></param>
    /// <param name="vfxParent"></param>
    public void ScaleParticles(float scaleFactor, GameObject vfxParent)
    {
        ParticleSystem[] particleSystems = vfxParent.GetComponentsInChildren<ParticleSystem>();
        foreach (var particleSystem in particleSystems)
        {
            var main = particleSystem.main;
            /*main.startSizeMultiplier *= scaleFactor;
            main.startSpeedMultiplier *= scaleFactor;
            main.startLifetimeMultiplier *= scaleFactor;*/
            main.scalingMode = ParticleSystemScalingMode.Hierarchy; // So apparently this makes the particles scale with the parent transform
        }

        vfxParent.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }
}
