using System;
using System.Collections.Generic;
using Google.Protobuf;
using MLAgents.CommunicatorObjects;
using UnityEngine;


namespace MLAgents
{
    // Basic sensor interface
    public interface ISensor
    {
        // Get the shape of the observation.
        // Float arrays would have shape {numFloats}
        // RGB images would have shape { x, y, 3 }
        // Grayscale images images would have shape { x, y, 1 } or {x, y}

        // TBD - should we hint that the observation should be used for convolution?
        // It's currently implicit in the shape - 1D observations aren't convolved, 3D are.
        // What if you wanted 1D convolutional features (e.g. raycast depth map)?
        int[] GetShape();
        void WriteFloats(float[] observationsOut);
    }

    // Maps 1:1 with Agent.AgentParameters.agentCameras
    // TBD should this and RenderTextureSensor share a common class? Similar implementations.
    public class SampleCameraSensor : ISensor
    {
        private Camera m_Camera;
        public int Width;
        public int Height;

        public int[] GetShape()
        {
            return new [] {Width, Height, 3};
        }

        public void WriteFloats(float[] observationsOut)
        {
            // render to texture
            // get color[]
            // write rgbs to observationsOut
        }

    }

    // Maps 1:1 with Agent.AgentParameters.agentRenderTextures
    public class RenderTextureSensor : ISensor
    {
        private RenderTexture m_RenderTexture;
        public int Width;
        public int Height;

        public int[] GetShape()
        {
            return new [] {Width, Height, 3};
        }

        public void WriteFloats(float[] observationsOut)
        {
            // get color[]
            // write rgbs to observationsOut
        }
    }

    // Abstract class for "blittable" structs.
    // An example from the Ball3DAgent is below
    // Implementations have to implement GetObservation() which creates the Observation type.
    // TBD - use a delegate instead on abstract method
    public abstract class StructSensor<T> : ISensor where T : struct
    {
        public int[] GetShape()
        {
            //var numFloats = sizeof(T) / sizeof(float);
            var numFloats = 4;
            return new [] { numFloats };
        }

        public void WriteFloats(float[] observationsOut)
        {
            T obs = GetObservation();
            // memcopy T to observationsOut (or use reflection)
        }

        public abstract T GetObservation();
    }

    public struct Ball3DAgentObservation
    {
        float RotationZ;
        float RotationX;
        Vector3 PositionDelta;
        Vector3 RigidBodyVelocity;
    }

    public class Ball3DAgentSensor : StructSensor<Ball3DAgentObservation>
    {
        // Ball3DAgent m_Ball3dAgent;
        // RigidBody m_BallRb;
        public override Ball3DAgentObservation GetObservation()
        {
            // AddVectorObs(gameObject.transform.rotation.z);
            // AddVectorObs(gameObject.transform.rotation.x);
            // AddVectorObs(ball.transform.position - gameObject.transform.position);
            // AddVectorObs(m_BallRb.velocity);
            return new Ball3DAgentObservation();
        }
    }
}
