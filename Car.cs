using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Crossroad.RoadSizes;
using System.Diagnostics;

namespace Crossroad
{
    public class Car
    {
        public Car(RoadParts roadPart, Directions carDirection)
        {
            RoadPart = roadPart;
            Direction = carDirection;

            var random = new Random();
            int i = random.Next(DIRECTIONS_COUNT);

            string str ="car";
            switch (i)
            {
                case 0: str += "Blue"; break;
                case 1: str += "Green"; break;
                case 2: str += "Pink"; break;
            }

            switch (carDirection)
            {
                case Directions.OnLeft: str += "L"; break;
                case Directions.OnRight: str += "R"; break;
            }

            View.Fill = new ImageBrush(new BitmapImage(new
            Uri("C:\\Users\\user\\Desktop\\Crossroad\\Resources\\" + str + ".png")));

            View.RenderTransform = TransformGroup;
            LocateOnRoad();

        }

        public Rectangle View { get; set; } = new();
        public TransformGroup TransformGroup { get; set; } = new();

        public RoadParts RoadPart { get; set; } = 0;
        public Directions Direction { get; set; } = 0;
        public uint QueueIndex { get; set; } = 0;
        public uint InLane { get; set; } = 0;
        public double Offset { get; set; } = 0;
        public bool InDelay { get; set; } = false;
        public int PedestrianDelay { get; set; } = 0;

        public void TurnLeft()
        {

            var tt = new TranslateTransform();
            TransformGroup.Children.Add(tt);

            var destination = -1 * GEWAY_ONE_LANE_WIDTH;
            var animaton = new DoubleAnimation(0, destination,
                TimeSpan.FromSeconds(Road.Lane * SHORT_SHORT_DUR));

            animaton.Completed += (s, e) => {

                var lastTT = new TranslateTransform();
                TransformGroup.Children.Add(lastTT);

                bool stopBeforeCrosswalk = !Road.CrosswalkSet[(int)Road.GetFutureRoad((int)Direction, (int)RoadPart)].IsFree;
                TimeSpan? time = TimeSpan.Zero;
                if (stopBeforeCrosswalk) time = TimeSpan.FromMilliseconds(PEDESTRIAN_TIME);

                var lastRoadPart =  - 1 * (FILD / 2 + ROAD_CENTRE + MARGIN_SMAL);
                var lastPartAnimation = new DoubleAnimation(0, lastRoadPart, TimeSpan.FromSeconds(LONG_DUR*Road.Lane));
                lastPartAnimation.BeginTime = time;
                lastTT.BeginAnimation(TranslateTransform.XProperty, lastPartAnimation);

                var anStarted = new Timer();
                anStarted.Elapsed += ( s, e ) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SetQueue();
                        anStarted.Stop();
                    });
                };

