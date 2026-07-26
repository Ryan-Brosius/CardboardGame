using DG.Tweening;
using TMPro;
using UnityEngine;

public class Anim_Typewriter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;
    [SerializeField] private float charactersPerSecond = 30f;

    // Optional SFX trigger on character reveal
    [SerializeField] private AudioClip typeSound;
    [SerializeField] private float soundPitchJitter = 0.05f;

    private Tween typewriterTween;

    private void OnEnable()
    {
        ShowMessage("The Sword has claimed your life\r\nthe virtue of your life amounted to");
    }

    public void ShowMessage(string fullMessage)
    {
        typewriterTween?.Kill();

        // Set complete text first, but hide all characters
        textUI.text = fullMessage;
        textUI.maxVisibleCharacters = 0;

        int totalCharacters = textUI.textInfo.characterCount;
        float duration = totalCharacters / charactersPerSecond;

        int lastVisibleCount = 0;

        // Tween maxVisibleCharacters from 0 to total count
        typewriterTween = DOTween.To(
            () => textUI.maxVisibleCharacters,
            x => {
                textUI.maxVisibleCharacters = x;

                // Play SFX only when a new character actually appears
                if (x > lastVisibleCount && typeSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(typeSound, pitchJitter: soundPitchJitter);
                    lastVisibleCount = x;
                }
            },
            totalCharacters,
            duration
        ).SetEase(Ease.Linear);
    }

    public void CompleteInstantly()
    {
        if (typewriterTween != null && typewriterTween.IsActive())
        {
            typewriterTween.Complete();
        }
    }
}
