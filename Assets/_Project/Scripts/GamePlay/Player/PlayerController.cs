using OSK;
using UnityEngine;

public class PlayerController : MonoBehaviour, IUpdate, IFixedUpdate
{
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 5f;

    private CharacterController characterController;
    private CharacterEffect characterEffect;
    private Joystick joystick;
    private Vector3 moveDirection;
    private float currentSpeed;
    private float speedFloat;
    private float deltaTime;

    private void Awake()
    {
        Main.Mono.Register(this);
    }
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        characterEffect = GetComponent<CharacterEffect>();
        joystick = Main.UI.Get<GamePlayUI>().Joystick;
    }

    public void Tick(float deltaTime)
    {
        this.deltaTime = deltaTime;
        HandleInput();
        HandleSpeed();
        HandleAnimation();
        HandleLookDirection();
    }
    public void FixedTick(float fixedDeltaTime)
    {
        characterController.Move(moveDirection * currentSpeed * fixedDeltaTime);
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
            animator.SetBool("IsRunning", true);
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }
        speedFloat = currentSpeed / moveSpeed;
        animator.SetFloat("MoveSpeed", speedFloat);
    }
    private void HandleLookDirection()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * 10f);
        }
    }
    private void OnDestroy()
    {
        Main.Mono.UnRegister(this);
    }
}
