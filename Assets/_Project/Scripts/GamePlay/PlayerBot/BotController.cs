using System.Collections.Generic;
using OSK;
using UnityEngine;

public class BotController : MonoBehaviour, IUpdate, IFixedUpdate
{
    [Header("Bot Settings")]
    [SerializeField] private Animator[] animators;
    [SerializeField] private EPlayerType playerType = EPlayerType.Edward;
    [SerializeField] private float moveSpeed = 5f;

    // Cached references for performance
    private CharacterController characterController;
    private CharacterEffect characterEffect;
    private FoodSpawner foodSpawner;
    private Transform cachedTransform;
    private Animator skinAnimator;

    // Movement variables
    private Vector3 targetPosition;
    private Vector3 moveDirection;
    private Vector3 currentPosition;
    private float deltaTime;
    private float currentSpeed;
    private float speedFloat;
    private bool hasTarget;
    private Food currentTargetFood;

    // Performance constants
    private readonly float targetReachDistance = 1f;
    private float targetReachDistanceSquared;
    private readonly float magnitudeThreshold = 0.1f;
    private readonly float rotationSpeed = 10f;
    private bool canMove;

    // Animation hash IDs for performance
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");

    // Public properties
    public EPlayerType PlayerType => playerType;

    private void Awake()
    {
        Main.Mono.Register(this);
        Main.Observer.Add(EEvent.OnGameStateChange, UpdateState);
        Main.Observer.Add(EEvent.OnGameOver, OnEndGame);
        characterController = GetComponent<CharacterController>();
        characterEffect = GetComponent<CharacterEffect>();
        // Cache transform reference for performance
        cachedTransform = transform;

        // Pre-calculate squared distances to avoid sqrt operations
        targetReachDistanceSquared = targetReachDistance * targetReachDistance;
    }

    private void Start()
    {
        foodSpawner = FoodSpawner.Instance;
        SetRandomSkinAnimator();

        // Start looking for food targets
        FindRandomFoodTarget();
    }
    private void SetRandomSkinAnimator()
    {
        int randomIndex = Random.Range(0, animators.Length);
        skinAnimator = animators[randomIndex];
        // Disable all animators first
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].gameObject.SetActive(false);
        }

        // Enable only the selected one
        skinAnimator.gameObject.SetActive(true);
    }
    public void Tick(float deltaTime)
    {
        if (!canMove) return;

        this.deltaTime = deltaTime;
        currentPosition = cachedTransform.position;

        // Always ensure we have a food target
        if (!hasTarget || currentTargetFood == null || !currentTargetFood.gameObject.activeInHierarchy)
        {
            FindRandomFoodTarget();
        }

        HandleSpeed();
        HandleAnimation();
        HandleLookDirection();
    }

    public void FixedTick(float fixedDeltaTime)
    {
        // Handle movement in FixedUpdate for consistent physics-based movement
        if (!canMove) return;
        HandleMovement(fixedDeltaTime);
    }

    private void FindRandomFoodTarget()
    {
        if (foodSpawner == null)
        {
            foodSpawner = FoodSpawner.Instance;
            if (foodSpawner == null) return;
        }

        var spawnedFoods = foodSpawner.SpawnedFoods;
        if (spawnedFoods == null || spawnedFoods.Count == 0)
        {
            hasTarget = false;
            currentTargetFood = null;
            return;
        }

        // Get all active food
        var activeFoods = new List<Food>();
        for (int i = 0; i < spawnedFoods.Count; i++)
        {
            Food food = spawnedFoods[i];
            if (food != null && food.gameObject.activeInHierarchy)
            {
                activeFoods.Add(food);
            }
        }

        if (activeFoods.Count == 0)
        {
            hasTarget = false;
            currentTargetFood = null;
            return;
        }

        // Select a random food from all active food
        int randomIndex = Random.Range(0, activeFoods.Count);
        currentTargetFood = activeFoods[randomIndex];
        targetPosition = currentTargetFood.transform.position;
        targetPosition.y = transform.position.y; // Keep bot's Y position
        hasTarget = true;

        Debug.Log($"Bot {playerType} targeting random food: {currentTargetFood.name} at {targetPosition}");
    }



    private void HandleMovement(float fixedDeltaTime)
    {
        if (!hasTarget || currentTargetFood == null)
        {
            Debug.Log($"Bot {playerType}: No movement - hasTarget={hasTarget}, currentTargetFood={currentTargetFood}");
            return;
        }

        // Update current position for FixedUpdate calculations
        Vector3 fixedCurrentPosition = cachedTransform.position;
        float originalYPos = fixedCurrentPosition.y;

        // Calculate direction to target
        Vector3 directionToTarget = targetPosition - fixedCurrentPosition;
        moveDirection = directionToTarget.normalized;

        Vector3 movement = moveDirection * currentSpeed * fixedDeltaTime;

        Debug.Log($"Bot {playerType}: DETAILED - directionToTarget={directionToTarget}, moveDirection={moveDirection}, currentSpeed={currentSpeed}, fixedDeltaTime={fixedDeltaTime}, movement={movement}");

        characterController.Move(movement);

        // Lock the Y position to prevent any vertical movement
        Vector3 tempPos = cachedTransform.position;
        tempPos.y = originalYPos;
        cachedTransform.position = tempPos;

        // Check if we reached the target
        if ((fixedCurrentPosition - targetPosition).sqrMagnitude <= targetReachDistanceSquared)
        {
            Debug.Log($"Bot {playerType}: Reached target, finding new target");
            // Find new random food target
            FindRandomFoodTarget();
        }
    }

    private void HandleSpeed()
    {
        currentSpeed = moveSpeed;

        currentSpeed *= (characterEffect.IsSlowed ? 0.5f : 1f);
        currentSpeed *= (characterEffect.IsStunned ? 0f : 1f);
    }

    private void HandleAnimation()
    {
        bool isRunning = moveDirection.magnitude > magnitudeThreshold && currentSpeed > 0;
        speedFloat = currentSpeed / moveSpeed;

        // Debug information
        if (hasTarget && currentTargetFood != null)
        {
            Debug.Log($"Bot {playerType}: canMove={canMove}, hasTarget={hasTarget}, currentSpeed={currentSpeed}, moveDirection.magnitude={moveDirection.magnitude}, isRunning={isRunning}");
        }

        // Use hashed parameter names for better performance
        skinAnimator.SetBool(IsRunningHash, isRunning);
        skinAnimator.SetFloat(MoveSpeedHash, speedFloat);
    }

    private void HandleLookDirection()
    {
        // Early return if movement is too small
        if (moveDirection.magnitude <= magnitudeThreshold) return;

        cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, Quaternion.LookRotation(moveDirection), deltaTime * rotationSpeed);
    }


    private void UpdateState(object data)
    {
        if (data is EGameState state)
        {
            canMove = state == EGameState.Playing;
        }
    }
    private void OnEndGame(object data)
    {
        if (GamePlay_Manager.Instance.Winner == playerType)
        {
            skinAnimator.CrossFade("Cheering", 0.1f);
        }
        else
        {
            skinAnimator.CrossFade("SadIdle", 0.1f);
        }
    }

    private void OnDestroy()
    {
        Main.Mono.UnRegister(this);
        Main.Observer.Remove(EEvent.OnGameStateChange, UpdateState);
        Main.Observer.Remove(EEvent.OnGameOver, OnEndGame);
    }
}
