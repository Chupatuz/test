using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerCC_InputSystem : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float turnSmoothTime = 0.1f;

    [Header("Ссылки")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float turnSmoothVelocity;
    private bool isGrounded;

    // Ссылка на сгенерированный класс Input Actions
    private PlayerControls playerControls;

    // Переменные для хранения текущего ввода
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool sprintHeld;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Создаём экземпляр Input Actions
        playerControls = new PlayerControls();

        // Подписываемся на события нажатия кнопок (для Jump и Sprint)
        playerControls.Player.Jump.performed += ctx => jumpPressed = true;
        playerControls.Player.Jump.canceled += ctx => jumpPressed = false;

        // Sprint можно читать как значение (будем проверять в Update)
        // Для Move будем читать значение в Update через MoveInput
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Update()
    {
        // Читаем движение (Vector2) из Input System
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();

        // Читаем, зажат ли Sprint
        sprintHeld = playerControls.Player.Sprint.IsPressed();

        // Проверка на земле
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Движение (теперь используем moveInput.x и moveInput.y)
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = sprintHeld ? runSpeed : walkSpeed;
            controller.Move(moveDir * currentSpeed * Time.deltaTime);
        }

        // Прыжок (если нажата кнопка и мы на земле)
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            // Сбрасываем флаг, чтобы прыгнуть только один раз за нажатие
            jumpPressed = false;
        }

        // Гравитация
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ВАЖНО: если вы используете этот метод для прыжка, то событие выше уже сработает.
    // Можно оставить как есть, но учтите, что jumpPressed будет true только в момент нажатия,
    // а не всё время удержания (что нам и нужно).
}