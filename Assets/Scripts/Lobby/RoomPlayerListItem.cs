using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class RoomPlayerListItem : MonoBehaviour
{
    [SerializeField] Button Kick_Btn;
    [SerializeField] TextMeshProUGUI PlayerName_Txt;
    [SerializeField] Image Character_Img;
    [SerializeField] TextMeshProUGUI PrepareOrHost_Txt;
    [SerializeField] Button Voice_Btn;
    [SerializeField] GameObject Lock_Obj;

    /// <summary>
    /// 重製房間玩家列表項目
    /// </summary>
    /// <param name="isLock"></param>
    public void ResetRoomPlayerListItem(bool isLock)
    {
        Kick_Btn.gameObject.SetActive(false);
        PlayerName_Txt.text = "";
        PrepareOrHost_Txt.text = "";
        Voice_Btn.gameObject.SetActive(false);
        Lock_Obj.SetActive(isLock);
    }

    /// <summary>
    /// 設置房間玩家列表項目
    /// </summary>
    /// <param name="roomPlayerData"></param>
    public void SetRoomPlayerListItem(RoomPlayerData roomPlayerData)
    {
        bool isLocalHost = NetworkManager.Singleton.IsHost;
        bool isRoomHost = roomPlayerData.IsRoomHost;
        bool isLocalItem = roomPlayerData.NetworkClientId == NetworkManager.Singleton.LocalClientId;
        
        Kick_Btn.gameObject.SetActive(isLocalHost && !isRoomHost && !isLocalItem);
        Kick_Btn.onClick.RemoveAllListeners();
        Kick_Btn.onClick.AddListener(() =>
        {
            RoomRpcManager.I.KickRoomPlayerServerRpc(roomPlayerData.NetworkClientId);
        });

        PlayerName_Txt.text = $"{roomPlayerData.Nickname}";

        if (isRoomHost)
        {
            /*是Host*/
            LanguageManager.I.SetText(PrepareOrHost_Txt, LocalizationTableEnum.Room_Table, "Host");
        }           
        else
        {
            // 準備狀態
            if (roomPlayerData.IsPrepare)
            {
                LanguageManager.I.SetText(PrepareOrHost_Txt, LocalizationTableEnum.Room_Table, "Prepare");
            }
            else
            {
                PrepareOrHost_Txt.text = "";
            }
        }
    }
}
