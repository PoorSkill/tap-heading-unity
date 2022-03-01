using System.Collections;
using tap_heading.Game.States;
using tap_heading.manager;
using tap_heading.Player;
using tap_heading.Services;
using tap_heading.Services.Google;
using tap_heading.input;
using UnityEngine;

namespace tap_heading.Game
{
    public class GameManager : MonoBehaviour, IGameManager, ISignInListener, IPlayerInputListener

    {
        [SerializeField] private ManagerCollector managers;

        private static int _highScore;

        private int _score;

        private bool _isIarPopUpPossible;
        private const int TimesToOpenB4IarCall = 10;
        private const int TimesToPlayB4IarCall = 50;

        [SerializeField] internal bool isSingleClick = true;

        private IGameState _gameState = new Running();

        private void Start()
        {
            SetHighScoreLocal();
            LoadFlagsFromPlayerPrefs();
            managers.GetUIManager().ShowStartMenuUI();
            managers.GetAudioManager().PlayStartApplication();
            managers.GetPlayerManager().Spawn();

            var input = gameObject.AddComponent<TouchInput>();
            input.AddListener(this);
#if UNITY_EDITOR
            var inputDebug = gameObject.AddComponent<DebugEditorInput>();
            inputDebug.AddListener(this);
#endif
        }

        private void LoadFlagsFromPlayerPrefs()
        {
            _isIarPopUpPossible = managers.GetSettings().GetTimesOpen() < TimesToOpenB4IarCall &&
                                  managers.GetSettings().GetTimesPlayed() < TimesToPlayB4IarCall;

            managers.GetAudioManager().SetSound(managers.GetSettings().IsSoundOn());
        }

        private void SetHighScore(int value)
        {
            _highScore = value;
            managers.GetUIManager().UpdateHighScoreText(_highScore);
            managers.GetSettings().SetLocalHighScore(_highScore);
        }

        private void SetHighScoreLocal()
        {
            SetHighScore(managers.GetSettings().GetLocalHighScore());
        }

        private void OverwriteGPSHighScore()
        {
            GooglePlayServicesManager.Instance.SubmitScore(managers.GetSettings().GetLocalHighScore());
        }

        private void SetHighScoreFromGPS(long highScore)
        {
            SetHighScore((int) highScore);
        }


        private void OnUserClick(Vector2 position)
        {
        }

        private void UserInteractionWhilePlaying(Vector2 position)
        {
            var playerManager = managers.GetPlayerManager();
            var changedDirection = isSingleClick
                ? playerManager.ChangeDirection()
                : position.x > Screen.width / 2.0f
                    ? playerManager.ChangeDirection(IPlayerManager.Direction.Right)
                    : playerManager.ChangeDirection(IPlayerManager.Direction.Left);

            //play click audio if changed direction
            if (changedDirection)
            {
                managers.GetAudioManager().PlayPlayerTap();
            }
        }

        public void PlayerChangeDirection(Vector2 clickPosition)
        {
            UserInteractionWhilePlaying(clickPosition);
        }

        public bool IsClickForGame()
        {
            if (managers.GetUIManager().isAboutOn()) return false;
            managers.GetAudioManager().PlayPlayerTap();
            return true;
        }

        public void ReadyToStartGameCallback()
        {
            _gameState = new WaitingRestart();
        }

        public void SetSingleClick(bool isSingle)
        {
            isSingleClick = isSingle;
        }


        public void Restart()
        {
            _score = 0;
            var uiManager = managers.GetUIManager();
            uiManager.UpdateScoreText(_score);
            _gameState = new Running();
            uiManager.ShowPlayUI();
            managers.GetPlayerManager().StartMoving();
            managers.GetLevelManager().Restart();
        }

        public void CoinPickedUpCallback()
        {
            managers.GetAudioManager().PlayCollectCoin();
            managers.GetUIManager().UpdateScoreText(++_score);
            managers.GetLevelManager().IncreaseSpeed();
        }

        public void DestroyPlayerCallback()
        {
            OnPlayerDestroy();
        }

        private void OnPlayerDestroy()
        {
            managers.GetAudioManager().PlayPlayerDeath();
            managers.GetCameraManager().StartShaking();
            _gameState = new WaitForAnimation();
            managers.GetUIManager().ShowReturningMenuUI();
            StartCoroutine(WaitToRestart());
            CheckNewHighScore();
            GooglePlayServicesManager.Instance.SubmitScore(_score);
            GooglePlayServicesManager.Instance.CheckAchievement(_score);
            managers.GetLevelManager().EndLevel();
            managers.GetPlayerManager().Spawn();
            CheckForIARPopUp();
        }

        private void CheckNewHighScore()
        {
            if (_highScore >= _score) return;
            SetHighScore(_score);
            managers.GetAudioManager().PlayNewHighScore();
            managers.GetUIManager().FadeInNewHighScore();
        }

        private IEnumerator WaitToRestart()
        {
            yield return new WaitForSecondsRealtime(1f);
            _gameState = new WaitingRestart();
        }

        private void CheckForIARPopUp()
        {
            if (!_isIarPopUpPossible) return;
            if (managers.GetSettings().GetTimesPlayed() > TimesToPlayB4IarCall ||
                managers.GetSettings().GetTimesOpen() > TimesToOpenB4IarCall)
            {
                IAReviewManager.Instance.RequestReview();
            }

            managers.GetSettings().IncrementTimesOpen();
            managers.GetSettings().IncrementTimesPlayed();
        }

        public void OnSignInSuccess(long playerScoreValue)
        {
            OverwriteGPSHighScore();
            SetHighScoreFromGPS(playerScoreValue);
        }

        public void OnSignInFailed()
        {
            SetHighScoreLocal();
        }

        public void OnClick(Vector2 position)
        {
            _gameState.OnUserClick(this, position);
        }
    }
}