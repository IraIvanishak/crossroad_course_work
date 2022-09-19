using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace Crossroad
{
    public class Road 
    {
        public Road( Canvas fild )
        {
            this.fild = fild;
            TrafficLight.fild = fild;
            SetTLights();

            Lane = 1;
            LaneWidth = RoadWidth;
            SetPattern();


            var bSetLane = new Button()
            {
                Width = 30,
                Height = 30,
                Content = "+",
                
            };

            bSetLane.Click += new RoutedEventHandler(LaneHandler);



            Canvas.SetBottom(bSetLane, 0);
            Canvas.SetLeft(bSetLane, fild.Width/2 - bSetLane.Width);
            fild.Children.Add(bSetLane);


        }

        public Canvas fild;

        static int RoadWidth = 86;
        static int RoadHeight = 214;

        public void SetPattern()
        {
            lanesSet = new Canvas[4];

            for (int j = 0; j < 360; j += 90)
            {
                var laneUnit = new Canvas()
                {
                    Width = 2 * RoadWidth,
                    Height = RoadHeight,
                 //      Background = new SolidColorBrush(Colors.Blue),

                };

                Canvas.SetBottom(laneUnit, 0);
                Canvas.SetLeft(laneUnit, fild.Width / 2 - laneUnit.Width / 2);

                var r = new RotateTransform();
                r.Angle = j;
                r.CenterX = RoadWidth;
                r.CenterY = -1 * RoadWidth;

                laneUnit.RenderTransform = r;
                

                TrafficLight trafficLight = new TrafficLight();
                Canvas.SetRight(trafficLight.TLight, -1*trafficLight.Size-10);
                laneUnit.Children.Add(trafficLight.TLight);

                fild.Children.Add(laneUnit);
                lanesSet[j / 90] = laneUnit;

            }
        }
        public void SetTLights()
        {
           

        }
      
        public int LaneWidth { get; set; }

        private Canvas[] lanesSet;

        public void LaneHandler(object sender, RoutedEventArgs e)
        {
          
            Button x = sender as Button;
            if (x.Content.ToString() == "-")
            {
                for (int i = 0; i < 4; i++)
                {
                    int index=0;
                    for (int j = 0; j < lanesSet[i].Children.Count; j++)
                    {
                        if(lanesSet[i].Children[j] is Line) {
                            index = j;
                            break;
                        }
                    }

                    lanesSet[i].Children.RemoveRange(index, 2);
                }
                Lane--;
                x.Content = "+";
            }
            else
            {
                Lane++;
                x.Content = "-";
            }   

            LaneWidth = RoadWidth / Lane;

             for (int j = 0;  j < 4; j++)
            {

                                
                for (int i = LaneWidth, q = 1; i < 2 * RoadWidth; i += LaneWidth, q++)
                {
                    if (q == Lane)
                    {
                        i += 5;
                        continue;
                    }
                    var line = new Line()
                    {
                        StrokeDashArray = new DoubleCollection() { 4, 3 },
                        StrokeThickness = 2,
                        X1 = i,
                        X2 = i,
                        Y1 = 0,
                        Y2 = lanesSet[j].Height*0.7,
                        Stroke = new SolidColorBrush(Colors.White),
                    };
                    Canvas.SetBottom(line, 0);
                    lanesSet[j].Children.Add(line);

                    
                }

                
            }
        }


        private int lane;
        public int Lane { 
            get { return lane; } 
            set {                 
                lane = value; 
                
            } 
        }



    }
}
