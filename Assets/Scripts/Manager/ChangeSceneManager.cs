using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 場景列表
/// </summary>
public enum SceneEnum
{
    Entry,                  // 入口
    Lobby,                  // 大廳
    Game,                   // 遊戲
}

public class ChangeSceneManager : UnitySingleton<ChangeSceneManager>
{
    private RectTransform _sceneLoadView;           // 載入場景介面

    /// <summary>
    /// 轉換場景
    /// </summary>
    /// <param name="scene"></param>
    public void ChangeScene(SceneEnum scene)
    {
        StartCoroutine(ILoadSceneAsync(scene));
    }
    private IEnumerator ILoadSceneAsync(SceneEnum scene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync($"{scene}");

        // 等待場景加載完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log($"進入場景:{scene} !");

        ViewManager.I.ResetViewData();
        _sceneLoadView = ViewManager.I.OpenSceneLoadView();

        // 產生場景初始化
        switch (scene)
        {
            // 大廳
            case SceneEnum.Lobby:
                ViewManager.I.OpenView<RectTransform>(ViewEnum.LobbyView, (view) =>
                {
                    CloseSceneLoadView();
                });
                break;
        }
    }

    /// <summary>
    /// 關閉場景載入介面
    /// </summary>
    public void CloseSceneLoadView()
    {
        if (_sceneLoadView)
        {
            _sceneLoadView.gameObject.SetActive(false);
        }
    }
}
