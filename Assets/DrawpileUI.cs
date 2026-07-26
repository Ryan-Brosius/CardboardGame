using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DrawpileUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] DeckManager deckManager;
    [SerializeField] GameObject drawPilePanel;
    [SerializeField] RectTransform textParent;
    [SerializeField] GameObject cardTitleText;

    private void Start()
    {
        CloseDrawPile();
    }

    private void OpenDrawPile()
    {
        foreach (CardData card in deckManager.DrawPile)
        {
            var newText = Instantiate(cardTitleText, textParent);
            newText.GetComponent<TextMeshProUGUI>().text = card.title;
            newText.SetActive(true);
        }
        drawPilePanel.SetActive(true);
    }

    private void CloseDrawPile()
    {
        TextMeshProUGUI[] allTexts = textParent.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI child in allTexts)
        {
            Destroy(child.gameObject);
        }
        drawPilePanel.SetActive(false);
    }

    public void ToggleDrawPile()
    {
        if (drawPilePanel.activeSelf) CloseDrawPile();
        else if (!drawPilePanel.activeSelf) OpenDrawPile();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (drawPilePanel.activeSelf)CloseDrawPile();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!drawPilePanel.activeSelf) OpenDrawPile();
    }
}
