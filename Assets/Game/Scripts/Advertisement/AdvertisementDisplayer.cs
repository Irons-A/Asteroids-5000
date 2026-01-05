using System;
using System.Collections;
using System.Collections.Generic;
using Analytics;
using GoogleMobileAds.Api;
using UnityEngine;
using Zenject;

namespace Advertisement
{
    public class AdvertisementDisplayer : IInitializable, IDisposable
    {
        private const string TestAndroidBannerId = "ca-app-pub-3940256099942544/9214589741";
        private const string TestIOSBannerId = "ca-app-pub-3940256099942544/2435281174";
        private const string TestInterstitialAndroidId = "ca-app-pub-3940256099942544/1033173712";
        private const string TestInterstitialIOSId = "ca-app-pub-3940256099942544/4411468910";
        
        private string _androidBannerId = TestAndroidBannerId;
        private string _iosBannerId = TestIOSBannerId;
        private string _androidInterstitialId = TestInterstitialAndroidId;
        private string _iosInterstitialId = TestInterstitialIOSId;
        
        private string _currentBannerId;
        private string _currentInterstitialId;

        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private AnalyticsService  _analyticsService;

        private bool _isInitialized = false;
        private bool _isBannerShown = false;

        public AdvertisementDisplayer(AnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public void Initialize()
        {
    #if UNITY_ANDROID
            _currentBannerId = _androidBannerId;
            _currentInterstitialId = _androidInterstitialId;
    #elif UNITY_IOS
            _currentBannerId = _iosBannerId;
            _currentInterstitialId = _iosInterstitialId;
    #else
            Debug.LogWarning("Unsupported platform for AdMob");
            
            _currentBannerId = _androidBannerId;
            _currentInterstitialId = _androidInterstitialId;
    #endif
            
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                if (initStatus == null)
                {
                    Debug.LogError("Google Mobile Ads initialization failed.");
                    return;
                }

                _isInitialized = true;
                
                LoadInterstitialAd();
            });
        }

        #region Adaptive Banner Ad
        
        public void ShowBanner()
        {
            if (_isInitialized == false)
            {
                Debug.LogWarning("AdMob not initialized. Cannot show banner.");
                
                return;
            }

            if (_bannerView != null)
            {
                _bannerView.Show();
                _isBannerShown = true;
                
                return;
            }

            try
            {
                int deviceWidth = MobileAds.Utils.GetDeviceSafeWidth();
                
                AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(deviceWidth);
                
                _bannerView = new BannerView(_currentBannerId, adaptiveSize, AdPosition.Bottom);
                
                SetupBannerEventHandlers();
                
                AdRequest request = new AdRequest();
                _bannerView.LoadAd(request);
                
                _isBannerShown = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create banner: {e.Message}");
            }
        }
        
        public void HideBanner()
        {
            if (_bannerView != null && _isBannerShown)
            {
                _bannerView.Hide();
                _isBannerShown = false;
            }
        }
        
        private void DestroyBanner()
        {
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
                _isBannerShown = false;
            }
        }

        private void SetupBannerEventHandlers()
        {
            if (_bannerView == null) return;

            _bannerView.OnBannerAdLoaded += () =>
            {
            };

            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError($"Banner ad failed to load: {error.GetMessage()}");
            };

            _bannerView.OnAdFullScreenContentClosed += () =>
            {
            };
        }

        #endregion

        #region Interstitial Ad
        
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

            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialAd.Show();
            }
            else
            {
                Debug.LogWarning("Interstitial ad is not ready to show.");

                LoadInterstitialAd();
            }
        }

        private void SetupInterstitialEventHandlers(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                _analyticsService.LogInterstitialAdShown();
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

        #endregion

        #region State Control
        
        public void UpdateAdIds(string androidBannerId, string iosBannerId, 
            string androidInterstitialId, string iosInterstitialId)
        {
            _androidBannerId = androidBannerId;
            _iosBannerId = iosBannerId;
            _androidInterstitialId = androidInterstitialId;
            _iosInterstitialId = iosInterstitialId;
            
            if (_isInitialized)
            {
                DestroyBanner();
                LoadInterstitialAd();
            }
        }
        
        public bool IsInterstitialReady()
        {
            return _interstitialAd != null && _interstitialAd.CanShowAd();
        }
        
        public bool IsBannerVisible()
        {
            return _isBannerShown;
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            DestroyBanner();
            
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
        }

        #endregion
    }
}
