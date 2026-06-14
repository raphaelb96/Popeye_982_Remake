// InputManager.cs
// Singleton that manages all player inputs using Unity's New Input System.
// Make sure the "Input System" package is installed via Package Manager.
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Singleton instance — accessible from any script via InputManager.Instance
    public static InputManager Instance { get; private set; }

    // Popeye input actions (Player 1)
    private InputAction pMove;   // 2D movement vector (WASD)
    private InputAction pJump;   // Jump (W)
    private InputAction pDrop;   // Drop through platform (S)
    private InputAction pPunch;  // Punch (C)

    // Bluto input actions (Player 2)
    private InputAction bMove;   // 2D movement vector (Arrow keys)
    private InputAction bJump;   // Jump (Up Arrow)
    private InputAction bDrop;   // Drop through platform (Down Arrow)
    private InputAction bPunch;  // Heavy punch (Enter)
    private InputAction bThrow;  // Throw bottle (Right Shift)

    // System actions
    private InputAction pause;     // Pause game (Escape or P)
    private InputAction uiConfirm; // Confirm UI action (Space or Enter)
    private InputAction restart;   // Restart round from pause menu (R)
    private InputAction quit;      // Quit game from pause menu (Q)

    private void Awake()
    {
        // Singleton pattern — only one InputManager can exist at a time
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        SetupInputs();
    }

    private void SetupInputs()
    {
        // ── POPEYE (Player 1) ── WASD to move, C to punch ──────────────────

        // 2D composite binding reads all 4 directions as a single Vector2
        pMove = new InputAction("PMove", InputActionType.Value);
        pMove.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // W is both the Up direction AND the jump trigger
        pJump = new InputAction("PJump", InputActionType.Button, "<Keyboard>/w");
        // S is both the Down direction AND the drop-through trigger
        pDrop = new InputAction("PDrop", InputActionType.Button, "<Keyboard>/s");
        // C triggers Popeye's quick punch
        pPunch = new InputAction("PPunch", InputActionType.Button, "<Keyboard>/c");

        // ── BLUTO (Player 2) ── Arrow keys to move, Enter to punch ─────────

        bMove = new InputAction("BMove", InputActionType.Value);
        bMove.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        // Up Arrow is both the Up direction AND the jump trigger
        bJump = new InputAction("BJump", InputActionType.Button, "<Keyboard>/upArrow");
        // Down Arrow is both the Down direction AND the drop-through trigger
        bDrop = new InputAction("BDrop", InputActionType.Button, "<Keyboard>/downArrow");
        // Enter triggers Bluto's heavy punch
        bPunch = new InputAction("BPunch", InputActionType.Button, "<Keyboard>/enter");
        // Right Shift triggers Bluto's bottle throw (costs 1 bottle from inventory)
        bThrow = new InputAction("BThrow", InputActionType.Button, "<Keyboard>/rightShift");

        // ── SYSTEM ───────────────────────────────────────────────────────────

        // Pause accepts both Escape and P
        pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        pause.AddBinding("<Keyboard>/p");

        // UI confirm accepts both Space and Enter
        uiConfirm = new InputAction("UIConfirm", InputActionType.Button, "<Keyboard>/space");
        uiConfirm.AddBinding("<Keyboard>/enter");

        // Pause menu shortcuts: R = restart round, Q = quit game
        restart = new InputAction("Restart", InputActionType.Button, "<Keyboard>/r");
        quit = new InputAction("Quit", InputActionType.Button, "<Keyboard>/q");

        // Enable all actions so they start listening for input
        EnableAll();
    }

    // Enable all input actions — called once at startup
    private void EnableAll()
    {
        pMove.Enable(); pJump.Enable(); pDrop.Enable(); pPunch.Enable();
        bMove.Enable(); bJump.Enable(); bDrop.Enable(); bPunch.Enable(); bThrow.Enable();
        pause.Enable(); uiConfirm.Enable(); restart.Enable(); quit.Enable();
    }

    // Disable all input actions when this object is destroyed — prevents memory leaks
    private void OnDestroy()
    {
        pMove.Disable(); pJump.Disable(); pDrop.Disable(); pPunch.Disable();
        bMove.Disable(); bJump.Disable(); bDrop.Disable(); bPunch.Disable(); bThrow.Disable();
        pause.Disable(); uiConfirm.Disable(); restart.Disable(); quit.Disable();
    }

    // ── PUBLIC PROPERTIES ────────────────────────────────────────────────────
    // These are read by PopeyeController, BlutoController, PauseManager, etc.

    // Popeye — returns a Vector2 with X (left/right) and Y (up/down) values
    public Vector2 PopeyeMove => pMove.ReadValue<Vector2>();
    // WasPressedThisFrame() returns true only on the exact frame the key is pressed
    public bool PopeyeJumpDown => pJump.WasPressedThisFrame();
    public bool PopeyeDropDown => pDrop.WasPressedThisFrame();
    public bool PopeyePunchDown => pPunch.WasPressedThisFrame();

    // Bluto
    public Vector2 BlutoMove => bMove.ReadValue<Vector2>();
    public bool BlutoJumpDown => bJump.WasPressedThisFrame();
    public bool BlutoDropDown => bDrop.WasPressedThisFrame();
    public bool BlutoPunchDown => bPunch.WasPressedThisFrame();
    public bool BlutoThrowDown => bThrow.WasPressedThisFrame();

    // System
    public bool PauseDown => pause.WasPressedThisFrame();
    public bool UIConfirmDown => uiConfirm.WasPressedThisFrame();
    public bool RestartDown => restart.WasPressedThisFrame();
    public bool QuitDown => quit.WasPressedThisFrame();
}
