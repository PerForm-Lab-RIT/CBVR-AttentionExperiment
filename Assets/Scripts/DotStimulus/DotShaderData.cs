using UnityEngine;

namespace DotStimulus
{
    public class DotShaderData
    {
        public MeshProperties[] MeshProps { get; }
        public ComputeBuffer MeshPropertiesBuffer { get; private set; }
        public ComputeBuffer ArgsBuffer { get; private set; }
        
        public DotShaderData(Mesh dotMesh, int numDots)
        {
            MeshProps = new MeshProperties[numDots];
            MeshPropertiesBuffer = new ComputeBuffer(numDots, MeshProperties.Size());

            var args = new uint[5]
            {
                dotMesh.GetIndexCount(0),
                (uint) numDots,
                dotMesh.GetIndexStart(0),
                dotMesh.GetBaseVertex(0),
                0
            };
            ArgsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            ArgsBuffer.SetData(args);
        }

        public void UpdateMeshPropertiesBuffer(Dot[] dots, Matrix4x4 stimulusCenterLocalToWorld, int i)
        {
            MeshProps[i].LocalPosition = dots[i].GetPosition();
            MeshProps[i].LocalScale = dots[i].GetScale();
            MeshProps[i].ParentLocalToWorld = stimulusCenterLocalToWorld;
        }

        public void ClearBuffers()
        {
            MeshPropertiesBuffer?.Release();
            MeshPropertiesBuffer = null;

            ArgsBuffer?.Release();
            ArgsBuffer = null;
        }
        
        public struct MeshProperties {
            public Vector3 LocalPosition;
            public float LocalScale;
            public Matrix4x4 ParentLocalToWorld;

            public static int Size()
            {
                return (sizeof(float) * 4 * 4) +
                       (sizeof(float) * 4);
            }
        }
    }
}
