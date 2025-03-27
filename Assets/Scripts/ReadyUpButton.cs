using UnityEngine;
using UnityEngine.UI;

public class ReadyUpButton : MonoBehaviour
{
    [SerializeField] private Button readyButton; // The button the player uses to "Ready up" to progress to the next scene

    private void Awake()
    {
        readyButton.onClick.AddListener(() =>
        {
            PlayerReadyUp.Instance.SetPlayerReady();
        });
    }
}
