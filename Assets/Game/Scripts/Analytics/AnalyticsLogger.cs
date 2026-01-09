using Firebase.Analytics;

namespace Analytics
{
    public class AnalyticsLogger
    {
        private bool _isInitialized;

        public void SetIsInitialized()
        {
            _isInitialized = true;
        }
        
        public void LogGameStartEvent()
        {
            if (_isInitialized == false) return;
            
            FirebaseAnalytics.LogEvent("session_start");
        }

        public void LogInterstitialAdShown()
        {
            if (_isInitialized == false) return;
            
            FirebaseAnalytics.LogEvent("interstitial_ad_shown");
        }
        
        public void LogGameOverEventWithScore(int finalScore)
        {
            if (_isInitialized == false) return;
            
            FirebaseAnalytics.LogEvent("game_over", FirebaseAnalytics.ParameterScore, finalScore);
        }
    }
}
