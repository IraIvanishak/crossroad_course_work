using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using static Crossroad.RoadSizes;
using System.Collections.Generic;
using System.Windows.Media;
using System.Diagnostics;

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
        public static bool GenerationStarted { set; get; } = false;
        public static bool Reset { set; get; } = false;



        public static double CarPeriod { set; get; } = 0;
        public static double PedestrianPeriod { set; get; } = 0;
        public static Timer GoTimer { set; get; } = new();
        public static Timer CarTimer { set; get; } = new();
        public static Timer PedestrianTimer { set; get; } = new();


        public static void Go()
        {
            GoTimer.Interval = UPDATE_TIME; 
            if(!Reset) 
            GoTimer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                   // Debug.WriteLine("go timer " + DateTime.Now.Second);
                    var workPedestrians = CrosswalkSet
                        .Where(cs => cs.LocAxis == Axis);

                    foreach (var p in workPedestrians)
                        if (p.Pedestrians.Count != 0) p.Go();

                    var workCars = Cars
                        .Where(c =>
                            ((int)c.RoadPart % 2 == (int)Axis)
                            && (c.QueueIndex == 0))
                        .ToList();

                    foreach (var c in workCars)
                    {
                        c.PrepareToMove();
                    }
                });
            };
            GoTimer.Start();
        }
        public static void UpdateCars()
        {
            Car.EndPoint = new uint[ROADS_COUNT, MAX_LANE_COUNT];           
            foreach (Car car in Cars)
            {
                LanesSet[(int)car.RoadPart].Children.Remove(car.View);
                car.Offset = 0;
                car.TransformGroup.Children.Clear();
                car.LocateOnRoad();
            }
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
            var car = new Car((RoadParts)random.Next(ROADS_COUNT),
                (Directions)random.Next(DIRECTIONS_COUNT));
            Cars.Add(car);

            CarTimer.Interval = CarPeriod;

            if (CarTimer.Enabled)
            {
                CarTimer.Stop();
                PedestrianTimer.Stop();
            }

            if (!GenerationStarted) 
            CarTimer.Elapsed += (s,e) =>
            {

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var activeCars = Car.InMovement.Where(c =>
                        c.Direction == Directions.OnLeft);

                    var availableRoads = new List<RoadParts>()
                            { RoadParts.South,  RoadParts.West, RoadParts.North, RoadParts.East };

                    //if (activeCars is not null)
                    //{
                    //    foreach (var activeCar in activeCars)
                    //    {
                    //        availableRoads.Remove(GetOppositeRoad((int)activeCar.RoadPart));
                    //    }
                    //}

                    var r = random.Next(availableRoads.Count);
                    var car = new Car(availableRoads[r],
                        (Directions)random.Next(DIRECTIONS_COUNT));

                    Cars.Add(car);

                });

            };
           
            CarTimer.Start();

            PedestrianTimer.Interval = PedestrianPeriod;

            if(!GenerationStarted) 
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
                            CrosswalkSet[availableRoads[r]].AddPedestrian(availableDirections[d]);
                    }
                });
            };

            PedestrianTimer.Start();
        }
    }
}