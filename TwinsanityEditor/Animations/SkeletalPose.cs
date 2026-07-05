using OpenTK;

namespace TwinsanityEditor.Animations
{
    public class SkeletalPose
    {
        public string Name;

        public Matrix4 this[int i]
        {
            get => MatrixArray[i]; set => MatrixArray[i] = value;
        }

        public Matrix4[] MatrixArray { get; }

        public SkeletalPose(int count)
        {
            MatrixArray = new Matrix4[count];
        }

        public SkeletalPose(int count, Matrix4 template)
        {
            MatrixArray = new Matrix4[count];
            for (int i = 0; i < count; i++)
            {
                MatrixArray[i] = template;
            }
        }

        public SkeletalPose(Matrix4[] matrices)
        {
            MatrixArray = matrices;
        }

        public Vector3 Position(int boneIndex)
        {
            Vector3 tr = MatrixArray[boneIndex].ExtractTranslation();
            return new Vector3(tr.X, tr.Y, tr.Z);
        }

        public Quaternion Rotation(int boneIndex)
        {
            return MatrixArray[boneIndex].ExtractRotation();
        }

        public void Set(int boneIndex, Vector3 position, Quaternion rotation)
        {
            MatrixArray[boneIndex] = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(position);
        }

        public void Set(int boneIndex, Matrix4 transform)
        {
            MatrixArray[boneIndex] = transform;
        }

        public SkeletalPose Clone(string name)
        {
            return new SkeletalPose((Matrix4[])MatrixArray.Clone()) { Name = name };
        }
    }
}
