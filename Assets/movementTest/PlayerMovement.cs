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
    // Variabili che devono essere assegnate dall'inspector o che si riferiscono a classi esterne
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private SplineProjector _splineProjector;
    private Rigidbody rb;

    [Header("Checks")]
    [SerializeField] private Checks _checks;

    [Header("Assist")]
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _jumpBufferTime = 0.1f;
    #endregion

    #region Class Variables
    // Variabili necessarie per il corretto funzionamento della classe
    private Vector2 moveInput;
    private bool wantsToJump;
    private float _lastTimeGrounded;
    #endregion

    #region Cached Variables
    // Sono variabili che vengono salvate solo per evitare di appesantire il Garbage Collector
    Vector3 _targetVelocity;
    Vector3 _velocityChange;
    Vector3 _zeroVelocity;
    Vector3 _forewardTimesSpeed;
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

        playerStats.Recalculate();
        _checks.CorrectHalfSize();

        InputManager.Instance.OnMove += (move) => moveInput = move;
        InputManager.Instance.OnJump += () => StartCoroutine(Jump());
    }

    void FixedUpdate(){
        #region Variables

        if(IsGrounded) _lastTimeGrounded = Time.time;

        _forewardTimesSpeed = _splineProjector.result.forward * playerStats.horizontalMaxRunningSpeed;

        #endregion


        #region Horizontal Movement

        if(moveInput.x > 0){
            _targetVelocity = _forewardTimesSpeed;
            spriteRenderer.flipX = false;
        }
        else if(moveInput.x < 0){
            _targetVelocity = -_forewardTimesSpeed;
            spriteRenderer.flipX = true;
        }
        else _targetVelocity = _zeroVelocity;

        _velocityChange = playerStats.horizontalAcceleration * (_targetVelocity - rb.linearVelocity);
        _velocityChange.y = 0;
        rb.AddForce(_velocityChange, ForceMode.Acceleration);

        #endregion


        #region Vertical Movement

        if(wantsToJump){
            if(Time.time - _lastTimeGrounded < _coyoteTime){
                wantsToJump = false;
                rb.AddForce(_splineProjector.result.up * (playerStats.jumpStartSpeed - rb.linearVelocity.y), ForceMode.VelocityChange);
            } else if(IsFront){
                wantsToJump = false;
                rb.AddForce((_splineProjector.result.up * (playerStats.jumpStartSpeed - rb.linearVelocity.y)) - _forewardTimesSpeed, ForceMode.VelocityChange);
            } else if(IsBack){
                wantsToJump = false;
                rb.AddForce((_splineProjector.result.up * (playerStats.jumpStartSpeed - rb.linearVelocity.y)) + _forewardTimesSpeed, ForceMode.VelocityChange);
            }
        } else {
            _velocityChange.y = 0;
        }
        
        #endregion
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