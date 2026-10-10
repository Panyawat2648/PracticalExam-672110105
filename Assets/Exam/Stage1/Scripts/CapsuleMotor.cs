using UnityEngine;
using UnityEngine.InputSystem;
namespace PanyawatHainak {
[RequireComponent(typeof(Rigidbody),typeof(CapsuleCollider))]
public class CapsuleMotor:MonoBehaviour {
    public float walkSpeed=6, sprintSpeed=9, turnSpeed=150, jumpHeight=1.3f;
    public bool ControlsEnabled=true;
    Rigidbody body; InputActionMap actions; InputAction move,sprint,jump;
    Vector2 input; bool jumpQueued;
    public bool Grounded {get;private set;}
    void Awake(){
        body=GetComponent<Rigidbody>();
        actions=new InputActionMap("Capsule");
        move=actions.AddAction("Move",InputActionType.Value);
        move.AddCompositeBinding("2DVector").With("Up","<Keyboard>/w").With("Down","<Keyboard>/s").With("Left","<Keyboard>/a").With("Right","<Keyboard>/d");
        move.AddCompositeBinding("2DVector").With("Up","<Keyboard>/upArrow").With("Down","<Keyboard>/downArrow").With("Left","<Keyboard>/leftArrow").With("Right","<Keyboard>/rightArrow");
        sprint=actions.AddAction("Sprint",InputActionType.Button,"<Keyboard>/leftShift");
        jump=actions.AddAction("Jump",InputActionType.Button,"<Keyboard>/space");
    }
    void OnEnable(){actions?.Enable();}
    void OnDisable(){actions?.Disable();}
    void OnDestroy(){actions?.Dispose();}
    void Update(){
        input=ControlsEnabled?move.ReadValue<Vector2>():Vector2.zero;
        if(ControlsEnabled&&jump.WasPressedThisFrame())jumpQueued=true;
    }
    void FixedUpdate(){
        Grounded=Physics.SphereCast(transform.position,.28f,Vector3.down,out _, .82f,1,QueryTriggerInteraction.Ignore);
        if(!ControlsEnabled){body.linearVelocity=new Vector3(0,body.linearVelocity.y,0);return;}
        var rotation=body.rotation*Quaternion.Euler(0,input.x*turnSpeed*Time.fixedDeltaTime,0);
        body.MoveRotation(rotation);
        float speed=sprint.IsPressed()?sprintSpeed:walkSpeed;
        var direction=rotation*Vector3.forward*input.y*speed*(input.y<0?.4f:1);
        float y=body.linearVelocity.y;
        if(jumpQueued&&Grounded)y=Mathf.Sqrt(2*-Physics.gravity.y*jumpHeight);
        jumpQueued=false;body.linearVelocity=new Vector3(direction.x,y,direction.z);
    }
}
}