using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateRoomView : BasePopUpView
{
    [Space(30)]
    [Header("房間名稱")]
    [SerializeField] TMP_InputField RoomName_If;

    [Space(30)]
    [Header("房間人數")]
    [SerializeField] TextMeshProUGUI RoomPlayerCount_Txt;
    [SerializeField] Slider RoomPlayerCount_Sli;

    [Space(30)]
    [Header("房間密碼")]
    [SerializeField] Toggle Password_Tog;
    [SerializeField] TMP_InputField Password_If;

    [Space(30)]
    [Header("創建房間按鈕")]
    [SerializeField] Button Create_Btn;

    private void Start()
    {
        // 預設房間名稱
        string recodeNickname = PlayerPrefs.GetString(LocalSaveKey.LOCAL_NICKNAME_KEY);
        RoomName_If.text = recodeNickname;

        // 預設房間人數
        RoomPlayerCount_Sli.minValue = GameData.MIN_PLAYER_COUNT;
        RoomPlayerCount_Sli.maxValue = GameData.MAX_PLAYER_COUNT;
        RoomPlayerCount_Sli.value = GameData.MAX_PLAYER_COUNT;
        RoomPlayerCount_Txt.text = $"{RoomPlayerCount_Sli.value}";

        // 預設房間密碼
        Password_Tog.isOn = false;
        Password_If.interactable = false;

        EventListener();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 房間密碼Tog
        Password_Tog.onValueChanged.AddListener((isOn) =>
        {
            Password_If.interactable = isOn;
            Password_If.text = "";
        });

        // 創建房間按鈕
        Create_Btn.onClick.AddListener(() =>
        {
            string roomName = RoomName_If.text.Trim();
            int maxPlayers = (int)RoomPlayerCount_Sli.value;

            if (roomName.Length == 0)
            {
                roomName = PlayerPrefs.GetString(LocalSaveKey.LOCAL_NICKNAME_KEY);
            }

            RoomManager.I.CreateRoom(roomName, maxPlayers, (joinLobby) =>
            {
                ViewManager.I.CloseCurrView();
                ViewManager.I.CloseCurrView();
                ViewManager.I.OpenView<RectTransform>(ViewEnum.RoomView);
            });
        });
    }
}
