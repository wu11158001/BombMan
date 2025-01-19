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
    [SerializeField] Button PrepareOrStart_Btn;
    [SerializeField] TextMeshProUGUI PrepareOrStartBtn_Txt;

    [Space(30)]
    [Header("玩家列表")]
    [SerializeField] RectTransform RoomPlayerListNode;
    [SerializeField] GameObject RoomPlayerListSample;

    private List<RoomPlayerListItem> _roomPlayerListItemList;                       // 房間玩家列表

    private RectTransform _waitingView;

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
        _waitingView = ViewManager.I.OpenPermanentView(PermanentViewEnum.WaitingView);

        LanguageManager.I.SetText(RoomName_Txt, LocalizationTableEnum.Lobby_Table, "Room", $": {RoomManager.I.JoinLobby.Name}");
        LanguageManager.I.SetText(RoomJoinCode_TxtHyperLink, LocalizationTableEnum.Lobby_Table, "Join Code",
            $": <link={RoomManager.I.JoinLobby.Id}>{RoomManager.I.JoinLobby.Id}</link>");

        UpdateRoomView();
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
            CloseRoomView();           
        });

        // 準備/開始按鈕
        PrepareOrStart_Btn.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    /*是Host*/
                }
                else
                {
                    /*一般玩家*/
                    RoomPlayerData roomPlayerData = RoomRpcManager.I.GetLocalRoomPlayerData(NetworkManager.Singleton.LocalClientId);
                    roomPlayerData.IsPrepare = !roomPlayerData.IsPrepare;
                    RoomRpcManager.I.UpdateRoomPlayerServerRpc(NetworkManager.Singleton.LocalClientId, roomPlayerData);
                }
            }
        });
    }

    /// <summary>
    /// 關閉房間
    /// </summary>
    public void CloseRoomView()
    {
        RoomManager.I.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        ViewManager.I.CloseCurrView();
        ViewManager.I.OpenView<RectTransform>(ViewEnum.LobbyView);
    }

    /// <summary>
    /// 更新房間
    /// </summary>
    public void UpdateRoomView()
    {
        if (RoomRpcManager.I.RoomPlayerDataList == null ||
            RoomRpcManager.I.RoomPlayerDataList.Count == 0 ||
            _roomPlayerListItemList == null)
        {
            return;
        }

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

        if (NetworkManager.Singleton.IsConnectedClient)
        {
            string keyStr =
                NetworkManager.Singleton.IsHost ?
                "Start" :
                "Prepare";
            LanguageManager.I.SetText(PrepareOrStartBtn_Txt, LocalizationTableEnum.Room_Table, keyStr);
        }

        if (_waitingView != null)
        {
            Destroy(_waitingView.gameObject);
            _waitingView = null;
        }
    }
}