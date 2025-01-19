using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class RoomView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI RoomName_Txt;
    [SerializeField] TextMeshProUGUI RoomJoinCode_TxtHyperLink;
    [SerializeField] Button Leave_Btn;

    [Space(30)]
    [Header("玩家列表")]
    [SerializeField] RectTransform RoomPlayerListNode;
    [SerializeField] GameObject RoomPlayerListSample;

    private List<RoomPlayerListItem> _roomPlayerListItemList;                       // 房間玩家列表

    private void Start()
    {
        // 產生房間玩家列表
        _roomPlayerListItemList = new();
        for (int i = 0; i < GameData.MAX_PLAYER_COUNT; i++)
        {
            RoomPlayerListItem roomPlayerListItem = Instantiate(RoomPlayerListSample, RoomPlayerListNode).GetComponent<RoomPlayerListItem>();
            _roomPlayerListItemList.Add(roomPlayerListItem);
        }
        Destroy(RoomPlayerListSample);
        UpdateRoomView();

        EventListener();
    }

    private void OnEnable()
    {
        LanguageManager.I.SetText(RoomName_Txt, LocalizationTableEnum.Lobby_Table, "Room", $": {RoomManager.I.JoinLobby.Name}");
        LanguageManager.I.SetText(RoomJoinCode_TxtHyperLink, LocalizationTableEnum.Lobby_Table, "Join Code",
            $": <link={RoomManager.I.JoinLobby.Id}>{RoomManager.I.JoinLobby.Id}</link>");
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 加入代碼複製_超連結
        HyperlinkHandler hyperlink = RoomJoinCode_TxtHyperLink.GetComponent<HyperlinkHandler>();
        hyperlink.ClickCallback = (linkId) => 
        {
            Utils.I.CopyText(linkId);
        };

        // 離開按鈕
        Leave_Btn.onClick.AddListener(() =>
        {
            RoomManager.I.LeaveLobby();
            ViewManager.I.CloseCurrView();
            ViewManager.I.OpenView<RectTransform>(ViewEnum.LobbyView);
        });
    }

    /// <summary>
    /// 更新房間
    /// </summary>
    public void UpdateRoomView()
    {
        for (int i = 0; i < _roomPlayerListItemList.Count; i++)
        {
            bool isLock = i >= RoomManager.I.JoinLobby.MaxPlayers;
            _roomPlayerListItemList[i].ResetRoomPlayerListItem(isLock);
        }

        int index = 0;
        foreach (var roomPlayerData in RoomRpcManager.I.RoomPlayerDataList)
        {
            _roomPlayerListItemList[index++].SetRoomPlayerListItem(roomPlayerData);
        }
    }
}