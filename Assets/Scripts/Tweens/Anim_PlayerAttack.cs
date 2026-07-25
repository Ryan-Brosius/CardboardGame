using DG.Tweening;
using UnityEngine;

public class Anim_PlayerAttack : MonoBehaviour
{
    [SerializeField] PuppetMovement puppetMovement;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float rotateAngle = -15f;

    private Sequence feedbackSequence;

    private void Awake()
    {
        if (puppetMovement == null) this.GetComponent<PuppetMovement>();
    }

    public void DamageFeedback(Vector3 targetPosition)
    {
        feedbackSequence?.Kill();

        Vector3 startPos = transform.localPosition;
        Vector3 startRot = transform.localEulerAngles;

        feedbackSequence = DOTween.Sequence();

        feedbackSequence.OnStart(() =>
        {
            if (puppetMovement != null) puppetMovement.PauseMovement();
        });

        // Attack Wind-Up
        feedbackSequence.Append(
            transform.DOLocalMoveY(transform.localPosition.y + jumpHeight, duration / 2).SetEase(Ease.OutSine));
        feedbackSequence.Join(
            transform.DOLocalRotate(startRot + new Vector3(0f, 0f, rotateAngle), duration / 2).SetEase(Ease.OutSine));

        // Actual Attack itself
        feedbackSequence.Append(
            transform.DOLocalMove(targetPosition, duration).SetEase(Ease.InOutBack));
        feedbackSequence.Join(
            transform.DOLocalRotate(startRot + new Vector3(0f, 180f, rotateAngle), duration).SetEase(Ease.InOutBack));

        // Reset back to position
        feedbackSequence.Append(
            transform.DOLocalMove(startPos, duration * 2).SetEase(Ease.InOutSine));
        feedbackSequence.Join(
            transform.DOLocalRotate(startRot, duration * 2).SetEase(Ease.InOutSine));

        feedbackSequence.OnComplete(() =>
        {
            if (puppetMovement != null) puppetMovement.StartMovement();
        });
    }
}
