using  DuckGame;
namespace MyMod
{
    [EditorGroup("Wires Plus")]
    [BaggedProperty("isOnlineCapable", true)]
    public class MagneticLockDoor: Thing, IWirePeripheral
    {
        public EditorProperty<bool> invisible = new EditorProperty<bool>(false);
        public EditorProperty<bool> startLocked = new EditorProperty<bool>(true);
        
        private SpriteMap _sprite;
        private bool _initializedFrame;
        private float displayTimer = 0, timer;
        
        public MagneticLockDoor(float x, float y) : base(x, y)
        {
            _canFlip = false;
            _sprite = new SpriteMap(GetPath("magneticLock"), 32, 35);
            graphic = _sprite;

            center = new Vec2(16f, 27f);
            collisionSize = new Vec2(6f, 35f);
            collisionOffset = new Vec2(-3f, -25f);
            _depth = -.5f;
            
            _editorName = "Magnetic Door Lock";
            editorTooltip = "Will lock/unlock the door this is placed over when a signal is received";

            invisible.name = "Invisible";
            invisible._tooltip = "If true, this block will be invisible during play";

            startLocked.name = "Start Locked";
            startLocked._tooltip = "It true, the door beneath this will start locked (Default true)";
            
            layer = Layer.Foreground;
        }
        
        
        public override void Initialize()
        {           
            if (!(Level.current is Editor))
            {                   
                //if invisible, set visible and solid = false and set collision size to sero
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
        
        //type 0 = stand 1 pulse, type 1 = hold, type 2 = turn off hold
        public void Pulse(int type, WireTileset wire)
        {

            if (timer <= 0)
            {
                Door checkDoor = Level.CheckRect<Door>(topLeft,
                    bottomRight);
                if (checkDoor != null)
                {
                    checkDoor.locked = !checkDoor.locked;
                    displayTimer = .3f;
                    timer = .1f;
                }
            }
        }
        
        public override void Update()
        {          
            if (!(Level.current is Editor))
            {               
                if (!_initializedFrame)
                {
                    if (startLocked)
                    {
                        Pulse(0,null);
                    }

                    _initializedFrame = true;
                }
                
                
                displayTimer -= Maths.IncFrameTimer();   
                timer -= Maths.IncFrameTimer();
               
                //change the sprite to the ON sprite when displayTimer > 0
                _sprite.frame = displayTimer > 0 ? 1 : 0;
            }
            base.Update();
        }

        public override void Draw()
        {
            if (Level.current is Editor)
            {
                //change the sprite to the invis sprite when invisible = true
                _sprite.frame = invisible ? 2 : 0;             
            }

            base.Draw();
        }
    }
}