using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
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
        Stopwatch swUI = new();

        public async void MainLoop()
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

                emit = solver.ObjectsCount < 100 ||
                    swSolver.Elapsed < TimeSpan.FromSeconds(dt)*2;

                swUI.Restart();
                Dispatcher.Invoke(UpdateUI);
                swUI.Stop();
                Dispatcher.Invoke(UpdateTitle);

                var waitTime = dt - swSolver.Elapsed.TotalSeconds - swUI.Elapsed.TotalSeconds;
                if (waitTime > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(waitTime));
                }
            }
        }

        Dictionary<int, UIElement> ObjectsUI = new();
        Dictionary<int, Vector2> ObjectsPos = new();
        float sizeRatio = 2;

        public void UpdateUI()
        {
            renderCanvas.InvalidateVisual();
        }
        public void UpdateUIByUIElement()
        {
            for (int id = 0; id < solver.ObjectsCount; id++)
            {
                if (!ObjectsUI.ContainsKey(id))
                {
                    var renderObject = new Ellipse
                    {
                        Fill = new SolidColorBrush { Color = solver[id].Color },
                        Width = sizeRatio * 2,
                        Height = sizeRatio * 2,
                    };
                    ObjectsUI.Add(id, renderObject);
                    ObjectsPos.Add(id, solver[id].Position);

                    renderCanvas.Children.Add(renderObject);
                    Canvas.SetLeft(ObjectsUI[id], solver[id].Position.X * sizeRatio);
                    Canvas.SetTop(ObjectsUI[id], solver[id].Position.Y * sizeRatio);
                }

                if (Vector2.DistanceSquared(ObjectsPos[id], solver[id].Position) > 1e-2)
                {
                    Canvas.SetLeft(ObjectsUI[id], solver[id].Position.X * sizeRatio);
                    Canvas.SetTop(ObjectsUI[id], solver[id].Position.Y * sizeRatio);
                }
            }
        }

        public void UpdateTitle()
        {
            Title = $"{solver.ObjectsCount} Solver:{(int)swSolver.Elapsed.TotalMicroseconds:10}us UI:{(int)swUI.Elapsed.TotalMicroseconds:10}us";
        }
    }
}
