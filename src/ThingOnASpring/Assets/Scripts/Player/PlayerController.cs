using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private float crouchJumpPower = 3f;
    [SerializeField] private float crouchJumpChargeTime = 0.6f;

    [Header("Ground check")]
    [SerializeField] private float _groundCheckFeetWidth = 0.2f;
    [SerializeField] private int _nGroundRays = 5;
    [SerializeField] private float _groundCheckDistance = 0.02f;
    [SerializeField] private LayerMask _groundCheckLayerMask;


    private Rigidbody2D _rigidBody;

    private Animator _animator;
    private const string _animState_Idle      = "ThingIdle";
    private const string _animState_WalkLeft  = "ThingWalkLeft";
    private const string _animState_WalkRight = "ThingWalkRight";
    private const string _animState_JumpLeft  = "ThingJumpLeft";
    private const string _animState_JumpRight = "ThingJumpRight";

    private const string _animProp_Crouch = "Crouch";
    private const string _animProp_Walk   = "Walk";
    private const string _animProp_Jump   = "Jump";
    private const string _animProp_Slide  = "Slide";
    private const string _animProp_Fly    = "Fly";

    private PlayerInput _playerInput;
    private InputAction _walkAction;
    private InputAction _jumpAction;
    private InputAction _crouchAction;

    //private enum AnimState { Idle, WalkLeft, WalkRight, JumpLeft, JumpRight }
    //private AnimState _animState = AnimState.Idle;

    private bool _isGrounded;
    private float _movement;
    private float _crouchStartTime;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();

        _playerInput  = GetComponentInChildren<PlayerInput>();
        _walkAction   = _playerInput.actions["Walk"];
        _jumpAction   = _playerInput.actions["Jump"];
        _crouchAction = _playerInput.actions["Crouch"];
    }

    private void OnEnable()
    {
        //var stateChange = _animator.GetBehaviour<StateChangeBehaviour>();
        //stateChange.RegisterOnStateEnter(Animator.StringToHash(_animState_Idle),      (animator, stateInfo, layer) => _animState = AnimState.Idle);
        //stateChange.RegisterOnStateEnter(Animator.StringToHash(_animState_WalkLeft),  (animator, stateInfo, layer) => _animState = AnimState.WalkLeft);
        //stateChange.RegisterOnStateEnter(Animator.StringToHash(_animState_WalkRight), (animator, stateInfo, layer) => _animState = AnimState.WalkRight);
        //stateChange.RegisterOnStateEnter(Animator.StringToHash(_animState_JumpLeft),  (animator, stateInfo, layer) => _animState = AnimState.JumpLeft);
        //stateChange.RegisterOnStateEnter(Animator.StringToHash(_animState_JumpRight), (animator, stateInfo, layer) => _animState = AnimState.JumpRight);

        _walkAction.performed   += OnWalkPerformed;
        _walkAction.canceled    += OnWalkCanceled;
        _jumpAction.performed   += OnJumpPerformed;
        _crouchAction.performed += OnCrouchPerformed;
        _crouchAction.canceled  += OnCrouchCanceled;
    }

    private void OnDisable()
    {
        // NOTE: I wanted to UnRegisterOnStateEnter in the animator behaviour but that appears to be null here.

        _walkAction.performed   -= OnWalkPerformed;
        _walkAction.canceled    -= OnWalkCanceled;
        _jumpAction.performed   -= OnJumpPerformed;
        _crouchAction.performed -= OnCrouchPerformed;
        _crouchAction.canceled  -= OnCrouchCanceled;
    }

    private void FixedUpdate()
    {
        _isGrounded = CheckGrounded();

        if (!_isGrounded)
        {
            return;
        }

        _rigidBody.velocity = new Vector2(_movement, _rigidBody.velocity.y);
    }

    private bool CheckGrounded()
    {
        var y = transform.position.y;
        for (var i = 0; i < _nGroundRays; i++)
        {
            var offset = ((float)i / (_nGroundRays - 1) - 0.5f) * _groundCheckFeetWidth;
            var x = transform.position.x - offset;
            var hit = Physics2D.Raycast (
                new Vector2(x, y),
                Vector2.down,
                _groundCheckDistance,
                _groundCheckLayerMask
            );
            if (hit.collider != null)
            {
                return true;
            }
        }

        return false;
    }

    private void OnWalkPerformed(InputAction.CallbackContext context)
    {
        var value = (int)context.ReadValue<float>();
        _animator.SetInteger(_animProp_Walk, value);
        _movement = value * walkSpeed;
    }
    private void OnWalkCanceled(InputAction.CallbackContext obj)
    {
        _animator.SetInteger(_animProp_Walk, 0);
        _movement = 0f;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!_isGrounded || !(_movement < 0f || _movement > 0f))
        {
            return;
        }

        _animator.SetTrigger(_animProp_Jump);
        _rigidBody.AddForce(new Vector2(Mathf.Sign(_movement) * 0.7071f, 0.7071f) * jumpPower, ForceMode2D.Impulse);
    }

    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        if (!_isGrounded)
        {
            return;
        }

        _crouchStartTime = Time.time;
        _animator.SetBool(_animProp_Crouch, true);
    }
    private void OnCrouchCanceled(InputAction.CallbackContext obj)
    {
        _animator.SetBool(_animProp_Crouch, false);
        var chargeTime = Time.time - _crouchStartTime;
        var charge = Mathf.Clamp01(chargeTime / crouchJumpChargeTime) * crouchJumpPower;
        _rigidBody.AddForce(Vector2.up * charge, ForceMode2D.Impulse);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (_isGrounded)
        {
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.1f, 0.02f);
        }

        var y = transform.position.y;
        for (var i = 0; i < _nGroundRays; i++)
        {
            var offset = ((float)i / (_nGroundRays - 1) - 0.5f) * _groundCheckFeetWidth;
            var x = transform.position.x - offset;
            Gizmos.DrawLine(new Vector3(x, y), new Vector3(x, y - _groundCheckDistance));
        }
    }
}
