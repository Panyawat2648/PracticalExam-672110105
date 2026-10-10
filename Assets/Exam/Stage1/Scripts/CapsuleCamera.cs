using UnityEngine;
namespace PanyawatHainak {
public class CapsuleCamera:MonoBehaviour {
    public Transform target;
    void LateUpdate(){
        if(!target)return;
        var wanted=target.position+target.rotation*new Vector3(0,10,-12);
        transform.position=Vector3.Lerp(transform.position,wanted,1-Mathf.Exp(-7*Time.deltaTime));
        transform.LookAt(target.position+Vector3.up*.2f);
    }
}
}