using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private readonly PhysicSolver solver;
        private readonly DispatcherTimer timerUpdate = new(DispatcherPriority.Send);

        public MainWindow()
        {
            InitializeComponent();

            worldSize = new(300, 300);
            solver = new(worldSize);

            InitUpdateUI_RenderImage();

            timerUpdate.Interval = TimeSpan.FromSeconds(dtUpdate);
            timerUpdate.Tick += TimerUpdate_Tick;
        }

        Task? taskUpdateSolver;
        Task? taskUpdateRenderImage;
        const float dtUpdate = 1.0f / 60;


        bool updateRunning = false;
        private async void TimerUpdate_Tick(object? sender, EventArgs e)
        {
            if (updateRunning) return;

            updateRunning = true;

            await Task.Run(UpdateSolver);
            await Task.Run(UpdateRenderImage);

            UpdateWriteableBitmap();
            UpdateTitle();

            updateRunning = false;
        }

        readonly Stopwatch swUpdateSolver = new();
        float ballNumber = 1;
        private void UpdateSolver()
        {
            if (ballNumber < 10)
                ballNumber += dtUpdate * 0.5f;

            if (solver.ObjectsCount < 9_0000)
            {
                for (int i = (int)ballNumber; i > 0; i--)
                {
                    var id = solver.CreateObject(new(2.0f, 10.0f + 1.1f * i));
                    solver[id].AddVelocity(Vector2.UnitX * 0.2f);
                    solver[id].Color = ColorUtils.GetRainbow(id * 0.0001f);

                    id = solver.CreateObject(new(298.0f, 10.0f + 1.1f * i));
                    solver[id].AddVelocity(Vector2.UnitX * -0.2f);
                    solver[id].Color = ColorUtils.GetRainbow(id * 0.0001f);
                }
            }
            swUpdateSolver.Restart();
            solver.Update(dtUpdate);
            swUpdateSolver.Stop();
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
        ImageData buffer;

        [InlineArray(300 * RATIO * 300 * RATIO)]
        private struct ImageData
        {
#pragma warning disable IDE0051 // 删除未使用的私有成员
#pragma warning disable IDE0044 // 添加只读修饰符
            Pixel24 _elememt;
#pragma warning restore IDE0044 // 添加只读修饰符
#pragma warning restore IDE0051 // 删除未使用的私有成员
        }

        private void InitUpdateUI_RenderImage()
        {
            wbDirtyRect = new(0, 0, 300 * RATIO, 300 * RATIO);
            writeableBitmap = new WriteableBitmap(300 * RATIO, 300 * RATIO, 96, 96, PixelFormats.Bgr24, null);
            RenderImage.Source = writeableBitmap;
        }

        readonly Stopwatch swUpdateBitmap = new();
        private void UpdateRenderImage()
        {
            swUpdateBitmap.Restart();

            Span<Pixel24> bufferSpan = buffer;
            bufferSpan.Clear();

            var partitionerSize = (solver.ObjectsCount + Environment.ProcessorCount - 1) / Environment.ProcessorCount;
            var partitioner = Partitioner.Create(0, solver.ObjectsCount, partitionerSize);

            Parallel.ForEach(partitioner, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Span<Pixel24> span = buffer;
                    ref var obj = ref solver[i];
                    span.FillCircle(300 * RATIO, obj.Position.X * RATIO, obj.Position.Y * RATIO, 0.5f * RATIO, obj.Color);
                }
            });

            swUpdateBitmap.Stop();
        }

        readonly Stopwatch swUpdateRender = new();
        private unsafe void UpdateWriteableBitmap()
        {
            Debug.Assert(writeableBitmap != null);

            swUpdateRender.Restart();                 

            writeableBitmap.Lock();
            var pBuffer = Unsafe.AsPointer(ref buffer);
            writeableBitmap.WritePixels(wbDirtyRect, new IntPtr(pBuffer), sizeof(ImageData), 3 * 300 * RATIO);
            writeableBitmap.AddDirtyRect(wbDirtyRect);
            writeableBitmap.Unlock();

            swUpdateRender.Stop();
        }

        public void UpdateTitle()
        {
            txtBall.Text = $"{solver.ObjectsCount}";
            txtSolver.Text = $"{(int)swUpdateSolver.Elapsed.TotalMicroseconds}us";
            txtBitmap.Text = $"{(int)swUpdateBitmap.Elapsed.TotalMicroseconds}us";
            txtRender.Text = $"{(int)swUpdateRender.Elapsed.TotalMicroseconds}us";
        }


        DateTime lastButtonDown = DateTime.MinValue;
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var now = DateTime.Now;
            var dt = now - lastButtonDown;
            lastButtonDown = now;
            if (dt.TotalMilliseconds > 300)
                return;

            timerUpdate.IsEnabled = !timerUpdate.IsEnabled;
        }

    }
}
