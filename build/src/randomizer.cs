using System;
using System.Runtime.InteropServices;
using  DuckGame;

namespace MyMod
{
    [EditorGroup("Wires Plus")]
    [BaggedProperty("isOnlineCapable", true)]
    public class Randomizer : Thing, IWirePeripheral
    {
        public EditorProperty<float> initialDelay = new EditorProperty<float>(0, min: 0, max: 10, increment: .5f);
        public EditorProperty<float> minTime = new EditorProperty<float>(0, min: 0, max: 29.5f, increment: .5f);
        public EditorProperty<float> maxTime = new EditorProperty<float>(1, min: .5f, max: 30, increment: .5f);
        public EditorProperty<float> signalLength = new EditorProperty<float>(0, min: 0, max: 5,increment: .1f);
        public EditorProperty<bool> invisible = new EditorProperty<bool>(false);
        
        private SpriteMap _sprite;
        private bool _initializedFrame;
        private float randomTimer = 0, displayTimer = 0, signalTimer = 0,delayTimer;
        private PhysicsObject prevO;
        private Random rand;
        
        public Randomizer(float x, float y) : base(x, y)
        {
            _canFlip = false;
            _sprite = new SpriteMap(GetPath("randomizer"), 16, 16);
            graphic = _sprite;

            center = new Vec2(8f, 8f); 
            collisionOffset = new Vec2(-8f, -8f);
            collisionSize = new Vec2(16f, 16f);
            _depth = -.5f;
            
            _editorName = "Randomizer";
            editorTooltip = "Randomly sends out a signal every [minTime, maxTime] seconds";
            signalLength.name = "Signal Length";
            signalLength._tooltip = "How long the signal will be held once triggered";
            initialDelay.name = "Intial Delay";
            initialDelay._tooltip = "the delay before randomizer starts randomizing";
            minTime.name = "Min Time";
            minTime._tooltip = "Lower bound of the randomTimer";
            maxTime.name = "Max Time";
            maxTime._tooltip = "Upper bound of the randomTimer";
            invisible.name = "Invisible";
            invisible._tooltip = "If true, this block will be invisible during play";
            
            layer = Layer.Foreground;
        }
        
        
        public override void Initialize()
        {           
            if (!(Level.current is Editor))
            {
                //create a Random variable with the key as its hash code, since the default key is related to time
                //and is therfore not very random since they are all created at the same time
                rand = new Random(GetHashCode());
        
                //if invisible, set visible and solid = false and set collision size to sero
                if (invisible)
                {
                    visible = false;
                }
                //on initialization, set randomTimer to a random value
                randomTimer = getRandomValue();
                delayTimer = initialDelay;
            }
            else
            {//when in editor, set all values to default
               
                visible = true;              
            }
            base.Initialize();
        }
        
        //type 0 = stand 1 pulse, type 1 = hold, type 2 = turn off hold
        
        public override void Update()
        {
           
            if (!(Level.current is Editor))
            {
                if (delayTimer <= 0)
                {
                    randomTimer -= Maths.IncFrameTimer();
                    displayTimer -= Maths.IncFrameTimer();


                    if (randomTimer <= 0)
                    {
                        WireTileset checkWires = Level.CheckRect<WireTileset>(topLeft + new Vec2(2f, 2f),
                            bottomRight + new Vec2(-2f, -2f));
                        if (checkWires != null)
                        {
                            //if signalLength == 0, emit a type 0 pulse
                            if (signalLength.value < .01f)
                            {
                                checkWires.Emit(type: 0);
                                displayTimer = .3f;
                            }
                            else //send a type 1 pulse for (signalLength) seconds
                            {
                                checkWires.Emit(type: 1);
                                signalTimer = signalLength.value;
                                displayTimer = signalLength;
                            }
                        }

                        randomTimer = getRandomValue();

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

            base.Update();
        }

        //get a random float in the range of [maxTime,minTime]
        private float getRandomValue()
        {
            return (float)rand.NextDouble() * (maxTime - minTime) + minTime;
        }

        public override void Draw()
        {
            if (Level.current is Editor)
            {
                
                //prevent the signalLength from going out of bounds
                if (signalLength.value > .01f && Math.Round(signalLength.value,1) >= Math.Round(minTime.value,1))
                {
                    double val = Math.Round(signalLength.value, 1);
                    Mod.Debug.Log("signalLength cannot be >= minTime if signalLength is > 0!");
                    minTime.value = (float)(val - (val % .5) + minTime.info.increment);
                }
                //prevent the range from going out of bounds
                if (Math.Round(minTime.value,1) >= Math.Round(maxTime.value,1))
                {
                    Mod.Debug.Log("minTime cannot be >= maxTime!");
                    maxTime.value = minTime.value +minTime.info.increment;
                }
                

                //change the sprite to the invis sprite when invisible = true
                _sprite.frame = invisible ? 2 : 0;             
            }


            base.Draw();
        }

        public void Pulse(int type, WireTileset wire)
        {           
        }
    }
}