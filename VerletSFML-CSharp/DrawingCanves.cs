using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using VerletSFML_CSharp.Physics;

namespace VerletSFML_CSharp
{
    public class DrawingCanves : Canvas
    {
        public PhysicSolver? PhysicSolver;

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (PhysicSolver == null) return;

            for (int i = 0; i < PhysicSolver.ObjectsCount; i++)
            {
                var obj = PhysicSolver[i];
                dc.DrawEllipse(new SolidColorBrush { Color = obj.Color }, null, new(obj.Position.X, obj.Position.Y), 1, 1);
            }
        }
    }
}
