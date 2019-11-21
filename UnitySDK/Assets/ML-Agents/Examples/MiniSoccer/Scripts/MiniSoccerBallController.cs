using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniSoccerBallController : MonoBehaviour
{
    [HideInInspector]
    public MiniSoccerFieldArea area;
    public AgentMiniSoccer lastTouchedBy; //who was the last to touch the ball
    public string agentTag; //will be used to check if collided with a agent
    public string purpleGoalTag; //will be used to check if collided with red goal
    public string blueGoalTag; //will be used to check if collided with blue goal

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag(purpleGoalTag))
        {
            area.GoalTouched(AgentMiniSoccer.Team.Blue);
        }
        if (col.gameObject.CompareTag(blueGoalTag))
        {
            area.GoalTouched(AgentMiniSoccer.Team.Purple);
        }
    }
}
