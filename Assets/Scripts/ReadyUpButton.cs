using UnityEngine;
using UnityEngine.UI;

public class ReadyUpButton : MonoBehaviour
{
    [SerializeField] private Button readyButton;

    private void Awake()
    {
        readyButton.onClick.AddListener(() =>
        {
            PlayerReadyUp.Instance.SetPlayerReady();
        });
    }
}
