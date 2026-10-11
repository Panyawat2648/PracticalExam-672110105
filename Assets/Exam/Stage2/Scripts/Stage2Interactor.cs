using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
namespace PanyawatHainak {
public class Stage2Interactor:MonoBehaviour {
    public float radius=2.1f;
    public Stage2Item Target {get;private set;}
    StarterAssetsInputs input;
    Stage2Item[] items;
    void Awake(){input=GetComponent<StarterAssetsInputs>();items=FindObjectsByType<Stage2Item>(FindObjectsSortMode.None);}
    public bool CanReach(Stage2Item item){
        if(!item||!item.gameObject.activeInHierarchy||item.Collected)return false;
        Vector3 origin=transform.position+Vector3.up;
        var offset=item.transform.position-origin;
        if(offset.sqrMagnitude>radius*radius)return false;
        return !Physics.Raycast(origin,offset.normalized,offset.magnitude,1,QueryTriggerInteraction.Ignore);
    }
    void Update(){
        if(input)input.LookInput(Mouse.current!=null&&Mouse.current.rightButton.isPressed?Mouse.current.delta.ReadValue()*.12f:Vector2.zero);
        Target=null;float nearest=float.PositiveInfinity;
        foreach(var item in items){
            if(!item||!item.gameObject.activeInHierarchy||item.Collected)continue;
            float d=(item.transform.position-transform.position-Vector3.up).sqrMagnitude;
            if(d<nearest&&CanReach(item)){Target=item;nearest=d;}
        }
        if(Target&&Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame)Target.Collect(this);
    }
    void OnDisable(){if(input)input.LookInput(Vector2.zero);}
}
}