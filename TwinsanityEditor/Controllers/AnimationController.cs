using OpenTK;
using System;
using System.Collections.Generic;
using Twinsanity;

namespace TwinsanityEditor.Controllers
{
    public class AnimationController : ItemController
    {
        public new Animation Data { get; set; }

        public AnimationController(MainForm topform, Animation item) : base(topform, item)
        {
            Data = item;
            AddMenu("Open editor", Menu_OpenEditor);
        }

        public float[] GetFacialAnimationTransform(int curFrame, int nextFrame, float frameDisplacement)
        {
            Animation.JointSettings jointSetting = Data.FacialJointsSettings[0];
            int shapesAmount = (jointSetting.Flags >> 0x8) & 0xf;
            float[] shapeWeights = new float[shapesAmount];
            ushort transformIndex = jointSetting.TransformationIndex;
            ushort currentFrameTransformIndex = jointSetting.AnimatedTransformIndex;
            ushort nextFrameTransformIndex = jointSetting.AnimatedTransformIndex;
            ushort transformChoice = jointSetting.TransformationChoice;

            for (int i = 0; i < shapeWeights.Length; i++)
            {
                if ((transformChoice & 0x1) == 0)
                {
                    float f1 = Data.FacialAnimatedTransforms[curFrame].GetOffset(currentFrameTransformIndex++);
                    float f2 = Data.FacialAnimatedTransforms[nextFrame].GetOffset(nextFrameTransformIndex++);
                    shapeWeights[i] = VectorFuncs.Lerp(f1, f2, frameDisplacement);
                }
                else
                {
                    shapeWeights[i] = Data.FacialStaticTransforms[transformIndex++].Value;
                }
                transformChoice >>= 1;
            }

            return shapeWeights;
        }

