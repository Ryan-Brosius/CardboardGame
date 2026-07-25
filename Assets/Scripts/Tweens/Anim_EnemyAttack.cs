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

    private Sequence feedbackSequence;
    public bool IsPlaying => feedbackSequence != null && feedbackSequence.IsActive() && feedbackSequence.IsPlaying();

    private void Awake()
    {
        if (puppetMovement == null) this.GetComponent<PuppetMovement>();
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
}
