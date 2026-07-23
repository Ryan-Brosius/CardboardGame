using UnityEngine;

[CreateAssetMenu(fileName = "CardSettings", menuName = "Scriptable Objects/CardSettings")]
public class CardSettings : ScriptableObject
{
    [Header("Hover")]
    [Tooltip("How far the card lifts when hovered.")]
    public float hoverLift = 60f;
    [Tooltip("Card scale while hovered.")]
    public float hoverScale = 1.15f;
    [Tooltip("If true, a hovered card rotates upright instead of keeping its fan angle")]
    public bool straightenWhileHovered = true;

    [Header("Drag")]
    [Tooltip("Card scale while being dragged.")]
    public float dragScale = 1.08f;
    [Tooltip("If true, the card keeps the offset from where you grabbed it. If false, its center snaps under the cursor.")]
    public bool keepGrabOffset = true;
    [Tooltip("If true, a dragged card rotates upright instead of keeping its fan angle.")]
    public bool straightenWhileDragging = true;

    [Header("Drag Tilt")]
    [Tooltip("Degrees of tilt per unit of horizontal speed.")]
    public float tiltPerSpeed = 0.01f;
    [Tooltip("Maximum tilt in degrees.")]
    public float maxTilt = 25f;
    [Tooltip("How quickly the tilt eases in/out.")]
    public float tiltSmoothTime = 0.06f;

    [Header("Motion Smoothing")]
    [Tooltip("Time for gliding back to the slot.")]
    public float positionSmoothTime = 0.12f;
    [Tooltip("Time for position while following the cursor.")]
    public float dragSmoothTime = 0.05f;
    [Tooltip("Time for rotation.")]
    public float rotationSmoothTime = 0.08f;
    [Tooltip("Time for scale changes.")]
    public float scaleSmoothTime = 0.08f;

    [Header("Misc")]
    [Tooltip("Resting scale of a card in the hand.")]
    public float baseScale = 1f;
    [Tooltip("How close a released card must get to its slot before it drops back into normal draw order.")]
    public float settleDistance = 25f;
}
