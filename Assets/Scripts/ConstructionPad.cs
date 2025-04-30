using UnityEngine;

public enum ConstructionState
{
    Ready,
    Used
}


public class ConstructionPad : Unit
{
    private MeshRenderer meshRenderer;
    private ConstructionState constructionState;
    public void HideBuildPad()
    {
        meshRenderer.enabled = false;
    }

    public void ShowBuildPad()
    {
        meshRenderer.enabled = true;
    }

    public void SetConstructionState(ConstructionState _state)
    {
        constructionState = _state;
    }
}
