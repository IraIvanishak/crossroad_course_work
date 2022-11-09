using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using static Crossroad.RoadSizes;
using System.Collections.Generic;
using System.Windows.Media;

namespace Crossroad
{
    public static class Road
    {

        public static Canvas[] LanesSet { set; get; } = new Canvas[ROADS_COUNT];
        public static Crosswalk[] CrosswalkSet { set; get; } = new Crosswalk[ROADS_COUNT];
        public static TrafficLight[] LightsSet { set; get; } = new TrafficLight[ROADS_COUNT];
        public static ObservableCollection<Car> Cars { set; get; } = new();
        public static Axes Axis { set; get; } = Axes.Horizontal; 
        public static int Lane { set; get; } = 1;
        public static double LaneWidth { set; get; } = GEWAY_ONE_LANE_WIDTH;
        public static double TLTime { set; get; } = TRAFFIC_LIGHT_DEF_TIME;

        public static double CarPeriod { set; get; } = 0;
        public static double PedestrianPeriod { set; get; } = 0;
        public static Timer CarTimer { set; get; } = new();
        public static Timer PedestrianTimer { set; get; } = new();
        public static Timer[] TimersSet { set; get; } = new Timer[TIMERS_COUNT];
        public static int TimeFromLasReset { set; get; } = 0;

        public static void Go()
        {
            Timer GoTimer = new();
            GoTimer.Interval = UPDATE_TIME;
            GoTimer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var workPedestrians = CrosswalkSet
                        .Where(cs => cs.locAxis == Axis);

                    foreach (var p in workPedestrians)
                        if (p.Pedestrians.Count != 0) p.go();


                    var workCars = Cars
                        .Where(c =>
                            ((int)c.RoadPart % 2 == (int)Axis)
                            && (c.QueueIndex == 0))
                        .ToList();

                    foreach (var c in workCars)
                    {
                        c.move();
                        var currentLaneCars = Cars
                        .Where(a =>
                               a.RoadPart == c.RoadPart
                               && a.InLane == c.InLane);

                        foreach (var a in currentLaneCars)
                            a.getCloser();
                    }
                });
            };
            GoTimer.Start();

        }
        public static void UpdateCars()
        {
            Car.EndPoint = new uint[ROADS_COUNT, 2];           
            foreach (Car car in Cars)
            {
                LanesSet[(int)car.RoadPart].Children.Remove(car.view);
                car.transformGroup.Children.Clear();
                car.locateOnRoad();
            }
        }

        public static void BuildLightMode()
        {

            for (int i = 0; i < 4; i++)
                LightsSet[i].CurrentLight = Colors.Yellow;
            Axis = Axes.Undef;


            Timer startTimer = new();
            Timer swapTimer = new();
            swapTimer.Interval = TLTime;
            swapTimer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {

                    for (int i = 0; i < 4; i++)
                    {
                        LightsSet[i].swapColors();
                    }
                    if (LightsSet[0].CurrentLight == Colors.Green) Axis = Axes.Vertical; else Axis = Axes.Horizontal;
                    TimeFromLasReset = ((int)DateTime.Now.Ticks);

                }));

            };

            startTimer.Interval = YELLOW_TIME;
            startTimer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    TimeFromLasReset = ((int)DateTime.Now.Ticks);
                    swapTimer.Start();
                    for (int i = 0; i < 4; i++)
                    {
                        if (i % 2 == 0) LightsSet[i].CurrentLight = Colors.Red;
                        else LightsSet[i].CurrentLight = Colors.Green;
                    }
                    Axis = Axes.Horizontal;
                    startTimer.Enabled = false;
                }));


            };


            Timer yellowTimer = new();
            yellowTimer.Elapsed += (s, e) =>
            {

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    for (int i = 0; i < 4; i++)
                        LightsSet[i].CurrentLight = Colors.Yellow;
                    Axis = Axes.Undef;


                }));

            };
            yellowTimer.Interval = TLTime;
            yellowTimer.Start();
            startTimer.Start();

            TimersSet[0] = swapTimer;
            TimersSet[1] = startTimer;
            TimersSet[2] = yellowTimer;

        }
        public static int GetRemainigTime()
        {
            return (int)TLTime - YELLOW_TIME - (((int)DateTime.Now.Ticks) - TimeFromLasReset) / 10000;
        }
        public static RoadParts GetOppositeRoad(int r)
        {
            return (RoadParts)((r + 2) % ROADS_COUNT);
        }
        public static RoadParts GetFutureRoad(int d, int r)
        {
            return (RoadParts)((d + r + 1) % ROADS_COUNT);
        }
        public static void GenerateTraffic()
        {
            var random = new Random();
            var car = new Car((RoadParts)random.Next(ROADS_COUNT-1),
                (CarDirections)random.Next(DIRECTIONS_COUNT-1));
            Cars.Add(car);

            CarTimer.Interval = CarPeriod;
            CarTimer.Elapsed += (s,e) =>
            {

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var activeCars = Car.InMovement.Where(c =>
                        c.CarDirection == CarDirections.OnLeft);

                    var availableRoads = new List<RoadParts>()
                            { RoadParts.South,  RoadParts.West, RoadParts.North, RoadParts.East };

                    if (activeCars is not null)
                    {
                        foreach (var activeCar in activeCars)
                        {
                            availableRoads.Remove(GetOppositeRoad((int)activeCar.RoadPart));
                        }
                    }

                    var r = random.Next(availableRoads.Count);
                    var car = new Car(availableRoads[r],
                        (CarDirections)random.Next(DIRECTIONS_COUNT - 1));

                    Cars.Add(car);

                });

            };
           
            CarTimer.Start();

            PedestrianTimer.Interval = PedestrianPeriod;
            PedestrianTimer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var workPedestrians = CrosswalkSet
                         .SelectMany(cs => cs.Pedestrians)
                         .ToList();

                    var availableRoads = Enumerable.Range(0, ROADS_COUNT).ToList();
                    foreach (var road in CrosswalkSet)
                    {
                        if (workPedestrians
                            .Where(p => p.RoadPart == road.RoadPart)
                            .Count() == PEDESTRIAN_DIRECTIONS_COUNT)
                            availableRoads.Remove((int)road.RoadPart);
                    }

                    if (availableRoads.Count != 0)
                    {
                            var r = random.Next(availableRoads.Count);
                            var availableDirections = new List<PedestrianDirections>()
                            { PedestrianDirections.Forward, PedestrianDirections.Backward };

                            var pedestrian = workPedestrians.FirstOrDefault(p => (int)p.RoadPart == availableRoads[r]);
                            if (pedestrian is not null)
                                availableDirections.Remove(pedestrian.Direction);

                            var d = random.Next(availableDirections.Count);
                            CrosswalkSet[availableRoads[r]].addPedestrian(availableDirections[d]);
                    }
                });
            };

            PedestrianTimer.Start();
        }
    }
}