        public Tuple<Matrix4, Quaternion, bool> GetMainAnimationTransform(int jointIndex, int curFrame, int nextFrame, float frameDisplacement)
        {
            Vector4 currentFrameTranslation = new Vector4
            {
                W = 1.0f
            };
            Vector4 nextFrameTranslation = new Vector4
            {
                W = 1.0f
            };
            Vector4 scale = new Vector4
            {
                W = 1.0f
            };
            Animation.JointSettings jointSetting = Data.JointsSettings[jointIndex];
            bool useAddRot = ((jointSetting.Flags >> 0xC) & 0x1) != 0;
            ushort transformIndex = jointSetting.TransformationIndex;
            int currentFrameTransformIndex = jointSetting.AnimatedTransformIndex;
            ushort nextFrameTransformIndex = jointSetting.AnimatedTransformIndex;
            ushort transformChoice = jointSetting.TransformationChoice;
            bool translateXChoice = (transformChoice & 0x1) == 0;
            bool translateYChoice = (transformChoice & 0x2) == 0;
            bool translateZChoice = (transformChoice & 0x4) == 0;
            bool rotXChoice = (transformChoice & 0x8) == 0;
            bool rotYChoice = (transformChoice & 0x10) == 0;
            bool rotZChoice = (transformChoice & 0x20) == 0;
            bool scaleXChoice = (transformChoice & 0x40) == 0;
            bool scaleYChoice = (transformChoice & 0x80) == 0;
            bool scaleZChoice = (transformChoice & 0x100) == 0;
            if (translateXChoice)
            {
                float x1 = Data.AnimatedTransforms[curFrame].GetOffset(currentFrameTransformIndex++);
                float x2 = Data.AnimatedTransforms[nextFrame].GetOffset(nextFrameTransformIndex++);
                currentFrameTranslation.X = x1;
                nextFrameTranslation.X = x2;
            }
            else
            {
                currentFrameTranslation.X = Data.StaticTransforms[transformIndex].Value;
                nextFrameTranslation.X = Data.StaticTransforms[transformIndex++].Value;
            }

            if (translateYChoice)
            {
                float y1 = Data.AnimatedTransforms[curFrame].GetOffset(currentFrameTransformIndex++);
                float y2 = Data.AnimatedTransforms[nextFrame].GetOffset(nextFrameTransformIndex++);
                currentFrameTranslation.Y = y1;
                nextFrameTranslation.Y = y2;
            }
            else
            {
                currentFrameTranslation.Y = Data.StaticTransforms[transformIndex].Value;
                nextFrameTranslation.Y = Data.StaticTransforms[transformIndex++].Value;
            }

            if (translateZChoice)
            {
                float z1 = Data.AnimatedTransforms[curFrame].GetOffset(currentFrameTransformIndex++);
                float z2 = Data.AnimatedTransforms[nextFrame].GetOffset(nextFrameTransformIndex++);
                currentFrameTranslation.Z = z1;
                nextFrameTranslation.Z = z2;
            }
            else
            {
                currentFrameTranslation.Z = Data.StaticTransforms[transformIndex].Value;
                nextFrameTranslation.Z = Data.StaticTransforms[transformIndex++].Value;
            }


            float endRotX1;
            float endRotX2;
            if (rotXChoice)
            {
                int rot1 = Data.AnimatedTransforms[curFrame].GetPureOffset(currentFrameTransformIndex++) * 16;
                int rot2 = Data.AnimatedTransforms[nextFrame].GetPureOffset(nextFrameTransformIndex++) * 16;
                int diff = rot1 - rot2;
                if (diff < -0x8000)
                {
                    rot1 += 0x10000;
                }
                if (diff > 0x8000)
                {
                    rot1 -= 0x10000;
                }
                float rot1Rad = rot1 / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi;
                float rot2Rad = rot2 / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi;
                endRotX1 = rot1Rad;
                endRotX2 = rot2Rad;
            }
            else
            {
                float rot = Data.StaticTransforms[transformIndex++].GetRot(false);
                endRotX1 = rot;
                endRotX2 = rot;
            }

            float endRotY1;
            float endRotY2;
            if (rotYChoice)
            {
                int rot1 = Data.AnimatedTransforms[curFrame].GetPureOffset(currentFrameTransformIndex++) * 16;
                int rot2 = Data.AnimatedTransforms[nextFrame].GetPureOffset(nextFrameTransformIndex++) * 16;
                int diff = rot1 - rot2;
                if (diff < -0x8000)
                {
                    rot1 += 0x10000;
                }
                if (diff > 0x8000)
                {
                    rot1 -= 0x10000;
                }
                float rot1Rad = rot1 / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi;
                float rot2Rad = rot2 / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi;
                endRotY1 = rot1Rad;
                endRotY2 = rot2Rad;
            }
            else
            {
                float rot = Data.StaticTransforms[transformIndex++].GetRot(false);
                endRotY1 = rot;
                endRotY2 = rot;
            }

            float endRotZ1;
            float endRotZ2;
            if (rotZChoice)
            {
                int rot1 = Data.AnimatedTransforms[curFrame].GetPureOffset(currentFrameTransformIndex++) * 16;
                int rot2 = Data.AnimatedTransforms[nextFrame].GetPureOffset(nextFrameTransformIndex++) * 16;
                int diff = rot1 - rot2;
                if (diff < -0x8000)
                {
                    rot1 += 0x10000;
                }
                if (diff > 0x8000)
                {
                    rot1 -= 0x10000;
                }
                float rot1Rad = rot1 / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi;
                float rot2Rad = rot2 / (float)(ushort.MaxValue + 1) * MathHelper.TwoPi;
                endRotZ1 = rot1Rad;
                endRotZ2 = rot2Rad;
            }
            else
            {
                float rot = Data.StaticTransforms[transformIndex++].GetRot(false);
                endRotZ1 = rot;
                endRotZ2 = rot;
            }

            if (scaleXChoice)
            {
                float x1 = Data.AnimatedTransforms[curFrame].GetOffset(currentFrameTransformIndex++);
                float x2 = Data.AnimatedTransforms[nextFrame].GetOffset(nextFrameTransformIndex++);
                scale.X = VectorFuncs.Lerp(x1, x2, frameDisplacement);
            }
            else
            {
                scale.X = Data.StaticTransforms[transformIndex++].Value;
            }

            if (scaleYChoice)
            {
                float y1 = Data.AnimatedTransforms[curFrame].GetOffset(currentFrameTransformIndex++);
                float y2 = Data.AnimatedTransforms[nextFrame].GetOffset(nextFrameTransformIndex++);
                scale.Y = VectorFuncs.Lerp(y1, y2, frameDisplacement);
            }
            else
            {
                scale.Y = Data.StaticTransforms[transformIndex++].Value;
            }

            if (scaleZChoice)
            {
                float z1 = Data.AnimatedTransforms[curFrame].GetOffset(currentFrameTransformIndex++);
                float z2 = Data.AnimatedTransforms[nextFrame].GetOffset(nextFrameTransformIndex++);
                scale.Z = VectorFuncs.Lerp(z1, z2, frameDisplacement);
            }
            else
            {
                scale.Z = Data.StaticTransforms[transformIndex++].Value;
            }

            Vector4 resultTranslation = Vector4.Lerp(currentFrameTranslation, nextFrameTranslation, frameDisplacement);
            resultTranslation.X = -resultTranslation.X;

            Matrix3 rotX = Matrix3.CreateRotationX(endRotX1);
            Matrix3 rotY = Matrix3.CreateRotationY(endRotY1);
            Matrix3 rotZ = Matrix3.CreateRotationZ(endRotZ1);
            Matrix3 endRot = rotX * rotY * rotZ;
            Matrix3 rotX2 = Matrix3.CreateRotationX(endRotX2);
            Matrix3 rotY2 = Matrix3.CreateRotationY(endRotY2);
            Matrix3 rotZ2 = Matrix3.CreateRotationZ(endRotZ2);
            Matrix3 endRot2 = rotX2 * rotY2 * rotZ2;
            Quaternion quat1 = Quaternion.FromMatrix(endRot);
            Quaternion quat2 = Quaternion.FromMatrix(endRot2);
            Quaternion lerpedQuat = Quaternion.Slerp(quat1, quat2, frameDisplacement);

            Matrix4 transformMatrix = Matrix4.Zero;
            transformMatrix.Row0 = resultTranslation;
            transformMatrix.Row1 = scale;
            return new Tuple<Matrix4, Quaternion, bool>(transformMatrix, lerpedQuat, useAddRot);
        }

