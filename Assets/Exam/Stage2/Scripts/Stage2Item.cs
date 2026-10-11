using UnityEngine;
namespace PanyawatHainak {
public class Stage2Item:MonoBehaviour {
    public ItemEffect effect;
    public Transform visual;
    public bool Collected {get;private set;}
    Vector3 origin;
    public string Description => effect==ItemEffect.ScorePlus?"+500 points":effect==ItemEffect.ScoreMinus?"-500 points":effect==ItemEffect.TimePlus?"+3 seconds":"-5 seconds";
    void Awake(){if(visual)origin=visual.localPosition;}
    void Update(){if(visual){visual.Rotate(0,65*Time.deltaTime,0);visual.localPosition=origin+Vector3.up*Mathf.Sin(Time.time*2)*.1f;}}
    public bool Collect(Stage2Interactor actor){
        if(Collected||!actor||!actor.CanReach(this))return false;
        var game=FindAnyObjectByType<Stage2Game>();if(!game||game.Ended)return false;
        Collected=true;game.Apply(effect);gameObject.SetActive(false);return true;
    }
}
}