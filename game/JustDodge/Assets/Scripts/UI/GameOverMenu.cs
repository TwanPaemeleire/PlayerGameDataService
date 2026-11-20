using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeInDuration = 0.5f;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private Button _submitButton;

    private int _currentScore = 0;

    private void Awake()
    {
        SetInteractable(false);
    }

    public void OpenMenu(int score)
    {
        _currentScore = score;
        StopAllCoroutines();
        SetInteractable(true);
        StartCoroutine(FadeInMenu());
        _scoreText.text = score.ToString();
    }

    public async void OnSubmitClicked()
    {
        EventSystem.current.SetSelectedGameObject(null);
        string nameInput = _nameInput.text.Trim();
        if(nameInput.Length > 5 || nameInput.Length < 3)
        {
            _statusText.text = "Name must be between 3 and 5 characters";
            _statusText.color = Color.red;
            return;
        }

        _submitButton.interactable = false;
        _nameInput.interactable = false;
        _statusText.text = "Submitting...";
        _statusText.color = Color.white;

        SubmitScoreResult result = await LeaderboardService.Instance.SubmitScore(nameInput, _currentScore);
        if (this == null) return;

        _statusText.text = result.Message;
        _statusText.color = result.Success ? Color.green : Color.red;

        if (result.Success)
        {
            _submitButton.gameObject.SetActive(false);
        }
        else
        {
            _submitButton.interactable = true;
            _nameInput.interactable = true;
        }
    }

    private void SetInteractable(bool interactable)
    {
        _canvasGroup.interactable = interactable;
        _canvasGroup.blocksRaycasts = interactable;
    }

    private IEnumerator FadeInMenu()
    {
        float counter = 0.0f;
        while(counter < _fadeInDuration)
        {
            counter += Time.deltaTime;
            _canvasGroup.alpha = Mathf.SmoothStep(0.0f, 1.0f, counter / _fadeInDuration);
            yield return null;
        }
    }
}