        protected override string GetName()
        {
            return $"Animation [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            string hasAnimData = (Data.Bitfield & 0x1) == 1 ? "yes" : "no";
            string hasFacialAnimationData = (Data.Bitfield & 0x2) == 1 ? "yes" : "no";
            uint animationFps = (Data.Bitfield >> 0x12) & 0x1F;
            List<string> text = new List<string>
            {
                $"ID: {Data.ID}",
                $"Size: {Data.Size}",
                $"Has animation data: {hasAnimData}",
                $"Has facial animation data: {hasFacialAnimationData}",
                $"Animation FPS in-game: {animationFps}",
                $"Main animation total frames: {Data.TotalFrames}",
                $"Blob packed 1: 0x{Data.AnimationDataPacker:X}",
                $"Main animation joint settings: {Data.JointsSettings.Count}",
                $"Main animation static transforms: {Data.StaticTransforms.Count}",
                $"Main animation animated transforms: {Data.AnimatedTransforms.Count}",
                $"Facial animation total frames: {Data.FacialAnimationTotalFrames}",
                $"Blob packed 2: 0x{Data.FacialAnimationDataPacker:X}",
                $"Facial animation joint settings: {Data.FacialJointsSettings.Count}",
                $"Facial animation static transforms: {Data.FacialStaticTransforms.Count}",
                $"Facial animation animated transforms: {Data.FacialAnimatedTransforms.Count}",
            };
            TextPrev = text.ToArray();
        }

        private void Menu_OpenEditor()
        {
            MainFile.OpenEditor(this);
        }
    }
}
