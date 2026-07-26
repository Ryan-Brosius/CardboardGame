using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Anim_ExhaustedFlip : MonoBehaviour
{
    [SerializeField] Transform characterSprite;
    [SerializeField] PuppetMovement puppetMovement;
    [SerializeField] float duration = 0.5f;
    [SerializeField] float jumpStrength = 2.0f;

    private Sequence flipSequence;

    public void PlayFlipSequence()
    {
        StartCoroutine(FlipSequence());
    }

    public IEnumerator FlipSequence()
    {
        if (characterSprite == null) yield break;

        flipSequence?.Kill();

        flipSequence = DOTween.Sequence();

        flipSequence.OnStart(() =>
        {
            if (puppetMovement != null) puppetMovement.PauseMovement();
        });

        flipSequence.Append(
            characterSprite.DOLocalJump(characterSprite.localPosition, jumpStrength, 1, duration)).SetEase(Ease.InOutSine);
        flipSequence.Join(
            characterSprite.DOLocalRotate(characterSprite.localEulerAngles + new Vector3(180f, 0f, 0f), duration).SetEase(Ease.InOutBack));

        flipSequence.OnComplete(() =>
        {
            if (puppetMovement != null) puppetMovement.StartMovement();
        });

        yield return flipSequence.WaitForCompletion();
    }
}
