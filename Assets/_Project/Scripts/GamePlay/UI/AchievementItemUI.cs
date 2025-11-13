using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text achievementDescriptionText;
    [SerializeField] private Text progressText;
    [SerializeField] private Image progressSlider;
    [SerializeField] private Button claimRewardButton;
    [SerializeField] private Text rewardText;

    [Header("Button Sprites")]
    [SerializeField] private Sprite[] claimButtonSprites; // [0] = cannot claim, [1] = can claim

    private AchievementProgress achievementProgress;

    public AchievementProgress AchievementProgress => achievementProgress;

    private void Awake()
    {
        if (claimRewardButton != null)
        {
            claimRewardButton.onClick.AddListener(OnClaimRewardClicked);
        }
    }

    private void OnDestroy()
    {
        if (claimRewardButton != null)
        {
            claimRewardButton.onClick.RemoveListener(OnClaimRewardClicked);
        }
    }

    public void Initialize(AchievementProgress achievementProgress, bool isUnlocked)
    {
        this.achievementProgress = achievementProgress;

        UpdateDisplay();
        UpdateProgress();
        UpdateClaimButton();
    }

    private void UpdateDisplay()
    {
        if (achievementProgress?.Achievement == null) return;

        var achievement = achievementProgress.Achievement;

        // Update texts
        if (achievementDescriptionText != null)
        {
            achievementDescriptionText.text = achievement.Description;
        }

        if (rewardText != null)
        {
            string rewardString = "";
            if (achievement.GoldReward > 0)
            {
                rewardString += $"+{achievement.GoldReward}";
            }
            rewardText.text = rewardString;
        }
    }

    public void UpdateProgress()
    {
        if (achievementProgress == null) return;

        // Update progress text
        if (progressText != null)
        {
            progressText.text = achievementProgress.GetProgressText();
        }

        // Update progress slider
        if (progressSlider != null)
        {
            progressSlider.fillAmount = achievementProgress.ProgressPercentage;
        }

        // Update visual state
        UpdateClaimButton();
    }

    private void UpdateClaimButton()
    {
        if (claimRewardButton == null) return;

        bool canClaim = achievementProgress != null && achievementProgress.IsUnlocked && !achievementProgress.IsRewardClaimed;

        // Always show the button
        claimRewardButton.gameObject.SetActive(true);

        // Update button sprite based on claim state
        if (claimButtonSprites != null && claimButtonSprites.Length >= 2)
        {
            var buttonImage = claimRewardButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                // If reward is claimed, always show first sprite (cannot claim)
                // If can claim, show second sprite (can claim)
                buttonImage.sprite = canClaim ? claimButtonSprites[1] : claimButtonSprites[0];
                claimRewardButton.GetComponentInChildren<Text>().text = achievementProgress.IsRewardClaimed ? "Claimed" : "Claim";
            }
        }

        // Enable/disable button interaction
        claimRewardButton.interactable = canClaim;
    }

    private void OnClaimRewardClicked()
    {
        if (achievementProgress != null && achievementProgress.IsUnlocked && !achievementProgress.IsRewardClaimed)
        {
            achievementProgress.ClaimReward();
        }
        UpdateClaimButton();
    }
}