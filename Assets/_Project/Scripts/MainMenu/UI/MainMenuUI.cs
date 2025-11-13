using OSK;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : View
{
    [Header("Menu Buttons")]
    [SerializeField] private Button btnStart;
    [SerializeField] private Button btnQuest;
    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnTutorial;

    [Header("Skin Selection")]
    [SerializeField] private CharacterSkinSO[] availableSkins;
    [SerializeField] private Button btnSkinLeft;
    [SerializeField] private Button btnSkinRight;
    [SerializeField] private Text skinPriceText;
    [SerializeField] private Button btnBuySkin;
    [SerializeField] private Sprite[] btnBuySprites;

    private int currentSkinIndex = 0;
    private ESkin currentSelectedSkin = ESkin.Skin0;

    public override void Initialize(RootUI rootUI)
    {
        base.Initialize(rootUI);

        // Menu buttons
        btnStart.onClick.AddListener(StartGame);
        btnQuest.onClick.AddListener(OpenQuestUI);
        btnSettings.onClick.AddListener(OpenSetting);
        btnTutorial.onClick.AddListener(OpenTutorial);

        // Skin selection buttons
        btnSkinLeft.onClick.AddListener(SelectPreviousSkin);
        btnSkinRight.onClick.AddListener(SelectNextSkin);
        btnBuySkin.onClick.AddListener(BuyCurrentSkin);

        // Initialize skin selection
        InitializeSkinSelection();
    }

    public void StartGame()
    {
        SingletonManager.Instance.Get<Menu_Manager>().StartGame();
    }

    public void OpenQuestUI()
    {
        Main.UI.Open<DailyQuestUI>();
    }
    private void OpenSetting()
    {
        Main.UI.Open<SettingUI>();
    }
    private void OpenTutorial()
    {
        Main.UI.Open<TutorialUI>();
    }

    #region Skin Selection

    private void InitializeSkinSelection()
    {
        if (availableSkins == null || availableSkins.Length == 0)
        {
            Debug.LogWarning("No skins available in MainMenuUI!");
            return;
        }

        // Find current skin index based on ESkin enum from GameData
        currentSelectedSkin = GameData.CurrentSkin;
        currentSkinIndex = (int)currentSelectedSkin - 1; // Convert to 0-based index

        UpdateSkinDisplay();
    }

    private void SelectPreviousSkin()
    {
        if (availableSkins == null || availableSkins.Length == 0) return;

        currentSkinIndex--;
        if (currentSkinIndex < 0)
        {
            currentSkinIndex = availableSkins.Length - 1;
        }

        currentSelectedSkin = (ESkin)(currentSkinIndex + 1); // Convert back to 1-based enum

        // Notify observer about skin scroll (for visual preview)
        Main.Observer.Notify(EEvent.OnSkinScrolled, currentSelectedSkin);

        // Only update GameData if skin is actually selectable (unlocked or default)
        if (GameData.IsSkinUnlocked(currentSelectedSkin) || availableSkins[currentSkinIndex].IsDefaultSkin)
        {
            // Defer save to avoid multiple PlayerPrefs calls
            SetCurrentSkinDeferred(currentSelectedSkin);
        }

        UpdateSkinDisplay();
    }

    private void SelectNextSkin()
    {
        if (availableSkins == null || availableSkins.Length == 0) return;

        currentSkinIndex++;
        if (currentSkinIndex >= availableSkins.Length)
        {
            currentSkinIndex = 0;
        }

        currentSelectedSkin = (ESkin)(currentSkinIndex + 1); // Convert back to 1-based enum

        // Notify observer about skin scroll (for visual preview)
        Main.Observer.Notify(EEvent.OnSkinScrolled, currentSelectedSkin);

        // Only update GameData if skin is actually selectable (unlocked or default)
        if (GameData.IsSkinUnlocked(currentSelectedSkin) || availableSkins[currentSkinIndex].IsDefaultSkin)
        {
            // Defer save to avoid multiple PlayerPrefs calls
            SetCurrentSkinDeferred(currentSelectedSkin);
        }

        UpdateSkinDisplay();
    }

    // Deferred skin setting to avoid excessive PlayerPrefs saves
    private System.Collections.IEnumerator deferredSkinCoroutine;
    
    private void SetCurrentSkinDeferred(ESkin skin)
    {
        // Cancel any existing deferred operation
        if (deferredSkinCoroutine != null)
        {
            StopCoroutine(deferredSkinCoroutine);
        }
        
        // Start new deferred operation
        deferredSkinCoroutine = SetCurrentSkinAfterDelay(skin);
        StartCoroutine(deferredSkinCoroutine);
    }
    
    private System.Collections.IEnumerator SetCurrentSkinAfterDelay(ESkin skin)
    {
        // Wait a short time to batch multiple rapid button presses
        yield return new WaitForSeconds(0.1f);
        
        // Set the skin (this will trigger PlayerPrefs save)
        GameData.CurrentSkin = skin;
        
        deferredSkinCoroutine = null;
    }

    private void UpdateSkinDisplay()
    {
        if (availableSkins == null || availableSkins.Length == 0) return;
        if (currentSkinIndex < 0 || currentSkinIndex >= availableSkins.Length) return;

        var currentSkin = availableSkins[currentSkinIndex];
        bool isUnlocked = GameData.IsSkinUnlocked(currentSelectedSkin);
        bool isCurrentlySelected = GameData.CurrentSkin == currentSelectedSkin;

        // Update buy button - only if it exists
        if (btnBuySkin != null)
        {
            // Cache current state to avoid unnecessary updates
            bool shouldHide = currentSkin.IsDefaultSkin || isUnlocked;
            bool currentlyHidden = !btnBuySkin.gameObject.activeSelf;

            // Only update visibility if it changed
            if (shouldHide != currentlyHidden)
            {
                btnBuySkin.gameObject.SetActive(!shouldHide);
            }

            // Only update button content if visible
            if (!shouldHide)
            {
                // Cache values to avoid multiple calls
                bool canAfford = GameData.Gold >= currentSkin.GoldCost;
                bool currentlyEnabled = btnBuySkin.enabled;
                
                // Update price text (assuming it changes less frequently)
                if (skinPriceText != null)
                {
                    string priceText = currentSkin.GoldCost.ToString();
                    if (skinPriceText.text != priceText)
                    {
                        skinPriceText.text = priceText;
                    }
                }

                // Only update button state if it changed
                if (canAfford != currentlyEnabled)
                {
                    btnBuySkin.enabled = canAfford;
                }

                // Update sprite only if needed
                if (btnBuySprites != null && btnBuySprites.Length >= 2)
                {
                    var buttonImage = btnBuySkin.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        var targetSprite = canAfford ? btnBuySprites[1] : btnBuySprites[0];
                        if (buttonImage.sprite != targetSprite)
                        {
                            buttonImage.sprite = targetSprite;
                        }
                    }
                }
            }
        }
    }

    private void BuyCurrentSkin()
    {
        if (availableSkins == null || currentSkinIndex < 0 || currentSkinIndex >= availableSkins.Length)
            return;

        var currentSkin = availableSkins[currentSkinIndex];
        bool isUnlocked = GameData.IsSkinUnlocked(currentSelectedSkin);

        if (!isUnlocked && !currentSkin.IsDefaultSkin)
        {
            // Buy this skin
            if (GameData.Gold >= currentSkin.GoldCost)
            {
                GameData.Gold -= currentSkin.GoldCost;
                GameData.UnlockSkin(currentSelectedSkin);

                // Automatically select the newly purchased skin
                GameData.CurrentSkin = currentSelectedSkin;

                UpdateSkinDisplay();

                Debug.Log($"Purchased and equipped skin: {currentSkin.SkinName}");
            }
            else
            {
                Debug.Log("Not enough gold to buy this skin!");
                // You could show a popup here
            }
        }
    }

    #endregion
}
