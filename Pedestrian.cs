using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Crossroad
{
    public class Pedestrian
    {
        public Pedestrian(RoadParts road, PedestrianDirections dir)
        {
            Direction = dir;
            RoadPart = road;
            view = new Rectangle()
            {
                Width = RoadSizes.UNIT_SIZE/(Road.Lane),
                Height = RoadSizes.CROSSWALK_ZEBRA_WIDTH/(Road.Lane),
                Fill = new ImageBrush(new BitmapImage(new
                Uri("C:\\Users\\user\\Desktop\\Crossroad\\Resources\\"+"pedestrianManStat"+ ".png")))
            };
            view.RenderTransform = transformGroup;
            if (Direction == PedestrianDirections.Backward)
            {
                Canvas.SetLeft(view, -1*RoadSizes.MARGIN_BIG);
                var r = new RotateTransform(180, RoadSizes.UNIT_SIZE/2, RoadSizes.CROSSWALK_ZEBRA_WIDTH/2);
                transformGroup.Children.Add(r);
            }
            else Canvas.SetRight(view, -1 * RoadSizes.MARGIN_BIG);
            Road.CrosswalkSet[(int)road].CrosswalkFild.Children.Add(view);
        }

        private bool currA { get; set; } = false;
        private Timer goTimer { get; set; } = new();
        public PedestrianDirections Direction { get; set; } = PedestrianDirections.Forward;
        public TransformGroup transformGroup { get; set; } = new();
        public Rectangle view { get; set; } = new();

        public RoadParts RoadPart { get; set; } = 0;
        public void move()
        {
            var t = new TranslateTransform();
            transformGroup.Children.Add(t);
            int dest = -1*(int)Direction*RoadSizes.FILD;

            var db = new DoubleAnimation(0, dest, TimeSpan.FromSeconds(5));
            currA = true;
            animateMovement();

            db.Completed += endAnimation;
            t.BeginAnimation(TranslateTransform.XProperty, db);
        }

        private void endAnimation(object? sender, EventArgs e)
        {
            goTimer.Stop();
        }

        public void animateMovement()
        {
            goTimer.Interval = 200;
            goTimer.Elapsed += new ElapsedEventHandler((object? source, ElapsedEventArgs e) =>
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    currA = !currA;
                    view.Fill = new ImageBrush(new BitmapImage(new
                    Uri("C:\\Users\\user\\Desktop\\Crossroad\\Resources\\" + "pedestrianManDyn" + Convert.ToByte(currA).ToString() + ".png")));

                }));

            });
            goTimer.Start();
            if (Direction == PedestrianDirections.Backward) Road.CrosswalkSet[(int)RoadPart].IsFree = false;
            bool ready = false;

            System.Timers.Timer free = new();
            free.Interval = 900;
            free.Elapsed += new ElapsedEventHandler((object? source, ElapsedEventArgs e) =>
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    if (Direction == PedestrianDirections.Backward||ready)
                    {
                        Road.CrosswalkSet[(int)RoadPart].IsFree = true;
                        free.Stop();
                    }
                    else
                    {
                        ready = true;
                        Road.CrosswalkSet[(int)RoadPart].IsFree = false;
                    }

                }));

            });
            
            free.Start();

        }


    }



}
