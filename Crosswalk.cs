using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Crossroad
{
    public class Crosswalk
    {
        public Crosswalk(RoadParts road)
        {
            RoadPart = road;
            LocAxis = (int)road % 2 == 0 ? Axes.Horizontal : Axes.Vertical;
            CrosswalkFild = new()
            {
                Width = RoadSizes.ROAD_WIDTH,
                Height = RoadSizes.CROSSWALK_ZEBRA_WIDTH,
            };
        }

        public Canvas CrosswalkFild { set; get; } = new();
        public Axes LocAxis { set; get; } = Axes.Vertical;
        public RoadParts RoadPart { set; get; } = 0;
        public ObservableCollection<Pedestrian> Pedestrians { set; get; } = new();

        public void AddPedestrian(object sender, RoutedEventArgs e)
        {
            var dir = Canvas.GetRight((sender as Button)) == 0 ? 
                PedestrianDirections.Forward 
                : PedestrianDirections.Backward;

            var pedastrian = new Pedestrian(RoadPart, dir);
            Pedestrians.Add(pedastrian);
        }

        public void AddPedestrian(PedestrianDirections dir)
        {
            var pedastrian = new Pedestrian(RoadPart, dir);
            Pedestrians.Add(pedastrian);
        }
        public void Go()
        {
            Pedestrians
                .ToList()
                .ForEach(p =>
                {
                    p.Move();
                    Pedestrians.Remove(p);
                });
        }
        public bool IsFree { set; get; } = true;
    }
}