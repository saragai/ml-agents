using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class MiniSoccerPlayerState
{
    public int playerIndex;
    [FormerlySerializedAs("agentRB")]
    public Rigidbody agentRb;
    public Vector3 startingPos;
    public AgentMiniSoccer agentScript;
    public float ballPosReward;
}

public class MiniSoccerFieldArea : MonoBehaviour
{
    public GameObject ball;
    [FormerlySerializedAs("ballRB")]
    [HideInInspector]
    public Rigidbody ballRb;
    public GameObject ground;
    public GameObject centerPitch;
    MiniSoccerBallController m_BallController;
    public List<MiniSoccerPlayerState> playerStates = new List<MiniSoccerPlayerState>();

    [HideInInspector]
    public Vector3 ballStartingPos;

    Material m_GroundMaterial;
    Renderer m_GroundRenderer;
    MiniSoccerAcademy m_Academy;

    public IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        m_GroundRenderer.material = m_GroundMaterial;
    }

    private void Awake()
    {
        m_Academy = FindObjectOfType<MiniSoccerAcademy>();
        m_GroundRenderer = centerPitch.GetComponent<Renderer>();
        m_GroundMaterial = m_GroundRenderer.material;

        ballRb = ball.GetComponent<Rigidbody>();
        m_BallController = ball.GetComponent<MiniSoccerBallController>();
        m_BallController.area = this;
        ballStartingPos = ball.transform.position;
    }

    public List<float> ObserveFieldStates()
    {
        List<float> fieldStates = new List<float>();

        fieldStates.Add(ball.transform.localPosition.x);
        fieldStates.Add(ball.transform.localPosition.y);
        fieldStates.Add(ball.transform.localPosition.z);

        foreach(var ps in playerStates)
        {
            fieldStates.Add(ps.agentRb.transform.localPosition.x);
            fieldStates.Add(ps.agentRb.transform.localPosition.y);
            fieldStates.Add(ps.agentRb.transform.localPosition.z);
            fieldStates.Add(ps.agentRb.transform.forward.x);
            fieldStates.Add(ps.agentRb.transform.forward.y);
            fieldStates.Add(ps.agentRb.transform.forward.z);
        }

        return fieldStates;
    }

    public void GoalTouched(AgentMiniSoccer.Team scoredTeam)
    {
        foreach(var ps in playerStates)
        {
            if(ps.agentScript.team == scoredTeam)
            {
                RewardPlayer(ps, m_Academy.strikerReward, m_Academy.goalieReward);
            }
            else
            {
                RewardPlayer(ps, m_Academy.strikerPunish, m_Academy.goaliePunish);
            }

            if (scoredTeam == AgentMiniSoccer.Team.Purple)
            {
                StartCoroutine(GoalScoredSwapGroundMaterial(m_Academy.purpleMaterial, 1));
            }
            else
            {
                StartCoroutine(GoalScoredSwapGroundMaterial(m_Academy.blueMaterial, 1));
            }

            ps.agentScript.Done();
        }
    }

    public void RewardPlayer(MiniSoccerPlayerState ps, float striker, float goalie)
    {
        switch (ps.agentScript.agentRole)
        {
            case AgentMiniSoccer.AgentRole.Striker:
                ps.agentScript.AddReward(striker);
                break;

            case AgentMiniSoccer.AgentRole.Goalie:
                ps.agentScript.AddReward(goalie);
                break;
        }
    }

    public Vector3 GetRandomSpawnPos(AgentMiniSoccer.AgentRole role, AgentMiniSoccer.Team team, float scale)
    {
        var xOffset = 0f;
        if (role == AgentMiniSoccer.AgentRole.Goalie)
        {
            xOffset = 13f;
        }
        if (role == AgentMiniSoccer.AgentRole.Striker)
        {
            xOffset = 7f;
        }
        if (team == AgentMiniSoccer.Team.Blue)
        {
            xOffset = xOffset * -1f;
        }

        Vector3 offset = new Vector3(xOffset, 0f, 0f)
            + (Random.insideUnitSphere * 2);

        var randomSpawnPos = ground.transform.position + offset * scale;

        randomSpawnPos.y = ground.transform.position.y + 2 * scale;

        return randomSpawnPos;
    }

    public Vector3 GetBallSpawnPosition()
    {
        var randomSpawnPos = ground.transform.position +
            new Vector3(0f, 0f, 0f)
            + (Random.insideUnitSphere * 2);
        randomSpawnPos.y = ground.transform.position.y + 2;
        return randomSpawnPos;
    }

    public void ResetBall()
    {
        ballStartingPos = GetBallSpawnPosition();

        ball.transform.position = ballStartingPos;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        var ballScale = m_Academy.resetParameters["ball_scale"];
        ballRb.transform.localScale = new Vector3(ballScale, ballScale, ballScale);
    }
}
