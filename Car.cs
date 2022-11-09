using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class Car
    {
        public Car(RoadParts roadPart, CarDirections carDirection)
        {
            RoadPart = roadPart;
            CarDirection = carDirection;

            var random = new Random();
            int i;
            do { 
                i = random.Next(3);
            }
            while (i == PrevColor);
            PrevColor = i;

            string str ="car";
            switch (i)
            {
                case 0: str += "Blue"; break;
                case 1: str += "Green"; break;
                case 2: str += "Pink"; break;
            }

            switch (carDirection)
            {
                case CarDirections.OnLeft: str += "L"; break;
                case CarDirections.OnRight: str += "R"; break;
            }

            color = str;

            view.Fill = new ImageBrush(new BitmapImage(new
            Uri("C:\\Users\\user\\Desktop\\Crossroad\\Resources\\"+str+".png")));

            view.RenderTransform = transformGroup;
            locateOnRoad();

        }

        public Rectangle view { get; set; } = new();
        public TransformGroup transformGroup { get; set; } = new();


        public RoadParts RoadPart { get; set; } = 0;
        public uint QueueIndex { get; set; } = 0;
        public CarDirections CarDirection { get; set; } = 0;
        public uint InLane { get; set; } = 0;
        public double Offset { get; set; } = 0;
        public bool InDelay { get; set; } = false;

        public void turnLeft()
        {
            var dest2 = -1 * GEWAY_ONE_LANE_WIDTH;
            var t2 = new TranslateTransform();
            transformGroup.Children.Add(t2);
            var db3 = new DoubleAnimation(0, dest2, TimeSpan.FromSeconds(Road.Lane == 2 ? SHORT_DUR : SHORT_SHORT_DUR));
            int count2 = 0;
            db3.Completed += (s, e) => {

                count2 = Road.CrosswalkSet[((int)CarDirection + (int)RoadPart + 1) % 4].isFree ? 0 : 1;
                TimeSpan? time = TimeSpan.FromSeconds(count2);
                var dest3 =  - 1 * (FILD / 2 + ROAD_CENTRE + MARGIN_SMAL);
                var t3 = new TranslateTransform();
                transformGroup.Children.Add(t3);

                var db4 = new DoubleAnimation(0, dest3, TimeSpan.FromSeconds(Road.Lane == 2 ? LONG_LONG_DUR : LONG_DUR));
                db4.BeginTime = time;

                t3.BeginAnimation(TranslateTransform.XProperty, db4);

                System.Timers.Timer anStarted = new();
                anStarted.Elapsed += new ElapsedEventHandler((object? source, ElapsedEventArgs e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        setQueue();
                        anStarted.Stop();

                    });

                });
                anStarted.Interval = time.Value.TotalMilliseconds+1;
                anStarted.Start();
            };

            t2.BeginAnimation(TranslateTransform.XProperty, db3);


        }

        public void turnRight()
        {
 
            int count2 = Road.CrosswalkSet[((int)CarDirection + (int)RoadPart + 1) % 4].isFree ? 0 : 1;             
            TimeSpan? time  = TimeSpan.FromSeconds(count2);

            var dest3 =  (FILD / 2 + ROAD_CENTRE + MARGIN_SMAL);
            var t3 = new TranslateTransform();
            transformGroup.Children.Add(t3);

            var db4 = new DoubleAnimation(0, dest3, TimeSpan.FromSeconds(Road.Lane == 2 ? LONG_DUR : SHORT_DUR));
            db4.BeginTime = time;

            t3.BeginAnimation(TranslateTransform.XProperty, db4);
            System.Timers.Timer anStarted = new();
            anStarted.Elapsed += new ElapsedEventHandler((object? source, ElapsedEventArgs e) =>
            {

                Application.Current.Dispatcher.Invoke(() =>
                {
                    setQueue();
                    anStarted.Stop();
                });

            });
            anStarted.Interval = time.Value.TotalMilliseconds+1;
            anStarted.Start();
        }

        public void move()
        {
            Road.Cars.Remove(this);
            InMovement.Add(this);
            if (CarDirection == CarDirections.Straight)
            {
                var t = new TranslateTransform();
                transformGroup.Children.Add(t);

                var dest = -1 * FILD;
                var db = new DoubleAnimation(0, dest, TimeSpan.FromSeconds(Road.Lane == 2 ? LONG_LONG_LONG_DUR : LONG_LONG_DUR));
                t.BeginAnimation(TranslateTransform.YProperty, db);
                setQueue();

            }
            else
            { 

                var t1 = new TranslateTransform();
                transformGroup.Children.Add(t1);
                double dest = CROSSWALK_WIDTH
                    + Road.LaneWidth
                    + (1 - CAR_WIDTH_COEF) * view.Height;

                if (CarDirection == 0) dest += (ROAD_WIDTH - Road.LaneWidth);
                dest *= -1;

                Duration duration = TimeSpan.FromSeconds(CarDirection == 0 ? LONG_DUR:SHORT_DUR);

                var db1 = new DoubleAnimation(0, dest, duration);

                db1.Completed += (s, e) =>
                {
                    double y = dest - Offset + view.Height / 2;
                    double x = view.Width / 2;

                    var r = new RotateTransform(0, x, y);
                    transformGroup.Children.Add(r);

                    var db2 = new DoubleAnimation(0, ((int)CarDirection - 1) * 90, TimeSpan.FromSeconds(SHORT_DUR));
                    db2.Completed += (s, e) =>
                    {
                        if (CarDirection == CarDirections.OnRight) turnRight();
                        else if (CarDirection == CarDirections.OnLeft) turnLeft();
                    };

                    if (CarDirection == CarDirections.OnRight)
                    {
                        r.BeginAnimation(RotateTransform.AngleProperty, db2);
                    }
                    else
                    {
                        double delayTime = 1;
                        Timer anStarted = new();
                        anStarted.Elapsed += new ElapsedEventHandler((object? source, ElapsedEventArgs e) =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {

                                delayTime = carsToSkip() * 1000;
                                if(delayTime!=0) setCarsInDelay();
                              //  Debug.WriteLine("delay" + delayTime);
                                if (delayTime == 0 || Road.Axis == Axes.Undef)
                                {
                                    anStarted.Stop();
                                    db2.BeginTime = TimeSpan.FromMilliseconds(delayTime);
                                    r.BeginAnimation(RotateTransform.AngleProperty, db2);
                                }
                                else
                                {
                                    int toNextYellow = InterfaceBuilder.GetRemainigTime();
                                    if (delayTime > toNextYellow)
                                    {
                                        anStarted.Stop();
                                        delayTime = toNextYellow + 500;
                                        db2.BeginTime = TimeSpan.FromMilliseconds(delayTime);
                                        r.BeginAnimation(RotateTransform.AngleProperty, db2);

                                    }
                                    anStarted.Interval = delayTime;
                                }
                                
                            });

                        });
                        anStarted.Interval = delayTime;
                        anStarted.Start();
                    }

                };

                t1.BeginAnimation(TranslateTransform.YProperty, db1);

            }
             

        }

        public void setCarsInDelay()
        {
            var restCars = Road.Cars.Where(a => !a.InDelay&& a.RoadPart == RoadPart && a.InLane == InLane);
            foreach(var car in restCars)
            {
                car.InDelay = true;
            }

        }
        public void getCloser()
        {
            Offset += view.Height;

            var t = new TranslateTransform();
            transformGroup.Children.Add(t);
            double dest = -1 * view.Height;

            DoubleAnimation db = new DoubleAnimation(0, dest, TimeSpan.FromSeconds(SHORT_DUR));
            t.BeginAnimation(TranslateTransform.YProperty, db);
        }

        public void setQueue()
        {
            EndPoint[(int)RoadPart, InLane]--;
            InMovement.Remove(this);
          //  Debug.WriteLine("get out");
            Road.Cars
                   .Where(a => a.RoadPart == RoadPart && a.InLane == InLane)
                   .ToList().ForEach(a =>
                   {
                       a.QueueIndex--;
                       a.InDelay = false;
                   });

        }

        public void locateOnRoad() {

            view.Width = CAR_WIDTH_COEF * Road.LaneWidth;
            view.Height = CAR_HEIGHT_COEF * Road.LaneWidth;

            double xDelta = (1 - CAR_WIDTH_COEF) * Road.LaneWidth / 2;

            if ((CarDirection == 0 ) && Road.Lane == 2)
            {
                InLane = 1;
                xDelta += xDelta + Road.LaneWidth;
            }
            double yDelta = EndPoint[(int)RoadPart, InLane]*view.Height + CROSSWALK_WIDTH;

            Canvas.SetRight(view, xDelta);
            Canvas.SetTop(view, yDelta);

            Road.LanesSet[(int)RoadPart].Children.Add(view);

            QueueIndex = EndPoint[(uint)RoadPart, InLane];
            EndPoint[(uint)RoadPart,InLane]++;

            var movingParent = InMovement.FirstOrDefault(c => c.RoadPart == RoadPart && c.InLane == InLane);
            if (movingParent != null)
            {
                getCloser();
                if (movingParent.CarDirection == CarDirections.OnLeft) InDelay = true;
            }
            else
            {
                var parent = Road.Cars.FirstOrDefault(c => c.InLane == InLane && c.RoadPart == RoadPart && c.QueueIndex == QueueIndex - 1);
                if (parent != null && parent.InDelay) InDelay = true;
            }


        }
        public double carsToSkip()
        {
            var delayCars = Road.Cars.Where(a => ((int)a.RoadPart % 2 == (int)Road.Axis) && a.InDelay == false && (a.RoadPart != RoadPart));
            if (delayCars.Count() == 0) return 0;
            double count = 0;
            foreach (var c in delayCars)
            {
                if (c.CarDirection == CarDirections.OnLeft) count += 2;
                else if (c.CarDirection == CarDirections.OnRight) count += 1.6;
                else count++;
            }
            if (Road.Lane == 2) count *= 0.7;
            return count;
        }
        private static int PrevColor { get; set; } = 0;
        public static uint[,] EndPoint { get; set; } = new uint[4,2] ;
        public static ObservableCollection<Car> InMovement { set; get; } = new();
    }
}
