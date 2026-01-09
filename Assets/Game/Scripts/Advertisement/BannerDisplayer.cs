using System;
using GoogleMobileAds.Api;
using UnityEngine;
using Zenject;

namespace Advertisement
{
    public class BannerDisplayer : IInitializable, IDisposable
    {
        private const string TestAndroidBannerId = "ca-app-pub-3940256099942544/9214589741";
        private const string TestIOSBannerId = "ca-app-pub-3940256099942544/2435281174";
        
        private const string AndroidBannerId = TestAndroidBannerId;
        private const string IosBannerId = TestIOSBannerId;
        
        private string _currentBannerId;
        private bool _isBannerShown = false;
        private BannerView _bannerView;
        
        private bool _isInitialized = false;

        public void Initialize()
        {
#if UNITY_ANDROID
            _currentBannerId = AndroidBannerId;
#elif UNITY_IOS
            _currentBannerId = _iosBannerId;
#else
            Debug.LogWarning("Unsupported platform for AdMob");
            
            _currentBannerId = _androidBannerId;
#endif
        }
        
        public void SetInitialized()
        {
            _isInitialized = true;
        }
        
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
        
        public bool IsBannerVisible()
        {
            return _isBannerShown;
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

        public void Dispose()
        {
            DestroyBanner();
        }
    }
}
