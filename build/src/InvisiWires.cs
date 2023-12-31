using System;
using DuckGame;

namespace MyMod
{
    [EditorGroup("Wires Plus")]
    [BaggedProperty("isOnlineCapable", true)]
    public class InvisiWires: WireTileset
    {      
        public InvisiWires(float x, float y) : base(x, y)
        {
            _editorName = "InvisiWires";
            editorTooltip = "Wires but invisible.";
            _sprite = new SpriteMap(GetPath("invisiWireTileset"), 16, 16);
            graphic = _sprite;
            
        }

        public override void Initialize()
        {
            //set the wires to be invisible on level start
            if (!(Level.current is Editor))
            {   
                visible = false;
            }
            else
            {//when in editor, set all values to default
               
                visible = true; 
            }
        }
        
        

    }
}