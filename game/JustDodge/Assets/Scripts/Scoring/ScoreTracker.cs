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

    [Header("Difficulty Increase Settings")]
    [SerializeField] private int _difficultyIncreaseThreshold = 500;
    [SerializeField] private float _difficultyIncreasePerThreshold = 0.2f;

    private int _currentScore = 0;
    private int _currentDisplayingScore = 0;
    private bool _isCounting = false;
    private Coroutine _UICoroutine = null;
    private Coroutine _updateScoreCoroutine = null;

    private float _difficultyMultiplier = 1.0f;
    public float DifficultyMultiplier => _difficultyMultiplier;
    private int _lastDifficultyIncreaseAmount = 0;

    public void OnAttackFinished()
    {
        if (!_isCounting) return;
        _currentScore += _scorePerAttackSurvived;
        OnScoreChange();
    }

    private IEnumerator UpdateScore()
    {
        while(_isCounting)
        {
            _currentScore += _scorePerSecond;
            OnScoreChange();
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void OnScoreChange()
    {
        while(_currentScore - _lastDifficultyIncreaseAmount >= _difficultyIncreaseThreshold)
        {
            _lastDifficultyIncreaseAmount += _difficultyIncreaseThreshold;
            _difficultyMultiplier += _difficultyIncreasePerThreshold;
        }

        if (_UICoroutine != null) StopCoroutine(_UICoroutine);
        _UICoroutine = StartCoroutine(UpdateScoreUICoroutine());
        _scorePunchUI.Punch();
    }

    private IEnumerator UpdateScoreUICoroutine()
    {
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
        LeaderboardService.Instance.StartRun();
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
