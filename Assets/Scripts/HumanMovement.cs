using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class HumanMovement : NetworkBehaviour
{
    [SyncVar]
    private float gravity = 10f;
    [SyncVar]
    private float speed = 5f;
    [SyncVar]
    private float acceleration = 10f;
    private float floatingStartTime;
    private float bobbingDistance = 0.05f;
    private float floatingSpeed = 5.0f;
    public float floatingHeight = -1.987f;
    private bool floating = false;
    private bool walking = false;    // to toggle between walking and running
    public float walkSpeed = 1f;    // how fast walking should be
    public float runSpeed = 5f;    // how fast running should be
    

    private Animator characterAnimator;
    private CharacterController characterController;
    private Vector2 currentDirection;
    private new Camera camera;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        characterAnimator = GetComponent<Animator>();
        camera = Camera.main;
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            // exit from update if this is not the local player
            return;
        }

        var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        currentDirection = Vector2.MoveTowards(currentDirection,Vector2.ClampMagnitude(input, 1f), acceleration * Time.deltaTime);
        

        if(Input.GetKeyDown(KeyCode.Tab))    // toggle between walking and running by pressing "Tab"
        {
            if (walking)
            {
                speed = runSpeed;
                walking = false;
            }
            else
            {
                speed = walkSpeed;
                walking = true;
            }
        }

        if (currentDirection.magnitude > 0)
        {
            // Move relative to the camera
            Transform relativeMovement = camera.transform;
            if(relativeMovement != null)
            {
                var forward = Vector3.ProjectOnPlane(relativeMovement.forward, Vector3.up);
                forward.Normalize();
                var right = Vector3.ProjectOnPlane(relativeMovement.right, Vector3.up);
                right.Normalize();
                
                var dir = right * currentDirection.x + forward * currentDirection.y;
                characterController.Move(dir * Time.deltaTime * (speed));
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
       
        if (characterController.transform.position.y > floatingHeight)
        {
            characterController.Move(Vector3.down * gravity * Time.deltaTime);
            if (walking)    // set animation based on input and whether player is walking or not
            {
                characterAnimator.SetFloat("Speed", Vector2.ClampMagnitude(input, 1f).magnitude/2);
            }
            else
            {
                characterAnimator.SetFloat("Speed", Vector2.ClampMagnitude(input, 1f).magnitude);
            }
            floating = false;
        } 
        else
        {            
            if(!floating)
            {
                floating = true;
                floatingStartTime = Time.time;
                characterAnimator.SetFloat("Speed", 0f);
            }
            var pos = characterController.transform.position;
            var floatPos = new Vector3(characterController.transform.position.x,
             floatingHeight - bobbingDistance - Mathf.Cos(Mathf.PI + (Time.time - floatingStartTime) * floatingSpeed) * bobbingDistance,
             characterController.transform.position.z);
            characterController.transform.position = floatPos;
        }
    }
}