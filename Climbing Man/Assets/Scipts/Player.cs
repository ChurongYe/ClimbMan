using System;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using static System.Net.WebRequestMethods;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.UI.Image;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class Player : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 2.0f;

        public float SprintSpeed = 2.0f;

        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        public float SpeedChangeRate = 10.0f;

        public float Gravity = -15.0f;

        [Header("Player Grounded")]

        public bool Grounded = true;

        public float GroundedOffset = 0.5f;

        public float GroundedRadius = 0.28f;

        public LayerMask GroundLayers;

        [Header("Player Jump")]
        public float jumpHeight = 5f;

        public float Sensitivity = 2f;

        private float jumpForce;
        private Vector3 inputDirection;
        private float LookRotation = 0.0f;

        private Vector3 PlayDirection;

        [Header("Player Climb")]

        public GameObject HeadCheck;

        public GameObject FootCheck;

        public GameObject LadderCheck;

        public GameObject FallDownCheck;

        public LayerMask Ladder;

        private GameObject  Firsthit;
        private  bool IfclimbOn = false;
        private  bool IfclimbOff = false;
        //private  bool IfclimbRight = false;
        private  bool IfclimbingOn = false;
        private  bool IfclimbingOff = false;
        //private  bool IfRayRight = false;
        GameObject hitObjectOn;
        GameObject hitObjectOff;
        private bool Climbmove = false;
        private bool Left;
        private bool Right;
        private float Falltime = 0.0f;
        private bool Iffall = false;
        private bool Ifgo = true;
        Vector3 Target;
        Vector3 target;
        float timeout = 0.3f;
        bool Topcheck = false;
        public GameObject HitObject;
        private GameObject P1;
        private GameObject P2;
        private GameObject Turn;
        private bool Ifturn1;
        private bool Ifturn2;
        [Header("Cinemachine")]

        public GameObject CinemachineCameraTarget;

        public GameObject Camera;

        public float CameraAngleOverride = 0.0f;

        public bool LockCameraPosition = false;

        //mouse control
        public bool Ifjump = false;
        [Range(-5, 5)]
        private float mouseX = 0f;
        [Range(-5, 5)]
        private float mouseY = 0f;

        // cinemachine
        private float _cinemachineTargetYaw;
        private bool Camerarotate;

        // player
        private float _speed;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }
        //
        public enum PlayerState 
        {  
            Move,
            Jump,
            Fall,
            Climb,
            Falldown,
            Top,
            Jumpdown

        }
        public PlayerState State = PlayerState.Move;

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            Cursor.lockState = CursorLockMode.Locked;
        }
        private void Start()
        {
            CinemachineCameraTarget.transform.parent = this.transform;
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
        }

        private void Update()
        {
            //Debug.DrawRay(FallDownCheck.transform.position, Vector3.up.normalized * 0.5f, Color.blue);
            switch (State )
            {
                case PlayerState.Move:
                    Move();
                    if(Input.GetMouseButton(0))
                    {
                        BeforeJump();
                        State = PlayerState.Jump;
                    }
                    else if(!Grounded)
                    {
                        State = PlayerState.Jumpdown;
                    }
                    break;
                case PlayerState.Jumpdown:
                    JumpDown();
                    {
                        if(Grounded)
                        {
                            State = PlayerState.Move;
                        }
                    }
                    break;
                case PlayerState.Jump:
                    Jump();
                    if(Input.GetMouseButtonUp(0))
                    {
                        State = PlayerState.Fall;
                    }
                    break;
                case PlayerState.Fall:
                    Fall();
                    ClimbCheck();
                    if (ClimbCheck())
                    {
                        Ifjump = false;
                        transform.rotation = Firsthit.transform.rotation;
                        HitObject = Firsthit;
                        if(Mathf.Approximately(Firsthit.transform.eulerAngles.y, 0f))
                        {
                            transform.position = new Vector3(transform.position.x, (Firsthit.transform.position.y - 0.5f),
                                (Firsthit.transform.position.z - 0.5f));
                            Debug.Log("0");
                        }
                        else if (Mathf.Approximately(Firsthit.transform.eulerAngles.y, 90f))
                        {
                            transform.position = new Vector3((Firsthit.transform.position.x - 0.5f), (Firsthit.transform.position.y - 0.5f),
                                transform.position.z);
                            Debug.Log("90");
                        }
                        else if(Mathf.Approximately(Firsthit.transform.eulerAngles.y, -90f))
                        {
                            transform.position = new Vector3((Firsthit.transform.position.x + 0.5f), (Firsthit.transform.position.y - 0.5f),
                                transform.position.z);
                            Debug.Log("-90");
                        }
                        else if (Mathf.Approximately(Firsthit.transform.eulerAngles.y, 180f))
                        {
                            transform.position = new Vector3(transform.position.x, (Firsthit.transform.position.y - 0.5f),
                                (Firsthit.transform.position.z + 0.5f));
                            Debug.Log("180");
                        }
                            Climbmove = true;
                        State = PlayerState.Climb;
                    }
                    else if(Grounded )
                    {
                        State = PlayerState.Move;
                    }   
                    break;
                case PlayerState.Climb:
                    Climb();
                    ClimbMove();
                    if (Input.GetMouseButtonDown(1))
                    {
                        HitObject = null;
                        Falltime = 0.2f;
                        State = PlayerState.Falldown;
                    }
                    if(Topcheck)
                    {
                        if(timeout >0)
                        {
                            timeout -= Time.deltaTime;
                        }
                        if (Input.GetKeyDown(KeyCode.W) && timeout <0)
                        {
                            timeout = -2f;
                            if (Mathf.Approximately(hitObjectOn.transform.eulerAngles.y, 0f))
                            {
                                Target = new Vector3(transform.position.x, (transform.position.y + 3f),
                                (transform.position.z + 1.5f));
                            }
                            else if(Mathf.Approximately(hitObjectOn.transform.eulerAngles.y, 90f))
                            {
                                Target = new Vector3((transform.position.x + 1.5f), (transform.position.y + 3f),
                                transform.position.z);
                            }
                            else if (Mathf.Approximately(hitObjectOn.transform.eulerAngles.y, -90f))
                            {
                                Target = new Vector3((transform.position.x - 1.5f), (transform.position.y + 3f),
                                transform.position.z);
                            }
                            else if (Mathf.Approximately(hitObjectOn.transform.eulerAngles.y, 180f))
                            {
                                Target = new Vector3(transform.position.x, (transform.position.y + 3f),
                                 (transform.position.z - 1.5f));
                            }
                            Ifgo = true;
                            State = PlayerState.Top;
                        }
                    }
                    break;
                case PlayerState.Top:
                    Totop();
                    {
                        if (Iffall && Grounded)
                        {
                            Iffall = false;
                            State = PlayerState.Move;
                        }
                    }
                    break;
                case PlayerState.Falldown:
                    Falldown();
                    FalldownCheck();
                    if (Grounded)
                    {
                        State = PlayerState.Move;
                    }
                    if (Falltime <0)
                    {
                        if (FalldownCheck())
                        {
                            transform.rotation = Firsthit.transform.rotation;
                            HitObject = Firsthit;
                            if (Mathf.Approximately(Firsthit.transform.eulerAngles.y, 0f))
                            {
                                transform.position = new Vector3(transform.position.x, (Firsthit.transform.position.y - 0.5f),
                                    (Firsthit.transform.position.z - 0.5f));
                            }
                            else if (Mathf.Approximately(Firsthit.transform.eulerAngles.y, 90f))
                            {
                                transform.position = new Vector3((Firsthit.transform.position.x - 0.5f), (Firsthit.transform.position.y - 0.5f),
                                    transform.position.z);
                            }
                            else if (Mathf.Approximately(Firsthit.transform.eulerAngles.y, -90f))
                            {
                                transform.position = new Vector3((Firsthit.transform.position.x + 0.5f), (Firsthit.transform.position.y - 0.5f),
                                    transform.position.z);
                            }
                            else if (Mathf.Approximately(Firsthit.transform.eulerAngles.y, 180f))
                            {
                                transform.position = new Vector3(transform.position.x, (Firsthit.transform.position.y - 0.5f),
                                    (Firsthit.transform.position.z + 0.5f));
                            }
                            Climbmove = true;
                            Falltime = 0.0f;
                            State = PlayerState.Climb;
                        }
                    }
                    break;


            }
        }
        private void FixedUpdate()
        {
             GroundedCheck();
        }
        private void LateUpdate()
        {
            CameraRotation();
        }

        private void GroundedCheck()
        {
            if (!Ifjump)
            {
                Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                    transform.position.z);
                Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers);
            }
        }
        private void CameraRotation()
        {
            if (!Camerarotate) return;
                #region 
                // if there is an input and camera position is not fixed
                if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);

            // Cinemachine will follow this target
             CinemachineCameraTarget.transform.rotation = Quaternion.Euler(CameraAngleOverride,
               _cinemachineTargetYaw, 0.0f);
            #endregion 
        }
        private void BeforeJump()
        {
            //CameraClose
            Camerarotate = false;
            mouseX = 0;
            mouseY = 0;
            PlayDirection = new Vector3(0f, 0f, 0f);
            jumpForce = 0f;
            CinemachineCameraTarget.transform.parent = null;
            Ifjump = true;
            Grounded = false;
        }
        private void Jump()
        {
            //Mousemove
            mouseX += Input.GetAxis("Mouse X");
            mouseY += Input.GetAxis("Mouse Y");
            float Movevalue = 0.8f;
            inputDirection = new Vector3(-mouseX, 0, -mouseY);
            //Playerlook
            if (inputDirection.magnitude > Movevalue)
            {
                LookRotation = Mathf.Atan2(inputDirection.normalized.x, inputDirection.normalized.z) * Mathf.Rad2Deg
                    + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, LookRotation, ref _rotationVelocity,
                    RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                PlayDirection = transform.forward;
                //JumpForce
                jumpForce = inputDirection.magnitude;
                jumpForce = Mathf.Clamp(jumpForce, 0.1f, 4f);
                _verticalVelocity = Mathf.Sqrt(jumpForce * -Sensitivity * Gravity);

                //Debug.Log($"{mouseX},{mouseY},{jumpForce}");
            }
        }
        private void Fall()
        {
            //CameraOpen
            Camerarotate = true;
            CinemachineCameraTarget.transform.parent = this.transform;

            //Jump
            _controller.Move(PlayDirection.normalized * (jumpForce * Time.deltaTime) * jumpHeight +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            //Fall

            if (jumpForce > 0)
            {
                jumpForce -= Time.deltaTime * 0.8f;
                jumpForce = Mathf.Clamp(jumpForce, 0.1f, 4f);
            }
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
                Ifjump = false;
            }
            if (_verticalVelocity >= 0.0f)
            {
                _verticalVelocity += Gravity * Time.deltaTime * 2f;
            }

        }
        private void Move()
        {
                //CameraOpen
                Camerarotate = true;
                #region
                float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

                if (_input.move == Vector2.zero) targetSpeed = 0.0f;

                float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

                float speedOffset = 0.1f;

                // accelerate or decelerate to target speed
                if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                    currentHorizontalSpeed > targetSpeed + speedOffset)
                {

                    _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                        Time.deltaTime * SpeedChangeRate);

                    // round speed to 3 decimal places
                    _speed = Mathf.Round(_speed * 1000f) / 1000f;
                }
                else
                {
                    _speed = targetSpeed;
                }

                // normalise input direction
                Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

                if (_input.move != Vector2.zero)
                {
                    _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                      _mainCamera.transform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        RotationSmoothTime);

                    // rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }

                // move the player
                Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime));
                #endregion
        }
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        private void OnDrawGizmos()
        {

           // Debug.DrawRay(LadderCheck.transform.position, Vector3.up.normalized * MaxDistanceOn, Color.blue);
        }
        private bool ClimbCheck()
        {
            float MaxDistanceOn = 0.5f;
            RaycastHit Ifhit;
            if (Physics.Raycast(LadderCheck.transform.position,
                Vector3.up.normalized, out Ifhit, MaxDistanceOn, Ladder))
            {
                Firsthit = Ifhit.transform.gameObject;
                return true;
            }
            else return false;
        }
        private bool FalldownCheck()
        {
            float MaxDistanceOn = 0.5f;
            RaycastHit Ifhit;
            if (Physics.Raycast(FallDownCheck.transform.position,
                Vector3.up.normalized, out Ifhit, MaxDistanceOn, Ladder))
            {
                Debug.Log("yes1");
                Firsthit = Ifhit.transform.gameObject;
                return true;
            }
            else return false;
        }
        private void Climb()
        {
            //ClimbCheck
            RaycastHit hitup;
            RaycastHit hitdown;
            float MaxDistanceOn = 1.5f;
            float MaxDistanceOff = 1.2f;
            IfclimbOn = Physics.Raycast(HeadCheck.transform.position,
                Vector3.up.normalized, out hitup, MaxDistanceOn, Ladder);
            IfclimbOff = Physics.Raycast(FootCheck.transform.position,
                Vector3.down.normalized, out hitdown, MaxDistanceOff, Ladder);

            //Debug.DrawRay(HeadCheck.transform.position, Vector3.up.normalized * MaxDistanceOn, Color.blue);
            //Debug.DrawRay(FootCheck.transform.position, Vector3.down.normalized * MaxDistanceOff, Color.blue);
            //Direction
            if (Input.GetKeyDown(KeyCode.W) && IfclimbOn)
            {
                Climbmove = false;
                hitObjectOn = hitup.collider.gameObject;
                HitObject = hitObjectOn;
                IfclimbingOn = true;
                timeout = 0.3f;
            }
            else if (Input.GetKeyDown(KeyCode.S) && IfclimbOff)
            {
                Climbmove = false;
                hitObjectOff = hitdown.collider.gameObject;
                HitObject = hitObjectOff;
                IfclimbingOff = true;
            }

            if (IfclimbingOn)
            {
                transform.rotation = hitObjectOn.transform.rotation;
                if (Mathf.Approximately(hitObjectOn.transform.eulerAngles.y, 0f))
                {
                    target = new Vector3(transform.position.x, (hitObjectOn.transform.position.y - 0.5f),
                        (hitObjectOn.transform.position.z - 0.5f));
                }
                else if (Mathf.Approximately(hitObjectOn.transform.eulerAngles.y, 90f))
                {
                    target = new Vector3((hitObjectOn.transform.position.x - 0.5f), (hitObjectOn.transform.position.y - 0.5f),
                        transform.position.z);
                }
                else if (Mathf.Approximately(hitObjectOn.transform.eulerAngles.y, -90f))
                {
                    target = new Vector3((hitObjectOn.transform.position.x + 0.5f), (hitObjectOn.transform.position.y - 0.5f),
                        transform.position.z);
                }
                else if (Mathf.Approximately(hitObjectOn.transform.eulerAngles.y, 180f))
                {
                    target = new Vector3(transform.position.x, (hitObjectOn.transform.position.y - 0.5f),
                        (hitObjectOn.transform.position.z + 0.5f));
                }
                transform.position = Vector3.MoveTowards(transform.position, target, 4f * Time.deltaTime);
                if (Vector3.Distance(transform.position, target) < 0.1f)
                {
                    IfclimbingOn = false;
                    Climbmove = true;
                }
            }
            if (IfclimbingOff)
            {
                transform.rotation = hitObjectOff.transform.rotation;
                if (Mathf.Approximately(hitObjectOff.transform.eulerAngles.y, 0f))
                {
                    target = new Vector3(transform.position.x, (hitObjectOff.transform.position.y - 0.5f),
                        (hitObjectOff.transform.position.z - 0.5f));
                }
                else if (Mathf.Approximately(hitObjectOff.transform.eulerAngles.y, 90f))
                {
                    target = new Vector3((hitObjectOff.transform.position.x - 0.5f), (hitObjectOff.transform.position.y - 0.5f),
                        transform.position.z);
                }
                else if (Mathf.Approximately(hitObjectOff.transform.eulerAngles.y, -90f))
                {
                    target = new Vector3((hitObjectOff.transform.position.x + 0.5f), (hitObjectOff.transform.position.y - 0.5f),
                        transform.position.z);
                }
                else if (Mathf.Approximately(hitObjectOff.transform.eulerAngles.y, 180f))
                {
                    target = new Vector3(transform.position.x, (hitObjectOff.transform.position.y - 0.5f),
                        (hitObjectOff.transform.position.z + 0.5f));
                }
                transform.position = Vector3.MoveTowards(transform.position,target, 4f * Time.deltaTime);
                if (Vector3.Distance(transform.position, target) < 0.1f)
                {
                    IfclimbingOff = false;
                    Climbmove = true;
                }
            }
        }
        private void ClimbMove()
        {
            float speed = 3f;
            if (Climbmove)
            {
                if (HitObject != null && HitObject.tag == "Ladder"|| HitObject.tag == "Top")
                {
                    P1 = HitObject.GetComponent<Ladder>().Point1;
                    P2 = HitObject.GetComponent<Ladder>().Point2;
                    if (_input.move.x > 0)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, P1.transform .position , speed * Time.deltaTime);

                    }
                    if (_input.move.x < 0)
                    {
                        //position.x += _input.move.x * speed * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(transform.position, P2.transform.position, speed * Time.deltaTime);

                    }
                }
                else if (HitObject != null && HitObject.tag == "SpLadder2")
                {
                    P1 = HitObject.GetComponent<SpecialLadder>().Point1;
                    P2 = HitObject.GetComponent<SpecialLadder>().Point2;
                    Turn = HitObject.GetComponent<SpecialLadder>().Turn;
                    if (Ifturn2)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, 
                            P2.transform.position, speed * Time.deltaTime);
                        if (Vector3.Distance(transform.position, P2.transform.position) < 0.1f)
                        {
                            Ifturn2 = false;
                        }
                    }
                    if (!Ifturn2)
                    {
                        if (_input.move.x > 0)
                        {
                            transform.position = Vector3.MoveTowards(transform.position, 
                                P1.transform.position, speed * Time.deltaTime);

                        }
                        if (_input.move.x < 0)
                        {
                            //position.x += _input.move.x * speed * Time.deltaTime;
                            transform.position = Vector3.MoveTowards(transform.position, 
                                P2.transform.position, speed * Time.deltaTime);

                        }
                        if (Vector3.Distance(transform.position, P2.transform.position) < 0.1f)
                        {
                            if (_input.move.x < 0)
                            {
                                HitObject = Turn;
                                transform.rotation = HitObject.transform.rotation;
                                Ifturn1 = true;
                            }
                        }
                    }
                }
                else if (HitObject != null && HitObject.tag == "SpLadder1")
                {
                    P1 = HitObject.GetComponent<SpecialLadder>().Point1;
                    P2 = HitObject.GetComponent<SpecialLadder>().Point2;
                    Turn = HitObject.GetComponent<SpecialLadder>().Turn;
                    if (Ifturn1)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, 
                            P1.transform.position, speed * Time.deltaTime);
                        if(Vector3.Distance(transform.position, P1.transform.position) < 0.1f)
                        {
                            Ifturn1 = false;
                            Debug.Log($"{P1.name}");
                        }
                    }
                    if (!Ifturn1)
                    {
                        if (_input.move.x > 0)
                        {
                            transform.position = Vector3.MoveTowards(transform.position, 
                                P1.transform.position, speed * Time.deltaTime);

                        }
                        if (_input.move.x < 0)
                        {
                            //position.x += _input.move.x * speed * Time.deltaTime;
                            transform.position = Vector3.MoveTowards(transform.position, 
                                P2.transform.position, speed * Time.deltaTime);

                        }

                        if (Vector3.Distance(transform.position, P1.transform.position) < 0.1f)
                        {
                            if (_input.move.x > 0)
                            {
                                HitObject = Turn;
                                transform.rotation = HitObject.transform.rotation;
                                Ifturn2 = true;
                            }
                        }
                    }

                }
            }
            //TopCheck
            if (hitObjectOn != null)
            {
                if (hitObjectOn.tag == "Top" && transform.position.y - (hitObjectOn.transform.position.y - 0.5f) < 0.01f)
                {
                    Topcheck = true;
                }
                else Topcheck = false;
            }

        }    
        private void Totop()
        {
            
            Vector3 velocity = new Vector3(0f, 0f, 0f);
            velocity.y += Gravity * Time.deltaTime * 0.3f;
            if(Ifgo )transform.position = Vector3.MoveTowards(transform.position, Target,8f * Time .deltaTime );
            if(Vector3 .Distance (transform.position, Target) < 0.1f)
            {
                Iffall = true;
                Ifgo = false;
            }
            if(Iffall)
            {
                Vector3 targetPosition = new Vector3(transform.position.x, velocity.y, transform.position.z);
                targetPosition = new Vector3(transform.position.x, velocity.y, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 4f);
            }
            
        }
        private void Falldown()
        {
            Vector3 velocity = new Vector3(0f, 0f, 0f);
            IfclimbingOn = false;
            IfclimbingOff = false;
            Climbmove = false;
            velocity.y += Gravity * Time.deltaTime * 0.3f;
            Vector3 targetPosition = new Vector3(transform.position.x, velocity.y, transform.position.z);
            targetPosition = new Vector3(transform.position.x, velocity.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 4f);
            if (Falltime > 0)
            {
                Falltime -= Time.deltaTime;
            }
        }
        private void JumpDown()
        {
            Vector3 velocity = new Vector3(0f, 0f, 0f);
            velocity.y += Gravity * Time.deltaTime * 0.3f;
            Vector3 targetPosition = new Vector3(transform.position.x, velocity.y, transform.position.z);
            targetPosition = new Vector3(transform.position.x, velocity.y, transform.position.z - 1.5f);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 6f);
        }

    }

}
