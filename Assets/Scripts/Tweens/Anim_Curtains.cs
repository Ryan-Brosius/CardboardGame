using DG.Tweening;
using UnityEngine;

public class Anim_Curtains : MonoBehaviour
{
    [SerializeField] private Transform LeftCurtain;
    [SerializeField] private Transform RightCurtain;
    [SerializeField] private Vector3 closedLeft;
    [SerializeField] private Vector3 closedRight;
    [SerializeField] private float duration = 1.0f;

    private Vector3 openLeft;
    private Vector3 openRight;

    private PuppetMovement leftPuppetMove;
    private PuppetMovement rightPuppetMove;
    private Sequence openSequence;
    private Sequence closeSequence;

    private void Start()
    {
        openLeft = LeftCurtain.transform.position;
        openRight = RightCurtain.transform.position;

        LeftCurtain.localPosition = closedLeft;
        RightCurtain.localPosition = closedRight;

        leftPuppetMove = LeftCurtain.GetComponentInChildren<PuppetMovement>();
        rightPuppetMove = RightCurtain.GetComponentInChildren<PuppetMovement>();

        OpenCurtains();
    }

    public void OpenCurtains()
    {
        openSequence?.Kill();
        closeSequence?.Kill();

        openSequence = DOTween.Sequence();

        openSequence.OnStart(() =>
        {
            if (leftPuppetMove != null) leftPuppetMove.PauseMovement();
            if (rightPuppetMove != null) rightPuppetMove.PauseMovement();
        });

        openSequence.Append(
            LeftCurtain.DOLocalMoveX(openLeft.x, duration).SetEase(Ease.InOutSine));
        openSequence.Join(
            RightCurtain.DOLocalMoveX(openRight.x, duration).SetEase(Ease.InOutSine));

        openSequence.OnComplete(() =>
        {
            if (leftPuppetMove != null) leftPuppetMove.StartMovement();
            if (rightPuppetMove != null) rightPuppetMove.StartMovement();
        });
    }

    public void CloseCurtains()
    {
        openSequence?.Kill();
        closeSequence?.Kill();

        closeSequence = DOTween.Sequence();

        closeSequence.OnStart(() =>
        {
            if (leftPuppetMove != null) leftPuppetMove.PauseMovement();
            if (rightPuppetMove != null) rightPuppetMove.PauseMovement();
        });

        closeSequence.Append(
            LeftCurtain.DOLocalMoveX(closedLeft.x, duration).SetEase(Ease.InOutSine));
        closeSequence.Join(
            RightCurtain.DOLocalMoveX(closedRight.x, duration).SetEase(Ease.InOutSine));

        closeSequence.OnComplete(() =>
        {
            if (leftPuppetMove != null) leftPuppetMove.StartMovement();
            if (rightPuppetMove != null) rightPuppetMove.StartMovement();
        });
    }

    public void OpenAndClose()
    {
        openSequence?.Kill();
        closeSequence?.Kill();

        openSequence = DOTween.Sequence();

        openSequence.OnStart(() =>
        {
            if (leftPuppetMove != null) leftPuppetMove.PauseMovement();
            if (rightPuppetMove != null) rightPuppetMove.PauseMovement();
        });

        openSequence.Append(
            LeftCurtain.DOLocalMoveX(openLeft.x, duration / 2).SetEase(Ease.InOutSine));
        openSequence.Join(
            RightCurtain.DOLocalMoveX(openRight.x, duration / 2).SetEase(Ease.InOutSine));

        openSequence.Append(
            LeftCurtain.DOLocalMoveX(closedLeft.x, duration / 2).SetEase(Ease.InOutSine));
        openSequence.Join(
            RightCurtain.DOLocalMoveX(closedRight.x, duration / 2).SetEase(Ease.InOutSine));

        openSequence.OnComplete(() =>
        {
            if (leftPuppetMove != null) leftPuppetMove.StartMovement();
            if (rightPuppetMove != null) rightPuppetMove.StartMovement();
        });
    }
}
