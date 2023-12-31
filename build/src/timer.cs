using System;
using System.Collections.Generic;
using DuckGame;
using Debug = System.Diagnostics.Debug;

namespace MyMod
{
    [EditorGroup("Wires Plus")]
    [BaggedProperty("isOnlineCapable", true)]
    public class timer: Thing, IWirePeripheral
    {
        public EditorProperty<float> initialDelay = new EditorProperty<float>(0, min: 0, max: 10,increment: .5f);
        public EditorProperty<float> timerLength = new EditorProperty<float>(.1f, min: .1f, max: 60,increment: .1f);
        public EditorProperty<float> signalLength = new EditorProperty<float>(0, min: 0, max: 5,increment: .1f);
        public EditorProperty<bool> stopwatch = new EditorProperty<bool>(false);
        public EditorProperty<bool> invisible = new EditorProperty<bool>(false);
        
        private SpriteMap _sprite;
        private bool _initializedFrame;
        private float timerCooldown = 0, displayTimer = 0, signalTimer = 0, delayTimer;
        private timerHand hand;
        private Queue<signal> signalQueue;
        
        public timer(float x, float y) : base(x, y)
        {
            _canFlip = false;
            _sprite = new SpriteMap(GetPath("timer"), 16, 16);
            graphic = _sprite;

            center = new Vec2(8f, 8f); 
            collisionOffset = new Vec2(-8f, -8f);
            collisionSize = new Vec2(16f, 16f);
            _depth = -.5f;
            
            _editorName = "Timer";
            editorTooltip = "Sends out a signal every (timerLength) seconds";
            initialDelay.name = "Intial Delay";
            initialDelay._tooltip = "the delay before timer starts ticking";
            timerLength.name = "Timer Length";
            timerLength._tooltip = "The time between pulses";
            signalLength.name = "Signal Length";
            signalLength._tooltip = "How long the signal will be held once triggered";
            stopwatch.name = "Stopwatch Mode";
            stopwatch._tooltip = "If true, any signals it receives be held for (timerLength) seconds before being sent out in all directions";
            invisible.name = "Invisible";
            invisible._tooltip = "If true, this block will be invisible during play";
            
            layer = Layer.Foreground;
        }
        
        
        public override void Initialize()
        {           
            if (!(Level.current is Editor))
            {
                timerCooldown = timerLength;
                delayTimer = initialDelay;

                if (stopwatch)
                {
                    signalQueue = new Queue<signal>();
                    timerCooldown = 0;
                }
                
                //if invisible, set visible and solid = false and set collision size to sero
                if (invisible)
                {
                    visible = false;
                }
                else//if visible, spawn the timer hand on top of the timer
                {
                    hand = new timerHand(x, y);
                    level.AddThing(hand);
                }
                
            }
            else
            {//when in editor, set all values to default
               
                visible = true;              
            }
            base.Initialize();
        }
        
        //type 0 = stand 1 pulse, type 1 = hold, type 2 = turn off hold
        public void Pulse(int type, WireTileset wire)
        {
            if (stopwatch)
            {
                wire.dullSignalUp = true;
                wire.dullSignalDown = true;
                wire.dullSignalLeft = true;
                wire.dullSignalRight = true;
                signalQueue.Enqueue(new signal(timerCooldown, type));
            }
        }
        
        public override void Update()
        {

            if (!(Level.current is Editor))
            {
                if (stopwatch)
                {
                   stopwatchMode();
                }
                else
                {
                    timerMode();
                }
                
            }
            

            base.Update();
        }


