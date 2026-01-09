using GoogleMobileAds.Api;
using UnityEngine;
using Zenject;

namespace Advertisement
{
    public class AdvertisementInitializer : IInitializable
    {
        private readonly BannerDisplayer _bannerDisplayer;
        private readonly InterstitialDisplayer _interstitialDisplayer;

        public AdvertisementInitializer(BannerDisplayer bannerDisplayer, InterstitialDisplayer interstitialDisplayer)
        {
            _bannerDisplayer = bannerDisplayer;
            _interstitialDisplayer = interstitialDisplayer;
        }
        
        public void Initialize()
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                if (initStatus == null)
                {
                    Debug.LogError("Google Mobile Ads initialization failed.");

                    return;
                }
                
                _bannerDisplayer.SetInitialized();
                _interstitialDisplayer.SetInitialized();
            });
        }
    }
}
