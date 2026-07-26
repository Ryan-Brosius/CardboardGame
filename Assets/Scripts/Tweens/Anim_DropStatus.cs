using DG.Tweening;
using UnityEngine;

public class Anim_DropStatus : MonoBehaviour
{
    [SerializeField] private float activeHeight = 5f;
    [SerializeField] private float inactiveHeight = 10f;
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float raiseDuration = 0.5f;
    [SerializeField] private bool startHidden = true;

    private Tween dropSequence;

    private void Awake()
    {
        if (startHidden) ResetStatusPosition();
    }

    public void DropStatusIcon(bool isActive)
    {
        dropSequence?.Kill();

        if (isActive) dropSequence = transform.DOLocalMoveY(activeHeight, dropDuration).SetEase(Ease.OutBounce);
        else if (!isActive) dropSequence = transform.DOLocalMoveY(inactiveHeight, dropDuration).SetEase(Ease.OutBounce);
    }

    public void ResetStatusPosition()
    {
        dropSequence?.Kill();

        dropSequence = transform.DOLocalMoveY(inactiveHeight, dropDuration).SetEase(Ease.OutBounce);
    }

    private void OnDestroy()
    {
        dropSequence?.Kill();
    }
}
