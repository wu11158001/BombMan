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
    [SerializeField] TextMeshProUGUI PasswordTip_Txt;

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
        PasswordTip_Txt.gameObject.SetActive(false);

        EventListener();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 房間人數滑條
        RoomPlayerCount_Sli.onValueChanged.AddListener((value) =>
        {
            RoomPlayerCount_Txt.text = $"{RoomPlayerCount_Sli.value}";
        });

        // 房間密碼Tog
        Password_Tog.onValueChanged.AddListener((isOn) =>
        {
            Password_If.interactable = isOn;
            Password_If.text = "";
            PasswordTip_Txt.gameObject.SetActive(isOn);
        });

        // 密碼輸入框
        Password_If.onValueChanged.AddListener((value) =>
        {
            PasswordTip_Txt.gameObject.SetActive(value.Length != 8);
        });

        // 創建房間按鈕
        Create_Btn.onClick.AddListener(() =>
        {
            RectTransform waitingView = ViewManager.I.OpenPermanentView(PermanentViewEnum.WaitingView);

            string roomName = RoomName_If.text.Trim();
            int maxPlayers = (int)RoomPlayerCount_Sli.value;
            string password = $"{Password_If.text}";

            if (roomName.Length == 0)
            {
                roomName = PlayerPrefs.GetString(LocalSaveKey.LOCAL_NICKNAME_KEY);
            }

            RoomManager.I.CreateRoom(roomName, maxPlayers, password, (joinLobby) =>
            {
                Destroy(waitingView.gameObject);
                ViewManager.I.CloseCurrView();
                ViewManager.I.CloseCurrView();
                ViewManager.I.OpenView<RectTransform>(ViewEnum.RoomView);
            });
        });
    }
}
