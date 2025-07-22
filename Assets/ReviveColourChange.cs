using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ReviveColourChange : NetworkBehaviour
{
    [SerializeField] private List<ParticleSystem> particleSystems;
    [SerializeField] private Color StartColor = Color.white;
    [SerializeField] private Color EndColor = Color.green;


    public void SetParticleColour(float _value)
    {
        Color newColor = Color.Lerp(StartColor, EndColor, _value);

        foreach(ParticleSystem particleSystem in particleSystems)
        {
            var main = particleSystem.main;
            main.startColor = newColor;
        }
    }
}
