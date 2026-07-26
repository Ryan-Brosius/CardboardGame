using System;
using DG.Tweening;
using UnityEngine;

public class Anim_EnemyAttack : MonoBehaviour
{
    [SerializeField] PuppetMovement puppetMovement;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Transform targetPosition;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float rotateAngle = -15f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Sequence feedbackSequence;
    private Sequence deathSequence;
    public bool IsPlaying => feedbackSequence != null && feedbackSequence.IsActive() && feedbackSequence.IsPlaying();

    private void Awake()
    {
        if (puppetMovement == null) this.GetComponent<PuppetMovement>();
        PlaySpawnSequence();
    }

    public Sequence DamageFeedback(Transform target = null, Action onImpact = null)
    {
        if (target != null)
        {
            targetPosition = target;
        }

        feedbackSequence?.Kill();

        Vector3 startPos = transform.localPosition;
        Vector3 startRot = transform.localEulerAngles;

        feedbackSequence = DOTween.Sequence();

        feedbackSequence.OnStart(() =>
        {
            if (puppetMovement != null) puppetMovement.PauseMovement();
        });

        // Attack Wind Up
        feedbackSequence.Append(
            transform.DOLocalMoveY(transform.localPosition.y + jumpHeight, duration / 2).SetEase(Ease.OutSine));
        feedbackSequence.Join(
            transform.DOLocalRotate(startRot + new Vector3(0f, 0f, rotateAngle), duration / 2).SetEase(Ease.OutSine));

        // Attack itself
        feedbackSequence.Append(
            transform.DOLocalMove(targetPosition.position, duration).SetEase(Ease.InOutBack));
        feedbackSequence.Join(
            transform.DOLocalRotate(startRot + new Vector3(0f, 0f, rotateAngle), duration).SetEase(Ease.InOutBack));

        if (onImpact != null)
            feedbackSequence.AppendCallback(() => onImpact());

        // Reset to Position
        feedbackSequence.Append(
            transform.DOLocalMove(startPos, duration * 2).SetEase(Ease.InOutSine));
        feedbackSequence.Join(
            transform.DOLocalRotate(startRot, duration * 2).SetEase(Ease.InOutSine));

        feedbackSequence.OnComplete(() =>
        {
            if (puppetMovement != null) puppetMovement.StartMovement();
        });

        return feedbackSequence;
    }

    public void PlayDeathSequence()
    {
        deathSequence?.Kill();

        deathSequence = DOTween.Sequence();

        deathSequence.OnStart(() =>
        {
            if (puppetMovement != null) puppetMovement.PauseMovement();
        });

        deathSequence.Append(
            transform.DOLocalMoveY(transform.localPosition.y - 3f, duration * 2).SetEase(Ease.OutSine));
        deathSequence.Join(
            spriteRenderer.DOFade(0f, duration * 2).SetEase(Ease.OutSine));

        feedbackSequence.OnComplete(() =>
        {
            deathSequence.Kill();
        });
    }

    public void PlaySpawnSequence()
    {
        if (gameObject.TryGetComponent<Anim_HarpyFlying>(out Anim_HarpyFlying harpyScript))
        {
            return;
        }
        deathSequence?.Kill();

        deathSequence = DOTween.Sequence();

        transform.position = new Vector3(transform.position.x, transform.position.y -4f, transform.position.z);
        spriteRenderer.DOFade(0f, 0.01f);

        deathSequence.OnStart(() =>
        {
            if (puppetMovement != null) puppetMovement.PauseMovement();
        });

        deathSequence.Append(
            transform.DOLocalMoveY(transform.localPosition.y + 4f, duration * 2).SetEase(Ease.OutSine));
        deathSequence.Join(
            spriteRenderer.DOFade(1f, duration * 2).SetEase(Ease.OutSine));

        feedbackSequence.OnComplete(() =>
        {
            deathSequence.Kill();
        });
    }
}
