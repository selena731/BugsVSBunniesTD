using System;
using UnityEngine;

namespace DefaultNamespace.Game
{
    public enum eTimeScale
    {
        UITimeScale,
        GameTimeScale,
    }
    
    public static class GameSpeed
    {
        private static float _uiTimeScale = 1;
        private static float _gameTimeScale = 1;

        public static float UIDeltaTime => _uiTimeScale * Time.deltaTime;
        public static float GameDeltaTime => _gameTimeScale * Time.deltaTime;

        public static float TimeScale(eTimeScale eTimeScale)
        {
            switch (eTimeScale)
            {
                case eTimeScale.UITimeScale:
                    return _uiTimeScale;
                case eTimeScale.GameTimeScale:
                    return _gameTimeScale;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eTimeScale), eTimeScale, null);
            }
        }

        public static void PauseGame(eTimeScale timeScale)
        {
            switch (timeScale)
            {
                case eTimeScale.UITimeScale:
                    _uiTimeScale = 0;
                    break;
                case eTimeScale.GameTimeScale:
                    _gameTimeScale = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeScale), timeScale, null);
            }
        }

        public static void ResumeGame(eTimeScale timeScale)
        {
            switch (timeScale)
            {
                case eTimeScale.UITimeScale:
                    _uiTimeScale = 1;
                    break;
                case eTimeScale.GameTimeScale:
                    _gameTimeScale = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeScale), timeScale, null);
            }
        }

        public static void TimeScale(eTimeScale timeScale, float newScale)
        {
            switch (timeScale)
            {
                case eTimeScale.UITimeScale:
                    if (_uiTimeScale == newScale)
                        _uiTimeScale = 1;
                    else
                        _uiTimeScale = newScale;
                    break;
                case eTimeScale.GameTimeScale:
                    if (_gameTimeScale == newScale)
                        _gameTimeScale = 1;
                    else
                        _gameTimeScale = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeScale), timeScale, null);
            }
        }

        public static bool GameTimerCheck(ref float passedTime, float timeRate)
        {
            bool isChecked = false;
            passedTime += GameSpeed.GameDeltaTime;
            if (passedTime > timeRate)
            {
                passedTime = 0;
                isChecked = true;
            }

            return isChecked;
        }

        public static void ResetTimeScales()
        {
            _uiTimeScale = 1;
            _gameTimeScale = 1;
        }
    }
}