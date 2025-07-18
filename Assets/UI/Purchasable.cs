using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Purchasable : RegistryItem
{
    [SerializeField] protected int price;
    public int Price => price;

    [FormerlySerializedAs("purchaseAbleIcon")]
    [SerializeField] protected Sprite icon;

    public Sprite Icon => icon;

    public virtual bool CanPurchase(IShopUser _shopUser)
    {
        if (_shopUser == null)
        {
            Debug.LogError($"{nameof(_shopUser)} is null!");
            return false;
        }

        if (_shopUser.Points < Price)
        {
            Debug.LogWarning($"Not enough gold to purchase! Required: {Price}, Available: {_shopUser.Points}");
            return false;
        }

        return true;
    }
    public abstract void ExecutePurchase(IShopUser _shopUser);

#if UNITY_EDITOR 
    public override void DrawInspector(SerializedObject _so)
    {
        base.DrawInspector(_so);

        SerializedProperty fieldShopPrice = _so.FindProperty("price");
        fieldShopPrice.intValue = EditorGUILayout.IntField("Shop Price", fieldShopPrice.intValue);
        if (fieldShopPrice.intValue < 0)
        {
            EditorGUILayout.HelpBox("Shop price cannot be less than zero!", MessageType.Error);
        }

        SerializedProperty fieldIcon = _so.FindProperty("icon");
        fieldIcon.objectReferenceValue = EditorGUILayout.ObjectField("Icon", fieldIcon.objectReferenceValue, typeof(Sprite), allowSceneObjects: false);
        if (fieldIcon.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Icon must be set!", MessageType.Error);
        }
    }
#endif
}
