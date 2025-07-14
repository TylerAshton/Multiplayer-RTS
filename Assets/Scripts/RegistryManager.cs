using UnityEngine;

/// <summary>
/// Loads and initialises the Registries
/// </summary>
public static class RegistryManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Registry<Ability>.Init("Abilities");
        Registry<Purchasable>.Init("");
        Registry<HitboxStats>.Init("AbilityStats");
        Registry<ProjectileStats>.Init("AbilityStats");
        Registry<ProjectionStats>.Init("AbilityStats");
    }
}
