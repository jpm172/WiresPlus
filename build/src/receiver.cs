using System;
using System.Collections.Generic;
using DuckGame;
namespace MyMod
{
    [EditorGroup("Wires Plus")]
    [BaggedProperty("isOnlineCapable", true)]
    public class Receiver : Thing, IWirePeripheral
    {       
        public EditorProperty<string> channel = new EditorProperty<string>("Alpha8");
        public EditorProperty<bool> invisible = new EditorProperty<bool>(false);
        
        private SpriteMap _sprite;
        private bool doPulse;
        private float displayTimer = 0;
        private int pulseType = 0;
        private string prevChannel = "";

        private Random rand;
        
        public Receiver(float x, float y) : base(x, y)
        {
            _canFlip = false;
            _sprite = new SpriteMap(GetPath("receiver"), 16, 16);
            graphic = _sprite;

            center = new Vec2(8f, 8f); 
            collisionOffset = new Vec2(-8f, -8f);
            collisionSize = new Vec2(16f, 16f);
            _depth = -.5f;
            
            _editorName = "Receiver";
            editorTooltip = "Receives the signals sent out by transmitters on the same channel";
            channel.name = "Channel";
            channel._tooltip = "The channel this will receive from";
            invisible.name = "Invisible";
            invisible._tooltip = "If true, this block will be invisible during play";

            layer = Layer.Foreground;
        }
        
        
        public override void Initialize()
        {
            //changeColor();
           
            if (!(Level.current is Editor))
            {
                
                //if invisible, set visible = false
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

        
        //I know i should make convert a function in a tools class, but its only used in two places
        private int convert(string str)
        {
            int result = 0;
            foreach (char c in str)
            {
                result += (int) c;
            }

            return result;
        }
        
        //not using the inherited Pulse because it made fadeback loops worse
        //wires and transmitters would call Pulse(), causing way too many pulses in a feedback loop scenario
        public void transmitterPulse(int type)
        {
            doPulse = true;
            pulseType = type;                      
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

        public override void Update()
        {
           
            if (!(Level.current is Editor))
            {
                
                displayTimer -= Maths.IncFrameTimer();
                
                //force emit to happen at next update to allow feedback protection to update on transmitter
                if (doPulse)
                {                    
                    WireTileset checkWires = Level.CheckRect<WireTileset>(topLeft + new Vec2(2f, 2f),
                        bottomRight + new Vec2(-2f, -2f));
                    if (checkWires != null)
                    {
                        checkWires.Emit(type: pulseType);
                    }
                    
                    doPulse = false;
                    displayTimer = .3f;
                }
                //change the sprite to the ON sprite when displayTimer > 0

                 _sprite.frame = displayTimer > 0 ? 1 : 0;

            }
            base.Update();
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
                }*/

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
