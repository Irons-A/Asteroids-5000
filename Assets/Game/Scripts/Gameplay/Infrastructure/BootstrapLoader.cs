using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Gameplay.Infrastructure
{
    public class BootstrapLoader : MonoBehaviour, IInitializable
    {
        public const string GameSceneName = "Game";

        public void Initialize()
        {
            //Check if all configs are loaded

            LoadGameScene();
        }

        private void LoadSaves()
        {

        }

        private void LoadGameScene()
        {
            //Another service can be used

            StartCoroutine(LoadSceneWithDelay());
        }

        private IEnumerator LoadSceneWithDelay()
        {
            //Change to UniTask?
            yield return new WaitForSeconds(0.5f);

            yield return SceneManager.LoadSceneAsync(GameSceneName, LoadSceneMode.Single);
        }
    }
}
