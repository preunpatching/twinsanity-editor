using OpenTK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Twinsanity;
using TwinsanityEditor.Controllers;
using TwinsanityEditor.Viewers;

namespace TwinsanityEditor
{
    public partial class AnimationEditor : Form
    {
        private readonly SectionController controller;
        private Animation animation;
        private Animation.JointSettings JointSettings;
        private Animation.JointSettings JointSettings2;
        private Animation.AnimatedTransform AnimatedTransform;
        private Animation.AnimatedTransform AnimatedTransform2;
        private Animation.Transformation StaticTransform;
        private Animation.Transformation StaticTransform2;
        private AnimationViewer viewer;

        private bool playing, loop;
        private int fps = 50;

        public AnimationEditor(SectionController c)
        {
            controller = c;
            InitializeComponent();
            Text = $"Animation editor";
            PopulateList();
        }

        private void PopulateList()
        {
            lbAnimations.SelectedIndex = -1;
            lbAnimations.Items.Clear();
            foreach (Animation anim in controller.Data.Records)
            {
                _ = controller.MainFile.Data.Type != TwinsFile.FileType.MonkeyBallRM && DefaultHashes.Hash_Animations.ContainsKey(anim.ID)
                    ? lbAnimations.Items.Add($"ID {anim.ID} - {DefaultHashes.Hash_Animations[anim.ID]}")
                    : lbAnimations.Items.Add($"ID {anim.ID} - Unknown animation");
            }
        }

        private void PopulateWithAnimData(IList list, IList data, Action<IList, string[], int> adder, params string[] namePattern)
        {
            list.Clear();
            int index = 1;
            foreach (object d in data)
            {
                adder.Invoke(list, namePattern, index++);
            }
        }

        private void lbAnimations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbAnimations.SelectedIndex == -1)
            {
                return;
            }

