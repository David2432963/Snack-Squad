using OSK;
using UnityEngine;

public class PlayerController : MonoBehaviour, IUpdate, IFixedUpdate
{
    [SerializeField] private Animator[] animators;
    [SerializeField] private float moveSpeed = 5f;

    private Animator skinAnimator;
    private CharacterController characterController;
    private CharacterEffect characterEffect;
    private Joystick joystick;
    private Vector3 moveDirection;
    private float currentSpeed;
    private float speedFloat;
    private float deltaTime;
    private float originalYPos;
    private Vector3 tempPos;
    private bool canMove;

    private void Awake()
    {
        Main.Mono.Register(this);
        Main.Observer.Add(EEvent.OnGameStateChange, UpdateState);
        Main.Observer.Add(EEvent.OnGameOver, OnEndGame);
        characterController = GetComponent<CharacterController>();
        characterEffect = GetComponent<CharacterEffect>();
        // Main.Observer.Add(EEvent.OnJoystickMove, OnJoystickMove);
    }
    private void OnJoystickMove(object data)
    {
        if (data is Vector2 input)
        {
            moveDirection = new Vector3(input.x, 0, input.y);
        }
    }
    private void Start()
    {
        joystick = Main.UI.Get<GamePlayUI>().Joystick;
        originalYPos = transform.position.y;
        SetupSkinAnimator();
    }
    private void SetupSkinAnimator()
    {
        skinAnimator = animators[(int)GameData.CurrentSkin - 1];
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
        HandleInput();
        HandleSpeed();
        HandleAnimation();
        HandleLookDirection();
    }
    public void FixedTick(float fixedDeltaTime)
    {
        characterController.Move(moveDirection * currentSpeed * fixedDeltaTime);

        tempPos = transform.position;
        tempPos.y = originalYPos;
        transform.position = tempPos;
    }
    private void HandleInput()
    {
        moveDirection = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
    }
    private void HandleSpeed()
    {
        currentSpeed = moveSpeed
                        * (characterEffect.IsSlowed ? 0.5f : 1f)
                        * (characterEffect.IsStunned ? 0f : 1f);
    }
    private void HandleAnimation()
    {
        if (moveDirection.magnitude > 0.1f && currentSpeed > 0)
        {
            skinAnimator.SetBool("IsRunning", true);
        }
        else
        {
            skinAnimator.SetBool("IsRunning", false);
        }
        speedFloat = currentSpeed / moveSpeed;
        skinAnimator.SetFloat("MoveSpeed", speedFloat);
    }
    private void HandleLookDirection()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * 10f);
        }
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
        if (GamePlay_Manager.Instance.Winner == EPlayerType.Player)
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
