using UnityEngine;
using UnityEngine.UI;

public class ConstructionProgressBar : MonoBehaviour
{
    private Slider slider;
    public Slider Slider => slider;

    private void Awake()
    {
        if (!TryGetComponent<Slider>(out slider))
        {
            Debug.LogError($"{GetType().Name} requires a {nameof(Slider)} component.");
        }
    }
}
