using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private ScoreUIPunch _scorePunchUI;
    [SerializeField] private float _timeToApplyScoreToUI = 0.3f;
    [SerializeField] private GameOverMenu _gameOverMenu;

    [Header("Score Settings")]
    [SerializeField] private int _scorePerSecond = 50;
    [SerializeField] private int _scorePerAttackSurvived = 500;

    private int _currentScore = 0;
    private int _currentDisplayingScore = 0;
    private bool _isCounting = false;
    private Coroutine _UICoroutine = null;
    private Coroutine _updateScoreCoroutine = null;

    public void OnAttackFinished()
    {
        if (!_isCounting) return;
        _currentScore += _scorePerAttackSurvived;
        UpdateUI();
    }

    private IEnumerator UpdateScore()
    {
        while(_isCounting)
        {
            _currentScore += _scorePerSecond;
            UpdateUI();
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void UpdateUI()
    {
        if (_UICoroutine != null) StopCoroutine(_UICoroutine);
        _UICoroutine = StartCoroutine(UpdateScoreUICoroutine());
        _scorePunchUI.Punch();
    }

    private IEnumerator UpdateScoreUICoroutine()
    {
        int startScore = _currentDisplayingScore;
        float counter = 0.0f;
        while(counter < _timeToApplyScoreToUI)
        {
            counter += Time.deltaTime;
            _currentDisplayingScore = Mathf.CeilToInt(Mathf.Lerp(_currentDisplayingScore, _currentScore, Mathf.Clamp01(counter / _timeToApplyScoreToUI)));
            _scoreText.text = "Score: " + _currentDisplayingScore;
            yield return null;
        }
        _UICoroutine = null;
    }

    public void OnGameStart()
    {
        _isCounting = true;
        _currentScore = 0;
        _updateScoreCoroutine = StartCoroutine(UpdateScore());
    }

    public void OnPlayerDeath()
    {
        _isCounting = false;
        StopCoroutine(_updateScoreCoroutine);
        _updateScoreCoroutine = null;
        _gameOverMenu.OpenMenu(_currentScore);
        // Submit score to leaderboard
    }
}
