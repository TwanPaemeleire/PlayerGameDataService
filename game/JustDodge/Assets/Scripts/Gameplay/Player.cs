using Microsoft.Extensions.Logging.Abstractions;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 50.0f;
    [SerializeField] private float _timeToAccelerate = 0.1f;
    [SerializeField] private float _timeToDecelerate = 0.1f;
    [SerializeField] private float _slowModeMultiplier = 0.5f;
    [SerializeField] private ArenaBounds _arenaBounds;
    [SerializeField] private float _edgePadding = 0.1f;

    private bool _isAlive = true;
    private bool _canMove = false;
    private bool _slowModeEnabled = false;
    private float _currentSpeed = 0.0f;
    private Vector2 _currentInput = Vector2.zero;
    private Coroutine _speedChangeCoroutine = null;

    public UnityEvent OnDeath = new UnityEvent();

    public void Update()
    {
        if (!_isAlive || !_canMove) return;
        if (_currentInput == Vector2.zero && _currentSpeed == 0.0f) return;
        Vector2 positionChange = _currentInput * _currentSpeed * Time.deltaTime;
        if (_slowModeEnabled) positionChange *= _slowModeMultiplier;
        Vector2 newPosition = (Vector2)transform.position + positionChange;
        newPosition = _arenaBounds.ClampPoint(newPosition, _edgePadding);
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }

    public void StartGame()
    {
        _canMove = true;
    }

    public void OnMoveInputReceived(InputAction.CallbackContext context)
    {
        if (!_canMove) return;
        Vector2 newInput = context.ReadValue<Vector2>();
        newInput.Normalize();

        if(_currentInput != Vector2.zero && newInput != Vector2.zero) // Change of buttons/direction
        {
            _currentInput = newInput;
            return;
        }
        if (_speedChangeCoroutine != null) StopCoroutine(_speedChangeCoroutine);
        float destSpeed = (_currentInput == Vector2.zero) ? _movementSpeed : 0.0f;
        float timeToReach = (_currentInput == Vector2.zero) ? _timeToAccelerate : _timeToDecelerate;
        _currentInput = newInput;
        _speedChangeCoroutine = StartCoroutine(ChangeSpeed(destSpeed, timeToReach));
    }

    public void OnSlowModeInputReceived(InputAction.CallbackContext context)
    {
        _slowModeEnabled = context.ReadValue<float>() != 0.0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isAlive) return;
        if(collision.gameObject.CompareTag("Projectile"))
        {
            collision.gameObject.GetComponent<ProjectileBase>().Despawn();
            _isAlive = false;
            _currentInput = Vector2.zero;
            _currentSpeed = 0.0f;
            _speedChangeCoroutine = null;
            _slowModeEnabled = false;
            GetComponentInChildren<Animator>().SetTrigger("Death");
            OnDeath.Invoke();
        }
    }

    private IEnumerator ChangeSpeed(float destSpeed, float timeToReach)
    {
        float startSpeed = _currentSpeed;
        float counter = 0.0f;
        while(counter < timeToReach)
        {
            counter += Time.deltaTime;
            _currentSpeed = Mathf.SmoothStep(startSpeed, destSpeed, Mathf.Clamp01(counter / timeToReach));
            yield return null;
        }
        _speedChangeCoroutine = null;
    }
}
