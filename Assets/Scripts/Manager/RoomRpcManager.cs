using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;
using System.Linq;

/// <summary>
/// 房間玩家資料
/// </summary>
public struct RoomPlayerData : INetworkSerializable, IEquatable<RoomPlayerData>
{
    public ulong NetworkClientId;
    public FixedString64Bytes Nickname;
    public int Character;
    public bool IsPrepare;
    public bool IsRoomHost;

    public bool Equals(RoomPlayerData other)
    {
        return NetworkClientId == other.NetworkClientId &&
               Nickname.Equals(other.Nickname) &&
               Character == other.Character &&
               IsPrepare == other.IsPrepare &&
               IsRoomHost == other.IsRoomHost;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NetworkClientId, Nickname, Character, IsPrepare, IsRoomHost);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref NetworkClientId);
        serializer.SerializeValue(ref Nickname);
        serializer.SerializeValue(ref Character);
        serializer.SerializeValue(ref IsPrepare);
        serializer.SerializeValue(ref IsRoomHost);
    }
}

public class RoomRpcManager : NetworkBehaviour
{
    private static RoomRpcManager _instance;
    public static RoomRpcManager I { get { return _instance; } }

    private NetworkObject _networkObject;

    public NetworkList<RoomPlayerData> RoomPlayerDataList { get; private set; }
        = new(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        if (_instance == null) _instance = this;
        DontDestroyOnLoad(this);

        _networkObject = GetComponent<NetworkObject>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.LogError("產生");

        if (NetworkManager.Singleton.IsServer)
        {
            RoomPlayerDataList.Clear();
        }

        // 註冊_房間玩家資料變化
        RoomPlayerDataList.OnListChanged -= OnRoomPlayerDataListChange;
        RoomPlayerDataList.OnListChanged += OnRoomPlayerDataListChange;

        // 新增玩家
        AddNewRoomPlayerServerRpc(
            new RoomPlayerData()
            {
                NetworkClientId = NetworkManager.Singleton.LocalClientId,
                Nickname = PlayerPrefs.GetString(LocalSaveKey.LOCAL_NICKNAME_KEY),
                Character = 0,
                IsPrepare = false,
                IsRoomHost = NetworkManager.Singleton.IsHost,
            });
    }

    /// <summary>
    /// 房間玩家資料變更
    /// </summary>
    /// <param name="changeEvent"></param>
    private void OnRoomPlayerDataListChange(NetworkListEvent<RoomPlayerData> changeEvent)
    {
        RoomView roomView = null;
        GameObject roomViewObj = GameObject.Find("RoomView");
        if (roomViewObj != null)
        {
            roomView = roomViewObj.GetComponent<RoomView>();
        }

        if (roomView != null && roomView.gameObject.activeSelf)
        {
            roomView.UpdateRoomView();
        }
    }

    /// <summary>
    /// 新增房間玩家
    /// </summary>
    /// <param name="newPlayer"></param>
    [ServerRpc(RequireOwnership =false)]
    public void AddNewRoomPlayerServerRpc(RoomPlayerData newPlayer)
    {
        Debug.Log($"新增房間玩家: {newPlayer.NetworkClientId}");
        RoomPlayerDataList.Add(newPlayer);
    }

    /// <summary>
    /// 移除房間玩家
    /// </summary>
    /// <param name="networkClientId"></param>
    [ServerRpc(RequireOwnership =false)]
    public void RemoveRoomPlayerServerRpc(ulong networkClientId)
    {
        RoomPlayerData removeData = new();
        foreach (var roomPlayerData in RoomPlayerDataList)
        {
            if (roomPlayerData.NetworkClientId == networkClientId)
            {
                removeData = roomPlayerData;
                break;
            }
        }

        if (removeData.NetworkClientId == networkClientId)
        {
            RoomPlayerDataList.Remove(removeData);
        }
        else
        {
            Debug.LogError($"玩家: {networkClientId} 移除房間玩家錯誤");
        }
    }
}
