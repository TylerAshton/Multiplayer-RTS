using UnityEngine;
using UnityEngine.UI;

public class QueueCell : MonoBehaviour
{
    private Image image;
    private void Awake()
    {
        if(!TryGetComponent<Image>(out image))
        {
            Debug.LogError($"{GetType().Name} requires an Image component to be assigned in gameobject {gameObject.name}!");
        }
    }
    public void SetQueueCell(ConstructionStats constructionStats)
    {
        if (constructionStats == null)
        {
            Debug.LogError($"{GetType().Name} received a null ConstructionStats.");
            return;
        }

        image.sprite = constructionStats.QueueIcon; 
    }
}
