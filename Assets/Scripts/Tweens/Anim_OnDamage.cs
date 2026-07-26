using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Anim_OnDamage : MonoBehaviour
{
    [SerializeField] PuppetMovement puppetMovement;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float moveStrength = 0.2f;
    [SerializeField] private float rotateAngle = 15f;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    [Header("Audio References")]
    [SerializeField] private AudioClip softImpact;
    [SerializeField] private AudioClip hardImpact;

    private Sequence feedbackSequence;
    private Color originalColor;
    private Tween flashTween;

    private void Awake()
    {
        if (puppetMovement == null) this.GetComponent<PuppetMovement>();
        originalColor = spriteRenderer.color;
    }

    public void DamageFeedback()
    {
        StartCoroutine(DamageFeedbackRoutine());
    }

    public IEnumerator DamageFeedbackRoutine()
    {
        feedbackSequence?.Kill();

        Vector3 startPos = transform.localPosition;
        Vector3 startRot = transform.localEulerAngles;

        feedbackSequence = DOTween.Sequence();

        feedbackSequence.OnStart(() =>
        {
            if (puppetMovement != null) puppetMovement.PauseMovement();
        });

        // Damage Knockback
        feedbackSequence.Append(
            transform.DOLocalMoveY(transform.localPosition.y + moveStrength, duration).SetEase(Ease.OutBack));
        feedbackSequence.Join(
            transform.DOLocalRotate(startRot + new Vector3(0f, 0f, rotateAngle), duration).SetEase(Ease.OutBack));

        AudioManager.Instance.PlaySFX(hardImpact, volumeScale: 1.0f, pitchJitter: 0.2f);

        // Reset to Position
        feedbackSequence.Append(
            transform.DOLocalMoveY(startPos.y, duration * 2).SetEase(Ease.InOutSine));
        feedbackSequence.Join(
            transform.DOLocalRotate(startRot, duration * 2).SetEase(Ease.InOutSine));

        feedbackSequence.OnComplete(() =>
        {
            if (puppetMovement != null) puppetMovement.StartMovement();
        });

        yield return feedbackSequence.WaitForCompletion();
    }

    public void Flash()
    {
        flashTween?.Kill();

        spriteRenderer.color = flashColor;
        flashTween = spriteRenderer.DOColor(originalColor, flashDuration)
            .SetEase(Ease.OutQuad);
    }
}
