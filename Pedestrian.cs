using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Crossroad.RoadSizes;


namespace Crossroad
{
    public class Pedestrian
    {
        public Pedestrian(RoadParts roadPart, PedestrianDirections dir)
        {
            Direction = dir;
            RoadPart = roadPart;

            var road = Road.GetRoad();

            View = new Rectangle()
            {
                Width = UNIT_SIZE/(road.Lane),
                Height = CROSSWALK_ZEBRA_WIDTH/(road.Lane),
                Fill = new ImageBrush(new BitmapImage(new
                Uri("Images/pedestrianManStat.png", UriKind.Relative)))
            };
            View.RenderTransform = TransformGroup;
            if (Direction == PedestrianDirections.Backward)
            {
                Canvas.SetLeft(View, -1*MARGIN_BIG);
                var r = new RotateTransform(180, UNIT_SIZE/2, CROSSWALK_ZEBRA_WIDTH/2);
                TransformGroup.Children.Add(r);
            }
            else Canvas.SetRight(View, -1 * MARGIN_BIG);
            road.CrosswalkSet[(int)roadPart].CrosswalkFild.Children.Add(View);
        }

        private bool CurrA { get; set; } = false;
        private Timer GoTimer { get; set; } = new();
        public PedestrianDirections Direction { get; set; } = PedestrianDirections.Forward;
        public TransformGroup TransformGroup { get; set; } = new();
        public Rectangle View { get; set; } = new();

        public RoadParts RoadPart { get; set; } = 0;
        public void Move()
        {
            var t = new TranslateTransform();
            TransformGroup.Children.Add(t);

            int dest = -1*(int)Direction*FILD;
            var db = new DoubleAnimation(0, dest, TimeSpan.FromMilliseconds(PEDESTRIAN_DURATION));

            CurrA = true;
            AnimateMovement();

            db.Completed += (s, o) => { GoTimer.Stop(); };
            t.BeginAnimation(TranslateTransform.XProperty, db);
        }


        public void AnimateMovement()
        {
            var road = Road.GetRoad();

            GoTimer.Interval = EXTRA_TIME;
            GoTimer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {                    
                    CurrA = !CurrA;
                    View.Fill = new ImageBrush(new BitmapImage(new
                    Uri("Images/" + "pedestrianManDyn" 
                        + Convert.ToByte(CurrA).ToString() + 
                        ".png" , UriKind.Relative)));
                });

            };
            GoTimer.Start();

            if (Direction == PedestrianDirections.Backward)
                road.CrosswalkSet[(int)RoadPart].IsFree = false;

            bool ready = false;

            Timer free = new();
            free.Interval = TIME_UNIT;
            free.Elapsed += (s,o) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Direction == PedestrianDirections.Backward||ready)
                    {
                        road.CrosswalkSet[(int)RoadPart].IsFree = true;
                        free.Stop();
                    }
                    else
                    {
                        ready = true;
                        road.CrosswalkSet[(int)RoadPart].IsFree = false;
                    }

                });
            };            
            free.Start();
        }
    }
}
