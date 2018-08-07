using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityStandardAssets.Characters.FirstPerson;

public class GameHUD : MonoBehaviour
{
    public GameObject PauseMenuUI;
    public Button LeaveMatchButton;
    public Button CancelPauseButton;
    public GameObject AimCanvas;

    public static bool IsPausedMenuShowing = false;

    void Start()
    {
        PauseMenuUI.SetActive(false);
        LeaveMatchButton.onClick.AddListener(LeaveMatchAction);
        CancelPauseButton.onClick.AddListener(CancelAction);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SetUpPauseLogic();
        }
    }

    private void SetUpPauseLogic()
    {
        if(IsPausedMenuShowing)
        {
            Resume();
        } else
        {
            Pause();
        }
    }

    private void Pause()
    {
        IsPausedMenuShowing = true;
        AimCanvas.SetActive(!IsPausedMenuShowing);
        PauseMenuUI.SetActive(IsPausedMenuShowing);
    }

    private void Resume()
    {
        IsPausedMenuShowing = false;
        AimCanvas.SetActive(!IsPausedMenuShowing);
        PauseMenuUI.SetActive(IsPausedMenuShowing);
    }

    public void LeaveMatchAction()
    {
        GameSparksManager.Instance.GetRTSession().Disconnect();
        GameSparksManager.Instance.GSReset();
        SceneManager.LoadScene(LoadingManager.LOBBY_SCENE);
    }

    public void CancelAction()
    {
        SetUpPauseLogic();
    }

}
