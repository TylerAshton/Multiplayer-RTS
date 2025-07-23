using UnityEngine;
using UnityEngine.UI;

public class UIActionCell : MonoBehaviour
{
    [SerializeField] protected Image image;
    public Image Image => image;
    [SerializeField] protected Button button;
    public Button Button => button;
}
