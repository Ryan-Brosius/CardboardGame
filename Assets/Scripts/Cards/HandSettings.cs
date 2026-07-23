using UnityEngine;

[CreateAssetMenu(fileName = "HandSettings", menuName = "Scriptable Objects/HandSettings")]
public class HandSettings : ScriptableObject
{
    [Header("Placement")]
    public Vector2 centerOffset = Vector2.zero;

    [Header("Fan Shape")]
    public float fanRadius = 1800f;
    [Tooltip("Degrees of arc between neighboring cards.")]
    public float anglePerCard = 5f;
    [Tooltip("The fan never spreads wider than this total angle")]
    public float maxFanAngle = 40f;
    [Tooltip("How much each card rotates to follow the arc")]
    [Range(0f, 2f)]
    public float rotationMultiplier = 1f;
}
