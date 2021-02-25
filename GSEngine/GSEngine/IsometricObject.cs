    using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSEngine
{
    public class IsometricObject
    {
        public Object3D Obj3D = new Object3D();

        public Polygon GetTopPolygon()
        {
            int IndexLastFound = -1;
            int PrevHighestZ = 0;
            int IndexCounter = 0;
            foreach (KeyValuePair<Polygon, int> A in Obj3D.Polygons)
            {
                if (A.Value > PrevHighestZ)
                {
                    PrevHighestZ = A.Value;
                    IndexLastFound = IndexCounter;
                }
                IndexCounter++;
            }
            if (IndexLastFound != -1)
            {
                return Obj3D.Polygons[IndexLastFound].Key;
            }
            return null;
        }

        public Polygon ReferencerPoly;

        public delegate void ClickEvent(object sender, ClickEventHandler e);
        public event ClickEvent DataReceived;
    }


    public class ClickEventHandler : EventArgs
    {
        Polygon ClickedPoly;
    }
}
    