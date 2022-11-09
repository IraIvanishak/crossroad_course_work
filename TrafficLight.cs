using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crossroad
{
    public class TrafficLight
    {
        public TrafficLight()
        {
            setView();
        }
        public Grid TLight { set; get; } = new();
        public Color PrevLight { set; get; } = Colors.Green;

        private Color currentLight;
        public Color CurrentLight {
            get    {  return currentLight;  }
            set
            {
                currentLight = value;
                int index = 0;
                switch (value.ToString())
                {
                    case "#FFFF0000": index = 0; break;
                    case "#FFFFFF00": index = 1; break;
                    case "#FF008000": index = 2; break;
                }
                if (index!=1) PrevLight = value;



                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        TLight.Children.Cast<Ellipse>().ElementAt(i).Fill = new SolidColorBrush(Colors.Gray);
                    }
                    TLight.Children.Cast<Ellipse>().ElementAt(index).Fill = new SolidColorBrush(value);

                }));


            }
        }
        public void swapColors()
        {
            if (PrevLight == Colors.Green) CurrentLight = Colors.Red;
            else CurrentLight = Colors.Green;
        }
        public void setView()
        {
            TLight = new Grid()
            {
                Width = RoadSizes.LIGHT_SIZE,
                Background = new SolidColorBrush(Colors.DarkGray),
                Margin = new Thickness(0, 10, 0, 0),
            };       

            for(int i=0; i<3; i++)
            {
                RowDefinition gridRow = new RowDefinition()
                {
                    Height = new GridLength(RoadSizes.LIGHT_SIZE)
                };
               
                TLight.RowDefinitions.Add(gridRow);
                Ellipse circle = new Ellipse()
                {
                    Width = RoadSizes.LIGHT_SIZE * RoadSizes.LIGHT_SIZE_COEF,
                    Height = RoadSizes.LIGHT_SIZE * RoadSizes.LIGHT_SIZE_COEF,
                    Fill = new SolidColorBrush(Colors.Gray),
                 };
                Grid.SetRow(circle, i);
                TLight.Children.Add(circle);
            }


        }

    }

}