        private void timerMode()
        {
            if (delayTimer <= 0)
                {
                    timerCooldown -= Maths.IncFrameTimer();
                    displayTimer -= Maths.IncFrameTimer();

                    if (!invisible)
                    {
                        //rotate hand clockwise to match the approximate time remaining
                        hand.angleDegrees = 360 * -(timerCooldown / timerLength);
                    }

                    if (timerCooldown <= 0)
                    {
                        WireTileset checkWires = Level.CheckRect<WireTileset>(topLeft + new Vec2(2f, 2f),
                            bottomRight + new Vec2(-2f, -2f));
                        if (checkWires != null)
                        {
                            //if signalLength == 0, emit a type 0 pulse
                            if (Math.Abs(signalLength.value) < .01f)
                            {
                                checkWires.Emit(type: 0);
                                //if timerLength is < the default displayTimer, then set displayTimer = timerLength/2
                                //just an aesthetic change so that a timer with really short timerLength doesnt constantly show its ON state
                                displayTimer = .3f < timerLength.value ? .3f : timerLength.value / 2;
                            }
                            else //send a type 1 pulse for (signalLength) seconds
                            {
                                checkWires.Emit(type: 1);
                                signalTimer = signalLength.value;
                                displayTimer = signalLength;
                            }
                        }

                        timerCooldown = timerLength;
                    }

                    //emit a signal for (signalTimer) seconds if signalTimer > 0
                    if (signalTimer > 0)
                    {
                        signalTimer -= Maths.IncFrameTimer();

                        //once signalTimer <= 0, emit a type 2 signal, which shuts off type 1 signals
                        if (signalTimer <= 0)
                        {
                            WireTileset checkWires = Level.CheckRect<WireTileset>(topLeft + new Vec2(2f, 2f),
                                bottomRight + new Vec2(-2f, -2f));
                            if (checkWires != null)
                            {
                                checkWires.Emit(type: 2);
                            }
                        }
                    }

                    //change the sprite to the ON sprite when displayTimer > 0
                    _sprite.frame = displayTimer > 0 ? 1 : 0;
                }
                else
                {
                    delayTimer -= Maths.IncFrameTimer();
                }
        }


        private void stopwatchMode()
        {
            timerCooldown += Maths.IncFrameTimer();
            displayTimer -= Maths.IncFrameTimer();

            if (!invisible)
            {
                //rotate hand clockwise to match the approximate time remaining
                if (signalQueue.Count > 0)
                {
                    hand.angleDegrees = 360 * Maths.Clamp(((timerCooldown - signalQueue.Peek().startTime) / timerLength), 0, 1);
                }
            }
                     
            //if a signal in the queue has waited timerLength seconds, emit 
            if (signalQueue.Count > 0 && timerCooldown - signalQueue.Peek().startTime >= timerLength)
            {
                WireTileset checkWires = Level.CheckRect<WireTileset>(topLeft + new Vec2(2f, 2f),
                    bottomRight + new Vec2(-2f, -2f));

                if (checkWires != null)
                {
                    checkWires.Emit(type: signalQueue.Dequeue().signalType);
                }
            }
        }

                    
        
        public override void Draw()
        {
            if (Level.current is Editor)
            {
                
                //prevent the signalLength from going out of bounds
                if (signalLength.value > .01f && Math.Round(signalLength.value,1) >= Math.Round(timerLength.value,1))
                {
                    double val = Math.Round(signalLength.value, 1);
                    Mod.Debug.Log("signalLength cannot be >= timerLength if signalLength is > 0!");
                    timerLength.value = (float)val + timerLength.info.increment;
                }
                
                //change the sprite to the invis sprite when invisible = true
                _sprite.frame = invisible ? 2 : 0;             
            }

            base.Draw();
        }
        
        
        private class timerHand : Thing
        {
        
            public timerHand(float x, float y) : base(x, y)
            {           
                graphic = new Sprite("timerHand");

                center = new Vec2(8f, 8f); 
                collisionOffset = new Vec2(-8f, -8f);
                collisionSize = new Vec2(16f, 16f);
                _depth = -.5f;
            
            
                layer = Layer.Foreground;
            }
        }        
        
        private struct signal
        {
            public float startTime;
            public int signalType;
            // private Vec2 origin;
            public signal(float newTime, int newType)
            {
                startTime = newTime;
                signalType = newType;                
            }
            
        }
    }
}