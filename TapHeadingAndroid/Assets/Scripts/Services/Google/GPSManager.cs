using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

// ReSharper disable once InconsistentNaming
/**
 * GooglePlayServices Manager
 */
public static class GPSManager
{
    /**
     * Activates PlayGamesPlatform
     */
    internal static void Activate()
    {
        var config = new PlayGamesClientConfiguration.Builder().Build();

        PlayGamesPlatform.InitializeInstance(config);
        // recommended for debugging:
        //PlayGamesPlatform.DebugLogEnabled = true;
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();
    }

    /**
     * Returns isAuthenticated from PlayGamesPlatform
     */
    private static bool IsAuthenticated()
    {
        return PlayGamesPlatform.Instance.IsAuthenticated();
    }

    /**
     * Tries to Add HighScore if User is Logged in to GooglePlayPlatform
     */
    internal static void SubmitScore(long highScore)
    {
        if (IsAuthenticated())
        {
            Social.ReportScore(highScore, GPGSIds.leaderboard_high_score, null);
        }
    }

    /**
     * Start OnlyOnce Prompt to SignIn to GooglePlayPlatform
     */
    internal static void SignInToGooglePlayServices()
    {
        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, result =>
        {
            switch (result)
            {
                case SignInStatus.Canceled:
                    SignInCanceled();
                    break;
                case SignInStatus.Success:
                    SignInSuccess();
                    break;
            }
        });
    }

    private static void SignInCanceled()
    {
        PlayerPrefsManager.SetAutoLogin(false);
        GameManager.SetHighScoreFromLocal();
    }

    private static void SignInSuccess()
    {
        PlayerPrefsManager.SetAutoLogin(true);
        GameManager.OverwriteGPSHighScore();
        LoadLeaderboardFromGPS();
    }

    /**
     * Tries to Report Progress of Achievements to GooglePlayService
     */
    internal static void CheckAchievement(int highScore)
    {
        if (!IsAuthenticated()) return;

        if (highScore >= 200)
        {
            UnlockAchievement(GPGSIds.achievement_oof);
        }

        if (highScore >= 100)
        {
            UnlockAchievement(GPGSIds.achievement_100);
        }

        if (highScore >= 69)
        {
            UnlockAchievement(GPGSIds.achievement_nice);
        }

        if (highScore >= 50)
        {
            UnlockAchievement(GPGSIds.achievement_50);
        }

        if (highScore >= 42)
        {
            UnlockAchievement(GPGSIds.achievement_answer_to_the_ultimate_question_of_life_the_universe_and_everything);
        }
    }

    /**
     * Tries to Report Progress of ThankYou Achievement to GooglePlayService
     */
    internal static void ThankYouAchievement()
    {
        if (!IsAuthenticated()) return;
        UnlockAchievement(GPGSIds.achievement_thank_you);
    }

    private static void UnlockAchievement(string id)
    {
        Social.ReportProgress(id, 100.0f, null);
    }

    /**
     * Tries to open LeaderboardUI and log in, if not logged in
     */
    public static void ShowLeaderboardUI()
    {
        if (!IsAuthenticated())
        {
            PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, result =>
            {
                switch (result)
                {
                    case SignInStatus.Canceled:
                        SignInCanceled();
                        break;
                    case SignInStatus.Success:
                        SignInSuccess();
                        PlayGamesPlatform.Instance.ShowLeaderboardUI();
                        break;
                }
            });
        }
        else
        {
            PlayGamesPlatform.Instance.ShowLeaderboardUI();
        }
    }

    /**
     * Tries to open AchievementsUI and log in, if not logged in
     */
    public static void ShowAchievementsUI()
    {
        if (!IsAuthenticated())
        {
            PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, result =>
            {
                switch (result)
                {
                    case SignInStatus.Canceled:
                        SignInCanceled();
                        break;
                    case SignInStatus.Success:
                        SignInSuccess();
                        PlayGamesPlatform.Instance.ShowAchievementsUI();
                        break;
                }
            });
        }
        else
        {
            PlayGamesPlatform.Instance.ShowAchievementsUI();
        }
    }

    // ReSharper disable once InconsistentNaming
    /**
     * Tries to load highScore of user from google play services and sets it as local highscore  
     */
    private static void LoadLeaderboardFromGPS()
    {
        PlayGamesPlatform.Instance.LoadScores(
            GPGSIds.leaderboard_high_score,
            LeaderboardStart.PlayerCentered,
            1,
            LeaderboardCollection.Public,
            LeaderboardTimeSpan.AllTime,
            data =>
            {
                if (!data.Valid) return;
                GameManager.SetHighScoreFromGPS(data.PlayerScore.value);
            });
    }
}