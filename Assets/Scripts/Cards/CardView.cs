using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static bool AnyCardDragging { get; private set; }

    // someone pls figure out how to do this later
    [Header("DEBUG FOR NOW SO I UNDERSTAND THE CARDS WITH NO ART")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Play Zone Hooks")]
    public UnityEvent onEnterPlayZone;
    public UnityEvent onExitPlayZone;

    private CardSettings settings;
    private HandController hand;
    private RectTransform rect;
    private RectTransform handRect;
    private CanvasGroup canvasGroup;

    // Hand controller info
    private Vector2 slotPosition;
    private float slotRotation;
    private int slotIndex;

    private bool hovered;
    private bool dragging;
    private bool elevated;

    private Vector2 dragTarget;
    private Vector2 grabOffset;

    private Vector2 posVelocity;
    private float rotVelocity;
    private float scaleVelocity;
    private float tilt;
    private float tiltVelocity;
    private Vector2 lastPosition;

    public CardData Data { get; private set; }
    public bool IsInPlayZone { get; private set; }
    public int SlotIndex => slotIndex;
    public bool IsElevated => elevated;

    public void Init(HandController hand, CardSettings settings)
    {
        this.hand = hand;
        this.settings = settings;
        handRect = hand.transform as RectTransform;

        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    public void SetData(CardData data)
    {
        Data = data;
        if (data == null) return;
        if (titleText != null) titleText.text = data.title;
        if (descriptionText != null) descriptionText.text = data.description;
    }

    public void SetSlot(int index, Vector2 position, float rotation)
    {
        slotIndex = index;
        slotPosition = position;
        slotRotation = rotation;
    }

    public void SnapToSlot()
    {
        rect.anchoredPosition = slotPosition;
        rect.localRotation = Quaternion.Euler(0f, 0f, slotRotation);
        rect.localScale = Vector3.one * (settings != null ? settings.baseScale : 1f);
        lastPosition = rect.anchoredPosition;
        posVelocity = Vector2.zero;
    }

    private void Awake()
    {
        rect = (RectTransform)transform;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (settings == null) return;

        float dt = Time.unscaledDeltaTime;

        // Position
        Vector2 targetPos = dragging ? dragTarget : slotPosition;
        if (hovered && !dragging)
            targetPos += Vector2.up * settings.hoverLift;

        float smoothTime = dragging ? settings.dragSmoothTime : settings.positionSmoothTime;
        rect.anchoredPosition = Vector2.SmoothDamp(
            rect.anchoredPosition, targetPos, ref posVelocity, smoothTime, Mathf.Infinity, dt);

        // Tilt
        float horizontalSpeed = (rect.anchoredPosition.x - lastPosition.x) / dt;
        lastPosition = rect.anchoredPosition;

        float tiltTarget = Mathf.Clamp(
            -horizontalSpeed * settings.tiltPerSpeed, -settings.maxTilt, settings.maxTilt);
        tilt = Mathf.SmoothDampAngle(tilt, tiltTarget, ref tiltVelocity,
            settings.tiltSmoothTime, Mathf.Infinity, dt);

        // Rotation
        bool straighten =
            (dragging && settings.straightenWhileDragging) ||
            (hovered && !dragging && settings.straightenWhileHovered);
        float baseRotation = straighten ? 0f : slotRotation;

        float newZ = Mathf.SmoothDampAngle(
            rect.localEulerAngles.z, baseRotation + tilt, ref rotVelocity,
            settings.rotationSmoothTime, Mathf.Infinity, dt);
        rect.localRotation = Quaternion.Euler(0f, 0f, newZ);

        // Scale
        float targetScale = settings.baseScale;
        if (dragging) targetScale = settings.dragScale;
        else if (hovered) targetScale = settings.hoverScale;

        float newScale = Mathf.SmoothDamp(
            rect.localScale.x, targetScale, ref scaleVelocity,
            settings.scaleSmoothTime, Mathf.Infinity, dt);
        rect.localScale = new Vector3(newScale, newScale, 1f);

        // Go back to position in hand
        if (elevated && !dragging && !hovered &&
            Vector2.Distance(rect.anchoredPosition, slotPosition) < settings.settleDistance)
        {
            elevated = false;
            hand.RestoreDrawOrder();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AnyCardDragging) return;
        hovered = true;
        Elevate();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        AnyCardDragging = true;
        hovered = false;
        canvasGroup.blocksRaycasts = false;
        Elevate();

        if (ScreenToHandLocal(eventData, out Vector2 localPoint))
        {
            grabOffset = rect.anchoredPosition - localPoint;
            dragTarget = localPoint + (settings.keepGrabOffset ? grabOffset : Vector2.zero);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ScreenToHandLocal(eventData, out Vector2 localPoint))
            dragTarget = localPoint + (settings.keepGrabOffset ? grabOffset : Vector2.zero);

        UpdatePlayZone(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool wantsToPlay = IsInPlayZone;

        dragging = false;
        AnyCardDragging = false;
        canvasGroup.blocksRaycasts = true;
        SetInPlayZone(false);

        if (wantsToPlay)
            hand.RequestPlay(this);
    }

    private void UpdatePlayZone(PointerEventData eventData)
    {
        bool inZone = eventData.position.y >= Screen.height * settings.playZoneHeightPercent;
        SetInPlayZone(inZone);
    }

    private void SetInPlayZone(bool inZone)
    {
        if (inZone == IsInPlayZone) return;
        IsInPlayZone = inZone;

        if (inZone) onEnterPlayZone.Invoke();
        else onExitPlayZone.Invoke();
    }

    private bool ScreenToHandLocal(PointerEventData eventData, out Vector2 localPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            handRect, eventData.position, eventData.pressEventCamera, out localPoint);
    }

    private void Elevate()
    {
        elevated = true;
        rect.SetAsLastSibling();
    }
}
