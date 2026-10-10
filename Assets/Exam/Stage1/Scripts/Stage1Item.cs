using UnityEngine;
namespace PanyawatHainak {
public enum ItemEffect { ScorePlus,ScoreMinus,TimePlus,TimeMinus }
public class Stage1Item:MonoBehaviour {
    public ItemEffect effect;
    public Transform visual;
    bool consumed; Vector3 origin;
    void Awake(){if(visual)origin=visual.localPosition;}
    void Update(){if(visual){visual.Rotate(0,70*Time.deltaTime,0);visual.localPosition=origin+Vector3.up*Mathf.Sin(Time.time*2.5f)*.12f;}}
    void OnTriggerEnter(Collider other){
        if(consumed)return;
        var player=other.GetComponentInParent<CapsuleMotor>();
        if(!player)return;
        var game=FindAnyObjectByType<Stage1Game>();
        if(!game||game.Ended)return;
        consumed=true;GetComponent<Collider>().enabled=false;
        game.Apply(effect);gameObject.SetActive(false);
    }
}
}