                anStarted.Interval = time.Value.TotalMilliseconds+1;
                anStarted.Start();
            };

            tt.BeginAnimation(TranslateTransform.XProperty, animaton);
        }

        public void TurnRight()
        {
            bool stopBeforeCrosswalk = !Road.CrosswalkSet[(int)Road
                .GetFutureRoad((int)Direction, (int)RoadPart)].IsFree;

            TimeSpan? time = TimeSpan.Zero;
            if (stopBeforeCrosswalk)
            {
                var movingCar = InMovement
                    .Where(c => c.RoadPart == Road.GetOppositeRoad((int)RoadPart))
                    .FirstOrDefault();
                if (movingCar is not null)
                    movingCar.PedestrianDelay++;

                time = TimeSpan.FromMilliseconds(PEDESTRIAN_TIME);
            }

            var TT = new TranslateTransform();
            TransformGroup.Children.Add(TT);

            var lastRoadPart =  (FILD / 2 + ROAD_CENTRE + MARGIN_SMAL);
            var animation = new DoubleAnimation(0, lastRoadPart, TimeSpan.FromSeconds(Road.Lane * SHORT_DUR));
            animation.BeginTime = time;

            TT.BeginAnimation(TranslateTransform.XProperty, animation);
            var anStarted = new Timer();
            anStarted.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SetQueue();
                    anStarted.Stop();
                });
            };
            anStarted.Interval = time.Value.TotalMilliseconds+1;
            anStarted.Start();
        }

        public void Move()
        {
            InMovement.Add(this);
            var currentLaneCars = Road.Cars
               .Where(a =>
               a.RoadPart == RoadPart
               && a.InLane == InLane);

            foreach (var a in currentLaneCars)
                a.GetCloser();

           // view.Fill = new SolidColorBrush(Colors.Red);

            if (Direction == Directions.Straight)
            {
                var transformation = new TranslateTransform();
                TransformGroup.Children.Add(transformation);
                var destination = -1 * FILD;
                var animation = new DoubleAnimation(0, destination, TimeSpan.FromSeconds(Road.Lane * LONG_LONG_DUR));
                transformation.BeginAnimation(TranslateTransform.YProperty, animation);
                SetQueue();
            }

            else
            {
                var transformation = new TranslateTransform();
                TransformGroup.Children.Add(transformation);

                double destination = CROSSWALK_WIDTH
                    + Road.LaneWidth
                    + (1 - CAR_WIDTH_COEF) * View.Height;

                if (Direction == 0) 
                    destination += (ROAD_WIDTH - Road.LaneWidth);

                destination *= -1;

                Duration duration = TimeSpan.FromSeconds(Direction == 0 ? LONG_DUR : SHORT_DUR);
                var animation = new DoubleAnimation(0, destination, duration);

                animation.Completed += (s, e) =>
                {

                    InDelay = true;
                    double y = destination - Offset + View.Height / 2;
                    double x = View.Width / 2;

                    var rotation = new RotateTransform(0, x, y);
                    TransformGroup.Children.Add(rotation);

                    var animationForRotation = new DoubleAnimation(0, ((int)Direction - 1) * 90, TimeSpan.FromSeconds(SHORT_DUR));
                    animationForRotation.Completed += (s, e) =>
                    {
                        if (Direction == Directions.OnRight) TurnRight();
                        else if (Direction == Directions.OnLeft) TurnLeft();
                    };

                    if (Direction == Directions.OnRight)
                    {
                        rotation.BeginAnimation(RotateTransform.AngleProperty, animationForRotation);
                    }
                    else
                    {

                        var continueToMove = new Timer();
                        continueToMove.Elapsed += (s, e) =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                continueToMove.Stop();
                                InDangerZone.Add(this);
                                animationForRotation.BeginTime = TimeSpan.FromSeconds(PedestrianDelay/Road.Lane);
                                rotation.BeginAnimation(RotateTransform.AngleProperty, animationForRotation);

                            });
                        };
                        continueToMove.Interval = 200;

                        double delayTime = 1;
                        var anStarted = new Timer();
                        anStarted.Elapsed += (s,e) =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                delayTime = CarsToSkip() * TIME_UNIT;
                                uint toNextYellow = (uint)TrafficLight.GetRemainigTime();

                                if (delayTime == 0 || Road.Axis == Axes.Undef)
                                {
                                    anStarted.Stop();
                                    continueToMove.Start();
                                }
                                else
                                {
                                    if (delayTime > toNextYellow)
                                        delayTime = toNextYellow;

                                    SetCarsInDelay();
                                    anStarted.Interval = delayTime;
                                }                                
                            });
                        };
                        anStarted.Interval = delayTime;
                        anStarted.Start();              
                    }
                };
                transformation.BeginAnimation(TranslateTransform.YProperty, animation);
            }          
        }

        public void PrepareToMove()
        {
            if(Road.Cars.Contains(this)) Road.Cars.Remove(this);
            var movingCar = InMovement
                .Where(c => c.RoadPart == Road.GetOppositeRoad((int)RoadPart)&&c.InDelay)
                .FirstOrDefault();



            var checkIfRoadFree = new Timer();
            checkIfRoadFree.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    checkIfRoadFree.Enabled = false;
                    if (Road.Axis == Axes.Undef)
                    {
                        Road.Cars.Add(this);
                        return;
                    }
                    else Move();
                    Debug.WriteLine("Start move");
                });
            };

            if (movingCar is not null)
            {
                Debug.WriteLine("is not null");
                if (InDangerZone.Contains(movingCar))
                {
                    Debug.WriteLine("Danger zone");
                    checkIfRoadFree.Interval = TIME_UNIT;
                    checkIfRoadFree.Start();
                    return;
                }
                if (Direction == Directions.OnLeft)
                {
                    checkIfRoadFree.Interval = Road.Lane == MAX_LANE_COUNT ? 1 : 1500;
                    checkIfRoadFree.Start();
                    return;
                }
                else Move();
            }
            else Move();

        }

        public void SetCarsInDelay()
        {
            var restCars = Road.Cars
                .Where(a => !a.InDelay
                && a.RoadPart == RoadPart 
                && a.InLane == InLane);

            foreach(var car in restCars)
                car.InDelay = true;
        }

        public void GetCloser()
        {
            Offset += View.Height;

            var transformation = new TranslateTransform();
            TransformGroup.Children.Add(transformation);

            var destination = -1 * View.Height;
            var animation = new DoubleAnimation(0, destination, TimeSpan.FromSeconds(SHORT_DUR));
            transformation.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        public void SetQueue()
        {
            EndPoint[(int)RoadPart, InLane]--;
            InMovement.Remove(this);
           // view.Fill = new SolidColorBrush(Colors.Green);

            Road.Cars
                   .Where(a => a.RoadPart == RoadPart 
                        && a.InLane == InLane)
                   .ToList().ForEach(a =>
                   {
                       a.QueueIndex--;
                       a.InDelay = false;
                   });

            if(InDangerZone.Contains(this)) InDangerZone.Remove(this);

        }

        public void LocateOnRoad() {

            View.Width = CAR_WIDTH_COEF * Road.LaneWidth;
            View.Height = CAR_HEIGHT_COEF * Road.LaneWidth;

            double xDelta = (1 - CAR_WIDTH_COEF) * Road.LaneWidth / 2;

            if ((Direction == 0 ) && Road.Lane == 2)
            {
                InLane = 1;
                xDelta += xDelta + Road.LaneWidth;
            }


            double yDelta = EndPoint[(int)RoadPart, InLane] * View.Height + CROSSWALK_WIDTH;

            Canvas.SetRight(View, xDelta);
            Canvas.SetTop(View, yDelta);

            Road.LanesSet[(int)RoadPart].Children.Add(View);

            QueueIndex = EndPoint[(int)RoadPart, InLane]; 
            EndPoint[(uint)RoadPart,InLane]++;

            var movingParent = InMovement.FirstOrDefault(c => c.RoadPart == RoadPart && c.InLane == InLane);
            if (movingParent != null)
            {
                GetCloser();
                if (movingParent.Direction == Directions.OnLeft) InDelay = true;
            }
            else
            {
                var parent = Road.Cars
                    .FirstOrDefault(c => 
                        c.InLane == InLane 
                        && c.RoadPart == RoadPart 
                        && c.QueueIndex == QueueIndex - 1
                        );

                if (parent != null && parent.InDelay) InDelay = true;
            }
        }

        public double CarsToSkip()
        {
            var delayCars = new List<Car>();

            if (Road.Lane == 1)
            {
                var laneCars = Road.Cars.Where(a =>
                    a.InDelay == false 
                    && (a.RoadPart == Road.GetOppositeRoad((int)RoadPart)));

                foreach (var car in laneCars)
                {
                    if (car.Direction == Directions.OnLeft) break;
                    else
                    {
                        delayCars.Add(car);
                    }
                }
            }

            else
            {
                delayCars = Road.Cars
                    .Where(a =>
                        (a.RoadPart == Road.GetOppositeRoad((int)RoadPart)
                        && a.InLane==0))
                   .ToList();
            }

            double time = 0;
            foreach (var c in delayCars)
            {
                if (c.Direction == Directions.OnRight) 
                    time += ON_RIGHT_DUR;
                else time+= STREIGHT_DUR;
            }
            if (Road.Lane == MAX_LANE_COUNT)
                time *= LANE_SWITCH_COEF;
            //time += PedestrianDelay;
            //PedestrianDelay = 0;

            return time;
        }

        public static uint[,] EndPoint { get; set; } = new uint[ROADS_COUNT, MAX_LANE_COUNT] ;
        public static ObservableCollection<Car> InMovement { set; get; } = new();
        public static ObservableCollection<Car> InDangerZone { set; get; } = new();

        
    }
}
