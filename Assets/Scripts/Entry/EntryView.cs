using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class EntryView : MonoBehaviour
{
    private static EntryView _instance;
    public static EntryView I { get { return _instance; } }

    [Header("Debug 工具")]
    [SerializeField] GameObject DebugTool;
    [SerializeField] bool IsUsingDebugTool;

    [Space(30)]
    [Header("設置暱稱")]
    [SerializeField] TMP_InputField SetNickname_If;
    [SerializeField] TextMeshProUGUI NicknameError_Txt;
    
    [Space(30)]
    [Header("語言")]
    [SerializeField] Toggle Chinese_Tog;
    [SerializeField] Toggle English_Tog;

    [Space(30)]
    [Header("載入畫面")]
    [SerializeField] GameObject Load_Obj;

    [Space(30)]
    [Header("進入遊戲按鈕")]
    [SerializeField] Button Enter_Btn;

    private Coroutine _nicknameError_Coroutine;

    private void Awake()
    {
        if (_instance == null) _instance = this;

        DebugTool.SetActive(IsUsingDebugTool);
    }

    private IEnumerator Start()
    {
        Load_Obj.SetActive(true);
        yield return IProjectInit();

        Load_Obj.SetActive(false);
        NicknameError_Txt.gameObject.SetActive(false);
        switch (LanguageManager.I.CurrLanguage)
        {
            // 中文
            case 0:
                Chinese_Tog.isOn = true;
                break;

            // 英文
            case 1:
                English_Tog.isOn = true;
                break;

            // 預設中文
            default:
                Chinese_Tog.isOn = true;
                break;
        }

        string recodeNickname = PlayerPrefs.GetString(LocalSaveKey.LOCAL_NICKNAME_KEY);
        SetNickname_If.Select();
        SetNickname_If.text = recodeNickname;

        EventListener();
    }

    private void Update()
    {
        // 發送進入遊戲
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            EnterTheGame();
        }
    }

    /// <summary>
    /// 專案初始準備
    /// </summary>
    /// <returns></returns>
    private IEnumerator IProjectInit()
    {
        LanguageManager.I.Init();

        yield return UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"登入ID:{AuthenticationService.Instance.PlayerId}");
        };
        yield return AuthenticationService.Instance.SignInAnonymouslyAsync();

        yield return ViewManager.I.Init();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 語言_中文
        Chinese_Tog.onValueChanged.AddListener((isOn) =>
        {
            if(isOn)
            {
                LanguageManager.I.ChangeLanguage(0);
            }            
        });

        // 語言_英文
        English_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                LanguageManager.I.ChangeLanguage(1);
            }            
        });

        // 暱稱輸入框
        SetNickname_If.onValueChanged.AddListener((value) =>
        {
            NicknameError_Txt.gameObject.SetActive(false);
        });

        // 進入遊戲按鈕
        Enter_Btn.onClick.AddListener(() =>
        {
            EnterTheGame();
        });
    }

    /// <summary>
    /// 暱稱格式錯誤提示效果
    /// </summary>
    private void NicknameErrorTipEffect()
    {
        if (_nicknameError_Coroutine != null)
        {
            StopCoroutine(_nicknameError_Coroutine);
        }

        _nicknameError_Coroutine = StartCoroutine(INicknameErrorTipEffect());
    }
    private IEnumerator INicknameErrorTipEffect()
    {
        if (NicknameError_Txt.gameObject.activeSelf)
        {
            NicknameError_Txt.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
        }

        NicknameError_Txt.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        NicknameError_Txt.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        NicknameError_Txt.gameObject.SetActive(true);
    }

    /// <summary>
    /// 進入遊戲
    /// </summary>
    private void EnterTheGame()
    {
        // 暱稱格式錯誤
        if (SetNickname_If.text.Trim().Length < 2)
        {
            NicknameErrorTipEffect();            
            return;
        }

        PlayerPrefs.SetString(LocalSaveKey.LOCAL_NICKNAME_KEY, SetNickname_If.text);
        ChangeSceneManager.I.ChangeScene(SceneEnum.Lobby);
    }
}
