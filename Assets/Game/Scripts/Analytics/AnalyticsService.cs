using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using Firebase.Extensions;
using UnityEngine;
using Zenject;

namespace Analytics
{
    public class AnalyticsService : IInitializable
    {
        public void Initialize()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }
        
        public void LogGameStartEvent()
        {
            FirebaseAnalytics.LogEvent("session_start");
        }

        public void LogInterstitialAdShown()
        {
            FirebaseAnalytics.LogEvent("interstitial_ad_shown");
        }
        
        public void LogGameOverEventWithScore(int finalScore)
        {
            FirebaseAnalytics.LogEvent("game_over", FirebaseAnalytics.ParameterScore, finalScore);
        }
    }
}
