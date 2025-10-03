using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonCamera : MonoBehaviour
{

    [Header("Movement Settings")]
    public float WalkSpeed = 3f;
    public float RunSpeed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Look Settings")]
    public Transform cameraHolder;          // 摄像机的 helper
    public float lookSpeed = 1f;            // 水平/垂直旋转灵敏度
    public Vector2 lookMinMax = new Vector2(-70f, 80f); // 垂直旋转限制
    public float smoothTime = 0.05f;        // 平滑过渡
    public float deadZone = 0.05f;          // 鼠标微抖死区

    // component needed
    private CharacterController characterController;
    private Camera cam;
    PlayerInput playerInput;
    private bool JumpPressed;
    private bool RunPressed;

    // parameters
    // for movement
    private Vector2 currentMoveInput;
    private Vector3 currentMove;
    private bool MovePressed;

    // for look
    private Vector2 lookInput;
    private Vector2 currentLook;
    private Vector2 lookVelocity;
    private float yaw;
    private float pitch;

    // for zoom
    private float zoomInput;

    private Vector3 velocity;

    // deal with event
    // 把 perform 和 cancel 的逻辑写在一起了如果需要优化再来改吧（）
    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMoveInput = context.ReadValue<Vector2>();
        currentMove.x = currentMoveInput.x;
        currentMove.z = currentMoveInput.y;
        MovePressed = currentMoveInput.x != 0 || currentMoveInput.y != 0;
    }

    void onRunInput(InputAction.CallbackContext context)
    {
        RunPressed = context.ReadValue<float>() > 0.5f;
    }

    void onJumpInput(InputAction.CallbackContext context)
    {
        JumpPressed = context.ReadValue<float>() > 0.5f;
    }

    void onLookPerformed(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    void onLookCanceled(InputAction.CallbackContext context)
    {
        lookInput = Vector2.zero;
    }

    void onZoomInput(InputAction.CallbackContext context)
    {
        zoomInput = context.ReadValue<Vector2>().y;
    }

    void onZoomCanceled(InputAction.CallbackContext context)
    {
        zoomInput = 0f;
    }


    void handleMovement()
    {
        // 计算前进方向 move
        // 获取摄像机方向
        Vector3 camForward = cameraHolder.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cameraHolder.right;
        camRight.y = 0f;
        camRight.Normalize();

        // 转换到世界空间
        Vector3 move = camRight * currentMoveInput.x + camForward * currentMoveInput.y;
        if (move.magnitude > 1f) move.Normalize();
        currentMove = move;

        // 应用移动
        if (MovePressed && !RunPressed)
        {
            characterController.Move(currentMove * WalkSpeed * Time.deltaTime);
        }
        else if (MovePressed && RunPressed)
        {
            characterController.Move(currentMove * RunSpeed * Time.deltaTime);
        }
        // 在地上
        if (characterController.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f; // always on the ground
        }// jump
        if (JumpPressed && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        // 给微小抖动设置 deadZone
        Vector2 input = lookInput;
        if (input.magnitude < deadZone) input = Vector2.zero;

        // 平滑
        currentLook = Vector2.SmoothDamp(currentLook, input, ref lookVelocity, smoothTime);

        yaw += currentLook.x * lookSpeed;
        pitch -= currentLook.y * lookSpeed;

        pitch = Mathf.Clamp(pitch, lookMinMax.x, lookMinMax.y);

        // 应用旋转
        transform.rotation = Quaternion.Euler(0f, yaw, 0f); // 角色水平旋转
        cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f); // 摄像机上下旋转
    }

    // 鼠标滚轮缩放
    void HandleZoom()
    {
        if (cam == null) return;
        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            cam.fieldOfView -= zoomInput * 0.1f; // 用 zoomInput 调整FOV
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, 30f, 90f);
        }
    }

    void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Enable();
        characterController = GetComponent<CharacterController>();
        cam = cameraHolder.GetComponent<Camera>();

        // 设置初始相机位置（相对于角色）
        Vector3 cameraOffset = new Vector3(0f, 1.7f, 0f); // x:左右偏移, y:高度, z:前后
        cameraHolder.localPosition = cameraOffset;


        playerInput.player.move.performed += onMovementInput;
        playerInput.player.move.canceled += onMovementInput;

        playerInput.player.lookAction.performed += onLookPerformed;
        playerInput.player.lookAction.canceled += onLookCanceled;

        playerInput.player.run.performed += onRunInput;
        playerInput.player.run.canceled += onRunInput;

        playerInput.player.jump.performed += onJumpInput;
        playerInput.player.jump.canceled += onJumpInput;

        playerInput.player.zoom.performed += onZoomInput;
        playerInput.player.zoom.canceled += onZoomCanceled;
    }

    void OnDisable() {
        playerInput.Disable();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        handleMovement();
        HandleLook();
        HandleZoom();
    }
}
