using System.Runtime.CompilerServices;
using DuckGame;
namespace MyMod
{
    [EditorGroup("Wires Plus")]
    [BaggedProperty("isOnlineCapable", true)]
    public class blockActuator: Thing, IWirePeripheral
    {
        
        public EditorProperty<bool> invisible = new EditorProperty<bool>(false);
        public EditorProperty<bool> startActuated = new EditorProperty<bool>(false);
        
        private SpriteMap _sprite;
        private bool _initializedFrame, doActuate;
        private float displayTimer = 0;
        private int prevFrame;
        private Block actuatedBlock;
        
        public blockActuator(float x, float y) : base(x, y)
        {
            _canFlip = false;
            _sprite = new SpriteMap(GetPath("actuator"), 16, 16);
            graphic = _sprite;

            center = new Vec2(8f, 8f); 
            collisionOffset = new Vec2(-8f, -8f);
            collisionSize = new Vec2(16f, 16f);
            _depth = -.5f;
            
            _editorName = "Block Actuator";
            editorTooltip = "When triggered, this will enable/disable the block it is placed over";

            invisible.name = "Invisible";
            invisible._tooltip = "If true, this block will be invisible during play";

            startActuated.name = "Start Actuated";
            startActuated._tooltip = "if true, the block will be actuated at the start of the round";

            layer = Layer.Foreground;
        }

        public override void Initialize()
        {
            if (!(Level.current is Editor))
            {    
                //if invisible, set visible and solid = false an
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
        

        public override void Update()
        {

            if (!_initializedFrame)
            {
                _initializedFrame = true;
                Block checkBlock = Level.CheckRect<Block>(topLeft + new Vec2(2f, 2f),
                    bottomRight + new Vec2(-2f, -2f));
                if (checkBlock != null)
                {
                    //if the block is in a blockGroup, remove it so it doesn't actuate all connected blocks
                    BlockGroup bGroup = Level.CheckRect<BlockGroup>(topLeft + new Vec2(2f, 2f),
                        bottomRight + new Vec2(-2f, -2f));

                    if (bGroup != null)
                    {
                         bGroup.Remove(checkBlock);
                    }

                    if (startActuated)
                    {
                        doActuate = true;
                    }
                }
                else//if not over a block, set this to inactive
                {
                    active = false;                   
                }
            }
            else
            {            
                if (active && actuatedBlock == null)
                {
                    Block checkBlock = Level.CheckRect<Block>(topLeft + new Vec2(2f, 2f),
                        bottomRight + new Vec2(-2f, -2f));
                    //save all the variables from checkBlock to use when actuating
                    //dont let actuated block == a door, doesnt really interact well
                    //doesnt crash, but cant really re-add a door to a level it seems
                    if (checkBlock != null && !(checkBlock is Door || checkBlock is VerticalDoor))
                    {
                        actuatedBlock = checkBlock;
                        prevFrame = checkBlock.frame;
                    }
                }

                displayTimer -= Maths.IncFrameTimer();

                if (doActuate)
                {
                    doActuate = false;

                    if (actuatedBlock != null && !actuatedBlock.destroyed)
                    {
                        //remove the block from the level 
                        if (actuatedBlock.visible)
                        {
                            actuatedBlock.frame =
                                0; //change the block's frame to 0 to remove the nubs for certain blocks  
                            actuatedBlock.Removed();

                            //updates any physics objects above the now actuated block
                            foreach (PhysicsObject physicsObject in Level.CheckRectAll<PhysicsObject>(
                                actuatedBlock.topLeft + new Vec2(0.0f, -2f), actuatedBlock.bottomRight))
                                physicsObject.sleeping = false;
                        }
                        else //re-add the block to the level and give it it's original frame
                        {
                            actuatedBlock.frame = prevFrame;
                            actuatedBlock.Added(Level.activeLevel);
                        }

                        //make the block visible/invible (removing the block makes it invisible, this is jsut an easy way to see if the block is actuated or not without new variables)
                        actuatedBlock.visible = !actuatedBlock.visible;
                    }
                }

                _sprite.frame = displayTimer > 0 ? 1 : 0;
            }

            base.Update();
        }

        public void Pulse(int type, WireTileset wire)
        {
            //set doActuate to true to actuate the block next update
            if (actuatedBlock != null)
            {
                doActuate = true;
                displayTimer = .3f;
            }
            
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