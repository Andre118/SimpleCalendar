using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using DynamicData.Binding;
using DynamicData.Kernel;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DesktopNotifications;
using System.Diagnostics;
using DesktopNotifications.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Xml;
using System.Globalization;
using Avalonia.Controls.ApplicationLifetimes;

namespace SimpleCalendar.Views;

public partial class MainView : UserControl
{
    private bool lastDaySelectionSet = false;
    private DateTime lastSelectedDay;
    private DateTime selectedDay;
    public DateTime SelectedDay {
        get { return selectedDay; }
        set {
            if (!lastDaySelectionSet) {
                lastSelectedDay = value;
                lastDaySelectionSet = true;
            }
            else {
                lastSelectedDay = selectedDay;
            }

            selectedDay = value;
            UpdateBorderVisualOnDaySelection();
            UpdateAlertDataVisuals();
            SetNoteText();
        }
    }

    private int weekDayOffset;
    Datamanager data;

    List<ILogical> dayFrames;


    public MainView()
    {
        InitializeComponent();

        //options setup
        Options options = Options.GetInstance();
        SetupOptionsMenu();

        //alert preperations
        alerts = new Alerts();
        HourComboBox.ItemsSource = alerts.hours;
        MinuteComboBox.ItemsSource = alerts.minutes;
        InstantiateAlertVisuals();
        alerts.OnAlertChange += UpdateAlertDataVisuals;
        alerts.LoadAlertsFromJson();

        data = new Datamanager();
        data.OnNoteChange += SetNoteText;
        data.LoadDailyNotes();

        SetupDayFieldBorders();
        dayFrames = DayGrid.GetLogicalChildren().AsList();
        SetupWeekDayOffset(DateTime.Now);
        SelectedDay = DateTime.Now;
        SetupCurrentMonthVisuals(selectedDay);
        SetupFieldInteractivity(selectedDay);

        SetupMonthYearText(selectedDay);


        AlertNotifications alertNotifications = new AlertNotifications(selectedDay, alerts);
    }

    private void SetupMonthYearText(DateTime dt) {
        MonthYearText.Text = $"{dt.ToString("MMMM", CultureInfo.InvariantCulture)} {dt.Year.ToString()}";
    }

    private void DefocusNoteText() {
        if (!NoteTextBox.IsFocused) {
            return;
        }

        var topLevel = (IClassicDesktopStyleApplicationLifetime) Application.Current.ApplicationLifetime;
        var focusManager = topLevel.MainWindow.FocusManager;
        focusManager.ClearFocus();
    }

    /// <summary>
    /// Returns border of last selected day back to normal and marks border of current selection by changing color and thickness.
    /// </summary>
    private void UpdateBorderVisualOnDaySelection() {
        NoteTextBox.Watermark = $"Add a note to the date {SelectedDay.Month}.{SelectedDay.Day}.{SelectedDay.Year}, simply click on another day or button and your message will be saved automatically!";
        DefocusNoteText();

        Brush unselectedBorderColor = new SolidColorBrush(Colors.Black);
        Thickness unselectedBorderThickness = new Thickness(1);

        Brush selectedBorderColor = new SolidColorBrush(Colors.Red);
        Thickness selectedBorderThickness = new Thickness(3);

        int lastOffset = CalculateWeekDayOffset(lastSelectedDay);
        Border lastSelectionBorder = (Border) dayFrames[lastOffset + lastSelectedDay.Day - 1];
        lastSelectionBorder.BorderBrush = unselectedBorderColor;
        lastSelectionBorder.BorderThickness = unselectedBorderThickness;

        int newOffset = CalculateWeekDayOffset(selectedDay);
        Border currentSelectionBorder = (Border)dayFrames[newOffset + SelectedDay.Day - 1];
        currentSelectionBorder.BorderBrush = selectedBorderColor;
        currentSelectionBorder.BorderThickness = selectedBorderThickness;
    }

    //creates the border object with text to represent days
    private void SetupDayFieldBorders() {
        int rowCount = DayGrid.RowDefinitions.Count;
        int collumnCount = DayGrid.ColumnDefinitions.Count;

        for (int row = 0; row < rowCount; row++) {
            for (int collumn = 0; collumn < collumnCount; collumn++) {
                Border border = UI_Code_Templates.DayFieldTemplate();
                DayGrid.Children.Add(border);
                border.SetValue(Grid.RowProperty, row);
                border.SetValue(Grid.ColumnProperty, collumn);
            }
        }
    }

