/*
MIT License
Copyright (c) 2021 Anton Kesy
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [Header("Manager")] [SerializeField] private PlayerManager playerManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraManager cameraManager;

    [SerializeField] private AudioManager audioManager;
    private int _highScore;

    //Flags
    private bool _isRunning;

    //Score
    private int _score;

    private bool _waitingToRestartGame;
    private bool _waitingToStartFreshGame;

    private bool _isAutoLoginEnabled;

    private void Awake()
    {
        GPSManager.Activate();
    }

    private void Start()
    {
        LoadFlagsFromPlayerPrefs();
        Application.targetFrameRate = 60;
        uiManager.ShowStartMenuUI();
        if (_isAutoLoginEnabled) GPSManager.SignInToGooglePlayServices();
        audioManager.PlayStartApplication();
        playerManager.SpawnPlayer();
    }


    private void Update()
    {
        //DEBUG----------------------------------------------
        ProcessEditorInput();
        //-------------------------------------------------
        ProcessUserInput();
    }

    private void LoadFlagsFromPlayerPrefs()
    {
        _isAutoLoginEnabled = PlayerPrefs.GetInt("autologin", 1) == 1;
        audioManager.SetSound(PlayerPrefs.GetInt("soundOff", 1) == 0);
    }

    private void ProcessEditorInput()
    {
        if (Input.GetKeyDown("space")) OnUserClick();
    }

    private void ProcessUserInput()
    {
        if (Input.touchCount <= 0 || Input.GetTouch(0).phase != TouchPhase.Began) return;
        if (Input.touches.Select(touch => touch.fingerId)
            .Any(id => EventSystem.current.IsPointerOverGameObject(id)))
            return;

        OnUserClick();
    }

    private void OnUserClick()
    {
        if (_isRunning)
        {
            if (playerManager.CallChangeDirection()) audioManager.PlayTapPlayer();
        }
        else if (_waitingToStartFreshGame)
        {
            if (IsClickForGame()) StartFreshGame();
        }

        else if (_waitingToRestartGame)
        {
            if (IsClickForGame()) RestartGame();
        }
    }

    private bool IsClickForGame()
    {
        if (uiManager.isAboutOn()) return false;
        audioManager.PlayTapPlayer();
        return true;
    }

    internal void ReadyToStartGameCallback()
    {
        _waitingToStartFreshGame = true;
    }

    private void RestartGame()
    {
        _waitingToRestartGame = false;
        StartGame();
        levelManager.RestartGame();
    }


    private void StartFreshGame()
    {
        _waitingToStartFreshGame = false;
        levelManager.StartFreshGame();
        StartGame();
    }

    private void StartGame()
    {
        _score = 0;
        uiManager.UpdateScoreText(_score);
        _isRunning = true;
        uiManager.ShowPlayUI();
        playerManager.StartMoving();
    }

    internal void CoinPickedUpCallback()
    {
        audioManager.PlayCollectCoin();
        uiManager.UpdateScoreText(++_score);
        levelManager.AddSpeed();
    }

    internal void DestroyPlayerCallback()
    {
        OnPlayerDestroy();
    }

    private void OnPlayerDestroy()
    {
        audioManager.PlayDestroyPlayer();
        cameraManager.StartShaking();
        _isRunning = false;
        uiManager.ShowReturningMenuUI();
        StartCoroutine(WaitToRestart());
        CheckNewHighScore();
        GPSManager.CheckAchievement(_score);
        levelManager.LostGame();
        playerManager.SpawnPlayer();
    }

    private void CheckNewHighScore()
    {
        if (_highScore >= _score) return;
        _highScore = _score;
        uiManager.UpdateHighScoreText(_highScore);
        PlayerPrefs.SetInt("highScore", _highScore);
        audioManager.PlayNewHighScore();
        uiManager.FadeInNewHighScore();
        GPSManager.AddHighScore(_highScore);
    }


    private IEnumerator WaitToRestart()
    {
        yield return new WaitForSecondsRealtime(1f);
        _waitingToRestartGame = true;
    }
}