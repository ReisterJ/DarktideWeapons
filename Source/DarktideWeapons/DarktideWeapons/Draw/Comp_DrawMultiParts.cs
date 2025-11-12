using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DarktideWeapons.Draw
{
    public class Comp_DrawMultiParts : ThingComp
    {
        public Graphic FrameGraphic;
        public float Xoffset;
        public float Yoffset;
        public float Zoffset;
        public override void PostDraw()
        {
            Vector3 drawPos = this.parent.DrawPos;
            drawPos.x += this.Props.Xoffset;
            drawPos.z += this.Props.Zoffset;
            Graphic frameGraphic = this.Props.frameGraphic.Graphic;
            drawPos.y = AltitudeLayer.Blueprint.AltitudeFor();
            FrameGraphic = frameGraphic.GetColoredVersion(frameGraphic.Shader, frameGraphic.color, frameGraphic.colorTwo);
            FrameGraphic.Draw(drawPos, this.parent.Rotation, this.parent);
        }
        public CompProperties_DrawMultiParts Props
        {
            get
            {
                return (CompProperties_DrawMultiParts)this.props;
            }
        }
    }
    public class CompProperties_DrawMultiParts : CompProperties
    {
        public CompProperties_DrawMultiParts()
        {
            this.compClass = typeof(Comp_DrawMultiParts);
        }
        public GraphicData frameGraphic;
        public float Xoffset;
        public float Yoffset;
        public float Zoffset;
    }
}
