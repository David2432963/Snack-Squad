using OSK;
using UnityEngine;

public class BotController : MonoBehaviour, IUpdate, IFixedUpdate
{
    [Header("Bot Settings")]
    [SerializeField] private Animator[] animators;
    [SerializeField] private Transform[] corners;
    [SerializeField] private EPlayerType playerType = EPlayerType.Edward;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float randomMoveInterval = 2f;

    // Cached references for performance
    private CharacterController characterController;
    private CharacterEffect characterEffect;
    private FoodSpawner foodSpawner;
    private Transform cachedTransform;
    private Animator skinAnimator;

    // Movement variables
    private Vector3 targetPosition;
    private Vector3 randomDirection;
    private Vector3 moveDirection;
    private Vector3 currentPosition;
    private float randomMoveTimer;
    private float deltaTime;
    private float currentSpeed;
    private float speedFloat;
    private bool hasTarget;

    // Boundary variables for corner constraints
    private Vector3 boundsMin;
    private Vector3 boundsMax;
    private bool boundsInitialized;

    // Performance constants
    private readonly float targetReachDistance = 1f;
    private float detectionRadiusSquared;
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
        detectionRadiusSquared = detectionRadius * detectionRadius;
        targetReachDistanceSquared = targetReachDistance * targetReachDistance;
    }

    private void Start()
    {
        foodSpawner = FoodSpawner.Instance;
        SetRandomSkinAnimator();

        // Calculate boundary constraints from corners
        CalculateBounds();

        // Ensure bot always starts moving
        SetRandomTarget();
        randomMoveTimer = randomMoveInterval; // Reset timer
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

        // Ensure we always have a target
        if (!hasTarget)
        {
            FindNearestFood();
            if (!hasTarget)
            {
                SetRandomTarget();
            }
        }

        HandleSpeed();
        HandleAnimation();
        HandleLookDirection();

        // Update random move timer - periodically look for better targets
        randomMoveTimer -= deltaTime;
        if (randomMoveTimer <= 0f)
        {
            // Look for nearby food to potentially switch targets
            if (HasNearbyFood())
            {
                // Try to find a better food target
                bool hadTarget = hasTarget;
                hasTarget = false; // Temporarily clear to allow new food search
                FindNearestFood();
                
                // If no new food found, restore previous target state
                if (!hasTarget && hadTarget)
                {
                    hasTarget = true; // Keep moving to current target
                }
            }
            else if (!hasTarget)
            {
                // No food nearby and no current target, set random
                SetRandomTarget();
            }
            
            randomMoveTimer = randomMoveInterval;
        }
    }

    public void FixedTick(float fixedDeltaTime)
    {
        // Handle movement in FixedUpdate for consistent physics-based movement
        if (!canMove) return;
        HandleMovement(fixedDeltaTime);
    }

    private bool HasNearbyFood()
    {
        if (foodSpawner == null) return false;

        var spawnedFoods = foodSpawner.SpawnedFoods;
        
        // Use for loop for better performance
        for (int i = 0; i < spawnedFoods.Count; i++)
        {
            Food food = spawnedFoods[i];

            // Quick null and active checks
            if (food == null || !food.gameObject.activeInHierarchy)
                continue;

            // Check if food is within detection radius and bounds
            Vector3 foodPosition = food.transform.position;
            float distanceSquared = (currentPosition - foodPosition).sqrMagnitude;

            if (distanceSquared < detectionRadiusSquared && IsPositionInBounds(foodPosition))
            {
                return true;
            }
        }

        return false;
    }

    private void FindNearestFood()
    {
        // Early return for null check - avoid property access if possible
        if (foodSpawner == null)
        {
            foodSpawner = FoodSpawner.Instance; // Try to re-cache
            if (foodSpawner == null)
            {
                return; // Don't set random target here, let caller handle it
            }
        }

        var spawnedFoods = foodSpawner.SpawnedFoods;
        if (spawnedFoods == null || spawnedFoods.Count == 0)
        {
            return; // No food available
        }

        Food nearestFood = null;
        float nearestDistanceSquared = detectionRadiusSquared; // Use squared distance

        // Use for loop for better performance than foreach
        for (int i = 0; i < spawnedFoods.Count; i++)
        {
            Food food = spawnedFoods[i];

            // Quick null and active checks
            if (food == null || !food.gameObject.activeInHierarchy)
                continue;

            // Use sqrMagnitude to avoid expensive sqrt calculation
            Vector3 foodPosition = food.transform.position;
            float distanceSquared = (currentPosition - foodPosition).sqrMagnitude;

            if (distanceSquared < nearestDistanceSquared && IsPositionInBounds(foodPosition))
            {
                nearestDistanceSquared = distanceSquared;
                nearestFood = food;
            }
        }

        if (nearestFood != null)
        {
            targetPosition = nearestFood.transform.position;
            hasTarget = true;
        }
        // If no suitable food found, hasTarget remains false and caller will set random target
    }

    private bool HasReachedTarget()
    {
        // Use squared distance to avoid expensive sqrt calculation
        return (currentPosition - targetPosition).sqrMagnitude <= targetReachDistanceSquared;
    }

    private void HandleMovement(float fixedDeltaTime)
    {
        // Update current position for FixedUpdate calculations
        Vector3 fixedCurrentPosition = cachedTransform.position;
        float originalYPos = fixedCurrentPosition.y;

        // Always ensure we have a valid target before moving
        if (!hasTarget)
        {
            SetRandomTarget();
        }

        // Calculate direction using current position
        moveDirection = (targetPosition - fixedCurrentPosition).normalized;

        Vector3 movement = moveDirection * currentSpeed * fixedDeltaTime;
        characterController.Move(movement);

        // Lock the Y position to prevent any vertical movement
        Vector3 tempPos = cachedTransform.position;
        tempPos.y = originalYPos;
        cachedTransform.position = tempPos;

        // Check if we reached the target using current position
        if (hasTarget && (fixedCurrentPosition - targetPosition).sqrMagnitude <= targetReachDistanceSquared)
        {
            Debug.Log($"Bot {playerType} reached target at {targetPosition}, finding new target...");
            
            // Clear current target first
            hasTarget = false;
            
            // Update position for food finding
            currentPosition = cachedTransform.position;
            
            // Try to find food first
            FindNearestFood();
            
            // If no food found, set random target
            if (!hasTarget)
            {
                SetRandomTarget();
                Debug.Log($"Bot {playerType} set random target: {targetPosition}");
            }
            else
            {
                Debug.Log($"Bot {playerType} found food target: {targetPosition}");
            }
        }

        // If target is outside bounds, find a new target immediately
        if (hasTarget && boundsInitialized && !IsPositionInBounds(targetPosition))
        {
            hasTarget = false;
            currentPosition = cachedTransform.position;
            SetRandomTarget();
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

    private void SetRandomTarget()
    {
        if (!boundsInitialized)
        {
            // Fallback to old behavior if no bounds
            randomDirection.Set(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            );
            randomDirection.Normalize();
            targetPosition = currentPosition + randomDirection * Random.Range(5f, 15f);
        }
        else
        {
            // Generate random target within bounds, ensuring it's a reasonable distance from current position
            Vector3 potentialTarget = Vector3.zero;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                potentialTarget.Set(
                    Random.Range(boundsMin.x, boundsMax.x),
                    currentPosition.y, // Keep same Y level
                    Random.Range(boundsMin.z, boundsMax.z)
                );
                attempts++;
            }
            while (attempts < maxAttempts && (currentPosition - potentialTarget).sqrMagnitude < 4f); // Minimum 2 unit distance

            targetPosition = potentialTarget;
        }

        hasTarget = true;
    }

    private void CalculateBounds()
    {
        if (corners == null || corners.Length == 0)
        {
            boundsInitialized = false;
            return;
        }

        // Initialize with first corner
        boundsMin = corners[0].position;
        boundsMax = corners[0].position;

        // Find min/max bounds from all corners
        for (int i = 1; i < corners.Length; i++)
        {
            if (corners[i] == null) continue;

            Vector3 cornerPos = corners[i].position;

            // Update min bounds
            if (cornerPos.x < boundsMin.x) boundsMin.x = cornerPos.x;
            if (cornerPos.z < boundsMin.z) boundsMin.z = cornerPos.z;

            // Update max bounds
            if (cornerPos.x > boundsMax.x) boundsMax.x = cornerPos.x;
            if (cornerPos.z > boundsMax.z) boundsMax.z = cornerPos.z;
        }

        boundsInitialized = true;
    }

    private bool IsPositionInBounds(Vector3 position)
    {
        if (!boundsInitialized) return true;

        return position.x >= boundsMin.x && position.x <= boundsMax.x &&
               position.z >= boundsMin.z && position.z <= boundsMax.z;
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
