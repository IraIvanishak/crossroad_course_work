using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static Crossroad.Road;
using static Crossroad.RoadSizes;

namespace Crossroad
{
    public class InterfaceBuilder : IInterfaceBuilder
    {
        private MainWindow Form { set; get; } = (MainWindow)Application.Current.Windows[0];
        private Modes Mode { set; get; } = Modes.Manual;

        public InterfaceBuilder BuildCrosswalks()
        {
            for (int j = 0; j < 4; j++)
            {
                var crosswalk = new Crosswalk((RoadParts)j);
                Canvas.SetRight(crosswalk.CrosswalkFild, 0);
                LanesSet[j].Children.Add(crosswalk.CrosswalkFild);
                CrosswalkSet[j] = crosswalk;
            }

            return this;
        }
        public InterfaceBuilder BuildButtoms()
        {

            StackPanel[] bPanel = new StackPanel[4];
            Button[] bPedPanel = new Button[8];

            for (int j = 0; j < 360; j += 90)
            {
                var bListAddCar = new StackPanel();
                for (int i = 0; i < 3; i++)
                {
                    var bAddCar = new Button()
                    {
                        Width = GEWAY_ONE_LANE_WIDTH / 3,
                        Height = 30,
                        Tag = (i).ToString(),
                        Background = new SolidColorBrush(Colors.White),
                        BorderThickness = new Thickness(0),
                    };
                    bAddCar.Click += new RoutedEventHandler(CarHandler);
                    bListAddCar.Children.Add(bAddCar);

                }
                ((Button)bListAddCar.Children[0]).Content = "<";
                ((Button)bListAddCar.Children[1]).Content = "^";
                ((Button)bListAddCar.Children[2]).Content = ">";

                bListAddCar.Orientation = Orientation.Vertical;
                bListAddCar.Tag = (j / 90).ToString();

                Canvas.SetBottom(bListAddCar, 0);
                Canvas.SetRight(bListAddCar, -GEWAY_ONE_LANE_WIDTH / 3);
                LanesSet[j / 90].Children.Add(bListAddCar);
                bPanel[j / 90] = bListAddCar;


                var bAddP1 = new Button()
                {
                    Width = CROSSWALK_ZEBRA_LINE_WIDTH,
                    Height = CROSSWALK_ZEBRA_WIDTH,
                    Content = "+",
                    Background = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(0),

                };

                var bAddP2 = new Button()
                {
                    Width = CROSSWALK_ZEBRA_LINE_WIDTH,
                    Height = CROSSWALK_ZEBRA_WIDTH,
                    Content = "+",
                    Background = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(0),
                };
                bPedPanel[j / 90] = bAddP1;
                bPedPanel[j / 90 + 4] = bAddP2;

                bAddP1.Click += new RoutedEventHandler(CrosswalkSet[j / 90].AddPedestrian);
                bAddP2.Click += new RoutedEventHandler(CrosswalkSet[j / 90].AddPedestrian);

                Canvas.SetRight(bAddP2, 0);
                CrosswalkSet[j / 90].CrosswalkFild.Children.Add(bAddP1);
                CrosswalkSet[j / 90].CrosswalkFild.Children.Add(bAddP2);

            }



            Form.bLane.Click += new RoutedEventHandler(LaneHandler);
            Form.go.Click += new RoutedEventHandler(GoHandler);


            //------------------------------------------------------

            Form.auto.Click += (s, e) =>
            {
                
                Mode = Modes.Auto;
                Form.control.Visibility = Visibility.Visible;
                foreach (var panel in bPanel)
                {
                    panel.Visibility = Visibility.Collapsed;
                }
                foreach (var panel in bPedPanel)
                {
                    panel.Visibility = Visibility.Collapsed;
                }
            };

            Form.manual.Click += (s, e) =>
            {

                if (Reset)
                {
                    StopMovement();
                    TrafficLight.TLTime = TRAFFIC_LIGHT_DEF_TIME;                    
                }

                Mode = Modes.Manual;
                Form.control.Visibility = Visibility.Collapsed;
                foreach (var panel in bPanel)
                {
                    panel.Visibility = Visibility.Visible;
                }
                foreach (var panel in bPedPanel)
                {
                    panel.Visibility = Visibility.Visible;
                }
            };

            Form.stop.Click += (s, e) =>
            {
                StopMovement();
            };

            return this;
        }
        public InterfaceBuilder BuildTrafficLights()
        {
            for (int j = 0; j < 4; j++)
            {
                var trafficLight = new TrafficLight();
                Canvas.SetRight(trafficLight.TLight, -1 * RoadSizes.LIGHT_SIZE);
                Canvas.SetTop(trafficLight.TLight, RoadSizes.MARGIN_BIG);
                LanesSet[j].Children.Add(trafficLight.TLight);
                LightsSet[j] = trafficLight;
            }

            return this;

        }
        public InterfaceBuilder BuildPattern()
        {
            for (int j = 0; j < 360; j += 90)
            {
                var laneUnit = new Canvas()
                {
                    Width = ROAD_WIDTH,
                    Height = ROAD_HEIGHT,
                };

                Canvas.SetBottom(laneUnit, 0);
                Canvas.SetLeft(laneUnit, FILD / 2 - ROAD_WIDTH / 2);

                var r = new RotateTransform
                {
                    Angle = j,
                    CenterX = ROAD_CENTRE,
                    CenterY = -1 * CROSS_SQUARE / 2
                };
                laneUnit.RenderTransform = r;
                Form.fild.Children.Add(laneUnit);
                LanesSet[j / 90] = laneUnit;
            }

            return this;

        }
        public void StopMovement()
        {
            GoTimer.Enabled = false;
            PedestrianTimer.Enabled = false;
            CarTimer.Enabled = false;
            TrafficLight.SwapTimer.Enabled = false;
            TrafficLight.YellowTimer.Enabled = false;
            TrafficLight.TimeFromLasReset = 0;

        }

