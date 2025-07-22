using UnityEngine;

public class IconSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MinimapHandler.Instance.updateList();
        MinimapHandler.Instance.createIcon(this.gameObject);
    }
}
