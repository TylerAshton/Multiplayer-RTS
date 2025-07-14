using UnityEngine;

public static class RegistryManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Registry<Ability>.Init("Abilities");
    }
}
