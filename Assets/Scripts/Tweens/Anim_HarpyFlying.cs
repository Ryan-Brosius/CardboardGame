using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Anim_HarpyFlying : MonoBehaviour
{
    [SerializeField] PuppetMovement puppetMovement;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Vector3 flyingPosition;
    [SerializeField] private Vector3 landedPositon;

    private Sequence flyingSequence;

    public void PlayFlyingTween(bool isFlying)
    {
        StartCoroutine(FLyingTween(isFlying));
    }

    public IEnumerator FLyingTween(bool isFlying)
    {
        flyingSequence?.Kill();

        flyingSequence = DOTween.Sequence();

        flyingSequence.OnStart(() =>
        {
            if (puppetMovement != null) puppetMovement.PauseMovement();
        });

        if (isFlying == true) flyingSequence.Append(transform.DOLocalMoveY(flyingPosition.y, duration).SetEase(Ease.OutBack));
        else if (isFlying == false) flyingSequence.Append(transform.DOLocalMoveY(landedPositon.y, duration).SetEase(Ease.OutBack));

        flyingSequence.OnComplete(() =>
        {
            if (puppetMovement != null)
            {
                puppetMovement.StartMovement();
            }
        });


        yield return flyingSequence.WaitForCompletion();
    }
}
