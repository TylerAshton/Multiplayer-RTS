using UnityEngine;
using UnityEngine.UI;

public class AbilityCell : MonoBehaviour
{
    [SerializeField] private Slider slider;
    public Slider Slider => slider;
    [SerializeField] private Image image;
    public Image Image => image;
    [SerializeField] private Button button;
    public Button Button => button;


}