    private void SetupWeekDayOffset(DateTime dt) {
        //sets the calendar frame dates, since the frames represent monday to sunday, calculate the offset
        //and set the 1st day accordingly, weekday offset 0 = monday and so on
        DateTime startOfMonth = new DateTime(dt.Year, dt.Month, 1);
        weekDayOffset = mod(DateAndTime.Weekday(startOfMonth) - 2, 7);
    }

    private int CalculateWeekDayOffset(DateTime dt) {
        DateTime startOfMonth = new DateTime(dt.Year, dt.Month, 1);
        int offset = mod(DateAndTime.Weekday(startOfMonth) - 2, 7);

        return offset;
    }

    public void SetupFieldInteractivity(DateTime dt) {
        int totalDaysInLastMonth = DateTime.DaysInMonth(dt.Year, dt.Month == 1 ? 12 : dt.Month - 1);
        int daysInCurrentMonthPlusOffset = DateTime.DaysInMonth(dt.Year, dt.Month) + weekDayOffset;

        for(int i=0; i<dayFrames.Count; i++) {
            Border dayFrameBorder = (Border)dayFrames[i];
            dayFrameBorder.PointerPressed -= Test_PointerPressed;
        }

        for (int i = weekDayOffset; i < daysInCurrentMonthPlusOffset; i++) {
            Border dayFrameBorder = (Border)dayFrames[i];
            dayFrameBorder.PointerPressed += Test_PointerPressed;
        }
    }

    private void SetupCurrentMonthVisuals(DateTime dt) {
        int totalDaysInLastMonth = DateTime.DaysInMonth(dt.Year, dt.Month == 1 ? 12 : dt.Month - 1);
        int daysInCurrentMonthPlusOffset = DateTime.DaysInMonth(dt.Year, dt.Month) + weekDayOffset;


        //set numbers for current month
        for (int i = weekDayOffset; i < daysInCurrentMonthPlusOffset; i++) {
            Border dayFrameBorder = (Border)dayFrames[i];
            dayFrameBorder.Background = new SolidColorBrush(Colors.White);

            TextBlock iDayNumberTextBlock = DayFrameNumberText((Border) dayFrames[i]); 
            iDayNumberTextBlock.Text = (i - weekDayOffset + 1).ToString();
        }

        //change border color of all frames representing days not in the current month
        SolidColorBrush inactiveFieldColor = new SolidColorBrush(Colors.LightGray);

        //set day frames before the 1st of current month to the final days of the last month
        for (int i = weekDayOffset-1; i >= 0 ; i--) {
            Border dayFrameBorder = (Border) dayFrames[i];
            dayFrameBorder.Background = inactiveFieldColor;

            TextBlock iDayNumberTextBlock = DayFrameNumberText(dayFrameBorder);
            iDayNumberTextBlock.Text = (totalDaysInLastMonth + i + 1 - weekDayOffset).ToString();
        }

        //set day franes after the last day of this month to the first days of next month
        for (int i = daysInCurrentMonthPlusOffset; i < DayGrid.Children.Count; i++) {
            Border dayFrameBorder = (Border)dayFrames[i];
            dayFrameBorder.Background = inactiveFieldColor;

            TextBlock iDayNumberTextBlock = DayFrameNumberText(dayFrameBorder);
            iDayNumberTextBlock.Text = (i - daysInCurrentMonthPlusOffset + 1).ToString();
        }
    }

    public void ChangeToNextMonth(object sender, RoutedEventArgs args) {

        int newDay = Math.Min(selectedDay.Day, DateTime.DaysInMonth(selectedDay.Year, selectedDay.Month));
        DateTime newDate;


        if (selectedDay.Month == 12) {
            newDate =  new DateTime(selectedDay.Year+1, 1, newDay);
        }
        else {
            newDate = new DateTime(selectedDay.Year, selectedDay.Month + 1, newDay);
        }

        SetupWeekDayOffset(newDate);
        SetupCurrentMonthVisuals(newDate);
        SetupFieldInteractivity(newDate);
        SetupMonthYearText(newDate);
        SelectedDay = newDate;
    }

    public void ChangeToLastMonth(object sender, RoutedEventArgs args) {

        int newDay = Math.Min(selectedDay.Day, DateTime.DaysInMonth(selectedDay.Year, selectedDay.Month));
        DateTime newDate;


        if (selectedDay.Month == 1) {
            newDate = new DateTime(selectedDay.Year - 1, 12, newDay);
        }
        else {
            newDate = new DateTime(selectedDay.Year, selectedDay.Month - 1, newDay);
        }

        SetupWeekDayOffset(newDate);
        SetupCurrentMonthVisuals(newDate);
        SetupFieldInteractivity(newDate);
        SetupMonthYearText(newDate);
        SelectedDay = newDate;
    }

