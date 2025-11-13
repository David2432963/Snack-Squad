using OSK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : View
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnNext;
    [SerializeField] private GameObject[] tutorialSteps;
    private int currentStepIndex = 0;

    public override void Initialize(RootUI rootUI)
    {
        base.Initialize(rootUI);
        btnClose.onClick.AddListener(Close);
        btnNext.onClick.AddListener(OnNextClicked);
    }
    private void Close()
    {
        Main.UI.Hide(this);
    }
    private void OnNextClicked()
    {
        currentStepIndex++;
        if (currentStepIndex < tutorialSteps.Length)
        {
            UpdateTutorialStep();
        }
        else
        {
            Close();
        }
    }
    private void UpdateTutorialStep()
    {
        for (int i = 0; i < tutorialSteps.Length; i++)
        {
            tutorialSteps[i].SetActive(i == currentStepIndex);
        }
        if (currentStepIndex == tutorialSteps.Length - 1)
        {
            btnNext.GetComponentInChildren<TextMeshProUGUI>().text = "Get it!";
        }
        else
        {
            btnNext.GetComponentInChildren<TextMeshProUGUI>().text = "Next";
        }
    }
}
