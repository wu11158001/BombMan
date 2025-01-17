using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyView : MonoBehaviour
{
    [Header("房間按鈕")]
    [SerializeField] Button Quick_Btn;
    [SerializeField] Button Create_Btn;
    [SerializeField] Button Refresh_Btn;

    private void Start()
    {
        EventListener();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 快速加入按鈕
        Quick_Btn.onClick.AddListener(() =>
        {
        });

        // 創建房間按鈕
        Create_Btn.onClick.AddListener(() =>
        {
            ViewManager.I.OpenView<RectTransform>(ViewEnum.CreateRoomView);
        });
        // 刷新房間按鈕
        Refresh_Btn.onClick.AddListener(() =>
        {
        });
    }
}
