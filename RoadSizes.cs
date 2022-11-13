using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crossroad
{
    static class RoadSizes
    {
        public const int FILD = 600;
        public const double ROAD_WIDTH = 177.66;
        public const double CROSS_SQUARE = 177.66;
        public const double ROAD_CENTRE = ROAD_WIDTH/2;
        public const double ROAD_HEIGHT = 211.11;
        public const double GEWAY_ONE_LANE_WIDTH = ((ROAD_WIDTH - MAIN_MARKING) / 2);
        public const double GEWAY_TWO_LANE_WIDTH = (((ROAD_WIDTH - MAIN_MARKING) / 2) - SUB_MARKING) / 2;
        public const int MAIN_MARKING = 15;
        public const int SUB_MARKING = 2;

        public const double CAR_WIDTH_COEF = 0.9;
        public const double LIGHT_SIZE_COEF = 0.8;
        public const double CAR_HEIGHT_COEF = 1.3;
        public const double CROSSWALK_WIDTH = 56;
        public const double CROSSWALK_ZEBRA_WIDTH = 36;
        public const double CROSSWALK_ZEBRA_LINE_WIDTH = 13;
        public const int TRAFFIC_LIGHT_DEF_TIME = 30000;
        public const int UPDATE_TIME = 800;
        public const int EXTRA_TIME = 200;
        public const int PEDESTRIAN_TIME = 700;
        public const int TIME_UNIT = 1000;
        public const int YELLOW_TIME = 3000;
        public const int LEFT_DELAY = 1500;
        public const int PEDESTRIAN_DURATION = 5000;

        public const int MARGIN_SMAL = 10;
        public const int MARGIN_BIG = 30;
        public const int UNIT_SIZE = 30;
        public const int LIGHT_SIZE = 25;



        public const double LANE_SWITCH_COEF = 0.7;

        public const double SHORT_SHORT_DUR = 0.25;
        public const double ON_RIGHT_DUR = 1.5;
        public const double STREIGHT_DUR = 0.9;
        public const double SHORT_DUR = 0.5;
        public const double LONG_DUR = 1.2;
        public const double LONG_LONG_DUR = 2.2;
        public const double LONG_LONG_LONG_DUR = 3.5;

        public const int ROADS_COUNT = 4;
        public const int DIRECTIONS_COUNT = 3;
        public const int CAR_COLORS_COUNT = 3;

        public const int PEDESTRIAN_DIRECTIONS_COUNT = 2;
        public const int TIMERS_COUNT = 3;
        public const int MAX_LANE_COUNT = 2;

    }
}
