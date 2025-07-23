using UnityEngine;

public class Idol : Factory, IAmalgamCore
{
    private AmalgamUpgradeManager amalgamUpgradeManager;
    public AmalgamUpgradeManager AmalgamUpgradeManager => amalgamUpgradeManager;

    protected override void Awake()
    {
        base.Awake();

        if (!TryGetComponent<AmalgamUpgradeManager>(out amalgamUpgradeManager))
        {
            Debug.LogError($"{nameof(amalgamUpgradeManager)} is required for {GetType().Name} in gameobject {gameObject.name}!");
        }
    }
}
