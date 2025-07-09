using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTerminator : NetworkBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    [SerializeField] private int HostDisconnectDelay = 10;
    [SerializeField] private int ClientDisconnectDelay = 5;
    public void Init(bool _hasWon)
    {
        if (_hasWon)
        {
            winScreen.SetActive(true);
        }
        else
        {
            loseScreen.SetActive(true);
        }

        
        StartCoroutine(LeaveGame());

    }

    IEnumerator LeaveGame()
    {
        if (IsHost)
        {
            yield return new WaitForSeconds(HostDisconnectDelay);
            /*NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(0);*/

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        else if (!IsHost)
        {
            yield return new WaitForSeconds(ClientDisconnectDelay);
/*            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(0);*/

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }


}
