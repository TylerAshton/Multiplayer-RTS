using UnityEngine;

public class InteractionPopUp : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    public void SetVisible(bool _value)
    {
        canvas.enabled = _value;
    }

    private void Update()
    {
        canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - Camera.main.transform.position);
    }
}
