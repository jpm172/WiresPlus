using System;
using DuckGame;

namespace MyMod
{
    [EditorGroup("Wires Plus")]
    [BaggedProperty("isOnlineCapable", true)]
    public class wireBeam: FunBeam
    {

        public EditorProperty<bool> invisible = new EditorProperty<bool>(false);
        public EditorProperty<float> emitCooldown = new EditorProperty<float>(.1f,min: .1f, max: 10, increment: .1f);
        public EditorProperty<float> signalLength = new EditorProperty<float>(0, min: 0, max: 5,increment: .1f);
        public EditorProperty<bool> emitLeft = new EditorProperty<bool>(true);
        public EditorProperty<bool> emitRight = new EditorProperty<bool>(true);
        public EditorProperty<bool> triggerOnDuck = new EditorProperty<bool>(true);
        public EditorProperty<bool> triggerOnOther = new EditorProperty<bool>(true);

        protected SpriteMap _beamerSprite;
        protected float timer = 0, signalTimer;
        
        public wireBeam(float xpos, float ypos) : base(xpos, ypos)
        {
            _beam = new SpriteMap(GetPath("wireBeam"), 16, 16);
            _beam.ClearAnimations();
            _beam.AddAnimation("idle", 1f, true, 0, 1, 2, 3, 4, 5, 6, 7);
            _beam.SetAnimation("idle");
            _beam.speed = 0.2f;
            _beam.alpha = 0.3f;
            _beam.center = new Vec2(0.0f, 8f);
            _beamerSprite = new SpriteMap(GetPath("wireBeamer"),16,16);
            graphic = _beamerSprite;
            center = new Vec2(9f, 8f);
            collisionOffset = new Vec2(-2f, -5f);
            collisionSize = new Vec2(4f, 10f);
            depth = -0.5f;
            
            _editorName = "Wire Beam";
            editorTooltip = "Place 2 generators near each other to create a beam that will emit a signal when triggered";

            invisible.name = "Invisible";
            invisible._tooltip = "If true, this block will be invisible during play";

            emitCooldown.name = "Cooldown";
            emitCooldown._tooltip = "the time required between each trigger (default .1)";
            
            signalLength.name = "Signal Length";
            signalLength._tooltip = "How long the signal will be held once triggered";
            
            emitLeft.name = "Emit Left";
            emitLeft._tooltip = "If true, this will emit a signal out of the left beamer when triggered";
            
            emitRight.name = "Emit Right";
            emitRight._tooltip = "If true, this will emit a signal out of the right beamer when triggered";

            triggerOnDuck.name = "Trigger on duck";
            triggerOnDuck._tooltip = "If true, this will emit a signal whenever a duck passes through";
            
            triggerOnOther.name = "Trigger on other";
            triggerOnOther._tooltip = "If true, this will emit a signal whenever an object that is not a duck passes through";
            
            hugWalls = WallHug.Left;
        }

        
        
        public override void Initialize()
        {
            if (!(Level.current is Editor))
            {
                //if invisible, set visible and solid = false an       
                if (invisible)
                {
                    _beamerSprite.frame = 3;
                }    
            }
            else
            {
                //when in editor, set all values to default
                _beamerSprite.frame = 0;
            }  
            base.Initialize();
        }
        
        public override void OnSoftImpact(MaterialThing with, ImpactedFrom @from)
        {
            if (timer > 0 || !enabled)
                return;

            int type = Math.Abs(signalLength.value) < .01f ? 0 : 1;                     
            
            if (triggerOnDuck && with is Duck)
            {
                emit(type);
            }
            else if (triggerOnOther && !(with is Duck))
            {
                emit(type);
            }
            
            
        }


        public override void Update()
        {
            timer -= Maths.IncFrameTimer();            
            
            if (signalTimer > 0)
            {
                signalTimer -= Maths.IncFrameTimer();
                    
                //once signalTimer <= 0, emit a type 2 signal, which shuts off type 1 signals
                if (signalTimer <= 0)
                {
                   emit(2);
                }
            }
            
            base.Update();
        }

        protected virtual void emit(int type)
        {
            if (emitLeft)
            {
                WireTileset checkWires = Level.CheckRect<WireTileset>(topLeft + new Vec2(-8f,2f),
                    _prev + new Vec2(-2f, 4f));
                if (checkWires != null)
                {
                    checkWires.Emit(type: type);                              
                }
            }
            
            if (emitRight)
            {
                
                WireTileset checkWires = Level.CheckRect<WireTileset>(_endPoint + new Vec2(2f, -2f),
                    _endPoint + new Vec2(10f, 8f));
                if (checkWires != null)
                {
                    checkWires.Emit(type: type);                              
                }
            }

            if (type == 1)
            {
                signalTimer = signalLength;
            }
            timer = emitCooldown;
        }
        
        public override void Draw()
        {
            if (Editor.editorDraw)
            {
                if (signalLength.value > .01f && Math.Round(signalLength.value,1) >= Math.Round(emitCooldown.value,1))
                {
                    double val = Math.Round(signalLength.value, 1);
                    Mod.Debug.Log("signalLength cannot be >= cooldown if signalLength is > 0!");
                    emitCooldown.value = (float)val + emitCooldown.info.increment;
                }
                _beamerSprite.frame = invisible ? 2 : 0;       
                return;
            }
            
            if (this.enabled && this.GetType() == typeof (wireBeam))
            {
                if (this._prev != this.position)
                {
                    this._endPoint = Vec2.Zero;
                    for (int index = 0; index < 32; ++index)
                    {
                        Thing thing = (Thing) Level.CheckLine<Block>(this.position + new Vec2((float) (4 + index * 16), 0.0f), this.position + new Vec2((float) ((index + 1) * 16 - 6), 0.0f));
                        if (thing != null)
                        {
                            this._endPoint = new Vec2(thing.left - 2f, this.y);
                            break;
                        }
                    }
                    this._prev = this.position;
                }
                if (this._endPoint != Vec2.Zero)
                {
                    this.graphic.flipH = true;
                    this.graphic.depth = this.depth;
                    Graphics.Draw(this.graphic, this._endPoint.x, this._endPoint.y);
                    this.graphic.flipH = false;
                    this._beam.depth = this.depth - 2;
                    float x = this._endPoint.x - this.x;
                    int num = (int) Math.Ceiling((double) x / 16.0);
                    for (int index = 0; index < num; ++index)
                    {
                        this._beam.cutWidth = index != num - 1 ? 0 : 16 - (int) ((double) x % 16.0);
                        if (!invisible || Level.current is Editor)
                        {
                            Graphics.Draw((Sprite) this._beam, this.x + (float) (index * 16), this.y);
                        }
                    }
                    this.collisionOffset = new Vec2(-1f, -4f);
                    this.collisionSize = new Vec2(x, 8f);
                }
                else
                {
                    this.collisionOffset = new Vec2(-1f, -5f);
                    this.collisionSize = new Vec2(4f, 10f);
                }
            }
            base.Draw();
        }    
           
    }
}