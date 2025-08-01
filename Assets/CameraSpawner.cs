using Cinemachine;
using FMODUnity;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Spawns the Camera for the respective client
/// </summary>
public class CameraSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cameraPrefab;

    protected GameObject spawnedCamera;
    protected CinemachineVirtualCamera virtualCamera;
    public CinemachineVirtualCamera VirtualCamera => virtualCamera;
    public GameObject SpawnedCamera => spawnedCamera;
    private Camera spawnedCameraComponent;
    [SerializeField] private Vector3 cameraSpawnOffset;
    [SerializeField] private bool isChampion = false;
    [SerializeField] private Transform cameraTarget;

    /// <summary>
    /// Spawns the camera with the allocated Offset and sets it to the main Camera
    /// </summary>
    public void Init()
    {
        SpawnCamera();
        SetCameraMain();
    }

    /// <summary>
    /// Spawns the given camera prefab
    /// </summary>
    protected virtual void SpawnCamera()
    {
        spawnedCamera = Instantiate(cameraPrefab, transform.position, cameraPrefab.transform.rotation);
        if (!spawnedCamera.TryGetComponent<Camera>(out spawnedCameraComponent))
        {
            Debug.LogError($"{nameof(Camera)} was not found on {spawnedCamera.name}!");
            return;
        }
        spawnedCamera.transform.position += cameraSpawnOffset;

        virtualCamera = spawnedCamera.GetComponentInChildren<CinemachineVirtualCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError($"{nameof(CinemachineVirtualCamera)} not found!");
            return;
        }

        if (cameraTarget)
        {
            
            virtualCamera.Follow = cameraTarget;
            virtualCamera.LookAt = cameraTarget;
            virtualCamera.enabled = true;
        }
        
    }

    /// <summary>
    /// Sets the camera spawned by SpawnCamera to be CameraMain while disabling all other cameras
    /// </summary>
    private void SetCameraMain()
    {
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach(Camera camera in allCameras)
        {
            if (camera != spawnedCameraComponent)
            {
                camera.enabled = false;
            }

            if (camera.CompareTag("Minimap Cam"))
            {
                camera.enabled = true;
            }

            Camera.main.tag = "Untagged";
            spawnedCameraComponent.tag = "MainCamera";
            spawnedCameraComponent.enabled = true;
        }
    }
}
