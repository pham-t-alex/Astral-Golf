using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBall : MonoBehaviour
{
    [Header("Player Stats")]
    [Tooltip("The force multiplier applied to the ball when launched.")]
    [SerializeField] private float launchForce = 1f;

    [Header("Visual Stats")]
    [Tooltip("Scaling for the launch line visualizer.")]
    [SerializeField] private float launchLineScale = 1f;

    [Header("Internal Stats")]
    [Tooltip("Threshold for stopping movement")]
    [SerializeField] private float moveVelocityThreshold = 0.2f;
    public float MoveVelocityThreshold => moveVelocityThreshold;
    [Tooltip("Threshold for mouse movement to trigger launch")]
    [SerializeField] private float launchThreshold = 5f;

    // Components
    private PlayerInput playerInput;
    private Rigidbody2D rb;

    // Input action for mouse position
    private InputAction mousePosition;
    // The mouse position when the left mouse is pressed
    private Vector2 startingMousePos;

    // Whether the player is eligible for movement
    private bool canMove = true;
    // Whether the player is moving from a launch
    private bool isMovingFromLaunch = false;
    public bool IsMovingFromLaunch => isMovingFromLaunch;
    // Whether the ball is selected
    private bool ballSelected = false;
    // The launch line
    private LineRenderer launchLine;
    // The wormhole the player is exiting through (ensures that the player won't teleport back through)
    private Wormhole exitWormhole;
    public Wormhole ExitWormhole => exitWormhole;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();

        mousePosition = playerInput.actions["MousePosition"];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.linearVelocity.magnitude < moveVelocityThreshold) // stop the ball movement if it is almost still
        {
            if (rb.linearVelocity.magnitude > 0)
            {
                rb.linearVelocity = Vector2.zero;
            }
            canMove = true; // reset when ball is still
            isMovingFromLaunch = false;
        }
        if (ballSelected && launchLine != null)
        {
            launchLine.SetPosition(1, launchLineScale * (startingMousePos - mousePosition.ReadValue<Vector2>()));
        }
    }

    public void HandleLMouse(InputAction.CallbackContext ctx)
    {
        if (!canMove) return;
        if (ctx.started)
        {
            startingMousePos = mousePosition.ReadValue<Vector2>();
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(startingMousePos);
            LayerMask layerMask = LayerMask.GetMask("Player");
            RaycastHit2D hit = Physics2D.Raycast(worldMousePos, Vector2.zero, Mathf.Infinity, layerMask);
            
            if (hit.collider == null) return;

            ballSelected = true; // ball is selected
            launchLine = Instantiate(Manager.Instance.LoadedPrefabs.ShootLine, transform).GetComponent<LineRenderer>();
        }
        else if (ctx.canceled && ballSelected)
        {
            Vector2 newMousePos = mousePosition.ReadValue<Vector2>();
            Vector2 direction = startingMousePos - newMousePos;
            if (direction.magnitude > launchThreshold) // don't move if mouse barely moved
            {
                rb.AddForce(direction * launchForce, ForceMode2D.Impulse);
            }
            startingMousePos = Vector2.zero; // reset starting position
            canMove = false;
            isMovingFromLaunch = true;
            ballSelected = false; // unselects ball
            if (launchLine != null)
            {
                Destroy(launchLine.gameObject); // destroy the launch line
                launchLine = null;
            }
        }
    }

    public void SetExitWormhole(Wormhole w)
    {
        exitWormhole = w;
    }
}
