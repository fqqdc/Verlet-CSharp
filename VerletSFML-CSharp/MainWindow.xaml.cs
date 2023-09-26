using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VerletSFML_CSharp.Engine.Common;
using VerletSFML_CSharp.Physics;

namespace VerletSFML_CSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Vector2 worldSize;
        private PhysicSolver solver;

        public MainWindow()
        {
            InitializeComponent();

            worldSize = new(300, 300);
            solver = new(worldSize);
            renderCanvas.PhysicSolver = solver; ;
            mainTask = Task.Run(MainLoop);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (mainTask != null && mainTask.Status == TaskStatus.Running)
            {
                IsMainLoopRunning = false;
                e.Cancel = true;
            }
        }


        bool IsMainLoopRunning = true;
        bool emit = true;
        int fpsCap = 60;


        Task? mainTask;
        Stopwatch swSolver = new();

        public void MainLoop()
        {
            float dt = 1.0f / fpsCap;

            while (IsMainLoopRunning)
            {
                swSolver.Restart();
                if (solver.ObjectsCount < 80000 && emit)
                {
                    for (int i = 20; i > 0; i--)
                    {
                        var id = solver.CreateObject(new(2.0f, 10.0f + 1.1f * i));
                        solver[id].AddVelocity(Vector2.UnitX * 0.2f);
                        solver[id].Color = ColorUtils.GetRainbow(id * 0.0001f);
                    }
                }

                solver.Update(dt);
                swSolver.Stop();

                Dispatcher.Invoke(UpdateUI);
                Dispatcher.Invoke(UpdateTitle);

                var waitTime = dt - swSolver.Elapsed.TotalSeconds;
                if (waitTime > 0)
                    Thread.Sleep(TimeSpan.FromSeconds(waitTime));
            }
        }

        public void UpdateUI()
        {
            renderCanvas.InvalidateVisual();
        }

        TimeSpan maxSolver = TimeSpan.Zero;
        TimeSpan maxRender = TimeSpan.Zero;
        public void UpdateTitle()
        {
            var tsSolver = swSolver.Elapsed;
            if (tsSolver > maxSolver) maxSolver = tsSolver;
            var tsRender = renderCanvas.TimeSpanRender;
            if (tsRender > maxRender) maxRender = tsRender;

            StringBuilder sbTitle = new();
            sbTitle.Append($"{solver.ObjectsCount,10}");
            sbTitle.Append($" | Solver:{(int)tsSolver.TotalMicroseconds,10}us({(int)maxSolver.TotalMicroseconds,10}us)");
            sbTitle.Append($" | UI:{(int)tsRender.TotalMicroseconds,10}us({(int)maxRender.TotalMicroseconds,10}us)");
            Title = sbTitle.ToString();
        }
    }
}
