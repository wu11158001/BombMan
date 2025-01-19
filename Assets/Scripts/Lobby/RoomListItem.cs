using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] Image Map_Img;
    [SerializeField] TextMeshProUGUI RoomName_Txt;
    [SerializeField] TextMeshProUGUI JoinCode_Txt;
    [SerializeField] TextMeshProUGUI PlayerCount_Txt;
    [SerializeField] TextMeshProUGUI State_Txt;
    [SerializeField] GameObject UsingPasswordIcon_Obj;
    [SerializeField] Button Join_Btn;

    /// <summary>
    /// 設置房間列表項目
    /// </summary>
    /// <param name="joinLobby"></param>
    public void SetRoomListItem(Lobby joinLobby)
    {
        LanguageManager.I.SetText(RoomName_Txt, LocalizationTableEnum.Lobby_Table, "Room", $": {joinLobby.Name}");
        LanguageManager.I.SetText(JoinCode_Txt, LocalizationTableEnum.Lobby_Table, "Join Code", $": {joinLobby.Id}");

        PlayerCount_Txt.text = $"{joinLobby.Players.Count} / {joinLobby.MaxPlayers}";
        UsingPasswordIcon_Obj.SetActive(joinLobby.HasPassword);

        Join_Btn.onClick.RemoveAllListeners();
        Join_Btn.onClick.AddListener(() =>
        {
            RectTransform waitingView = ViewManager.I.OpenPermanentView(PermanentViewEnum.WaitingView);

            RoomManager.I.JoinRoom(joinLobby, () =>
            {
                Destroy(waitingView.gameObject);
                ViewManager.I.CloseCurrView();
                ViewManager.I.OpenView<RectTransform>(ViewEnum.RoomView);
            });
        });
    }
}
