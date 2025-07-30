using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SelectionMode
{
    None,
    Hover,
    Select
}

public class SelectionHighlighter : MonoBehaviour
{
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    [SerializeField] private Material[] hoverMaterials;
    [SerializeField] private Material[] selectedMaterials;
    private SelectionMode selectionMode = SelectionMode.None;


    private void Awake()
    {
        if (selectedMaterials == null || selectedMaterials.Length == 0)
        {
            Debug.LogError($"{GetType().Name} requires at least one highlight material to be assigned in gameobject {gameObject.name}!");
            return;
        }

        SetupOriginalMaterials();
    }

    public void SetSelectionMode(SelectionMode _selectionMode)
    {
        selectionMode = _selectionMode;

        switch (selectionMode)
        {
            case SelectionMode.Hover:
                ApplyHighlightShder(hoverMaterials);
                break;
            case SelectionMode.Select:
                ApplyHighlightShder(selectedMaterials);
                break;
            case SelectionMode.None:
                ResetShaders();
                break;
            default:
                Debug.LogWarning($"Unknown selection mode: {selectionMode}");
                break;
        }
    }

    private void SetupOriginalMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            originalMaterials[renderer] = renderer.materials;
        }
    }

    private void ApplyHighlightShder(Material[] _highlightShaders)
    {
        foreach (var renderer in originalMaterials.Keys)
        {
            if (renderer == null)
            {
                Debug.LogError("Renderer is null!");
                continue;
            }

            Material[] original = originalMaterials[renderer];
            Material[] combined = original.Concat(_highlightShaders).ToArray();
            renderer.materials = combined;
        }
    }

    private void ResetShaders()
    {
        foreach (var renderer in originalMaterials.Keys)
        {
            if (renderer == null)
            {
                Debug.LogError("Renderer is null!");
                continue;
            }

            Material[] original = originalMaterials[renderer];
            renderer.materials = original;

        }
    }
}
