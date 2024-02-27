using SimpleCalendar.Views;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using Windows.ApplicationModel;

namespace SimpleCalendar.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    public string AlertDescription => "Create an alert.\r\nA reminder will be send on program start \r\nup to 3 days ahead.";

    public string AlertNameMenuString => "Enter alert name here!";
}
