using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Linq;
using DuckGame;
namespace MyMod
{
    
    [EditorGroup("Wires Plus")]
    [BaggedProperty("isOnlineCapable", true)]
    public class Transmitter : Thing, IWirePeripheral
    {       
        public EditorProperty<string> channel = new EditorProperty<string>("Alpha8");

        public EditorProperty<bool> invisible = new EditorProperty<bool>(false);
        
        private SpriteMap _sprite;
        private bool _initializedFrame, isMaster;
        private float displayTimer = 0,pulseCountTimer = .1f;
        public float feedBackTimer = 0;
        private int pulseCount = 0;

        private string prevChannel = "";
        private Random rand;
        
        private List<Receiver> connectedReceivers;
        
        public Transmitter(float x, float y) : base(x, y)
        {
            _canFlip = false;
            _sprite = new SpriteMap(GetPath("transmitter") , 16, 16);
            graphic = _sprite;

            center = new Vec2(8f, 8f); 
            collisionOffset = new Vec2(-8f, -8f);
            depth = -.5f;
            collisionSize = new Vec2(16f, 16f);

            connectedReceivers = new List<Receiver>();
            
            _editorName = "Transmitter";
            editorTooltip = "Any signal sent into the transmitter will be sent out to all receivers on the same channel";
            channel.name = "Channel";
            channel._tooltip = "The channel the signal will be sent out on (set channel to 'multicast' to send to all receivers)";
            invisible.name = "Invisible";
            invisible._tooltip = "If true, this block will be invisible during play";
            
            layer = Layer.Foreground;
        }
        
        public override void Initialize()
        {
            //changeColor();
            if (!(Level.current is Editor))
            {    
                //if invisible, set visible
                
                
                
                if (invisible)
                {
                    visible = false;
                }               
            }
            else
            {//when in editor, set all values to default
               
                visible = true; 
            }
            
            base.Initialize();
        }

        private int convert(string str)
        {
            int result = 0;
            foreach (char c in str)
            {
                result += (int) c;
            }

            return result;
        }
        
        private void changeColor()
        {
            prevChannel = channel.value;
            rand = new Random(convert(channel.value));
            int R = rand.Next(0, 255);
            int G = rand.Next(0, 255);
            int B = rand.Next(0, 255);
            //Mod.Debug.Log(R + " " + G + " " + B);
            graphic.color = new Color(R,G,B);
        }
       
        //type 0 = stand 1 pulse, type 1 = hold, type 2 = turn off hold
        public void Pulse(int type, WireTileset wire)
        {
            pulseCount++;
            
            if (feedBackTimer <= 0)
            {
                //feedback loop prevention
                //if pulse gets called > 3 times in a .1 second interval, prevent any more signals from going through for the next 5 seconds
                if (pulseCount > 3)
                {
                    Mod.Debug.Log("Potential FeedBack loop at (" + position.x + ", " + position.y + ")");
                    feedBackTimer = 3;
                }
                else
                {
                    foreach (Receiver r in connectedReceivers)
                    {
                        r.transmitterPulse(type);
                    }

                    displayTimer = .3f;
                }
            }
            else
            {
                wire.dullSignalDown = true;
                wire.dullSignalLeft = true;
                wire.dullSignalRight = true;
                wire.dullSignalUp = true;
            }
       }
        
        
        public override void Update()
        {

            if (!(Level.current is Editor))
            {
                //when the level begins, initialize the transmitter on the first update
                if (!_initializedFrame)
                {
                    transmitterSetup();
                }

                displayTimer -= Maths.IncFrameTimer();
                feedBackTimer -= Maths.IncFrameTimer();
                pulseCountTimer -= Maths.IncFrameTimer();
                
                
                //if the feedback prevention was triggered, display the caution sprite
                if (feedBackTimer > 0)
                {
                    visible = true;
                    _sprite.frame = 3;                    
                }
                else//change the sprite to the ON sprite when displayTimer > 0
                {
                    _sprite.frame = displayTimer > 0 ? 1 : 0;
                }

                //when pulseCountTimer is <= 0, reset pulseCountTimer and pulseCount
                if (pulseCountTimer <= 0)
                {
                    pulseCountTimer = .1f;
                    pulseCount = 0;
                }
            }

            base.Update();
        }

        //search the map to find any receivers that share this transmitters channel and store them in a list
        private void transmitterSetup()
        {
            //get all receivers from the level's thing list
            List<Thing> rList = Level.current.things.Where(t => t is Receiver).ToList();
            //Mod.Debug.Log("Linq: " + rList.ToList().Count + "");  
            
            if (rList.Count > 0)
            {
                foreach (Receiver r in rList)
                {
                    //connect to all receivers that have the same channel, or if this has multicast enabled, connect to all receivers
                    if (r.channel.value.Equals(channel.value) || channel.value.Equals("multicast")) //|| r.channel.value.Equals("multicast")) multicast in the other direction, not sure how useful tho
                    {
                        connectedReceivers.Add(r);
                    }
                } 
            }
            else
            {
                Mod.Debug.Log("No receivers found");
            }
                    
            _initializedFrame = true;
        }

        public override void DrawHoverInfo()
        {
            Graphics.DrawString(channel, this.position + new Vec2((float) (-(double) Graphics.GetStringWidth(channel) / 2.0), -16f), Color.White, (Depth) 0.9f);
            base.DrawHoverInfo();
        }


        public override void Draw()
        {
            if (Level.current is Editor)
            {
                /*
                if (!prevChannel.Equals(channel.value))
                {
                    //changeColor();
                }
                */

                //change the sprite to the invis sprite when invisible = true
                _sprite.frame = invisible ? 2 : 0;  
            }

            base.Draw();
        }
        
        
    }
    
    
}
