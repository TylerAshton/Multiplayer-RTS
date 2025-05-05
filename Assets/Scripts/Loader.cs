using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static loader to load a given scene
/// </summary>
public static class Loader
{
    public enum Scene //An enum of the different scenes that the network can load between
    {
        MainMenu,
        LoadingScene,
        MainWorld,
        Tyler,
        MainWorldOLD
    }

    private static Scene targetScene; //The scene the game will transition to

    /// <summary>
    /// Loads the scene across the network
    /// </summary>
    /// <param name="targetScene"></param>
    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    /// <summary>
    /// Loads the scene locally for whoever calls it
    /// </summary>
    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
