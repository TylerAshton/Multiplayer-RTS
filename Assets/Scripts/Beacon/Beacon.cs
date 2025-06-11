using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Beacon : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/Beacon")]
    public static void ShowExample()
    {
        Beacon wnd = GetWindow<Beacon>();
        wnd.titleContent = new GUIContent("Beacon");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }
}
