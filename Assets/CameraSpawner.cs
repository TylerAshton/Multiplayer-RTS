using NUnit.Framework;
using UnityEngine;

public class CameraSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cameraPrefab;

    private GameObject spawnedCamera;
    private Camera spawnedCameraComponent;

    private void Awake()
    {
        
    }

    /// <summary>
    /// Spawns the given camera prefab
    /// </summary>
    public void SpawnCamera()
    {
        spawnedCamera = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
        spawnedCameraComponent = spawnedCamera.GetComponent<Camera>();
    }

    /// <summary>
    /// Sets the camera spawned by SpawnCamera to be CameraMain while disabling all other cameras
    /// </summary>
    public void SetCameraMain()
    {
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach(Camera camera in allCameras)
        {
            if (camera != spawnedCamera)
            {
                camera.enabled = false;
            }

            Camera.main.tag = "Untagged";
            spawnedCameraComponent.tag = "MainCamera";
            spawnedCameraComponent.enabled = true;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnCamera();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
