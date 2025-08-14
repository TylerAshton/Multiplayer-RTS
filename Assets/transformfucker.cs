using UnityEngine;

public class transformfucker : MonoBehaviour
{
    Vector3 lastPos;

    void LateUpdate()
    {
        gameObject.SetActive(false);
        gameObject.SetActive(true);

    }
}
