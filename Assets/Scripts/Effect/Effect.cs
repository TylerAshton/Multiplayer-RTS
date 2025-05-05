using UnityEngine;

public abstract class Effect : ScriptableObject
{
    [SerializeField] float duration = 5f;
    public float Duration => duration;
    public abstract void OnStart(EffectManager _effectManager);

    public abstract void OnUpdate(EffectManager _effectManager);

    public abstract void OnEnd(EffectManager _effectManager);
}
