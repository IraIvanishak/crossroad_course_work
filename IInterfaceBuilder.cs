using System.Timers;
using System.Windows;

namespace Crossroad
{
    public interface IInterfaceBuilder
    {
        InterfaceBuilder BuildButtoms();
        InterfaceBuilder BuildCrosswalks();
        InterfaceBuilder BuildPattern();
        InterfaceBuilder BuildTrafficLights();

    }
}