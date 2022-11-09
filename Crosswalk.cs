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
            locAxis = (int)road % 2 == 0 ? Axes.Horizontal : Axes.Vertical;
            crosswalkFild = new()
            {
                Width = RoadSizes.ROAD_WIDTH,
                Height = RoadSizes.CROSSWALK_ZEBRA_WIDTH,
            };

        }

        public Canvas crosswalkFild { set; get; } = new();
        public Axes locAxis { set; get; } = Axes.Vertical;
        public RoadParts RoadPart { set; get; } = 0;
        public ObservableCollection<Pedestrian> Pedestrians { set; get; } = new();

        public void addPedestrian(object sender, RoutedEventArgs e)
        {
            var dir = Canvas.GetRight((sender as Button)) == 0 ? PedestrianDirections.Forward : PedestrianDirections.Backward;
            var pedastrian = new Pedestrian(RoadPart, dir);
            Pedestrians.Add(pedastrian);
        }

        public void addPedestrian(PedestrianDirections dir)
        {
            var pedastrian = new Pedestrian(RoadPart, dir);
            Pedestrians.Add(pedastrian);
        }
        public void go()
        {
            Pedestrians.ToList().ForEach(p =>
            {
                p.move();
                Pedestrians.Remove(p);
            });

        }
        public bool isFree { set; get; } = true;
    }
}