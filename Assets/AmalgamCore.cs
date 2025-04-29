using UnityEngine;

public class AmalgamCore : MonoBehaviour
{
    private Runestone_Controller runestoneController;

    private void Awake()
    {
        runestoneController = GetComponent<Runestone_Controller>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        runestoneController.ToggleRuneStone(true);
    }

}
