// InputManager.cs
// מנהל את כל הקלטים באמצעות מערכת האינפוט החדשה (New Input System).
// חובה לוודא שחבילת "Input System" מותקנת ב-Package Manager.
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputAction pMove, pJump, pDrop, pPunch;
    private InputAction bMove, bJump, bDrop, bPunch, bThrow;
    private InputAction pause, uiConfirm;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        SetupInputs();
    }

    private void SetupInputs()
    {
        // פופאי (חיצים + Enter)
        pMove = new InputAction("PMove", InputActionType.Value);
        pMove.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        pJump = new InputAction("PJump", InputActionType.Button, "<Keyboard>/upArrow");
        pDrop = new InputAction("PDrop", InputActionType.Button, "<Keyboard>/downArrow");
        pPunch = new InputAction("PPunch", InputActionType.Button, "<Keyboard>/enter");

        // בלוטו (WASD + F/Shift)
        bMove = new InputAction("BMove", InputActionType.Value);
        bMove.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        bJump = new InputAction("BJump", InputActionType.Button, "<Keyboard>/w");
        bDrop = new InputAction("BDrop", InputActionType.Button, "<Keyboard>/s");
        bPunch = new InputAction("BPunch", InputActionType.Button, "<Keyboard>/f");
        bThrow = new InputAction("BThrow", InputActionType.Button, "<Keyboard>/leftShift");

        // מערכת
        pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        pause.AddBinding("<Keyboard>/p");
        uiConfirm = new InputAction("UIConfirm", InputActionType.Button, "<Keyboard>/space");
        uiConfirm.AddBinding("<Keyboard>/enter");

        EnableAll();
    }

    private void EnableAll()
    {
        pMove.Enable(); pJump.Enable(); pDrop.Enable(); pPunch.Enable();
        bMove.Enable(); bJump.Enable(); bDrop.Enable(); bPunch.Enable(); bThrow.Enable();
        pause.Enable(); uiConfirm.Enable();
    }

    private void OnDestroy()
    {
        pMove.Disable(); pJump.Disable(); pDrop.Disable(); pPunch.Disable();
        bMove.Disable(); bJump.Disable(); bDrop.Disable(); bPunch.Disable(); bThrow.Disable();
        pause.Disable(); uiConfirm.Disable();
    }

    public Vector2 PopeyeMove => pMove.ReadValue<Vector2>();
    public bool PopeyeJumpDown => pJump.WasPressedThisFrame();
    public bool PopeyeDropDown => pDrop.WasPressedThisFrame();
    public bool PopeyePunchDown => pPunch.WasPressedThisFrame();

    public Vector2 BlutoMove => bMove.ReadValue<Vector2>();
    public bool BlutoJumpDown => bJump.WasPressedThisFrame();
    public bool BlutoDropDown => bDrop.WasPressedThisFrame();
    public bool BlutoPunchDown => bPunch.WasPressedThisFrame();
    public bool BlutoThrowDown => bThrow.WasPressedThisFrame();

    public bool PauseDown => pause.WasPressedThisFrame();
    public bool UIConfirmDown => uiConfirm.WasPressedThisFrame();
}