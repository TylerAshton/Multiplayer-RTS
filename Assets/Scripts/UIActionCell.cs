using UnityEngine;
using UnityEngine.UI;

public class UIActionCell : MonoBehaviour
{
    [SerializeField] private Image image;
    public Image Image => image;
    [SerializeField] private Button button;
    public Button Button => button;
}
