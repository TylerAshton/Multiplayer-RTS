using UnityEngine;

public static class RegistryManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Registry<Ability>.Init("Abilities");
        Registry<Purchasable>.Init("AbilityStats");
        Registry<HitboxStats>.Init("AbilityStats");
        Registry<ProjectileStats>.Init("AbilityStats");
        Registry<ProjectionStats>.Init("AbilityStats");
    }
}
