using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerSoul : MonoBehaviour
{
    [SerializeField] private float movementSpeed;

    private InputAction moveAction;
    private InputAction mousePosition;
    private bool mouseHeldDown = false;
    private bool movingByMouse = false;
    private Vector2 targetPosition;

    private TextFollow infoText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        mousePosition = playerInput.actions["MousePosition"];
        infoText = Instantiate(ClientManager.Instance.LoadedPrefabs.FollowText).GetComponent<TextFollow>();
        infoText.SetTarget(transform);
        infoText.SetText("Player Soul\n(WASD/click to move)");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        if (mouseHeldDown)
        {
            movingByMouse = true;
            targetPosition = Camera.main.ScreenToWorldPoint(mousePosition.ReadValue<Vector2>());
        }
        if (moveValue.magnitude > 0)
        {
            transform.position += (Vector3)(moveValue.normalized * movementSpeed * Time.deltaTime);
            movingByMouse = false;
        }
        else if (movingByMouse)
        {
            float moveDist = movementSpeed * Time.deltaTime;
            if (Vector2.Distance(transform.position, targetPosition) <= moveDist)
            {
                transform.position = targetPosition;
                movingByMouse = false;
            }
            else
            {
                transform.position += moveDist * ((Vector3)targetPosition - transform.position).normalized;
            }
        }
    }

    public void HandleMouse(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            mouseHeldDown = true;
        }
        else if (ctx.canceled)
        {
            mouseHeldDown = false;
        }
    }

    private void OnDestroy()
    {
        Destroy(infoText.gameObject);
    }
}
