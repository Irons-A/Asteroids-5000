using System;
using Analytics;
using GoogleMobileAds.Api;
using UnityEngine;
using Zenject;

namespace Advertisement
{
    public class InterstitialDisplayer : IInitializable, IDisposable
    {
        private const string TestInterstitialAndroidId = "ca-app-pub-3940256099942544/1033173712";
        private const string TestInterstitialIOSId = "ca-app-pub-3940256099942544/4411468910";
        
        private const string AndroidInterstitialId = TestInterstitialAndroidId;
        private const string IosInterstitialId = TestInterstitialIOSId;
        
        private readonly AnalyticsLogger _analyticsLogger;
        
        private InterstitialAd _interstitialAd;
        private string _currentInterstitialId;
        
        private bool _isInitialized = false;

        public InterstitialDisplayer(AnalyticsLogger analyticsLogger)
        {
            _analyticsLogger = analyticsLogger;
        }
        
        public void Initialize()
        {
#if UNITY_ANDROID
            _currentInterstitialId = AndroidInterstitialId;
#elif UNITY_IOS
            _currentInterstitialId = _iosInterstitialId;
#else
            Debug.LogWarning("Unsupported platform for AdMob");
            
            _currentInterstitialId = _androidInterstitialId;
#endif
        }
        
        public void SetInitialized()
        {
            _isInitialized = true;
            
            LoadInterstitialAd();
        }
        
        public void LoadInterstitialAd()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("AdMob not initialized. Cannot load interstitial.");
                
                return;
            }
            
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
            
            InterstitialAd.Load(_currentInterstitialId, new AdRequest(), (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError($"Interstitial ad failed to load: {error?.GetMessage()}");
                        
                        return;
                    }

                    _interstitialAd = ad;
                    SetupInterstitialEventHandlers(ad);
                });
        }
        
        public void ShowInterstitialAd()
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("AdMob not initialized. Cannot show interstitial.");
                
                return;
            }

            if (IsInterstitialReady())
            {
                _interstitialAd.Show();
            }
            else
            {
                Debug.LogWarning("Interstitial ad is not ready to show.");

                LoadInterstitialAd();
            }
        }
        
        public bool IsInterstitialReady()
        {
            return _interstitialAd != null && _interstitialAd.CanShowAd();
        }

        public void DestroyInterstitialAd()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
        }

        private void SetupInterstitialEventHandlers(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                _analyticsLogger.LogInterstitialAdShown();
                LoadInterstitialAd();
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"Interstitial ad failed to show full screen content: {error.GetMessage()}");
                
                LoadInterstitialAd();
            };

            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log($"Interstitial ad paid: {adValue.Value} {adValue.CurrencyCode}");
            };

            ad.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad clicked.");
            };
        }

        public void Dispose()
        {
            DestroyInterstitialAd();
        }
    }
}
