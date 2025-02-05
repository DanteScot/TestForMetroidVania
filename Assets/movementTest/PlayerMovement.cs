using System;
using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

[System.Serializable]
struct Checks{
    public LayerMask groundLayer;
    public Transform groundCheck;
    public Vector3 groundCheckSize;
    public Transform frontCheck;
    public Vector3 frontCheckSize;
    public Transform backCheck;
    public Vector3 backCheckSize;

    public void CorrectHalfSize(){
        groundCheckSize /= 2;
        frontCheckSize /= 2;
        backCheckSize /= 2;
    }
}

[System.Serializable]
struct PlayerStats{
    [Header("Movement")]
    public float horizontalAcceleration;
    public float horizontalDeceleration;
    public float horizontalMaxRunningSpeed;
    public float horizontalMaxSpeed;

    public float verticalMaxSpeed;
    public float verticalFastFallingSpeed;

    [Header("Jumping")]
    public float jumpHeight;
    public float timeToJumpApex;

    [HideInInspector] public float jumpStartSpeed;
    [HideInInspector] public float gravity;

    public void Recalculate(){
        jumpHeight += 0.15f;
        timeToJumpApex += 0.03f;

        gravity = -2*jumpHeight/(timeToJumpApex*timeToJumpApex);
        jumpStartSpeed = 2*jumpHeight/timeToJumpApex;

        Physics.gravity = new Vector3(0, gravity, 0);
    }
}

public class PlayerMovement : MonoBehaviour
{
    #region Required Variables
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Rigidbody rb;

    [Header("Checks")]
    [SerializeField] private Checks _checks;


    [Header("Speed multiplier")]
    [SerializeField] private float _speedMultiplier;



    [Header("Assist")]
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _jumpBufferTime = 0.1f;

    [Header("Debug")]
    public Vector3 _velocity;

    private SplineProjector _splineProjector;
    #endregion


    #region Class Variables
    private Vector2 moveInput;

    private bool wantsToJump;
    private bool isJumping;
    private float _lastTimeGrounded;
    #endregion

    // System.Diagnostics.Stopwatch sw = new ();

    #region Cached Variables
    Vector3 _targetVelocity;
    Vector3 _velocityChange;
    Vector3 _zeroVelocity;
    // Vector3 _rightVelocity;
    // Vector3 _leftVelocity;
    #endregion

    private bool IsGrounded { get => Physics.CheckBox(_checks.groundCheck.position, _checks.groundCheckSize, Quaternion.identity, _checks.groundLayer); }
    private bool IsFront { get => Physics.CheckBox(_checks.frontCheck.position, _checks.frontCheckSize, Quaternion.identity, _checks.groundLayer); }
    private bool IsBack { get => Physics.CheckBox(_checks.backCheck.position, _checks.backCheckSize, Quaternion.identity, _checks.groundLayer); }

    private void OnValidate() {
        Transform parent = transform.Find("Checks");
        _checks.groundCheck = parent.Find("Ground");
        _checks.frontCheck = parent.Find("Front");
        _checks.backCheck = parent.Find("Back");
    }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        _splineProjector = GetComponent<SplineProjector>();
    }

    private void Start() {
        _zeroVelocity = Vector3.zero;
        // _rightVelocity = -_splineProjector.result.forward * playerStats.horizontalMaxRunningSpeed;
        // _leftVelocity = _splineProjector.result.forward * playerStats.horizontalMaxRunningSpeed;

        playerStats.Recalculate();
        _checks.CorrectHalfSize();

        InputManager.Instance.OnMove += (move) => moveInput = move;
        InputManager.Instance.OnJump += () => StartCoroutine(Jump());
        
        StartCoroutine(CustomDebug());
    }

    IEnumerator CustomDebug(){
        while(true){
            _velocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);
            yield return null;
        }
    }

    void FixedUpdate(){
        #region Horizontal Movement

        _targetVelocity = moveInput.x > 0 ? _splineProjector.result.forward * playerStats.horizontalMaxRunningSpeed :
                         moveInput.x < 0 ? -_splineProjector.result.forward * playerStats.horizontalMaxRunningSpeed : _zeroVelocity;

        _velocityChange = playerStats.horizontalAcceleration * (_targetVelocity - rb.linearVelocity);
        _velocityChange.y = 0;
        rb.AddForce(_velocityChange, ForceMode.Acceleration);

        #endregion


        #region Vertical Movement

        if(wantsToJump){
            if(IsGrounded || Time.time - _lastTimeGrounded < _coyoteTime){
                // StartCoroutine(Test(transform.position.y));
                // isJumping = true;
                wantsToJump = false;
                // _velocityChange.x = rb.linearVelocity.x;
                // _velocityChange.y = playerStats.jumpStartSpeed - rb.linearVelocity.y;
                // rb.linearVelocity = new Vector3(rb.linearVelocity.x, playerStats.jumpStartSpeed - rb.linearVelocity.y, rb.linearVelocity.z);
                rb.AddForce(_splineProjector.result.up*(playerStats.jumpStartSpeed - rb.linearVelocity.y), ForceMode.VelocityChange);
            } else if(IsFront){
                wantsToJump = false;
                // _velocityChange.x = -_splineProjector.result.forward.x * playerStats.horizontalMaxRunningSpeed;
                // _velocityChange.y = playerStats.jumpStartSpeed - rb.linearVelocity.y;
                // rb.linearVelocity = new Vector3(rb.linearVelocity.x, playerStats.jumpStartSpeed - rb.linearVelocity.y, rb.linearVelocity.z);
                rb.AddForce((_splineProjector.result.up * (playerStats.jumpStartSpeed - rb.linearVelocity.y)) - (_splineProjector.result.forward * playerStats.horizontalMaxRunningSpeed), ForceMode.VelocityChange);
            } else if(IsBack){
                wantsToJump = false;
                // _velocityChange.x = _splineProjector.result.forward.x * playerStats.horizontalMaxRunningSpeed;
                // _velocityChange.y = playerStats.jumpStartSpeed - rb.linearVelocity.y;
                // rb.linearVelocity = new Vector3(rb.linearVelocity.x, playerStats.jumpStartSpeed - rb.linearVelocity.y, rb.linearVelocity.z);
                rb.AddForce((_splineProjector.result.up * (playerStats.jumpStartSpeed - rb.linearVelocity.y)) + (_splineProjector.result.forward * playerStats.horizontalMaxRunningSpeed), ForceMode.VelocityChange);
            }
        } else {
            _velocityChange.y = 0;
        }
        
        #endregion

        spriteRenderer.flipX = rb.linearVelocity.x < 0;
    }
    IEnumerator Test(float start){
        float max=start;
        yield return new WaitUntil(() => isJumping);
        while(true){
            if(transform.position.y > max) max = transform.position.y;
            if(!isJumping) break;
            yield return null;
        }
        Debug.Log("height: "+(max-start));
    }

    IEnumerator Jump(){
        wantsToJump = true;
        yield return new WaitForSeconds(_jumpBufferTime);
        wantsToJump = false;
    }


    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_checks.groundCheck.position, _checks.groundCheckSize);
        Gizmos.DrawWireCube(_checks.frontCheck.position, _checks.frontCheckSize);
        Gizmos.DrawWireCube(_checks.backCheck.position, _checks.backCheckSize);
    }
}