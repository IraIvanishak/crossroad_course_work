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
    public class TrafficLight
    {
        public static Canvas fild;
        public TrafficLight()
        {
            Size = 25;
            setView();

        }
                

        // сет прописати потім окремо - і це буде колордефайн ОАОАОАООА я геній 
        public LightMode currentLight { get; set; }
        public void updateView()
        {

        }

        // може як статік треба хмхм
        public  int Size { get; set; }

        public Grid TLight;
        public void setView()
        {
            TLight = new Grid()
            {
                Width = Size,
                Background = new SolidColorBrush(Colors.DarkGray),
                Margin = new Thickness(0, 10, 0, 0),
            };          
           


            for(int i=0; i<3; i++)
            {
                RowDefinition gridRow = new RowDefinition()
                {
                    Height = new GridLength(Size)
                };
               
                TLight.RowDefinitions.Add(gridRow);
                Ellipse circle = new Ellipse()
                {
                    Width = Size * 0.8,
                    Height = Size * 0.8,
                    Fill = new SolidColorBrush(Colors.Gray),
                 };
                Grid.SetRow(circle, i);
                TLight.Children.Add(circle);
            }


        }


    }

}
