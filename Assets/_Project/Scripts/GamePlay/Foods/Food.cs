using System;
using OSK;
using UnityEngine;
using DG.Tweening;

public class Food : MonoBehaviour, IUpdate
{
    [Header("Float Animation")]
    [SerializeField] protected float floatHeight;
    [SerializeField] protected float floatDuration;
    [SerializeField] protected int point;

    public Action OnRemoved;

    protected virtual string spawnedText => "Collected!";

    private float originalY;
    private Tween floatTween;

    protected virtual void Start()
    {
        Main.Mono.Register(this);
        originalY = transform.position.y;
    }
    protected virtual void OnEnable()
    {
        StartFloatAnimation();
    }

    public void Tick(float deltaTime)
    {
        transform.Rotate(0, 90f * deltaTime, 0);
    }

    private void StartFloatAnimation()
    {
        floatTween = DOTween.Sequence()
            .Append(transform.DOMoveY(originalY + floatHeight, floatDuration).SetEase(Ease.Linear))
            .Append(transform.DOMoveY(originalY, floatDuration).SetEase(Ease.Linear))
            .SetLoops(-1, LoopType.Yoyo).SetDelay(UnityEngine.Random.Range(0, .5f));
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameData_Manager.Instance.AddScore(point);
            Particle_Manager.Instance.SpawnFloatingText(transform.position + Vector3.up * 1.5f, spawnedText);
        }
        else if (other.CompareTag("Bot"))
        {
            var botController = other.GetComponent<BotController>();
            if (botController != null)
            {
                GameData_Manager.Instance.AddScore(botController.PlayerType, point);
            }
        }
        OnRemoved?.Invoke();
        Main.Pool.Despawn(gameObject);
        Particle_Manager.Instance.PlayRandomBlowFx(transform.position);
    }

    protected virtual void OnDisable()
    {
        // Stop float animation when disabled
        if (floatTween != null && floatTween.IsActive())
        {
            floatTween.Kill(true);
        }
        Main.Mono.UnRegister(this);
    }
}
