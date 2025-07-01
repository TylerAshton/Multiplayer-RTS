using System;
using UnityEditor;
using UnityEngine;

public enum HitboxType
{
    Sphere,
    Box,
    Cone
}

public class HitboxStats : BaseAbilityStat 
{
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private float sizeDelta = 0f;
    [SerializeField] private HitboxType hitboxType = HitboxType.Box;

    // Box-specific properties
    [SerializeField] private Vector3 boxStartSize = Vector3.one;
    [SerializeField] private Vector3 boxEndSize = Vector3.one;

    // Sphere-specific properties
    [SerializeField] private float sphereStartRadius = 1f;
    [SerializeField] private float sphereEndRadius = 1f;

    // Cone-specific properties
    [SerializeField] private float coneAngle = 45f;
    public override void DrawInspector(SerializedObject so)
    {
        SerializedProperty fieldOffset = so.FindProperty("offset");
        EditorGUILayout.PropertyField(fieldOffset, new GUIContent("Offset"));

        SerializedProperty fieldSizeDelta = so.FindProperty("sizeDelta");
        EditorGUILayout.PropertyField(fieldSizeDelta, new GUIContent("Size Delta"));
        if (sizeDelta < 0)
        {
            EditorGUILayout.HelpBox("Size Delta must be a positive value!", MessageType.Error);
        }

        SerializedProperty fieldHitboxType = so.FindProperty("hitboxType");
        EditorGUILayout.PropertyField(fieldHitboxType, new GUIContent("Hitbox Type"));

        // Type-specific properties
        switch (hitboxType) // I think type conditionals are fine. Doing derived classes would be overkill for this.
        {
            case HitboxType.Sphere:
                DrawSphere(so);
                break;
            case HitboxType.Box:
                DrawBox(so);
                break;
            case HitboxType.Cone:
                DrawCone(so);
                break;
            default:
                EditorGUILayout.HelpBox("Unknown hitbox type!", MessageType.Error);
                break;
        }

    }

    private void DrawSphere(SerializedObject so)
    {
        SerializedProperty fieldSphereStartRadius = so.FindProperty("sphereStartRadius");
        EditorGUILayout.PropertyField(fieldSphereStartRadius, new GUIContent("Sphere Start Radius"));
        if (fieldSphereStartRadius.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Sphere Start Radius must be a positive value!", MessageType.Error);
        }

        SerializedProperty fieldSphereEndRadius = so.FindProperty("sphereEndRadius");
        EditorGUILayout.PropertyField(fieldSphereEndRadius, new GUIContent("Sphere End Radius"));
        if (fieldSphereEndRadius.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Sphere End Radius must be a positive value!", MessageType.Error);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="so"></param>
    private void DrawCone(SerializedObject so) // A cone is a sphere so we can reuse the sphere properties.
    {
        SerializedProperty fieldSphereStartRadius = so.FindProperty("sphereStartRadius");
        EditorGUILayout.PropertyField(fieldSphereStartRadius, new GUIContent("Cone Start Radius"));
        if (fieldSphereStartRadius.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Sphere Start Radius must be a positive value!", MessageType.Error);
        }

        SerializedProperty fieldSphereEndRadius = so.FindProperty("sphereEndRadius");
        EditorGUILayout.PropertyField(fieldSphereEndRadius, new GUIContent("Cone End Radius"));
        if (fieldSphereEndRadius.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Sphere End Radius must be a positive value!", MessageType.Error);
        }

        SerializedProperty fieldConeAngle = so.FindProperty("coneAngle");
        EditorGUILayout.Slider(fieldConeAngle, 0f, 360f, new GUIContent("Cone Angle"));
    }

    private void DrawBox(SerializedObject so)
    {
        SerializedProperty fieldBoxStartSize = so.FindProperty("boxStartSize");
        EditorGUILayout.PropertyField(fieldBoxStartSize, new GUIContent("Box Start Size"));
        if (fieldBoxStartSize.vector3Value.x <= 0 || fieldBoxStartSize.vector3Value.y <= 0 || fieldBoxStartSize.vector3Value.z <= 0)
        {
            EditorGUILayout.HelpBox("Box Start Size must be positive values!", MessageType.Error);
        }


        SerializedProperty fieldBoxEndSize = so.FindProperty("boxEndSize");
        EditorGUILayout.PropertyField(fieldBoxEndSize, new GUIContent("Box End Size"));
        if (fieldBoxEndSize.vector3Value.x <= 0 || fieldBoxEndSize.vector3Value.y <= 0 || fieldBoxEndSize.vector3Value.z <= 0)
        {
            EditorGUILayout.HelpBox("Box End Size must be positive values!", MessageType.Error);
        }
    }

    public override bool IsValid()
    {
        throw new System.NotImplementedException();
    }
}
