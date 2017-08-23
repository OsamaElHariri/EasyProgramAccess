using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Newtonsoft.Json;

namespace EasyProgramAccess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string User = "WHATEVER USERNAME YOU WANT";
        FirebaseRest fbr = new FirebaseRest("YOUR FIREBASE URL HERE(WITHOUT THE .json IN THE END)", "YOUR SECRET AUTHENTICATION KEY HERE");
        ObservableCollection<PathGroup> paths = new ObservableCollection<PathGroup>();

        public MainWindow()
        {
            InitializeComponent();
            
            // List the information of the groups under a User
            foreach (KeyValuePair<string, PathGroup> grp in fbr.GetGroupNames(User))
            {
                PathGroup tempGroup = grp.Value;
                tempGroup.GroupName = grp.Key;
                paths.Add(tempGroup);

            }
            
            PathItems.ItemsSource = paths;
            

        }
        
        // Opens the paths that are contained in the selected groups
        private void OpenGrpButton_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in PathItems.SelectedItems)
            {
                string name = ((PathGroup) item).GroupName;
                fbr.OpenGroup(name, User);

            }
        }

        // Creates a group whose name the user provides in the text box
        private void CreateGrpButton_OnClick(object sender, RoutedEventArgs e)
        {
            string grpName = NewNameTextBox.Text;
            fbr.CaptureGroup(grpName, User);
            paths.Add(new PathGroup
            {
                GroupName = grpName,
                DateAdded = DateTime.Now.ToString("G"),
                DateOpened = DateTime.Now.ToString("G")
            });
        }

        // Deletes the selected group
        private void DeleteGrpButton_OnClick(object sender, RoutedEventArgs e)
        {
            PathGroup item = (PathGroup)PathItems.SelectedItems[0];
                string name = item.GroupName;
                fbr.Delete(name, User);
                paths.Remove(item);
        }
        
    }
}
