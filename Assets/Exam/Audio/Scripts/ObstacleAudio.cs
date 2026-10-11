using UnityEngine;
namespace PanyawatHainak {
public sealed class ObstacleAudio : MonoBehaviour {
    float nextSound;
    void Hit(Vector3 normal){
        if(Mathf.Abs(normal.y)>.5f||Time.unscaledTime<nextSound)return;
        nextSound=Time.unscaledTime+.6f;ExamAudio.Instance?.Impact();
    }
    void OnControllerColliderHit(ControllerColliderHit hit){if(hit.moveDirection.sqrMagnitude>.01f)Hit(hit.normal);}
    void OnCollisionEnter(Collision hit){for(int i=0;i<hit.contactCount;i++)Hit(hit.GetContact(i).normal);}
}
}
