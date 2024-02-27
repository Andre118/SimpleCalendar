using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SimpleCalendar.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCalendar {
    internal class UI_Code_Templates {

        //returns a bordered element representing a day in a calendar
        public static Border DayFieldTemplate() {

            Border dayFieldTemplate = new Border();
            dayFieldTemplate.Background = new SolidColorBrush(Colors.White);
            dayFieldTemplate.BorderBrush = new SolidColorBrush(Colors.Black);
            dayFieldTemplate.BorderThickness = new Thickness(1);


            //create a grid with 2 rows, one for the date of the day and one for text headlines
            Grid dayFieldTemplateGrid = new Grid();
            RowDefinitions gridRowDefinitions = new RowDefinitions();
            RowDefinition rowOne = new RowDefinition();
            rowOne.Height = new GridLength(1, GridUnitType.Star);
            gridRowDefinitions.Add(rowOne);
            RowDefinition rowTwo = new RowDefinition();
            rowTwo.Height = new GridLength(4, GridUnitType.Star);
            gridRowDefinitions.Add(rowTwo);
            dayFieldTemplateGrid.RowDefinitions = gridRowDefinitions;

            dayFieldTemplate.Child = dayFieldTemplateGrid;


            //now create textblocks showing the content
            TextBlock dateTextBlock = new TextBlock();
            dateTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
            dateTextBlock.Padding = new Thickness(0, 0, 4, 0);

            TextBlock headLineTextBlock = new TextBlock();
            headLineTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            headLineTextBlock.VerticalAlignment = VerticalAlignment.Top;
            headLineTextBlock.Padding = new Thickness(0, 0, 4, 0);

            dayFieldTemplateGrid.Children.Add(dateTextBlock);
            dateTextBlock.SetValue(Grid.RowProperty, 0);
            dayFieldTemplateGrid.Children.Add(headLineTextBlock);
            headLineTextBlock.SetValue(Grid.RowProperty, 1);


            return dayFieldTemplate;
        }

        //visualizes the time and name of an alert
        public static Border AlertTemplate(Action<object, RoutedEventArgs> editMethod, Action<object, RoutedEventArgs> deleteMethod) {
            Border alertTemplateBorder = new Border();
            alertTemplateBorder.Background = new SolidColorBrush(Colors.White);
            alertTemplateBorder.CornerRadius = new CornerRadius(10);
            alertTemplateBorder.Margin = new Thickness(10, 5, 10, 5);
            alertTemplateBorder.Height = 25;

            //border will have a grid with 5 columns as content. column 1, 3 and 5 will be used to show other elements,
            //while 2 and 4 will provide some whitespace between the elements
            Grid grid = new Grid();
            ColumnDefinitions colDef = new ColumnDefinitions();
            colDef.Add(new ColumnDefinition(2, GridUnitType.Star));
            colDef.Add(new ColumnDefinition(1, GridUnitType.Star));
            colDef.Add(new ColumnDefinition(2, GridUnitType.Star));
            colDef.Add(new ColumnDefinition(1, GridUnitType.Star));
            colDef.Add(new ColumnDefinition(2, GridUnitType.Star));
            grid.ColumnDefinitions = colDef;

            //the grid will contain  2 textblocks, one for time of the alert, one for the alert headline. additionally it will
            //contain a a stackpanel with 2 buttons for editing and deleting the alert repectively
            TextBlock timeTextBlock = new TextBlock();
            timeTextBlock.SetValue(Grid.ColumnProperty, 0);
            timeTextBlock.VerticalAlignment = VerticalAlignment.Center;
            timeTextBlock.Margin = new Thickness(10, 0, 0, 0);

            grid.Children.Add(timeTextBlock);

            TextBlock alertHeadlineTextBlock = new TextBlock();
            alertHeadlineTextBlock.SetValue(Grid.ColumnProperty, 2);
            alertHeadlineTextBlock.VerticalAlignment = VerticalAlignment.Center;
            alertHeadlineTextBlock.Margin = new Thickness(10, 0, 0, 0);
            grid.Children.Add(alertHeadlineTextBlock);


            //stackpanel for buttons
            StackPanel buttonPanel = new StackPanel();
            buttonPanel.SetValue(Grid.ColumnProperty, 4);
            buttonPanel.HorizontalAlignment = HorizontalAlignment.Right;
            buttonPanel.Orientation = Orientation.Horizontal;


            //button for editing alert
            Button editButton = new Button();
            editButton.Click += new EventHandler<RoutedEventArgs>(editMethod);


            Image editIcon = new Image();
            editIcon.Source = new Bitmap("J:\\Uni\\Random Programming Stuff\\C# app tutorial\\SimpleCalendar\\SimpleCalendar\\Assets\\edit_icon.png");
            editIcon.Stretch = Stretch.Uniform;

            editButton.Content = editIcon;


            //button for deleting alert
            Button deleteButton = new Button();
            deleteButton.Click += new EventHandler<RoutedEventArgs>(deleteMethod);

            Image deleteIcon = new Image();
            deleteIcon.Source = new Bitmap("J:\\Uni\\Random Programming Stuff\\C# app tutorial\\SimpleCalendar\\SimpleCalendar\\Assets\\trashbin.jpg");
            deleteIcon.Stretch = Stretch.Uniform;

            deleteButton.Content = deleteIcon;


            //add buttons to stackpanel
            buttonPanel.Children.Add(editButton);
            buttonPanel.Children.Add(deleteButton);



            grid.Children.Add(buttonPanel);





            alertTemplateBorder.Child = grid;

            return alertTemplateBorder;
        }
    }
}
