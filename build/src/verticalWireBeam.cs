using System;
using DuckGame;

namespace MyMod
{
    [EditorGroup("Wires Plus")]
    [BaggedProperty("isOnlineCapable", true)]
    public class verticalWireBeam: wireBeam
    {
        public verticalWireBeam(float xpos, float ypos) : base(xpos, ypos)
        {
            _editorName = "Wire Beam Vertical";
            editorTooltip = "Ever seen a wire beam? Now try tilting your head 90 degrees.";
            
              
            emitLeft.name = "Emit Top";
            emitLeft._tooltip = "If true, this will emit a signal out of the top beamer when triggered";
            
            emitRight.name = "Emit Bottom";
            emitRight._tooltip = "If true, this will emit a signal out of the bottom beamer when triggered";
            
            
            hugWalls = WallHug.Ceiling;
            angleDegrees = 90f;
            collisionOffset = new Vec2(-5f, -2f);
            collisionSize = new Vec2(10f, 4f);
            _placementCost += 2;
        }


        protected override void emit(int type)
        {
            if (emitLeft)
            {
                WireTileset checkWires = Level.CheckRect<WireTileset>(position + new Vec2(-2f, -10f),
                    position + new Vec2(2f, 4f));
                if (checkWires != null)
                {
                    checkWires.Emit(type: type);
                }
            }

            if (emitRight)
            {
                WireTileset checkWires = Level.CheckRect<WireTileset>(_endPoint + new Vec2(-4f, -2f),
                    _endPoint + new Vec2(4f, 4f));
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
            if (this.enabled)
            {
                if (this._prev != this.position)
                {
                    this._endPoint = Vec2.Zero;
                    for (int index = 0; index < 32; ++index)
                    {
                        Thing thing = (Thing) Level.CheckLine<Block>(this.position + new Vec2(0.0f, (float) (4 + index * 16)), this.position + new Vec2(0.0f, (float) ((index + 1) * 16 - 6)));
                        if (thing != null)
                        {
                            this._endPoint = new Vec2(this.x, thing.top - 2f);
                            break;
                        }
                    }
                    this._prev = this.position;
                }
                if (this._endPoint != Vec2.Zero)
                {
                    this.graphic.flipH = true;
                    this.graphic.depth = this.depth;
                    this.graphic.angleDegrees = 90f;
                    Graphics.Draw(this.graphic, this._endPoint.x, this._endPoint.y);
                    this.graphic.flipH = false;
                    this._beam.depth = this.depth - 2;
                    float y = this._endPoint.y - this.y;
                    int num = (int) Math.Ceiling((double) y / 16.0);
                    for (int index = 0; index < num; ++index)
                    {
                        if (index == num - 1)
                            this._beam.cutWidth = 16 - (int) ((double) y % 16.0);
                        else
                            this._beam.cutWidth = 0;
                        this._beam.angleDegrees = 90f;
                        if (!invisible || Level.current is Editor)
                        {
                            Graphics.Draw((Sprite) this._beam, this.x, this.y + (float) (index * 16));
                        }
                        
                    }
                    this.collisionOffset = new Vec2(-4f, -1f);
                    this.collisionSize = new Vec2(8f, y);
                }
                else
                {
                    this.collisionOffset = new Vec2(-5f, -1f);
                    this.collisionSize = new Vec2(10f, 4f);
                }
            }
            base.Draw();
        }
           
        
        
    }
}