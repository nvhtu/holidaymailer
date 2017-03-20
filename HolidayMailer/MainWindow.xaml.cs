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


namespace HolidayMailer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void letterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            textbox.SelectAll();
        }

        private void letterTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            textbox.SelectAll();
        }

        //credit: https://social.msdn.microsoft.com/Forums/vstudio/en-US/564b5731-af8a-49bf-b297-6d179615819f/how-to-selectall-in-textbox-when-textbox-gets-focus-by-mouse-click?forum=wpf
        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }


        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(editContactGrid.Visibility == Visibility.Visible)
                editContactGrid.Visibility = Visibility.Hidden;

            contactGrid.Visibility = Visibility.Visible;

            //TODO: show warning if email panel is showing
        }

        private void editContactBttn_Click(object sender, RoutedEventArgs e)
        {
            editContactGrid.Visibility = Visibility.Visible;
            SetPeopleEnable(false);
        }

        private void cancelEditBttn_Click(object sender, RoutedEventArgs e)
        {
            editContactGrid.Visibility = Visibility.Hidden;
            SetPeopleEnable(true);

        }

        private void saveEditBttn_Click(object sender, RoutedEventArgs e)
        {
            editContactGrid.Visibility = Visibility.Hidden;
            SetPeopleEnable(true);

        }

        private void newContactTool_Click(object sender, RoutedEventArgs e)
        {
            SetPeopleEnable(false);
            editContactGrid.Visibility = Visibility.Visible;
        }

        private void SetPeopleEnable(bool flag)
        {
            toolBar.IsEnabled = flag;
            listView.IsEnabled = flag;
            sortComboBox.IsEnabled = flag;
            letterTextBox.IsEnabled = flag;
        }


        private void sendMailContactBttn_Click(object sender, RoutedEventArgs e)
        {
            SetPeopleEnable(false);
            sendMailGrid.Visibility = Visibility.Visible;

        }

        private void newMailBttn_Click(object sender, RoutedEventArgs e)
        {
            SetPeopleEnable(false);
            sendMailGrid.Visibility = Visibility.Visible;
        }

        private void newMailAllBttn_Click(object sender, RoutedEventArgs e)
        {
            SetPeopleEnable(false);
            sendMailGrid.Visibility = Visibility.Visible;
        }

        private void newMailPreYearBttn_Click(object sender, RoutedEventArgs e)
        {
            SetPeopleEnable(false);
            sendMailGrid.Visibility = Visibility.Visible;
        }

        private void discardSendBttn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure to discard the email?", "Discard", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                bodyTextBox.Document = new FlowDocument();
                sendMailGrid.Visibility = Visibility.Hidden;
                SetPeopleEnable(true);
                return;
            }
            else
            {
                return;
            }
        }

        private void sendBttn_Click(object sender, RoutedEventArgs e)
        {
            bodyTemp.Text = RichTextBoxHelper.GetText(bodyTextBox.Document);
            bodyTemp.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            sendMailGrid.Visibility = Visibility.Hidden;
            SetPeopleEnable(true);
        }
    }
}
