using Firebase.Analytics;
using Firebase.Extensions;
using UnityEngine;
using Zenject;

namespace Analytics
{
    public class AnalyticsInitializer : IInitializable
    {
        private readonly AnalyticsLogger _logger;

        public AnalyticsInitializer(AnalyticsLogger logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    
                    _logger.SetIsInitialized();
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }
    }
}
