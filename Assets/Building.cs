using UnityEngine;

public class Building : Unit
{
    private MeshRenderer meshRenderer;
    public void HideBuildPad()
    {
        meshRenderer.enabled = false;
    }

    public void ShowBuildPad()
    {
        meshRenderer.enabled = true;
    }
}
