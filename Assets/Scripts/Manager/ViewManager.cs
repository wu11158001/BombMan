using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

// 一般介面
public enum ViewEnum
{
    LobbyView,
}

/// <summary>
/// 常駐介面列表
/// </summary>
public enum PermanentViewEnum
{
    SceneLoadView,                  // 場景轉換介面
}

public class ViewManager : UnitySingleton<ViewManager>
{
    private Dictionary<ViewEnum, RectTransform> _viewDic = new();                       // 一般介面
    private Dictionary<PermanentViewEnum, RectTransform> _permanentView = new();        // 常駐介面

    private Stack<RectTransform> _openedView = new();                                   // 已開啟介面

    public RectTransform CanvasRt { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    public IEnumerator Init()
    {
        // 常駐介面
        foreach (var permanentEnum in Enum.GetValues(typeof(PermanentViewEnum)))
        {
            ResourceRequest resourceRequest = Resources.LoadAsync<RectTransform>($"PermanentView/{permanentEnum}");
            yield return resourceRequest;
            RectTransform loadedResource = resourceRequest.asset as RectTransform;
            _permanentView.Add((PermanentViewEnum)permanentEnum, loadedResource);
        }

        Debug.Log("介面腳本準備完成。");
    }

    /// <summary>
    /// 設置當前場景Canvas
    /// </summary>
    private void SetCanvas()
    {
        CanvasRt = FindAnyObjectByType<Canvas>().GetComponent<RectTransform>();
    }

    /// <summary>
    /// 重製介面資料
    /// </summary>
    public void ResetViewData()
    {
        SetCanvas();
        _openedView.Clear();
    }

    /// <summary>
    /// 關閉當前介面
    /// </summary>
    public void CloseCurrView()
    {
        _openedView.Pop().gameObject.SetActive(false);
    }

    /// <summary>
    /// 產生介面處理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rt"></param>
    /// <param name="callback"></param>
    public void CreateViewHandle<T>(RectTransform rt, UnityAction<T> callback = null) where T : Component
    {
        rt.gameObject.SetActive(true);
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.eulerAngles = Vector3.zero;
        rt.localScale = Vector3.one;
        rt.name = rt.name.Replace("(Clone)", "");
        rt.SetSiblingIndex(CanvasRt.childCount + 1);

        // 獲取Component
        if (callback != null)
        {
            T component = rt.GetComponent<T>();
            if (component != null)
            {
                callback?.Invoke(component);
            }
            else
            {
                Debug.LogError($"{rt.name}: 介面不存在 Component");
            }
        }
    }

    /// <summary>
    /// 開啟介面
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="viewName"></param>
    /// <param name="callback"></param>
    public void OpenView<T>(ViewEnum viewName, UnityAction<T> callback = null) where T : Component
    {
        if (CanvasRt == null)
        {
            SetCanvas();
        }

        if (_viewDic.ContainsKey(viewName))
        {
            RectTransform view = _viewDic[viewName];
            CreateViewHandle(view, callback);
            _openedView.Push(view);
        }
        else
        {
            RectTransform newView = Resources.Load<RectTransform>($"View/{viewName}");
            if (newView != null)
            {
                CreateViewHandle(newView, callback);
                _openedView.Push(newView);
            }
            else
            {
                Debug.LogError($"載入介面失敗: {viewName}");
            }
        }
    }

    /// <summary>
    /// 開啟場景轉換介面
    /// </summary>
    /// <returns></returns>
    public RectTransform OpenSceneLoadView()
    {
        if (CanvasRt == null)
        {
            SetCanvas();
        }

        RectTransform sceneLoadView = _permanentView[PermanentViewEnum.SceneLoadView];
        RectTransform view = Instantiate(sceneLoadView, CanvasRt).GetComponent<RectTransform>();
        CreateViewHandle<RectTransform>(view);

        return view;
    }
}
