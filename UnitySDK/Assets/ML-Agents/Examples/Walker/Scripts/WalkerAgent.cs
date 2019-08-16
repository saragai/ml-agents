﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class WalkerAgent : Agent
{
    [Header("Specific to Walker")] [Header("Target To Walk Towards")] [Space(10)]
    public Transform target;

    Vector3 dirToTarget;
    public Transform hips;
    public Transform chest;
    public Transform spine;
    public Transform head;
    public Transform thighL;
    public Transform shinL;
    public Transform footL;
    public Transform thighR;
    public Transform shinR;
    public Transform footR;
    public Transform armL;
    public Transform forearmL;
    public Transform handL;
    public Transform armR;
    public Transform forearmR;
    public Transform handR;
    JointDriveController jdController;
    bool isNewDecisionStep;
    int currentDecisionStep;

    private Rigidbody hipsRb;
    private Rigidbody chestRb;
    private Rigidbody spineRb;

    private ResetParameters resetParams;
    Quaternion lookRotation;
    Matrix4x4 targetDirMatrix;
    AvgCenterOfMass avgCOM;
    public LayerMask groundLayer;
    public override void InitializeAgent()
    {
        avgCOM = GetComponent<AvgCenterOfMass>();

        jdController = GetComponent<JointDriveController>();
        jdController.SetupBodyPart(hips);
        jdController.SetupBodyPart(chest);
        jdController.SetupBodyPart(spine);
        jdController.SetupBodyPart(head);
        jdController.SetupBodyPart(thighL);
        jdController.SetupBodyPart(shinL);
        jdController.SetupBodyPart(footL);
        jdController.SetupBodyPart(thighR);
        jdController.SetupBodyPart(shinR);
        jdController.SetupBodyPart(footR);
        jdController.SetupBodyPart(armL);
        jdController.SetupBodyPart(forearmL);
        jdController.SetupBodyPart(handL);
        jdController.SetupBodyPart(armR);
        jdController.SetupBodyPart(forearmR);
        jdController.SetupBodyPart(handR);

        hipsRb = hips.GetComponent<Rigidbody>();
        chestRb = chest.GetComponent<Rigidbody>();
        spineRb = spine.GetComponent<Rigidbody>();

        var academy = FindObjectOfType<WalkerAcademy>() as WalkerAcademy;
        resetParams = academy.resetParameters;

        SetResetParameters();
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp)
    {
        var rb = bp.rb;
        AddVectorObs(bp.groundContact.touchingGround ? 1 : 0); // Is this bp touching the ground
        Vector3 velocityRelativeToLookRotationToTarget = targetDirMatrix.inverse.MultiplyVector(rb.velocity);
        AddVectorObs(velocityRelativeToLookRotationToTarget);

        Vector3 angularVelocityRelativeToLookRotationToTarget = targetDirMatrix.inverse.MultiplyVector(rb.angularVelocity);
        AddVectorObs(angularVelocityRelativeToLookRotationToTarget);
        // AddVectorObs(rb.velocity);
        // AddVectorObs(rb.angularVelocity);
        Vector3 localPosRelToHips = hips.InverseTransformPoint(rb.position);
        AddVectorObs(localPosRelToHips);

        // if (bp.rb.transform != hips && bp.rb.transform != handL && bp.rb.transform != handR &&
        //     bp.rb.transform != footL && bp.rb.transform != footR && bp.rb.transform != head)
        if (bp.rb.transform != hips && bp.rb.transform != handL && bp.rb.transform != handR && bp.rb.transform != chest && bp.rb.transform != head)
        {
            AddVectorObs(bp.currentXNormalizedRot);
            AddVectorObs(bp.currentYNormalizedRot);
            AddVectorObs(bp.currentZNormalizedRot);
            AddVectorObs(bp.currentStrength / jdController.maxJointForceLimit);
        }
    }
    // /// <summary>
    // /// Add relevant information on each body part to observations.
    // /// </summary>
    // public void CollectObservationBodyPart(BodyPart bp)
    // {
    //     var rb = bp.rb;
    //     AddVectorObs(bp.groundContact.touchingGround ? 1 : 0); // Is this bp touching the ground
    //     AddVectorObs(rb.velocity);
    //     AddVectorObs(rb.angularVelocity);
    //     Vector3 localPosRelToHips = hips.InverseTransformPoint(rb.position);
    //     AddVectorObs(localPosRelToHips);

    //     if (bp.rb.transform != hips && bp.rb.transform != handL && bp.rb.transform != handR &&
    //         bp.rb.transform != footL && bp.rb.transform != footR && bp.rb.transform != head)
    //     {
    //         AddVectorObs(bp.currentXNormalizedRot);
    //         AddVectorObs(bp.currentYNormalizedRot);
    //         AddVectorObs(bp.currentZNormalizedRot);
    //         AddVectorObs(bp.currentStrength / jdController.maxJointForceLimit);
    //     }
    // }

    float RaycastDownDist(Vector3 pos, float maxDist)
    {
        RaycastHit hit;
        float val = 1;
        if (Physics.Raycast(pos, Vector3.down, out hit, maxDist, groundLayer))
        {
            val = hit.distance/maxDist;
        }
        return val;
    }

    /// <summary>
    /// Loop over body parts to add them to observation.
    /// </summary>
    public override void CollectObservations()
    {
        jdController.GetCurrentJointForces();


        lookRotation = Quaternion.LookRotation(dirToTarget);
        targetDirMatrix = Matrix4x4.TRS(Vector3.zero, lookRotation, Vector3.one);


        // AddVectorObs(dirToTarget.normalized);
        // AddVectorObs(jdController.bodyPartsDict[hips].rb.position);
        // AddVectorObs(hips.forward);
        // AddVectorObs(hips.up);

        AddVectorObs(RaycastDownDist(hips.position, 5));
        AddVectorObs(RaycastDownDist(head.position, 5));



        // Vector3 hipsForwardRelativeToLookRotationToTarget = targetDirMatrix.inverse.MultiplyVector(hips.forward);
        //Hips forward relative to lookRotation to target
        AddVectorObs(targetDirMatrix.inverse.MultiplyVector(hips.forward));
        //Hips up relative to lookRotation to target
        AddVectorObs(targetDirMatrix.inverse.MultiplyVector(hips.up));
        //Head forward relative to lookRotation to target
        AddVectorObs(targetDirMatrix.inverse.MultiplyVector(head.forward));
        //Head up relative to lookRotation to target
        AddVectorObs(targetDirMatrix.inverse.MultiplyVector(head.up));

        // Vector3 hipsUpRelativeToLookRotationToTarget = targetDirMatrix.inverse.MultiplyVector(hips.up);
        // AddVectorObs(hipsUpRelativeToLookRotationToTarget);
        // Vector3 headForwardRelativeToLookRotationToTarget = targetDirMatrix.inverse.MultiplyVector(head.forward);
        // AddVectorObs(headForwardRelativeToLookRotationToTarget);

        // Vector3 headUpRelativeToLookRotationToTarget = targetDirMatrix.inverse.MultiplyVector(head.up);
        // AddVectorObs(headUpRelativeToLookRotationToTarget);

        foreach (var bodyPart in jdController.bodyPartsDict.Values)
        {
            CollectObservationBodyPart(bodyPart);
        }
        AddVectorObs(hips.InverseTransformPoint(avgCOM.avgCOMWorldSpace));

    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        dirToTarget = target.position - jdController.bodyPartsDict[hips].rb.position;

        // Apply action to all relevant body parts. 
        if (isNewDecisionStep)
        {
            var bpDict = jdController.bodyPartsDict;
            int i = -1;

            // bpDict[chest].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
            bpDict[spine].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
            // bpDict[head].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);

            bpDict[thighL].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[thighR].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[shinL].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[shinR].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[footR].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
            bpDict[footL].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);


            bpDict[armL].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[armR].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[forearmL].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[forearmR].SetJointTargetRotation(vectorAction[++i], 0, 0);

            //update joint strength settings
            // bpDict[chest].SetJointStrength(vectorAction[++i]);
            bpDict[spine].SetJointStrength(vectorAction[++i]);
            // bpDict[head].SetJointStrength(vectorAction[++i]);
            bpDict[thighL].SetJointStrength(vectorAction[++i]);
            bpDict[shinL].SetJointStrength(vectorAction[++i]);
            bpDict[footL].SetJointStrength(vectorAction[++i]);
            bpDict[thighR].SetJointStrength(vectorAction[++i]);
            bpDict[shinR].SetJointStrength(vectorAction[++i]);
            bpDict[footR].SetJointStrength(vectorAction[++i]);
            bpDict[armL].SetJointStrength(vectorAction[++i]);
            bpDict[forearmL].SetJointStrength(vectorAction[++i]);
            bpDict[armR].SetJointStrength(vectorAction[++i]);
            bpDict[forearmR].SetJointStrength(vectorAction[++i]);
        }

        IncrementDecisionTimer();

        // Set reward for this step according to mixture of the following elements.
        // a. Velocity alignment with goal direction.
        // b. Rotation alignment with goal direction.
        // c. Encourage head height.
        // d. Discourage head movement.
        AddReward(
            // +0.03f * Vector3.Dot(dirToTarget.normalized, jdController.bodyPartsDict[hips].rb.velocity)
            + 0.01f * Vector3.Dot(dirToTarget.normalized, jdController.bodyPartsDict[hips].rb.velocity)
            + 0.01f * Vector3.Dot(dirToTarget.normalized, hips.forward)
            + 0.02f * (head.position.y - hips.position.y)
            - 0.01f * Vector3.Distance(jdController.bodyPartsDict[head].rb.velocity,
                jdController.bodyPartsDict[hips].rb.velocity)
        );
    }

    /// <summary>
    /// Only change the joint settings based on decision frequency.
    /// </summary>
    public void IncrementDecisionTimer()
    {
        if (currentDecisionStep == agentParameters.numberOfActionsBetweenDecisions ||
            agentParameters.numberOfActionsBetweenDecisions == 1)
        {
            currentDecisionStep = 1;
            isNewDecisionStep = true;
        }
        else
        {
            currentDecisionStep++;
            isNewDecisionStep = false;
        }
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void AgentReset()
    {
        if (dirToTarget != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dirToTarget);
        }

        foreach (var bodyPart in jdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }

        isNewDecisionStep = true;
        currentDecisionStep = 1;
        SetResetParameters();
    }

    public void SetTorsoMass()
    {
        chestRb.mass = resetParams["chest_mass"];
        spineRb.mass = resetParams["spine_mass"];
        hipsRb.mass = resetParams["hip_mass"];
    }

    public void SetResetParameters()
    {
        SetTorsoMass();
    }
}
