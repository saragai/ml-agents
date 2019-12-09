using UnityEngine;
using MLAgents;

public class AgentMiniSoccer : Agent
{
    public enum Team
    {
        Purple,
        Blue
    }

    public enum AgentRole
    {
        Striker,
        Goalie
    }

    public Team team;
    public AgentRole agentRole;

    float m_KickPower;
    int m_PlayerIndex;

    public MiniSoccerFieldArea area;

    [HideInInspector]
    public Rigidbody agentRigitBody;

    MiniSoccerAcademy m_Academy;
    Renderer m_AgentRenderer;
    // RayPerception m_RayPerception;
    float lastBallPos; 


    float[] m_RayAngles = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };
    string[] m_DetectableObjectsPurple = { "ball", "purpleGoal", "blueGoal",
                                           "wall", "purpleAgent", "blueAgent" };
    string[] m_DetectableObjectsBlue = { "ball", "blueGoal", "purpleGoal",
                                         "wall", "blueAgent", "purpleAgent" };


    public override void InitializeAgent()
    {
        base.InitializeAgent();

        m_AgentRenderer = GetComponentInChildren<Renderer>();
        // m_RayPerception = GetComponent<RayPerception>();
        m_Academy = FindObjectOfType<MiniSoccerAcademy>();
        agentRigitBody = GetComponent<Rigidbody>();
        agentRigitBody.maxAngularVelocity = 500;

        var playerState = new MiniSoccerPlayerState
        {
            agentRb = agentRigitBody,
            startingPos = transform.position,
            agentScript = this,
        };
        area.playerStates.Add(playerState);
        m_PlayerIndex = area.playerStates.IndexOf(playerState);
        playerState.playerIndex = m_PlayerIndex;
    }

    public override void CollectObservations()
    {
        /*
        var rayDistance = 20f;
        string[] detectableObjects;

        if(team == Team.Purple)
        {
            detectableObjects = m_DetectableObjectsPurple;
        }
        else
        {
            detectableObjects = m_DetectableObjectsBlue;
        }

        AddVectorObs(m_RayPerception.Perceive(rayDistance, m_RayAngles, detectableObjects, 0f, 0f));
        AddVectorObs(m_RayPerception.Perceive(rayDistance, m_RayAngles, detectableObjects, 1f, 0f));
        */

        AddVectorObs(area.ObserveFieldStates());
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float ballPos = area.ball.transform.position.x;

        if (agentRole == AgentRole.Striker)
        {
            AddReward(-1f / 3000f);
            AddReward(-(ballPos - lastBallPos) / 30f);
        }
        if (agentRole == AgentRole.Goalie)
        {
            AddReward(1f / 3000f);
            AddReward((ballPos - lastBallPos) / 30f);
        }

        lastBallPos = ballPos;

        MoveAgent(vectorAction);
    }

    private void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[0]);

        if(agentRole == AgentRole.Goalie)
        {
            m_KickPower = 0f;
            switch (action)
            {
                case 1:
                    dirToGo = transform.forward * 1f;
                    m_KickPower = 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;
                case 3:
                    dirToGo = transform.right * -1f;
                    break;
                case 4:
                    dirToGo = transform.right * 1f;
                    break;
            }
        }
        else
        {
            m_KickPower = 0f;

            switch (action)
            {
                case 1:
                    dirToGo = transform.forward * 1f;
                    m_KickPower = 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;
                case 3:
                    rotateDir = transform.up * 1f;
                    break;
                case 4:
                    rotateDir = transform.up * -1f;
                    break;
                case 5:
                    dirToGo = transform.right * -0.75f;
                    break;
                case 6:
                    dirToGo = transform.right * 0.75f;
                    break;
            }
        }

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRigitBody.AddForce(dirToGo * m_Academy.agentRunSpeed, ForceMode.VelocityChange);
    }


    public override void AgentReset()
    {
        if(team == Team.Purple)
        {
            JoinPurpleTeam(agentRole);
            transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }
        else
        { 
            JoinBlueTeam(agentRole);
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        float scale = m_Academy.resetParameters["stage_scale"];
        transform.position = area.GetRandomSpawnPos(agentRole, team, scale);

        Vector3 scaleVector = 0.01f * scale * Vector3.one;
        scaleVector.y = 0.01f;

        area.ground.transform.localScale = scaleVector;

        agentRigitBody.velocity = Vector3.zero;
        agentRigitBody.angularVelocity = Vector3.zero;

        area.ResetBall();
        lastBallPos = area.ballStartingPos.x;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var force = 2000f * m_KickPower;
        if (collision.gameObject.CompareTag("ball"))
        {
            var dir = collision.contacts[0].point - transform.position;
            dir = dir.normalized;
            collision.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
            // AddReward(m_Academy.strikerKickReward);
        }
    }

    private void JoinPurpleTeam(AgentRole role)
    {
        agentRole = role;
        team = Team.Purple;
        m_AgentRenderer.material = m_Academy.purpleMaterial;
        tag = "purpleAgent";
    }

    private void JoinBlueTeam(AgentRole role)
    {
        agentRole = role;
        team = Team.Blue;
        m_AgentRenderer.material = m_Academy.blueMaterial;
        tag = "blueAgent";
    }
}
