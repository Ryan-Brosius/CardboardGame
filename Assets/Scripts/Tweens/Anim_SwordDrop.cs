using DG.Tweening;
using UnityEngine;

public class Anim_SwordDrop : MonoBehaviour
{
    [SerializeField] private Transform startPosition;   // Full health position
    [SerializeField] private Transform endPosition;     // Death position
    [SerializeField] private float dropDuration = 0.25f;
    [SerializeField] private float raiseDuration = 0.5f;
    [SerializeField] private int swordMax = 100;
    [SerializeField] private AudioClip swordChainSfx;

    private Tween dropSequence;
    private Tween raiseSequence;

    public void DropSword(int swordInt)
    {
        if (startPosition == null || endPosition == null) return;

        float percent = Mathf.Clamp01((float)swordInt / swordMax);
        Vector3 targetPosition = Vector3.Lerp(endPosition.position, startPosition.position, percent);

        dropSequence?.Kill();

        dropSequence = transform.DOLocalMove(targetPosition, dropDuration).SetEase(Ease.OutBounce);

        AudioManager.Instance.PlaySFX(swordChainSfx, 1, 0.2f);
    }

    private void OnDestroy()
    {
        dropSequence?.Kill();
        raiseSequence?.Kill();
    }
}
