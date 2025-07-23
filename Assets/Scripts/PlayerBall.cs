using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBall : NetworkBehaviour
{
    [Header("Player Stats")]
    [Tooltip("The force multiplier applied to the ball when launched.")]
    [SerializeField] private float launchForce = 100f;

    [Header("Visual Stats")]
    [Tooltip("Scaling for the launch line visualizer.")]
    [SerializeField] private float launchLineScale = 1f;

    [Header("Internal Stats")]
    [Tooltip("Threshold for stopping movement")]
    [SerializeField] private float moveVelocityThreshold = 0.2f;
    public float MoveVelocityThreshold => moveVelocityThreshold;
    [Tooltip("Threshold for mouse movement to trigger launch")]
    [SerializeField] private float launchThreshold = 0.2f;

    // Components
    private PlayerInput playerInput;
    private Rigidbody2D rb;

    // Input action for mouse position
    private InputAction mousePosition;
    // The mouse world position when the left mouse is pressed
    private Vector2 startingMousePos;

    // Server side
    // Whether the player has completed their turn
    private bool completedTurn = false;

    // Whether the ball is selected
    private bool ballSelected = false;
    // The launch line
    private LineRenderer launchLine;
    // The wormhole the player is exiting through (ensures that the player won't teleport back through)
    private Wormhole exitWormhole;
    public Wormhole ExitWormhole => exitWormhole;

    // Server side
    private List<Powerup> powerups = new List<Powerup>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        else
        {
            Destroy(GetComponent<Rigidbody2D>());
        }
        if (IsClient && IsOwner)
        {
            playerInput = GetComponent<PlayerInput>();
            mousePosition = playerInput.actions["MousePosition"];
            ClientManager.Instance.SetPlayer(this);
        }
        else
        {
            Destroy(GetComponent<PlayerInput>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            if (rb.linearVelocity.magnitude < moveVelocityThreshold) // stop the ball movement if it is almost still
            {
                if (rb.linearVelocity.magnitude > 0)
                {
                    rb.linearVelocity = Vector2.zero;
                }
                if (completedTurn)
                {
                    completedTurn = false;
                    Manager.Instance.NextPlayerTurn();
                }
            }
        }
        if (IsClient && IsOwner)
        {
            if (ballSelected && launchLine != null)
            {
                launchLine.SetPosition(1, launchLineScale * (startingMousePos - (Vector2)Camera.main.ScreenToWorldPoint(mousePosition.ReadValue<Vector2>())));
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (completedTurn)
        {
            Manager.Instance.NextPlayerTurn();
        }
    }

    public void HandleLMouse(InputAction.CallbackContext ctx)
    {
        if (!(IsClient && IsOwner && ClientManager.Instance.CurrentTurn && !ClientManager.Instance.AstralProjecting)) return;
        if (ctx.started)
        {
            Vector2 mousePos = mousePosition.ReadValue<Vector2>();
            startingMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            LayerMask layerMask = LayerMask.GetMask("Player");
            RaycastHit2D hit = Physics2D.Raycast(startingMousePos, Vector2.zero, Mathf.Infinity, layerMask);
            
            if (hit.collider == null || hit.collider.gameObject != gameObject) return;

            ballSelected = true; // ball is selected
            launchLine = Instantiate(ClientManager.Instance.LoadedPrefabs.ShootLine, transform).GetComponent<LineRenderer>();
        }
        else if (ctx.canceled && ballSelected)
        {
            Vector2 newMousePos = Camera.main.ScreenToWorldPoint(mousePosition.ReadValue<Vector2>());
            Vector2 direction = startingMousePos - newMousePos;
            if (direction.magnitude > launchThreshold) // don't move if mouse barely moved
            {
                ClientManager.Instance.PlayerLaunch(); // notify the client manager that the player is launching
                LaunchRpc(direction, default);
                startingMousePos = Vector2.zero; // reset starting position
            }
            ballSelected = false; // unselects ball
            if (launchLine != null)
            {
                Destroy(launchLine.gameObject); // destroy the launch line
                launchLine = null;
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void LaunchRpc(Vector2 direction, RpcParams rpcParams)
    {
        completedTurn = true;
        rb.AddForce(direction * launchForce, ForceMode2D.Impulse);
    }

    public void SetExitWormhole(Wormhole w)
    {
        if (!IsServer) return;
        exitWormhole = w;
    }

    public void PickupPowerup(Powerup powerup)
    {
        if (!IsServer) return;
        if (powerups.Count == 3)
        {
            powerups.RemoveAt(0);
        }
        powerups.Add(powerup);
        PickupPowerupRpc(powerup.PowerupName, default);
    }

    [Rpc(SendTo.Owner)]
    public void PickupPowerupRpc(string name, RpcParams rpcParams)
    {
        UIManager.Instance.PickupPowerup(name);
    }
}
