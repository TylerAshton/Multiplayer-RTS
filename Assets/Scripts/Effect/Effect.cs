using UnityEngine;

public abstract class Effect : ScriptableObject
{
    [SerializeField] float duration = 5f;
    public float Duration => duration;
    public abstract void Start(EffectManager _effectManager);

    public abstract void Update(EffectManager _effectManager);

    public abstract void End(EffectManager _effectManager);
}
