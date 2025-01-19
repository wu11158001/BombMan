using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Unity.Netcode;

/// <summary>
/// 房間資料字典Key列表
/// </summary>
public enum LobbyDataKeyEnum
{
    RelayJoinCode,          // Relay加入權杖
    RelayConnectionType,    // Relay通訊方式
    Map,                    // 地圖
}

public class RoomManager : UnitySingleton<RoomManager>
{
    public Lobby JoinLobby { get; private set; }                // 加入的房間

    /// <summary>
    /// 查詢房間列表
    /// </summary>
    /// <param name="callback"></param>
    public async void QueryLobbiesAsync(UnityAction<QueryResponse> callback)
    {
        try
        {
            // 篩選排序房間
            QueryLobbiesOptions queryLobbiesOptions = new()
            {
                Order = new()
                {
                    new QueryOrder(true, QueryOrder.FieldOptions.Created)
                }
            };

            // 查詢房間
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            callback?.Invoke(queryResponse);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"查詢房間列表錯誤:{e}");
        }
    }

    /// <summary>
    /// 創建房間
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="maxPlayers"></param>
    /// <param name="password"></param>
    /// <param name="callback"></param>
    public async void CreateRoom(string roomName, int maxPlayers, string password, UnityAction<Lobby> callback)
    {
        try
        {
            RelayConnectionTypeEnum relayConnectionType = RelayConnectionTypeEnum.dtls;
            string relayJoinCode = await RelayManager.I.CreateRelay(maxPlayers - 1, relayConnectionType);

            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>()
                {
                    { $"{LobbyDataKeyEnum.RelayJoinCode}", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode)},
                    { $"{LobbyDataKeyEnum.RelayConnectionType}", new DataObject(DataObject.VisibilityOptions.Public, $"{relayConnectionType}")},
                    { $"{LobbyDataKeyEnum.Map}", new DataObject(DataObject.VisibilityOptions.Public, "0")},
                },
            };
            // 有設置密碼
            if (password.Length == 8)
            {
                createLobbyOptions.Password = password;
            }

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, createLobbyOptions);
            JoinLobby = lobby;

            Debug.Log($"創建房間: LobbyId: {lobby.Id}, RelayJoinCode: {relayJoinCode}");

            callback?.Invoke(JoinLobby);

            InvokeRepeating(nameof(HandleLobbyHeartbeat), 15, 15);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"創建房間錯誤:{e}");
        }
    }

    /// <summary>
    /// 加入房間
    /// </summary>
    /// <param name="joinLobby"></param>
    /// <param name="callback"></param>
    public async void JoinRoom(Lobby joinLobby, UnityAction callback)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new()
            {

            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(joinLobby.Id, joinLobbyByIdOptions);
            JoinLobby = lobby;

            string relayJoinCode = joinLobby.Data[$"{LobbyDataKeyEnum.RelayJoinCode}"].Value;
            RelayConnectionTypeEnum relayConnectionType =
                (RelayConnectionTypeEnum)Enum.Parse(typeof(RelayConnectionTypeEnum), joinLobby.Data[$"{LobbyDataKeyEnum.RelayConnectionType}"].Value);
            await RelayManager.I.JoinRelay(relayJoinCode, relayConnectionType);

            callback?.Invoke();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"加入房間錯誤:{e}");
        }
    }

    /// <summary>
    /// 快速加入房間
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="notFindCallback"></param>
    public async void QuickJoinRoom(UnityAction<Lobby> callback, UnityAction notFindCallback)
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new()
            {

            };

            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            JoinLobby = lobby;

            string relayJoinCode = lobby.Data[$"{LobbyDataKeyEnum.RelayJoinCode}"].Value;
            RelayConnectionTypeEnum relayConnectionType =
                (RelayConnectionTypeEnum)Enum.Parse(typeof(RelayConnectionTypeEnum), lobby.Data[$"{LobbyDataKeyEnum.RelayConnectionType}"].Value);
            await RelayManager.I.JoinRelay(relayJoinCode, relayConnectionType);

            callback?.Invoke(JoinLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"'快速加入房間'未找到房間:{e}");
            notFindCallback?.Invoke();
        }
    }

    /// <summary>
    /// 離開房間
    /// </summary>
    public async void LeaveLobby()
    {
        try
        {
            if (JoinLobby != null)
            {
                CancelInvoke(nameof(HandleLobbyHeartbeat));
                await LobbyService.Instance.RemovePlayerAsync(JoinLobby.Id, AuthenticationService.Instance.PlayerId);
                JoinLobby = null; 
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"離開房間錯誤:{e}");
        }
    }

    /// <summary>
    /// 處理房間心跳
    /// </summary>
    public async void HandleLobbyHeartbeat()
    {
        if (IsRoomHost())
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(JoinLobby.Id);
        }
    }

    /// <summary>
    /// 是否是室長
    /// </summary>
    /// <returns></returns>
    public bool IsRoomHost()
    {
        if (JoinLobby != null)
        {
            return JoinLobby.HostId == AuthenticationService.Instance.PlayerId;
        }

        return false;
    }
}
