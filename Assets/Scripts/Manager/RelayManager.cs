using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

/// <summary>
/// Relay通訊方式
/// </summary>
public enum RelayConnectionTypeEnum
{
    udp,
    dtls,
}

public class RelayManager : UnitySingleton<RelayManager>
{
    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback +=
            (NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse) =>
            {
                connectionApprovalResponse.Approved = true;
            };
    }

    /// <summary>
    /// 創建Relay
    /// </summary>
    /// <param name="maxConnections"></param>
    /// <param name="connectionType"></param>
    public async Task<string> CreateRelay(int maxConnections, RelayConnectionTypeEnum connectionType)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            RelayServerData relayServerData = new(allocation, $"{connectionType}");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);            
            NetworkManager.Singleton.StartHost();
            
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"創建Relay錯誤:{e}");
            return "";
        }
    }

    /// <summary>
    /// 加入Relay
    /// </summary>
    /// <param name="joinCode"></param>
    /// <param name="connectionType"></param>
    public async Task JoinRelay(string joinCode, RelayConnectionTypeEnum connectionType)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new(joinAllocation, $"{connectionType}");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"加入Relay錯誤:{e}");
        }
    }
}