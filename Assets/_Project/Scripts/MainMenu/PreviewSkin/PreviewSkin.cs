using UnityEngine;
using OSK;

public class PreviewSkin : MonoBehaviour
{
    [SerializeField] private SkinId[] skinIds;
    
    private void Start()
    {
        // Subscribe to skin scroll events
        Main.Observer.Add(EEvent.OnSkinScrolled, OnSkinScrolled);
        
        // Initialize with current skin
        InitializeCurrentSkin();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        Main.Observer.Remove(EEvent.OnSkinScrolled, OnSkinScrolled);
    }
    
    private void InitializeCurrentSkin()
    {
        ESkin currentSkin = GameData.CurrentSkin;
        SwitchToSkin(currentSkin);
    }
    
    private void OnSkinScrolled(object data)
    {
        if (data is ESkin selectedSkin)
        {
            SwitchToSkin(selectedSkin);
        }
    }
    
    private void SwitchToSkin(ESkin targetSkin)
    {
        if (skinIds == null) return;
        
        // Single loop optimization: deactivate all and activate target in one pass
        SkinId targetSkinId = null;
        
        foreach (var skinId in skinIds)
        {
            if (skinId != null)
            {
                if (skinId.SkinID == targetSkin)
                {
                    targetSkinId = skinId;
                }
                else
                {
                    // Only deactivate if currently active (avoid unnecessary calls)
                    if (skinId.gameObject.activeInHierarchy)
                    {
                        skinId.gameObject.SetActive(false);
                    }
                }
            }
        }
        
        // Activate the target skin
        if (targetSkinId != null && !targetSkinId.gameObject.activeInHierarchy)
        {
            targetSkinId.gameObject.SetActive(true);
        }
    }
}
