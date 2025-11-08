using OSK;
using UnityEngine;

public class BotController : MonoBehaviour, IUpdate, IFixedUpdate
{
    [Header("Bot Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform[] corners;
    [SerializeField] private EPlayerType playerType = EPlayerType.Bot1;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float randomMoveInterval = 2f;

    // Cached references for performance
    private CharacterController characterController;
    private CharacterEffect characterEffect;
    private FoodSpawner foodSpawner;
    private Transform cachedTransform;

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

    // Animation hash IDs for performance
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");

    // Public properties
    public EPlayerType PlayerType => playerType;

    private void Awake()
    {
        Main.Mono.Register(this);

        // Cache transform reference for performance
        cachedTransform = transform;

        // Pre-calculate squared distances to avoid sqrt operations
        detectionRadiusSquared = detectionRadius * detectionRadius;
        targetReachDistanceSquared = targetReachDistance * targetReachDistance;
    }

    private void Start()
    {
        // Cache component references for performance
        characterController = GetComponent<CharacterController>();
        characterEffect = GetComponent<CharacterEffect>();
        foodSpawner = FoodSpawner.Instance;

        // Calculate boundary constraints from corners
        CalculateBounds();
        
        SetRandomTarget();
    }

    public void Tick(float deltaTime)
    {
        this.deltaTime = deltaTime;

        // Cache position once per frame instead of accessing transform multiple times
        currentPosition = cachedTransform.position;

        // Only check for food when we don't have a target or reached the current target
        if (!hasTarget || HasReachedTarget())
        {
            FindNearestFood();
        }

        HandleSpeed();
        HandleAnimation();
        HandleLookDirection();

        // Update random move timer
        randomMoveTimer -= deltaTime;
        if (randomMoveTimer <= 0f)
        {
            SetRandomTarget();
            randomMoveTimer = randomMoveInterval;
        }
    }

    public void FixedTick(float fixedDeltaTime)
    {
        // Handle movement in FixedUpdate for consistent physics-based movement
        HandleMovement(fixedDeltaTime);
    }

    private void FindNearestFood()
    {
        // Early return for null check - avoid property access if possible
        if (foodSpawner == null)
        {
            foodSpawner = FoodSpawner.Instance; // Try to re-cache
            if (foodSpawner == null)
            {
                // Fallback to random target if FoodSpawner is not available
                if (!hasTarget)
                {
                    SetRandomTarget();
                }
                return;
            }
        }

        var spawnedFoods = foodSpawner.SpawnedFoods;
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

            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearestFood = food;
            }
        }

        if (nearestFood != null)
        {
            Vector3 foodPosition = nearestFood.transform.position;
            
            // Only target food if it's within bounds (or if no bounds set)
            if (IsPositionInBounds(foodPosition))
            {
                targetPosition = foodPosition;
                hasTarget = true;
            }
            else
            {
                // Food is outside bounds, set random target within bounds
                if (!hasTarget)
                {
                    SetRandomTarget();
                }
            }
        }
        else
        {
            // No food found, maintain current target or set random target if no target exists
            if (!hasTarget)
            {
                SetRandomTarget();
            }
        }
    }

    private bool HasReachedTarget()
    {
        // Use squared distance to avoid expensive sqrt calculation
        return (currentPosition - targetPosition).sqrMagnitude <= targetReachDistanceSquared;
    }

    private void HandleMovement(float fixedDeltaTime)
    {
        // Calculate direction using cached position
        moveDirection = (targetPosition - currentPosition).normalized;

        Vector3 movement = moveDirection * currentSpeed * fixedDeltaTime;
        characterController.Move(movement);

        // Check if we reached the target (using already calculated values)
        if (hasTarget && HasReachedTarget())
        {
            hasTarget = false; // Clear target so we can look for a new one
        }
        
        // If target is outside bounds or we're approaching boundary, find a new target
        if (hasTarget && boundsInitialized && !IsPositionInBounds(targetPosition))
        {
            hasTarget = false;
        }
    }

    private void HandleSpeed()
    {
        currentSpeed = moveSpeed;

        if (characterEffect != null)
        {
            currentSpeed *= (characterEffect.IsSlowed ? 0.5f : 1f);
            currentSpeed *= (characterEffect.IsStunned ? 0f : 1f);
        }
    }

    private void HandleAnimation()
    {
        // Early return if no animator
        if (animator == null) return;

        bool isRunning = moveDirection.magnitude > magnitudeThreshold && currentSpeed > 0;
        speedFloat = currentSpeed / moveSpeed;

        // Use hashed parameter names for better performance
        animator.SetBool(IsRunningHash, isRunning);
        animator.SetFloat(MoveSpeedHash, speedFloat);
    }

    private void HandleLookDirection()
    {
        // Early return if movement is too small
        if (moveDirection.magnitude <= magnitudeThreshold) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, targetRotation, deltaTime * rotationSpeed);
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

    private void OnDestroy()
    {
        Main.Mono.UnRegister(this);
    }
}
