using UnityEngine;

[RequireComponent (typeof(RTSPlayerControls), typeof(UnitManager))]
public class RTSPlayer : MonoBehaviour
{
    public static RTSPlayer instance { get; private set; }
    private RTSPlayerControls rtsPlayerControls;
    public RTSPlayerControls RTSPlayerControls => rtsPlayerControls;
    private UnitManager unitManager;
    public UnitManager UnitManager => unitManager;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rtsPlayerControls = GetComponent<RTSPlayerControls>();
        unitManager = GetComponent<UnitManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
