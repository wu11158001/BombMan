using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

public class LobbyView : MonoBehaviour
{
    [Header("房間按鈕")]
    [SerializeField] Button Quick_Btn;
    [SerializeField] Button Create_Btn;
    [SerializeField] Button Refresh_Btn;

    [Space(30)]
    [Header("房間列表")]
    [SerializeField] RectTransform RoomItemListNode;
    [SerializeField] GameObject RoomItemSample;

    private ObjPool _objPool;

    private RectTransform _waitingView;

    private void OnDestroy()
    {
        CancelInvoke(nameof(RefreshRoolList));
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(RefreshRoolList));
    }

    private void OnEnable()
    {
        _waitingView = ViewManager.I.OpenPermanentView(PermanentViewEnum.WaitingView);
        InvokeRepeating(nameof(RefreshRoolList), 1.5f, 5);
    }

    private void Awake()
    {
        _objPool = new ObjPool(transform);
    }

    private void Start()
    {
        EventListener();

        RoomItemSample.SetActive(false);
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 快速加入按鈕
        Quick_Btn.onClick.AddListener(() =>
        {
            RoomManager.I.QuickJoinRoom((joinLobby) =>
            {
                /*找到房間*/
                RectTransform waitingView = ViewManager.I.OpenPermanentView(PermanentViewEnum.WaitingView);

                RoomManager.I.JoinRoom(joinLobby, () =>
                {
                    Destroy(waitingView.gameObject);
                    ViewManager.I.CloseCurrView();
                    ViewManager.I.OpenView<RectTransform>(ViewEnum.RoomView);
                });
            }, () =>
            {
                /*未找到房間*/
                ViewManager.I.OpenView<RectTransform>(ViewEnum.CreateRoomView);
            });
        });

        // 創建房間按鈕
        Create_Btn.onClick.AddListener(() =>
        {
            ViewManager.I.OpenView<RectTransform>(ViewEnum.CreateRoomView);
        });
        // 刷新房間按鈕
        Refresh_Btn.onClick.AddListener(() =>
        {
            RefreshRoolList();
        });
    }

    /// <summary>
    /// 刷新房間列表
    /// </summary>
    private void RefreshRoolList()
    {
        RoomManager.I.QueryLobbiesAsync(RefreshRoolListCallback);
    }
    /// <summary>
    /// 刷新房間列表回傳
    /// </summary>
    /// <param name="queryResponse"></param>
    private void RefreshRoolListCallback(QueryResponse queryResponse)
    {
        List<GameObject> roomItems = _objPool.GetObjList(RoomItemSample);
        foreach (var item in roomItems)
        {
            item.SetActive(false);
        }

        int index = 0;
        foreach (var lobby in queryResponse.Results)
        {
            // 產生房間項目
            RoomListItem roomItem = null;
            if (index >= roomItems.Count)
            {
                roomItem = _objPool.CreateObj<RoomListItem>(RoomItemSample, RoomItemListNode);
            }
            else
            {
                roomItem = roomItems[index].GetComponent<RoomListItem>();
            }
            roomItem.gameObject.SetActive(true);
            roomItem.SetRoomListItem(lobby);
            index++;
        }
        Utils.I.SetGridLayoutSize(RoomItemListNode, false, 1);

        ChangeSceneManager.I.CloseSceneLoadView();
        if(_waitingView != null)
        {
            Destroy(_waitingView.gameObject);
            _waitingView = null;
        }
    }
}
