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
    /// 創建房間
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="maxPlayers"></param>
    /// <param name="callback"></param>
    public async void CreateRoom(string roomName, int maxPlayers, UnityAction<Lobby> callback)
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
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, createLobbyOptions);
            JoinLobby = lobby;

            UnityMainThreadDispatcher.I.Enqueue(() =>
            {
                callback?.Invoke(JoinLobby);
            });

            InvokeRepeating(nameof(HandleLobbyHeartbeat), 15, 15);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"創建房間錯誤:{e}");
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
