using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static Crossroad.RoadSizes;

namespace Crossroad
{
    public class TrafficLight
    {
        public TrafficLight()
        {
            setView();
        }
        public static int TLTime { set; get; } = TRAFFIC_LIGHT_DEF_TIME;
        public Grid TLight { set; get; } = new();
        public Color PrevLight { set; get; } = Colors.Green;
        public static int TimeFromLasReset { set; get; } = 0;

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
        public static Timer swapTimer { set; get; } = new();
        public static Timer startTimer { set; get; } = new();
        public static Timer yellowTimer { set; get; } = new();
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
        public static int GetRemainigTime()
        {
            return TLTime - YELLOW_TIME - (((int)DateTime.Now.Ticks) - TimeFromLasReset) / 10000;
        }

        public static void BuildLightMode()
        {
            if (swapTimer.Enabled)
            {
                startTimer.Stop();
                swapTimer.Stop();
                yellowTimer.Stop();
            }
            for (int i = 0; i < ROADS_COUNT; i++)
                Road.LightsSet[i].CurrentLight = Colors.Yellow;
            Road.Axis = Axes.Undef;

            swapTimer.Interval = TLTime;
            yellowTimer.Interval = TLTime;

            if (!Road.Reset)
            {
                swapTimer.Elapsed += (s, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TimeFromLasReset = (int)DateTime.Now.Ticks;

                        for (int i = 0; i < ROADS_COUNT; i++)
                        {
                            Road.LightsSet[i].swapColors();
                        }

                        if (Road.LightsSet[0].CurrentLight == Colors.Green)
                            Road.Axis = Axes.Vertical;
                        else Road.Axis = Axes.Horizontal;
                    });

                };

                startTimer.Interval = YELLOW_TIME;

                startTimer.Elapsed += (s, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TimeFromLasReset = (int)DateTime.Now.Ticks;

                        for (int i = 0; i < ROADS_COUNT; i++)
                        {
                            if (i % 2 == 0) Road.LightsSet[i].CurrentLight = Colors.Red;
                            else Road.LightsSet[i].CurrentLight = Colors.Green;
                        }
                        Road.Axis = Axes.Horizontal;

                        swapTimer.Start();
                        startTimer.Stop();
                    });
                };

                yellowTimer.Elapsed += (s, e) =>
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        for (int i = 0; i < ROADS_COUNT; i++)
                            Road.LightsSet[i].CurrentLight = Colors.Yellow;
                        Road.Axis = Axes.Undef;
                    }));
                };
            }

            yellowTimer.Start();
            startTimer.Start();
        }

    }

}
