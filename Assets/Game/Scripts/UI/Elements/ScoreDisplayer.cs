using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Elements
{
    public class ScoreDisplayer : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] _highScoreTexts;
        [SerializeField] private TMP_Text _currentScoreText;

        public void RefreshScores(int highscore, int currentScore)
        {
            DisplayHighScore(highscore);
            DisplayCurrentScore(currentScore);
        }
        
        public void DisplayHighScore(int score)
        {
            foreach (TMP_Text highScoreText in _highScoreTexts)
            {
                if (highScoreText.isActiveAndEnabled)
                {
                    highScoreText.text = ($"HIGHSCORE: {score}");
                }
            }
        }

        private void DisplayCurrentScore(int score)
        {
            if (_currentScoreText.isActiveAndEnabled)
            {
                _currentScoreText.text = ($"CURRENT SCORE: {score}");
            }
        }
    }
}
