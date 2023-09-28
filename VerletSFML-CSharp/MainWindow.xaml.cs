using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Verlet_CSharp.Engine.Common;
using Verlet_CSharp.Physics;

namespace Verlet_CSharp
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

            InitUpdateUI_RenderImage();

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
        Stopwatch swRender = new();

        public void MainLoop()
        {
            float dt = 1.0f / fpsCap;

            while (IsMainLoopRunning)
            {
                swSolver.Restart();
                if (solver.ObjectsCount < 80000 && emit)
                {
                    for (int i = 10; i > 0; i--)
                    {
                        var id = solver.CreateObject(new(2.0f, 10.0f + 1.1f * i));
                        solver[id].AddVelocity(Vector2.UnitX * 0.2f);
                        solver[id].Color = ColorUtils.GetRainbow(id * 0.0001f);

                        id = solver.CreateObject(new(298.0f, 10.0f + 1.1f * i));
                        solver[id].AddVelocity(Vector2.UnitX * -0.2f);
                        solver[id].Color = ColorUtils.GetRainbow(id * 0.0001f);
                    }
                }

                solver.Update(dt);
                swSolver.Stop();

                Dispatcher.Invoke(UpdateUI_RenderImage);
                Dispatcher.Invoke(UpdateTitle);

                var waitTime = dt - swSolver.Elapsed.TotalSeconds;
                if (waitTime > 0)
                    Thread.Sleep(TimeSpan.FromSeconds(waitTime));
            }
        }
        const int RATIO = 6;

        //RenderTargetBitmap bitmap = new RenderTargetBitmap(
        //        300 * RATIO, 300 * RATIO, 72, 72, PixelFormats.Default);
        //public void UpdateUI_RenderImage()
        //{
        //    swRender.Restart();
        //    DrawingVisual drawingVisual = new DrawingVisual();
        //    using (DrawingContext dc = drawingVisual.RenderOpen())
        //    {
        //        dc.DrawRectangle(System.Windows.Media.Brushes.White, null, new(0, 0, 300 * RATIO, 300 * RATIO));
        //        for (int i = 0; i < solver.ObjectsCount; i++)
        //        {
        //            ref var obj = ref solver[i];
        //            dc.DrawEllipse(new SolidColorBrush { Color = obj.Color }, null, new(obj.Position.X * RATIO, obj.Position.Y * RATIO), 1 * RATIO, 1 * RATIO);
        //        }
        //    }
        //    bitmap.Render(drawingVisual);
        //    swRender.Stop();
        //}


        //Bitmap backBitmap;
        //public void UpdateUI_RenderImage_2()
        //{
        //    swRender.Restart();
        //    wb.Lock();

        //    Graphics graphics = Graphics.FromImage(backBitmap);
        //    graphics.Clear(System.Drawing.Color.White);
        //    for (int i = 0; i < solver.ObjectsCount; i++)
        //    {
        //        ref var obj = ref solver[i];
        //        graphics.FillEllipse(new System.Drawing.SolidBrush(obj.Color),
        //            obj.Position.X * RATIO - RATIO * 0.5f, obj.Position.Y * RATIO - RATIO * 0.5f, 1 * RATIO, 1 * RATIO);
        //    }
        //    wb.AddDirtyRect(wbDirtyRect);

        //    wb.Unlock();
        //    swRender.Stop();
        //}

        Int32Rect wbDirtyRect = Int32Rect.Empty;
        WriteableBitmap? writeableBitmap;
        Pixel24[] imageData = Array.Empty<Pixel24>();
        private void InitUpdateUI_RenderImage()
        {
            imageData = new Pixel24[300 * RATIO * 300 * RATIO];
            wbDirtyRect = new(0, 0, 300 * RATIO, 300 * RATIO);
            writeableBitmap = new WriteableBitmap(300 * RATIO, 300 * RATIO, 96, 96, PixelFormats.Bgr24, null);
            RenderImage.Source = writeableBitmap;
        }

        private void UpdateUI_RenderImage()
        {
            Debug.Assert(writeableBitmap != null);

            swRender.Restart();
            writeableBitmap.Lock();

            var spanPixels = new Span<Pixel24>(imageData);
            var byteSpan = MemoryMarshal.Cast<Pixel24, byte>(spanPixels);
            byteSpan.Fill(255);

            var partitioner = Partitioner.Create(0, solver.ObjectsCount, solver.ObjectsCount / Environment.ProcessorCount + 1);
            Parallel.ForEach(partitioner, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    ref var obj = ref solver[i];
                    imageData.FillCircle(300 * RATIO, obj.Position.X * RATIO, obj.Position.Y * RATIO, 0.5f * RATIO, obj.Color);
                }
            });

            writeableBitmap.WritePixels(wbDirtyRect, imageData, 3 * 300 * RATIO, 0);
            writeableBitmap.AddDirtyRect(wbDirtyRect);

            writeableBitmap.Unlock();
            swRender.Stop();
        }

        TimeSpan maxSolver = TimeSpan.Zero;
        TimeSpan maxRender = TimeSpan.Zero;
        public void UpdateTitle()
        {
            var tsSolver = swSolver.Elapsed;
            if (tsSolver > maxSolver) maxSolver = tsSolver;
            //var tsRender = renderCanvas.TimeSpanRender;
            var tsRender = swRender.Elapsed;
            if (tsRender > maxRender) maxRender = tsRender;

            StringBuilder sbTitle = new();
            sbTitle.Append("VerletSFML-CSharp");
            sbTitle.Append($" | {solver.ObjectsCount,10}");
            sbTitle.Append($" | Solver:{(int)tsSolver.TotalMicroseconds,10}us({(int)maxSolver.TotalMicroseconds,10}us)");
            sbTitle.Append($" | UI:{(int)tsRender.TotalMicroseconds,10}us({(int)maxRender.TotalMicroseconds,10}us)");
            Title = sbTitle.ToString();
        }
    }
}