            JointSettings = null;
            JointSettings2 = null;
            AnimatedTransform = null;
            AnimatedTransform2 = null;
            StaticTransform = null;
            StaticTransform2 = null;
            cbParentScale.Checked = false;
            cbAddRotation.Checked = false;
            tbJointTransformChoice.Text = "";
            tbJointTransformIndex.Text = "";
            tbJointAnimatedTransformIndex.Text = "";
            tbShapesAmount.Text = "";
            tbJointTransformChoice2.Text = "";
            tbJointTransformIndex2.Text = "";
            tbJointTwoPartTransformIndex2.Text = "";
            tbTransformation.Text = "";
            tbTransformation2.Text = "";
            tbJointFlags.Text = "";
            tbAnimationTimeline.Value = 0;
            cbOGIList.Items.Clear();
            playing = false;
            animation = (Animation)controller.Data.Records[lbAnimations.SelectedIndex];
            viewer?.Dispose();
            viewer = null;
            fps = 50;
            tbPlaybackFps.Text = fps.ToString();
            cbLoop.Checked = false;
            playing = false;
            UpdateLists();
        }

        private void UpdateLists()
        {
            void listAdder(IList list, string[] name, int index)
            {
                _ = list.Add($"{name[0]} {index}");
            }

            PopulateWithAnimData(lbJointSettings.Items, animation.JointsSettings, listAdder, "Joint setting");
            PopulateWithAnimData(lbTransformations.Items, animation.StaticTransforms, listAdder, "Transform");
            PopulateWithAnimData(lbAnimatedTransforms.Items, animation.AnimatedTransforms, listAdder, "Animated transform");
            PopulateWithAnimData(lbShapeSettings.Items, animation.FacialJointsSettings, listAdder, "Joint setting");
            PopulateWithAnimData(lbTransformations2.Items, animation.FacialStaticTransforms, listAdder, "Transform");
            PopulateWithAnimData(lbTwoPartTransforms2.Items, animation.FacialAnimatedTransforms, listAdder, "Animated transform");

            if (viewer != null)
            {
                viewer.FrameChanged -= Viewer_FrameChanged;
            }

            uint code_section = 10;
            if (controller.MainFile.Data.Type == TwinsFile.FileType.MonkeyBallRM)
            {
                code_section = 11;
            }
            SectionController ogis = controller.MainFile.GetItem<SectionController>(code_section).GetItem<SectionController>(3);
            List<GraphicsInfo> ogisList = new List<GraphicsInfo>();
            foreach (GraphicsInfo ogi in ogis.Data.Records.Cast<GraphicsInfo>())
            {
                if (ogi.Joints.Length <= lbJointSettings.Items.Count)
                {
                    _ = cbOGIList.Items.Add(ogi);
                    ogisList.Add(ogi);
                }
            }
            if (cbOGIList.Items.Count > 0)
            {
                int bestFitOgi = ogisList.FindIndex((gi) => { return gi.Joints.Length == lbJointSettings.Items.Count; });
                cbOGIList.SelectedIndex = bestFitOgi == -1 ? 0 : bestFitOgi;
                GraphicsInfo ogi = cbOGIList.SelectedItem as GraphicsInfo;
                GraphicsInfoController ogic = ogis.GetItem<GraphicsInfoController>(ogi.ID);
                AnimationViewer animViewer = new AnimationViewer(ogic, controller.GetItem<AnimationController>(animation.ID), controller.MainFile)
                {
                    Parent = tpPreview,
                    AutoSize = true,
                    Location = new System.Drawing.Point(3, 3),
                    Dock = DockStyle.Fill,
                    MinimumSize = new System.Drawing.Size(0, 450)
                };
                viewer = animViewer;
            }
            else
            {
                AnimationViewer animViewer = new AnimationViewer()
                {
                    Parent = tpPreview,
                    AutoSize = true,
                    Location = new System.Drawing.Point(3, 3),
                    Dock = DockStyle.Fill,
                    MinimumSize = new System.Drawing.Size(0, 450)
                };
                viewer = animViewer;
            }
            viewer.FPS = fps;
            viewer.DrawSkeletonOutline = cbShowSkeleton.Checked;
            tbAnimationTimeline.Maximum = animation.TotalFrames - 1;
            viewer.FrameChanged += Viewer_FrameChanged;
        }

        private void Viewer_FrameChanged(object sender, EventArgs e)
        {
            if (viewer.CurrentFrame > tbAnimationTimeline.Maximum)
            {
                return;
            }

            tbAnimationTimeline.Value = viewer.CurrentFrame;
        }

        private void lbDisplacements_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox list = (ListBox)sender;
            if (list.SelectedIndex == -1)
            {
                return;
            }

            Animation.JointSettings jointSettings = animation.JointsSettings[list.SelectedIndex];
            JointSettings = jointSettings;
            cbAddRotation.Checked = ((jointSettings.Flags >> 0xC) & 0x1) != 0;
            cbParentScale.Checked = ((jointSettings.Flags >> 0xD) & 0x1) != 0;
            tbJointTransformChoice.Text = jointSettings.TransformationChoice.ToString();
            tbJointTransformIndex.Text = jointSettings.TransformationIndex.ToString();
            tbJointAnimatedTransformIndex.Text = jointSettings.AnimatedTransformIndex.ToString();
            tbJointFlags.Text = $"{jointSettings.Flags:X4}";

            ushort transformChoice = JointSettings.TransformationChoice;
            List<string> timelineText = new List<string>();
            for (int i = 0; i < animation.TotalFrames - 1; i++)
            {
                ushort transformIndex = JointSettings.TransformationIndex;
                int currentFrameTransformIndex = JointSettings.AnimatedTransformIndex;
                ushort nextFrameTransformIndex = JointSettings.AnimatedTransformIndex;
                timelineText.Add($"Frame {i + 1}:\n");
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
                    timelineText.Add($"Animate Translate from X {animation.AnimatedTransforms[i].GetOffset(currentFrameTransformIndex++)}\n");
                    timelineText.Add($"Animate Translate to X {animation.AnimatedTransforms[i + 1].GetOffset(nextFrameTransformIndex++)}\n");
                }
                else
                {
                    timelineText.Add($"Static Translate X to {animation.StaticTransforms[transformIndex++].Value}\n");
                }

                if (translateYChoice)
                {
                    timelineText.Add($"Animate Translate from Y {animation.AnimatedTransforms[i].GetOffset(currentFrameTransformIndex++)}\n");
                    timelineText.Add($"Animate Translate to Y {animation.AnimatedTransforms[i + 1].GetOffset(nextFrameTransformIndex++)}\n");
                }
                else
                {
                    timelineText.Add($"Static Translate Y to {animation.StaticTransforms[transformIndex++].Value}\n");
                }

                if (translateZChoice)
                {
                    timelineText.Add($"Animate Translate from Z {animation.AnimatedTransforms[i].GetOffset(currentFrameTransformIndex++)}\n");
                    timelineText.Add($"Animate Translate to Z {animation.AnimatedTransforms[i + 1].GetOffset(nextFrameTransformIndex++)}\n");
                }
                else
                {
                    timelineText.Add($"Static Translate Z to {animation.StaticTransforms[transformIndex++].Value}\n");
                }

                if (rotXChoice)
                {
                    int rot1 = animation.AnimatedTransforms[i].GetPureOffset(currentFrameTransformIndex++) * 16;
                    int rot2 = animation.AnimatedTransforms[i + 1].GetPureOffset(nextFrameTransformIndex++) * 16;
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
                    timelineText.Add($"Animate Rotation from X {rot1Rad}");
                    timelineText.Add($"Animate Rotation to X {rot2Rad}");
                }
                else
                {
                    float rot = animation.StaticTransforms[transformIndex++].RotValue;
                    timelineText.Add($"Static Rotation X {rot}");
                }

                if (rotYChoice)
                {
                    int rot1 = animation.AnimatedTransforms[i].GetPureOffset(currentFrameTransformIndex++) * 16;
                    int rot2 = animation.AnimatedTransforms[i + 1].GetPureOffset(nextFrameTransformIndex++) * 16;
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
                    timelineText.Add($"Animate Rotation from Y {-rot1Rad}");
                    timelineText.Add($"Animate Rotation to Y {-rot2Rad}");
                }
                else
                {
                    float rot = animation.StaticTransforms[transformIndex++].RotValue;
                    timelineText.Add($"Static Rotation Y {-rot}");
                }

                if (rotZChoice)
                {
                    int rot1 = animation.AnimatedTransforms[i].GetPureOffset(currentFrameTransformIndex++) * 16;
                    int rot2 = animation.AnimatedTransforms[i + 1].GetPureOffset(nextFrameTransformIndex++) * 16;
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
                    timelineText.Add($"Animate Rotation from Z {-rot1Rad}");
                    timelineText.Add($"Animate Rotation to Z {-rot2Rad}");
                }
                else
                {
                    float rot = animation.StaticTransforms[transformIndex++].RotValue;
                    timelineText.Add($"Static Rotation Z {-rot}");
                }

                if (scaleXChoice)
                {
                    timelineText.Add($"Animate Scale from X {animation.AnimatedTransforms[i].GetOffset(currentFrameTransformIndex++)}\n");
                    timelineText.Add($"Animate Scale to X {animation.AnimatedTransforms[i + 1].GetOffset(nextFrameTransformIndex++)}\n");
                }
                else
                {
                    timelineText.Add($"Static Scale X to {animation.StaticTransforms[transformIndex++].Value}\n");
                }

                if (scaleYChoice)
                {
                    timelineText.Add($"Animate Scale from Y {animation.AnimatedTransforms[i].GetOffset(currentFrameTransformIndex++)}\n");
                    timelineText.Add($"Animate Scale to Y {animation.AnimatedTransforms[i + 1].GetOffset(nextFrameTransformIndex++)}\n");
                }
                else
                {
                    timelineText.Add($"Static Scale Y to {animation.StaticTransforms[transformIndex++].Value}\n");
                }

                if (scaleZChoice)
                {
                    timelineText.Add($"Animate Scale from Z {animation.AnimatedTransforms[i].GetOffset(currentFrameTransformIndex++)}\n");
                    timelineText.Add($"Animate Scale to Z {animation.AnimatedTransforms[i + 1].GetOffset(nextFrameTransformIndex++)}\n");
                }
                else
                {
                    timelineText.Add($"Static Scale Z to {animation.StaticTransforms[transformIndex++].Value}\n");
                }
            }
            tbJointTimelineView.Lines = timelineText.ToArray();
        }

        private void lbScales_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox list = (ListBox)sender;
            if (list.SelectedIndex == -1)
            {
                return;
            }

            Animation.Transformation scale = animation.StaticTransforms[list.SelectedIndex];
            StaticTransform = scale;
            tbTransformation.Text = scale.Value.ToString();
        }

        private void lbRotations_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox list = (ListBox)sender;
            if (list.SelectedIndex == -1)
            {
                return;
            }

            Animation.AnimatedTransform timeline = animation.AnimatedTransforms[list.SelectedIndex];
            AnimatedTransform = timeline;
        }

        private void lbDisplacements2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox list = (ListBox)sender;
            if (list.SelectedIndex == -1)
            {
                return;
            }

            Animation.JointSettings jointSettings = animation.FacialJointsSettings[list.SelectedIndex];
            JointSettings2 = jointSettings;
            ushort flags = jointSettings.Flags;
            tbShapesAmount.Text = ((flags >> 0x8) & 0xf).ToString();
            tbJointTransformChoice2.Text = jointSettings.TransformationChoice.ToString();
            tbJointTransformIndex2.Text = jointSettings.TransformationIndex.ToString();
            tbJointTwoPartTransformIndex2.Text = jointSettings.AnimatedTransformIndex.ToString();
            List<string> timelineText = new List<string>();
            int joints = (flags >> 0x8) & 0xf;
            float[] floats = new float[joints];


            for (int j = 0; j < animation.FacialAnimationTotalFrames - 1; j++)
            {
                ushort transformIndex = jointSettings.TransformationIndex;
                int currentFrameTransformIndex = jointSettings.AnimatedTransformIndex;
                ushort nextFrameTransformIndex = jointSettings.AnimatedTransformIndex;
                ushort transformChoice = jointSettings.TransformationChoice;

                timelineText.Add($"Frame {j + 1}:\n");

                for (int i = 0; i < floats.Length; i++)
                {
                    if ((transformChoice & 0x1) == 0)
                    {
                        float f1 = animation.FacialAnimatedTransforms[j].GetOffset(currentFrameTransformIndex++);
                        float f2 = animation.FacialAnimatedTransforms[j + 1].GetOffset(nextFrameTransformIndex++);
                        timelineText.Add($"\tAnimation value {i} from {f1} to {f2}");
                    }
                    else
                    {
                        floats[i] = animation.FacialStaticTransforms[transformIndex++].Value;
                        timelineText.Add($"\tSet value {i} to {floats[i]}");
                    }
                    transformChoice >>= 1;
                }
            }

            tbMorphTimeline.Lines = timelineText.ToArray();
        }

        private void lbScales2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox list = (ListBox)sender;
            if (list.SelectedIndex == -1)
            {
                return;
            }

            Animation.Transformation scale = animation.FacialStaticTransforms[list.SelectedIndex];
            StaticTransform2 = scale;
            tbTransformation2.Text = scale.Value.ToString();
        }

        private void lbRotations2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox list = (ListBox)sender;
            if (list.SelectedIndex == -1)
            {
                return;
            }

            Animation.AnimatedTransform timeline = animation.FacialAnimatedTransforms[list.SelectedIndex];
            AnimatedTransform2 = timeline;
        }

        private void tbDisB1_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!ushort.TryParse(tb.Text, out ushort result) || JointSettings == null)
            {
                return;
            }

            JointSettings.Flags = result;
        }

        private void tbDisB3_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!ushort.TryParse(tb.Text, out ushort result) || JointSettings == null)
            {
                return;
            }

            JointSettings.TransformationChoice = result;
        }

        private void tbDisB5_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!ushort.TryParse(tb.Text, out ushort result) || JointSettings == null)
            {
                return;
            }

            JointSettings.TransformationIndex = result;
        }

        private void tbDisB7_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!ushort.TryParse(tb.Text, out ushort result) || JointSettings == null)
            {
                return;
            }

            JointSettings.AnimatedTransformIndex = result;
        }

        private void tbTransformation_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!float.TryParse(tb.Text, out float result) || StaticTransform == null)
            {
                return;
            }

            StaticTransform.Value = result;
        }

        private void tbDis2B3_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!ushort.TryParse(tb.Text, out ushort result) || JointSettings2 == null)
            {
                return;
            }

            JointSettings2.TransformationChoice = result;
        }

        private void tbDis2B5_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!ushort.TryParse(tb.Text, out ushort result) || JointSettings2 == null)
            {
                return;
            }

            JointSettings2.TransformationIndex = result;
        }

        private void tbDis2B7_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!ushort.TryParse(tb.Text, out ushort result) || JointSettings2 == null)
            {
                return;
            }

            JointSettings2.AnimatedTransformIndex = result;
        }

        private void tbTransformation2_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!float.TryParse(tb.Text, out float result) || StaticTransform2 == null)
            {
                return;
            }

            StaticTransform2.Value = result;
        }

        private void tbTransformOffset_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!short.TryParse(tb.Text, out _) || AnimatedTransform == null)
            {
                return;
            }
            //TwoPartTransform.SetOffset(tbTimeline.Value, result);
        }

        private void tbTransformOffset2_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!short.TryParse(tb.Text, out _) || AnimatedTransform2 == null)
            {
                return;
            }
            //TwoPartTransform2.SetOffset(tbTimeline2.Value, result);
        }

        private void btnAddTimeline_Click(object sender, EventArgs e)
        {
            if (animation == null)
            {
                return;
            }

            animation.AnimatedTransforms.Add(new Animation.AnimatedTransform(animation.TotalFrames));
            for (int i = 0; i < animation.AnimatedTransforms[animation.AnimatedTransforms.Count - 1].Values.Capacity; ++i)
            {
                animation.AnimatedTransforms[animation.AnimatedTransforms.Count - 1].Values.Add(0);
            }
            UpdateLists();
        }

        private void cbOGIList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbOGIList.SelectedIndex == -1)
            {
                return;
            }

            uint code_section = 10;
            if (controller.MainFile.Data.Type == TwinsFile.FileType.MonkeyBallRM)
            {
                code_section = 11;
            }
            SectionController ogis = controller.MainFile.GetItem<SectionController>(code_section).GetItem<SectionController>(3);
            GraphicsInfo ogi = cbOGIList.SelectedItem as GraphicsInfo;
            GraphicsInfoController ogic = ogis.GetItem<GraphicsInfoController>(ogi.ID);
            viewer?.ChangeGraphicsInfo(ogic);
        }

        private void btnPlayAnim_Click(object sender, EventArgs e)
        {
            if (viewer != null)
            {
                playing = viewer.Finished || !playing;
                viewer.Playing = playing;
            }
        }

        private void tbPlaybackFps_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!ushort.TryParse(tb.Text, out ushort result) || viewer == null || result > 120 || result == 0)
            {
                return;
            }

            fps = result;
            viewer.FPS = fps;
        }

        private void cbLoop_CheckedChanged(object sender, EventArgs e)
        {
            if (viewer == null)
            {
                return;
            }

            loop = cbLoop.Checked;
            viewer.Loop = loop;

        }

        private void tbAnimationTimeline_Scroll(object sender, EventArgs e)
        {
            if (viewer == null)
            {
                return;
            }

            viewer.ChangeAnimationFrame(tbAnimationTimeline.Value);
            playing = false;
        }

        private void cbShowSkeleton_CheckedChanged(object sender, EventArgs e)
        {
            if (viewer == null)
            {
                return;
            }

            viewer.DrawSkeletonOutline = cbShowSkeleton.Checked;
        }

        private void btnDeleteTimeline2_Click(object sender, EventArgs e)
        {
            if (animation == null)
            {
                return;
            }

            animation.FacialAnimatedTransforms.RemoveAt(animation.FacialAnimatedTransforms.Count - 1);
            UpdateLists();
        }

    }
}
