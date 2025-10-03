using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// get the input
// move the character by character controller
// animate the character by animator

public class Move_Controller : MonoBehaviour
{
    // classname 
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    // public vaiables
    [Header("Movement Settings")]
    // for movement
    public float WalkSpeed = 2.0f;
    public float RunSpeed = 4.0f;
    
    public float gravity = -100f;
    public float rotationFactorPerFrame = 5.0f;

    // for animation
    [Header("Animation Settings")]
    
    public float acceleration = 0.8f;
    public float deceleration = 0.8f;
    private float max_Run_Velocity = 1.0f;
    private float max_Walk_Velocity = 0.5f;

    // public float jumpHeight = 1.5f;

    // initialize the input data
    Vector2 currentMoveInput;
    Vector3 currentMove;
    float velocity_A = 0.0f;
    bool MovePressed;
    bool RunPressed;
    // bool JumpPressed;
    private Vector3 velocity;

    // Hash 
    int VelocityHash;

    
    // 绑定事件
    void onMovementInput (InputAction.CallbackContext context)
    {
        currentMoveInput = context.ReadValue<Vector2>();
        currentMove.x = currentMoveInput.x;
        currentMove.z = currentMoveInput.y;
        MovePressed = currentMoveInput.x != 0 || currentMoveInput.y !=0;
    }
    void onRunInput(InputAction.CallbackContext context)
    {
        RunPressed = context.ReadValue<float>() > 0.5f;
    }

    // void onJumpInput(InputAction.CallbackContext context)
    // {
    //     JumpPressed = context.ReadValue<float>() > 0.5f;
    // }

    void hanleMovement()
    {
        // basic movement
        Vector3 move = Vector3.zero;
        if (MovePressed)
        {
         float speed = RunPressed ? RunSpeed : WalkSpeed;

         // 获取摄像机的 forward 和 right（忽略y）
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        // 用摄像机朝向来计算移动向量
        move = (camRight * currentMove.x + camForward * currentMove.z) * speed;
        }   

        // on the ground
        if (characterController.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            // animator.SetBool("isJumping", false);
        }
        // // jump
        // if (JumpPressed && characterController.isGrounded)
        // {
        //     velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        // }
        velocity.y += gravity * Time.deltaTime * 10000;

        // 把水平和垂直运动合并
        move.y = velocity.y;
        // 一次性调用 Move
        characterController.Move(move * Time.deltaTime);
        }

    void handleAnimation()
    {
        // // increase speed
        // float current_Max_Velovity = RunPressed ? max_Run_Velocity : max_Walk_Velocity;

        // if (MovePressed && velocity_A < current_Max_Velovity)
        // {
        //     // if(velocity_A < 0.3f){ velocity_A = 0.3f;}
        //     velocity_A += Time.deltaTime * acceleration;
        // }
        // // decrease speed
        // if (!MovePressed && velocity_A > 0.0f)
        // {
        //     velocity_A -= Time.deltaTime * deceleration;
        //     if (velocity_A < 0.05f) velocity_A = 0.0f; // 小于阈值就停止动画
        // }

        // // reset velocity 防止越界
        // // 防止小于0
        // if (!MovePressed && velocity_A < 0.0f)
        // {
        //     velocity_A = 0.0f;
        // } //防止超过最大速度
        // else if (MovePressed && RunPressed && velocity_A > current_Max_Velovity)
        // {
	    //     velocity_A = current_Max_Velovity;
        // } // 突然松开跑步，速度缓缓下降
        // else if (MovePressed && velocity_A > current_Max_Velovity)
        // {
	    //     velocity_A -= Time.deltaTime * deceleration;
	    //     // 防止下降过头:如果靠近了就直接设置，跳出循环
	    //     if (velocity_A > current_Max_Velovity 
	    //     && velocity_A < current_Max_Velovity + 0.05f)
	    //     {
		//         velocity_A = current_Max_Velovity;
	    //     }
        // }
        // else if (velocity_A > current_Max_Velovity - 0.05f 
        // && velocity_A < current_Max_Velovity)
        // {
	    //     velocity_A = current_Max_Velovity;
        // }
        
        // animator.SetFloat(VelocityHash, velocity_A);

        // 优化
        float targetVelocity = 0f;

        if (MovePressed)
        {
            targetVelocity = RunPressed ? max_Run_Velocity : max_Walk_Velocity;
        }

        // 平滑过渡
        if (velocity_A < targetVelocity)
        {
            velocity_A += Time.deltaTime * acceleration;
            if (velocity_A > targetVelocity) velocity_A = targetVelocity;
        }
        else if (velocity_A > targetVelocity)
        {
            velocity_A -= Time.deltaTime * deceleration;
            if (velocity_A < targetVelocity) velocity_A = targetVelocity;
        }

        animator.SetFloat(VelocityHash, velocity_A);
    }

    void handleRotation()
    {
        Vector3 positionToLookAt;
    
        // the change in position our character should point to
        positionToLookAt.x = currentMove.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMove.z;
    
        // the current rotation of our character
        Quaternion currentRotation = transform.rotation;

        // 后退的时候不要转！
        if (MovePressed && currentMove.z >= 0) {
        // creates a new rotation based on where the player is currently pressing
        Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
        // rotate the character to face the positionToLookAt
        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }

    void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Enable();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // 获得 Animator 参数的哈希 ID
        VelocityHash = Animator.StringToHash("speed");

        // get the move value
        playerInput.player.move.performed += onMovementInput;
        playerInput.player.move.canceled += onMovementInput;

        playerInput.player.run.performed += onRunInput;
        playerInput.player.run.canceled += onRunInput;
        
        // playerInput.player.jump.performed += onJumpInput;
        // playerInput.player.jump.canceled += onJumpInput;
    }


    void OnDisable() {
        playerInput.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        hanleMovement();
        handleRotation();
        handleAnimation();
    }
}
