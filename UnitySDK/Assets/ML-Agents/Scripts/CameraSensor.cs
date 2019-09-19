using System.Collections.Generic;
using Google.Protobuf;
using MLAgents.CommunicatorObjects;
using UnityEngine;
using System;

namespace MLAgents
{
    [Serializable]
    public class CameraSensor : ISensor
    {
        public int Width;
        public int Height;
        public Camera Camera;

        public int[] GetShape()
        {
            return new []{Width, Height, 3};
        }

        public void WriteFloats(float[] observationsOut)
        {
            int index = 0;
            for (int w = 0; w < Width; w++)
            {
                for (int h = 0; h < Height; h++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        observationsOut[index] = 1.0f;
                        index++;
                    }
                }
            }
        }
    }
}
