using NUnit.Framework;
using UnityEngine;

public class CameraSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cameraPrefab;

    private GameObject spawnedCamera;
    public GameObject SpawnedCamera => spawnedCamera;
    private Camera spawnedCameraComponent;
    [SerializeField] private Vector3 cameraSpawnOffset;

    public void Init()
    {
        SpawnCamera();
        SetCameraMain();
    }

    /// <summary>
    /// Spawns the given camera prefab
    /// </summary>
    public void SpawnCamera()
    {
        spawnedCamera = Instantiate(cameraPrefab, transform.position, cameraPrefab.transform.rotation);
        spawnedCameraComponent = spawnedCamera.GetComponent<Camera>();
        spawnedCamera.transform.position += cameraSpawnOffset;
    }

    /// <summary>
    /// Sets the camera spawned by SpawnCamera to be CameraMain while disabling all other cameras
    /// </summary>
    public void SetCameraMain()
    {
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach(Camera camera in allCameras)
        {
            if (camera != spawnedCameraComponent)
            {
                camera.enabled = false;
            }

            Camera.main.tag = "Untagged";
            spawnedCameraComponent.tag = "MainCamera";
            spawnedCameraComponent.enabled = true;
        }
    }
}
