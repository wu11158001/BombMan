using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class HyperlinkHandler : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI textMeshProUGUI;
    public UnityAction<string> ClickCallback { get; set; }

    void Start()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    // 處理點擊事件
    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshProUGUI, Input.mousePosition, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textMeshProUGUI.textInfo.linkInfo[linkIndex];
            string linkId = linkInfo.GetLinkID();

            ClickCallback?.Invoke(linkId);
        }
    }
}