    //returns the first textblock of a dayframe border element, which contains the number of the day the frame represents currently.
    //the elements are build in a structure border -> grid -> (textblock, textblock)
    //specifying type to be Border is unnecessary but intentional as only a specific usecase can be fulfilled because of
    //the architectural requirement
    private TextBlock DayFrameNumberText(Border dayFrame) {
        Grid grid = (Grid) dayFrame.GetLogicalChildren().AsList()[0];
        return (TextBlock) grid.GetLogicalChildren().AsList()[0];
    }

    //returns the second textblock of a dayframe border element, which is meant for useful headings providing a quick
    //overview over the days. the elements are build in a structure border -> grid -> (textblock, textblock)
    //specifying type to be Border is unnecessary but intentional as only a specific usecase can be fulfilled because of
    //the architectural requirement
    private TextBlock DayFrameHeadingText(Border dayFrame) {
        Grid grid = (Grid) dayFrame.GetLogicalChildren().AsList()[0];
        return (TextBlock) grid.GetLogicalChildren().AsList()[1];
    }

    private int mod(int x, int m) {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    private void SaveNote(object sender, RoutedEventArgs args) {
        TextBox textBox = (TextBox)sender;

        if (textBox.Text != null) {
            data.WriteDailyNote(lastSelectedDay, textBox.Text);
        }
    }

    private void SetNoteText() {
        NoteTextBox.Text = data.GetDailyNote(SelectedDay);
    }


    private void Test_PointerPressed(object sender, PointerPressedEventArgs args) {
        Border element = (Border) sender;
        TextBlock numberText = DayFrameNumberText(element);
        string? dayNumber = numberText.Text;

        if(dayNumber != null) {
            int pressedOnDay = int.Parse(dayNumber);
            SelectedDay = new DateTime(SelectedDay.Year, SelectedDay.Month, pressedOnDay);
        }

    }

    private TextBlock? GetCurrentDayHeadingTextblock() {
        return DayFrameHeadingText((Border) dayFrames[SelectedDay.Day + weekDayOffset - 1]);
    }




    #region alerts
    // ////////////////////////////////////////////////// alert logic

    Alerts alerts;
    List<Border> alertVisuals = new List<Border>();

    //create a number of deactivated alert visuals, if an alert is loaded from savefile or a new alert is created,
    //set its data to one of the visuals and activate it. This way most object instantiation can be prevented at runtime.
    //if an alert is to be deleted, deactivate its corresponding visual and if not enough visuals are available, create some
    //additional ones
    private void InstantiateAlertVisuals() {

        for (int i = 0; i < 10; i++) {
            Border alertTemplate = UI_Code_Templates.AlertTemplate(SetupAlertEditPopup, DeleteAlert);
            alertVisuals.Add(alertTemplate);
            alertTemplate.IsEnabled = false;
            alertTemplate.IsVisible = false;
            AlertPanel.Children.Add(alertTemplate);
        }
    }

    //if the add new alert button (top left with plus icon) is pressed, show the alert edit popup
    private void AddNewAlertButton_Click(object sender, RoutedEventArgs args) {
        AlertPopup.IsEnabled = true;
        AlertPopup.IsVisible = true;

        AlertSaveButton.IsEnabled = true;
        AlertSaveButton.IsVisible = true;

        AlertEditButton.IsEnabled = false;
        AlertEditButton.IsVisible = false;

    }

    //save a new alert or save an edited existing alert
    private void AlertSave_Click(object sender, RoutedEventArgs args) {

        if (HourComboBox.SelectedValue != null && MinuteComboBox.SelectedValue != null && AlertNameText.Text != null) {
            alerts.RegisterAlert(SelectedDay, new Alert((int)HourComboBox.SelectedValue, (int)MinuteComboBox.SelectedValue, AlertNameText.Text));
            AlertNameText.Text = "";
            AlertNameText.Watermark = "Saved!";

            AlertPopup.IsVisible = false;
            AlertPopup.IsEnabled = false;
        }
        else {
            AlertNameText.Text = "";
            AlertNameText.Watermark = "invalid format!";
        }
    }


    private void UpdateAlertDataVisuals() {
        List<Alert> todaysAlerts = alerts.RetrieveAlerts(SelectedDay);

        if(todaysAlerts == null) {
            //reset alert visuals if no alerts have been registered for a given day
            for (int i = 0; i < alertVisuals.Count; i++) {
                alertVisuals[i].IsEnabled = false;
                alertVisuals[i].IsVisible = false;
            }
            return;
        }


        //make sure enough visual elements are available
        while (todaysAlerts.Count > alertVisuals.Count) {
            Border alertTemplate = UI_Code_Templates.AlertTemplate(SetupAlertEditPopup, DeleteAlert);
            alertVisuals.Add(alertTemplate);
            alertTemplate.IsEnabled = false;
            alertTemplate.IsVisible = false;
            AlertPanel.Children.Add(alertTemplate);
        }

        //reset all existing visual elements to accomodate deletion
        for (int i = 0; i < alertVisuals.Count; i++) {
            alertVisuals[i].IsEnabled = false;
            alertVisuals[i].IsVisible = false;
        }

        for (int i = 0; i < todaysAlerts.Count; i++) {
            SetAlertDataToVisual(alertVisuals[i], todaysAlerts[i]);
        }

    }

    private void SetAlertDataToVisual(Border alertVisual, Alert alert) {
        Grid alertGrid = (Grid) alertVisual.Child;
        TextBlock timeText = (TextBlock) alertGrid.Children[0];
        TextBlock headlineText = (TextBlock) alertGrid.Children[1];

        timeText.Text = $"{alert.hour}:{alert.minute}";
        headlineText.Text = alert.alertName;

        alertVisual.IsEnabled = true;
        alertVisual.IsVisible = true;
    }

    int alertEditIndex = 0;

    public void SetupAlertEditPopup(object sender, RoutedEventArgs args) {
        Button alertVisualButton = sender as Button;
        StackPanel alertButtonPanel = alertVisualButton.Parent as StackPanel;
        Grid alertVisualgrid = alertButtonPanel.Parent as Grid;
        Border alertVisualBorder = alertVisualgrid.Parent as Border;

        alertEditIndex = alertVisuals.IndexOf(alertVisualBorder);

        Alert alert = alerts.RetrieveAlerts(SelectedDay)[alertEditIndex];
        MinuteComboBox.SelectedValue = alert.minute;
        HourComboBox.SelectedValue = alert.hour;
        AlertNameText.Text = alert.alertName;

        AlertPopup.IsVisible = true;
        AlertPopup.IsEnabled = true;

        AlertSaveButton.IsEnabled = false;
        AlertSaveButton.IsVisible = false;

        AlertEditButton.IsEnabled = true;
        AlertEditButton.IsVisible = true;
    }

    private void EditAlert_Click(object sender, RoutedEventArgs e) {
        alerts.EditAlert(SelectedDay, alertEditIndex, (int)HourComboBox.SelectedValue, (int) MinuteComboBox.SelectedValue, AlertNameText.Text);

        AlertPopup.IsVisible = false;
        AlertPopup.IsEnabled = false;
    }

    public void OpenAlertOptionsMenu_Click(object sender, RoutedEventArgs e) {
        AlertOptionsMenu.IsEnabled = true;
        AlertOptionsMenu.IsVisible = true;
    }

    private void CloseAlertOptionsMenu_Click(object sender, RoutedEventArgs e) {
        AlertOptionsMenu.IsEnabled = false;
        AlertOptionsMenu.IsVisible = false;
    }

    public void DeleteAlert(object sender, RoutedEventArgs args) {
        Button alertVisualButton = sender as Button;
        StackPanel alertButtonPanel = alertVisualButton.Parent as StackPanel;
        Grid alertVisualgrid = alertButtonPanel.Parent as Grid;
        Border alertVisualBorder = alertVisualgrid.Parent as Border;

        int alertDeleteIndex = alertVisuals.IndexOf(alertVisualBorder);

        alerts.DeleteAlert(SelectedDay, alertDeleteIndex);
    }

    private void CloseAlertPopup_Click(object sender, RoutedEventArgs args) {
        AlertPopup.IsEnabled = false;
        AlertPopup.IsVisible = false;
    }

    #endregion alerts


    #region options

    private void SetupOptionsMenu() {
        AlertsActiveCheckBox.IsChecked = Options.GetInstance().AlertsActivated;
    }

    private void ToggleAlertsActive(object sender, RoutedEventArgs e) {
        if(AlertsActiveCheckBox.IsChecked.HasValue) {
            Options.GetInstance().AlertsActivated = AlertsActiveCheckBox.IsChecked.Value;
        }
    }

    #endregion options
}
