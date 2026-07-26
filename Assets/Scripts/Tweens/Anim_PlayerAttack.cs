using System;
using DG.Tweening;
using UnityEngine;

public class Anim_PlayerAttack : MonoBehaviour
{
    [SerializeField] PuppetMovement puppetMovement;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float rotateAngle = -15f;

    private Sequence feedbackSequence;
    private Sequence castSequence;
    public bool IsPlaying => feedbackSequence != null && feedbackSequence.IsActive() && feedbackSequence.IsPlaying();

    private void Awake()
    {
        if (puppetMovement == null) this.GetComponent<PuppetMovement>();
    }

    public Sequence Play(Vector3 targetWorldPosition, Action onImpact = null)
    {
        feedbackSequence?.Kill(complete: true);
 
        Vector3 startPos = transform.localPosition;
        Vector3 startRot = transform.localEulerAngles;
 
        Vector3 targetLocal = transform.parent != null
            ? transform.parent.InverseTransformPoint(targetWorldPosition)
            : targetWorldPosition;
 
        feedbackSequence = DOTween.Sequence();
        feedbackSequence.OnStart(() =>
        {
            if (puppetMovement != null) puppetMovement.PauseMovement();
        });
 
        // Attack Wind-Up
        feedbackSequence.Append(
            transform.DOLocalMoveY(startPos.y + jumpHeight, duration / 2).SetEase(Ease.OutSine));
        feedbackSequence.Join(
            transform.DOLocalRotate(startRot + new Vector3(0f, 0f, rotateAngle), duration / 2).SetEase(Ease.OutSine));
 
        // Actual Attack itself
        feedbackSequence.Append(
            transform.DOLocalMove(targetLocal, duration).SetEase(Ease.InOutBack));
        feedbackSequence.Join(
            transform.DOLocalRotate(startRot + new Vector3(0f, 0f, -rotateAngle), duration).SetEase(Ease.InOutBack));
 
        if (onImpact != null)
            feedbackSequence.AppendCallback(() => onImpact());
 
        // Reset back to position
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

    public void PlayCastSequence()
    {
        castSequence?.Kill();

        Vector3 startPos = transform.localPosition;
        Vector3 startRot = transform.localEulerAngles;

        castSequence = DOTween.Sequence();

        castSequence.OnStart(() =>
        {
            if (puppetMovement != null) puppetMovement.PauseMovement();
        });

        castSequence.Append(
            transform.DOLocalMoveY(startPos.y + jumpHeight, duration / 2).SetEase(Ease.OutSine));
        castSequence.Join(
            transform.DOLocalRotate(startRot + new Vector3(0f, 180f, 0f), duration / 2).SetEase(Ease.OutSine));

        castSequence.Append(
            transform.DOLocalMove(startPos, duration).SetEase(Ease.InOutSine));
        castSequence.Join(
            transform.DOLocalRotate(startRot, duration).SetEase(Ease.InOutSine));

        castSequence.OnComplete(() =>
        {
            if (puppetMovement != null) puppetMovement.StartMovement();
        });
    }
}
