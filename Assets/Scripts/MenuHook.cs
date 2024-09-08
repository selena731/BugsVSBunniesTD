using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Game;
using UnityEngine;

public class MenuHook : MonoBehaviour
{
    public KeyCode ActivationKey=KeyCode.Escape;

    public CanvasGroup ActivateOnPress;
    
    public MenuManager MenuManager;
    public FadeType myFadeType;
    public enum FadeType {TweenEnable,TweenEnableImmediate };

    public bool PauseIfEnabled;
    public static bool IsPaused;
    private void OnDisable()
    {
        if(PauseIfEnabled)
        {
            IsPaused = false;
            // Time.timeScale = 1f;
            GameSpeed.ResumeGame(eTimeScale.GameTimeScale);
        }
    }
    private void OnEnable()
    {
        if(PauseIfEnabled)
        {
            IsPaused = true;
            // Time.timeScale = 0f;
            GameSpeed.PauseGame(eTimeScale.GameTimeScale);
        }
    }
    private void OnDestroy()
    {
        if (PauseIfEnabled)
        {
            IsPaused = false;
            // Time.timeScale = 1;
            GameSpeed.ResumeGame(eTimeScale.GameTimeScale);
        }
    }
    void Update()
    {
        if(Input.GetKeyDown(ActivationKey))
        {
            if (myFadeType == FadeType.TweenEnable)
            {
                MenuManager?.TweenEnable(ActivateOnPress);
            }
            else
            {
                MenuManager?.EnableImmediate(ActivateOnPress);

            }
        }
    }
}
