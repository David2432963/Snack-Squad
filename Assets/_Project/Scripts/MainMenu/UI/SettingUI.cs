using OSK;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : View
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnSoundToggle;
    [SerializeField] private Button btnMusicToggle;
    [SerializeField] private Image iconSound;
    [SerializeField] private Image iconMusic;
    [SerializeField] private Sprite[] btnSoundSprites; // [0] = off, [1] = on
    [SerializeField] private Sprite[] btnMusicSprites; // [0] = off, [1] = on
    public override void Initialize(RootUI rootUI)
    {
        base.Initialize(rootUI);
        btnSoundToggle.onClick.AddListener(OnClickSoundToggle);
        btnMusicToggle.onClick.AddListener(OnClickMusicToggle);
        btnClose.onClick.AddListener(Close);

        Setup();
    }
    private void Setup()
    {
        iconSound.sprite = Main.Sound.SFXVolume == 0 ? btnSoundSprites[0] : btnSoundSprites[1];
        iconMusic.sprite = Main.Sound.MusicVolume == 0 ? btnMusicSprites[0] : btnMusicSprites[1];
    }
    public void OnClickSoundToggle()
    {
        Main.Sound.SetAllVolume(SoundType.SFX, Main.Sound.SFXVolume == 0 ? 1f : 0f);
        iconSound.sprite = Main.Sound.SFXVolume == 0 ? btnSoundSprites[0] : btnSoundSprites[1];
    }

    public void OnClickMusicToggle()
    {
        Main.Sound.SetAllVolume(SoundType.MUSIC, Main.Sound.MusicVolume == 0 ? 1f : 0f);
        iconMusic.sprite = Main.Sound.MusicVolume == 0 ? btnMusicSprites[0] : btnMusicSprites[1];
    }
    private void Close()
    {
        Main.UI.Hide(this);
    }
}
