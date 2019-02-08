﻿using CrossMod.Rendering;
using CrossMod.Rendering.Models;
using CrossMod.Nodes;
using System;
using System.Windows.Forms;

namespace CrossMod.GUI
{
    /// <summary>
    /// A <see cref="Control"/> for playing RAnimation files with a skeleton.
    /// </summary>
    public partial class AnimationBar : UserControl
    {
        private static readonly string playingText = "||";
        private static readonly string stoppedText = ">";

        private int _FrameCount { get; set; }
        public int FrameCount
        {
            get => _FrameCount;
            protected set
            {
                //animationTrack.Maximum = value;
                _FrameCount = value;
                totalFrame.Maximum = value;
                currentFrame_UpDown.Maximum = value;
                totalFrame.Value = value;
            }
        }

        //public int Frame
        //{
        //    get => animationTrack.Value;
        //    set
        //    {
        //        animationTrack.Value = value;
        //        _ExactFrame = value;
        //    }
        //}

        //Comes with floating point precision so we can use frame speed modifiers
        public float Frame { get; set; }

        public float MotionRate
        {
            get
            {
                if (scriptNode == null)
                    return 1;
                return scriptNode.MotionRate;
            }
        }

        public RModel Model { get; set; }
        public RSkeleton Skeleton { get; set; }
        public ScriptNode scriptNode { get; set; }

        /// <summary>
        /// Sets the current animation.
        /// Setting it to null stops playback, setting it when an animation
        /// is ongoing starts a new animation but from frame 0.
        /// </summary>
        public IRenderableAnimation Animation
        {
            get
            {
                return animation;
            }
            set
            {
                if (value == null)
                {
                    Stop();
                    animation = null;
                }
                else if (animation != value)
                {
                    animation = value;
                    FrameCount = value.GetFrameCount();

                    // Set frame to 0 and ensure the animation always re-renders on swap.
                    // Value change events don't trigger if the value doesn't change.
                    if (Frame == 0)
                    {
                        UpdateAnimation();
                    }
                    else
                    {
                        currentFrame_UpDown.Value = 0;
                    }
                }
                UpdateScript();
            }
        }
        private IRenderableAnimation animation;

        private Timer animationPlayer;

        public bool IsPlaying => playButton.Text.Equals(playingText);

        public AnimationBar()
        {
            InitializeComponent();
            //animationTrack.TickFrequency = 1;
            SetupTimer();
            currentFrame_UpDown.Increment = (decimal)MotionRate;
        }

        public void Start()
        {
            playButton.Text = playingText;
            animationPlayer.Start();
        }

        public void Stop()
        {
            playButton.Text = stoppedText;
            animationPlayer.Stop();
        }

        private void SetupTimer()
        {
            animationPlayer = new Timer
            {
                Interval = 100 / 60
            };
            animationPlayer.Tick += new EventHandler(animationTimer_Tick);
        }
        
        /**
         * Updates the held animation object to hold the current state.
         * This includes the model, skeleton, and current frame.
         */
        private void UpdateAnimation()
        {
            if (Animation == null)
                return;

            Animation.SetFrameModel(Model, Frame);
            Animation.SetFrameSkeleton(Skeleton, Frame);
        }

        private void UpdateScript()
        {
            if (scriptNode == null)
                return;

            if (Frame == 0)
                scriptNode.Start();
            scriptNode.Update(Frame);

            currentFrame_UpDown.Increment = (decimal)MotionRate;
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            // Loop back to the beginning at the end.
            float nextFrame = Frame + MotionRate;

            if (nextFrame > FrameCount)
                currentFrame_UpDown.Value = 0;
            else
                currentFrame_UpDown.Value = (decimal)nextFrame;
        }

        //private void animationTrack_ValueChanged(object sender, EventArgs e)
        //{
        //    currentFrame_UpDown.Value = Frame;
        //    UpdateAnimation();
        //    UpdateScript();
        //}

        private void currentFrame_ValueChanged(object sender, EventArgs e)
        {
            Frame = (float)currentFrame_UpDown.Value;
            UpdateAnimation();
            UpdateScript();
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (IsPlaying)
                Stop();
            else
                Start();
        }
    }
}
