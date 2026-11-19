using System.Collections;
using TMPro;
using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeInDuration = 0.5f;
    [SerializeField] private TextMeshProUGUI _scoreText;

    public void OpenMenu(int score)
    {
        StopAllCoroutines();
        StartCoroutine(FadeInMenu());
        _scoreText.text = score.ToString();
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
