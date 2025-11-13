using UnityEngine;

[CreateAssetMenu(fileName = "New Skin", menuName = "Game Data/Character Skin")]
public class CharacterSkinSO : ScriptableObject
{
    [Header("Skin Information")]
    [SerializeField] private ESkin skinId;
    [SerializeField] private string skinName;

    [Header("Purchase Settings")]
    [SerializeField] private int goldCost;
    [SerializeField] private bool isDefaultSkin; // Free skin that's unlocked by default

    // Properties
    public ESkin SkinId => skinId;
    public string SkinName => skinName;
    public int GoldCost => goldCost;
    public bool IsDefaultSkin => isDefaultSkin;
}