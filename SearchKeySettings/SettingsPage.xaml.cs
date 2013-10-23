using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using System.Windows.Navigation;
using Coding4Fun.Phone.Controls;
using SearchKeySettings;
using InteropSvc;
using System.Text;
using System.IO.IsolatedStorage;
using ObjectModel;
using Microsoft.Phone.Shell;
namespace SearchKeySettings
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        
        private MainViewModel viewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }
        
        private bool isLoaded = false;

        public SettingsPage()
        {
            InitializeComponent();
            
            viewModel.OnSelectedItemChanged += new EventHandler(viewModel_OnSelectedItemChanged);
            viewModel.OnBusyStateChanged += new EventHandler(viewModel_OnBusyStateChanged);

            InteropSvc.InteropLib.Initialize();
            if (InteropSvc.InteropLib.Instance.HasRootAccess() == false)
            {
                MessageBox.Show(LocalizedResources.NoRootAccess, LocalizedResources.Error, MessageBoxButton.OK);
                throw new Exception("Quit");
            }
            isLoaded = false;
            viewModel.LoadApplicationsIntoCache();
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            isLoaded = true;
        }

        private void input_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            string result = e.Result;
            if (e.PopUpResult == PopUpResult.Cancelled || e.PopUpResult == PopUpResult.UserDismissed)
            {
                viewModel.SelectedApplication = viewModel.PreviousSelectedApplication;
                return;
            }
            if (e.Result == null)
                result = "";
            if (result.Contains("http"))
            {
                InteropSvc.InteropLib.Instance.RegistrySetDWORD7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "ActionType", (int)KeyAction.ActionType.ACTION_URL);
                InteropSvc.InteropLib.Instance.RegistrySetString7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "Url", result);
            }
            else
            {
                MessageBox.Show(LocalizedResources.IncorrectInput);
                viewModel.SelectedApplication = viewModel.PreviousSelectedApplication;
            }  
        }

        private void listActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void listActions_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        private void listActions_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (isLoaded == true)
            {
                var listbox = sender as ListBox;
                if (listbox.SelectedItem != null)
                {
                    ApplicationViewModel appView = listbox.SelectedItem as ApplicationViewModel;

                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newText = filterBox.Text;
            foreach (var item in viewModel.Applications)
            {
                if (item.Contains(newText))
                    item.Visible = Visibility.Visible;
                else
                    item.Visible = Visibility.Collapsed;
            }
        }

        public void viewModel_OnBusyStateChanged(object sender, EventArgs e)
        {
            if (SystemTray.ProgressIndicator == null)
                SystemTray.ProgressIndicator = new ProgressIndicator();


            SystemTray.ProgressIndicator.IsVisible = viewModel.IsLoadingList;
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            if (viewModel.IsLoadingList)
            {
                SystemTray.ProgressIndicator.Text = LocalizedResources.LoadingList;
                filterBox.IsEnabled = false;
            }
            else
            {
                filterBox.IsEnabled = true;
            }
        }
        public void viewModel_OnSelectedItemChanged(object sender, EventArgs e)
        {
            if (viewModel.SelectedApplication != null)
            {
                KeyAction action = viewModel.SelectedApplication.action;
                InteropSvc.InteropLib.Initialize();

                if (action.type != KeyAction.ActionType.ACTION_URL)
                {
                    InteropSvc.InteropLib.Instance.RegistrySetDWORD7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "ActionType", (int)action.type);
                }
                if (action.type == KeyAction.ActionType.ACTION_RUNAPPLICATION)
                {
                    InteropSvc.InteropLib.Instance.RegistrySetString7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "Uri", action.uri);
                }
                else if (action.type == KeyAction.ActionType.ACTION_URL)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder(300);
                    try
                    {
                        InteropSvc.InteropLib.Instance.RegistryGetString7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "Url", sb, 300);
                    }
                    catch (Exception ex)
                    {

                    }
                    InputPrompt input = new InputPrompt();
                    input.Completed += new EventHandler<PopUpEventArgs<string, PopUpResult>>(input_Completed);
                    input.Title = LocalizedResources.CustomUrl;
                    input.Message = LocalizedResources.CustomUrlText;
                    input.Value = sb.ToString();
                    input.InputScope = new InputScope { Names = { new InputScopeName() { NameValue = InputScopeNameValue.Url } } };
                    input.Show();

                }
                else if (action.type == KeyAction.ActionType.ACTION_KEYEVENT)
                {
                    InteropSvc.InteropLib.Instance.RegistrySetDWORD7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "KeyCode", (int)action.KeyCode);
                }

                InteropSvc.InteropLib.Instance.ApplySkSettings();
            }
        }

        private void grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/pageAbout.xaml", UriKind.Relative));
        }
    }
}