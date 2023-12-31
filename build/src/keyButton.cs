using System;
using DuckGame;

namespace MyMod
{
    [EditorGroup("Wires Plus")]
    [BaggedProperty("isOnlineCapable", true)]
    public class keyButton: Block, IWirePeripheral
    {
        
        public EditorProperty<float> signalLength = new EditorProperty<float>(0, min: 0, max: 5,increment: .1f);
        public EditorProperty<bool> invert = new EditorProperty<bool>(false);
        public EditorProperty<bool> blockSignals = new EditorProperty<bool>(false);
        private float signalTimer = 0;
        
        private SpriteMap _sprite;
        private bool isLocked = true, _initializedFrame, doInitialEmit;

        public keyButton(float x, float y) : base(x, y)
        {
            _canFlip = false;
            _sprite = new SpriteMap(GetPath("keyButton"), 16, 16);
            graphic = _sprite;
            center = new Vec2(8f, 8f);
            collisionOffset = new Vec2(-8f, -8f);
            collisionSize = new Vec2(16f, 16f);
            depth = -0.5f;
            
            _editorName = "Key Button";
            editorTooltip = "Sends out a signal when unlocked";
            
            signalLength.name = "Signal Length";
            signalLength._tooltip = "How long the signal will be held once unlocked";
            
            invert._tooltip = "If true, the key button will send signals as long as it's locked.";

            blockSignals.name = "Block Signals";
            blockSignals._tooltip =
                "If true, the key button will prevent other signals from passing through until unlocked.";
            
            
            thickness = 4f;
            physicsMaterial = PhysicsMaterial.Metal;
            layer = Layer.Foreground;
        }


        public override void OnImpact(MaterialThing with, ImpactedFrom @from)
        {
            if (isLocked)
            {
                //if the thing that hit this keybutton is either a key or a duck holding a key, unlock the button
                if (with is Key)
                {
                    unlockButton(with);
                }
                else if (with is Duck && ((Duck) with).holdObject is Key)
                {
                    unlockButton(((Duck) with).holdObject);
                }
            }

            base.OnImpact(with, @from);
        }   


        public override void Update()
        {
            if (!_initializedFrame)
            {
                initializeCheck();
            }

            if (signalLength > .01f && signalTimer > 0)
            {
                signalTimer -= Maths.IncFrameTimer();
                    
                //once signalTimer <= 0, emit a type 2 signal, which shuts off type 1 signals
                if (signalTimer <= 0)
                {
                    WireTileset checkWires = Level.CheckRect<WireTileset>(topLeft + new Vec2(2f, 2f),
                        bottomRight + new Vec2(-2f, -2f));
                    if (checkWires != null)
                    {
                        
                        checkWires.Emit(type: invert?1:2);                              
                    }    
                }
            }

            base.Update();
        }

        //if invert = true, we need to emit a pulse at the start of the level, but wires dont properly emit
        //in the initialize() method, which is why we do it in the update check, and further requires an extra update call before
        //actually emitting
        private void initializeCheck()
        {
            if (doInitialEmit)
            {
                doInitialEmit = false;
                _initializedFrame = true;
                WireTileset checkWires = Level.CheckRect<WireTileset>(topLeft + new Vec2(2f, 2f),
                    bottomRight + new Vec2(-2f, -2f));
                if (checkWires != null)
                {
                    checkWires.Emit(type: 1);
                }
            }
            
            if (invert)
            {         
                doInitialEmit = true;
            }
            else
            {
                _initializedFrame = true;
            }
        }
        

        private void unlockButton(MaterialThing key)
        {          
            isLocked = false;
            _sprite.frame = 1;
            level.RemoveThing(key);
            SFX.Play("deedleBeep");
            
            WireTileset checkWires = Level.CheckRect<WireTileset>(topLeft + new Vec2(2f, 2f),
                bottomRight + new Vec2(-2f, -2f));
            if (checkWires != null)
            {
                if (invert)
                {
                    checkWires.Emit(type: 2);
                }
                else
                {
                    checkWires.Emit(type: signalLength < .01f ? 0 : 1 );
                }
                signalTimer = signalLength;
            }
        }
        
        public void Pulse(int type, WireTileset wire)
        {
            //if blockSignals = true, prevent any signals from passing through the keyButton until unlocked
            if (blockSignals && isLocked)
            {
                wire.dullSignalDown = true;
                wire.dullSignalLeft = true;
                wire.dullSignalRight = true;
                wire.dullSignalUp = true;
            }          
        }
        
    }
    
}
//public EditorProperty<float> signalLength = new EditorProperty<float>(0, min: 0, max: 5,increment: .1f);
      