        private void CarHandler(object sender, RoutedEventArgs e)
        {
            var x = sender as FrameworkElement;
            if (x == null) return;
            var parent = x.Parent as FrameworkElement;
            if (parent == null) return;
            var j = Convert.ToInt16(parent.Tag);

            var car = new Car((RoadParts)j, (Directions)Convert.ToInt16(x.Tag));
            Cars.Add(car);
            // Debug.WriteLine("created "+ car.RoadPart);
        }
        private void LaneHandler(object sender, RoutedEventArgs e)
        {

            Button x = (Button)sender;
            if (x.Content.ToString() == "-")
            {
                for (int i = 0; i < 4; i++)
                {
                    int index = 0;
                    for (int j = 0; j < LanesSet[i].Children.Count; j++)
                    {
                        if (LanesSet[i].Children[j] is Line)
                        {
                            index = j;
                            break;
                        }
                    }
                    LanesSet[i].Children.RemoveRange(index, 2);
                }
                Lane--;
                LaneWidth = GEWAY_ONE_LANE_WIDTH;
                x.Content = "+";
            }
            else
            {
                Lane++;
                LaneWidth = GEWAY_TWO_LANE_WIDTH;
                x.Content = "-";
            }



            for (int j = 0; j < 4; j++)
            {
                for (double i = LaneWidth, q = 1; i < ROAD_WIDTH; i += LaneWidth, q++)
                {
                    if (q == Lane)
                    {
                        i += MAIN_MARKING;
                        continue;
                    }
                    var line = new Line()
                    {
                        StrokeDashArray = new DoubleCollection() { 4, 3 },
                        StrokeThickness = SUB_MARKING,
                        X1 = i,
                        X2 = i,
                        Y1 = 0,
                        Y2 = LanesSet[j].Height * 0.7,
                        Stroke = new SolidColorBrush(Colors.White),
                    };
                    i += SUB_MARKING;
                    Canvas.SetBottom(line, 0);
                    LanesSet[j].Children.Add(line);
                }
            }

            Form.val.Content = Lane.ToString();
            if (Cars.Count != 0) UpdateCars();

        }
        private void GoHandler(object sender, RoutedEventArgs e)
        {
            if (Mode == Modes.Auto)
            {
                CarPeriod = Convert.ToDouble(Form.carF.Text) * 1000;
                PedestrianPeriod = Convert.ToDouble(Form.pedestrianF.Text) * 1000;
                TrafficLight.TLTime = Convert.ToInt32(Form.lightF.Text) * 1000;
                GenerateTraffic();
                if (Reset) GenerationStarted = true;
            }
            Go();
            TrafficLight.BuildLightMode();
            if(!Reset) Reset = true;

        }

    }
}
