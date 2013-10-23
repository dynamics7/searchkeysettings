using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using SearchKeySettings;
using Coding4Fun.Phone.Controls;
using System.Threading;

namespace ObjectModel
{
    public class MainViewModel : BaseViewModel
    {
        public event EventHandler OnSelectedItemChanged;
        public event EventHandler OnBusyStateChanged;

        private List<ApplicationViewModel> _applicationCache = null;

        public List<ApplicationViewModel> Applications
        {
            get
            {
                return _applicationCache;
            }
        }

        private static ApplicationViewModel[] predefinedApplications = {new ApplicationViewModel() { Title = LocalizedResources.NoAction, Author = LocalizedResources.NoActionText, 
                                                    action = new KeyAction() { type = KeyAction.ActionType.ACTION_NO}, IsSpecial=true, SpecialItemIndex=0},

                                                 new ApplicationViewModel() { Title = LocalizedResources.StockAction, Author = LocalizedResources.StockActionText, 
                                                    action = new KeyAction() { type = KeyAction.ActionType.ACTION_STOCK}, IsSpecial=true, SpecialItemIndex=1},

                                                 new ApplicationViewModel() { Title = LocalizedResources.Bing, Author = LocalizedResources.BingText, 
                                                    action = new KeyAction() { type = KeyAction.ActionType.ACTION_BING}, IsSpecial=true, SpecialItemIndex=2},

                                                 new ApplicationViewModel() { Title = LocalizedResources.Search, Author = LocalizedResources.SearchText, 
                                                    action = new KeyAction() { type = KeyAction.ActionType.ACTION_SEARCH}, IsSpecial=true, SpecialItemIndex =3 },


                                                 new ApplicationViewModel() { Title = LocalizedResources.PowerKey, Author = LocalizedResources.PowerKeyText,
                                                     action = new KeyAction() { type = KeyAction.ActionType.ACTION_KEYEVENT, KeyCode = 129 }, IsSpecial=true, SpecialItemIndex=4 },

                                                 new ApplicationViewModel() { Title = LocalizedResources.CustomUrl, Author = "CustomUrlText", 
                                                    action = new KeyAction() { type = KeyAction.ActionType.ACTION_URL}, IsSpecial=true , SpecialItemIndex = 5},

                                  };

        private void LoadApplicationsThread()
        {
            var dispatcher = System.Windows.Deployment.Current.Dispatcher;
            dispatcher.BeginInvoke(new Action(() =>
            {
                IsLoadingList = true;
                if (OnBusyStateChanged != null)
                    OnBusyStateChanged(this, new EventArgs());
            }));

            ApplicationViewModel selectedModel = null;
            var list = GetActualApplicationList(ref selectedModel);
            _applicationCache = list;
            dispatcher.BeginInvoke(new Action(() =>
            {
                IsLoadingList = false;
                OnChange("Applications");
                SelectedApplication = selectedModel;
                if (OnBusyStateChanged != null)
                    OnBusyStateChanged(this, new EventArgs());
            }));
        }

        public void LoadApplicationsIntoCache()
        {
            if (_applicationCache == null)
            {
                var thread = new Thread(LoadApplicationsThread);
                thread.Start();
            }
        }

        private List<ApplicationViewModel> GetActualApplicationList(ref ApplicationViewModel selectedModel)
        {
            var appList = ApplicationApi.GetAllVisibleApplications();
            selectedModel = null;

            System.Text.StringBuilder currentUrl = new System.Text.StringBuilder(300);
            System.Text.StringBuilder currentUri = new System.Text.StringBuilder(300);

            int keyCode = 0;
            int dwCurActionType = (int)KeyAction.ActionType.ACTION_NO;

            {
                InteropSvc.InteropLib.Instance.RegistryGetDWORD7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "ActionType", out dwCurActionType);
                InteropSvc.InteropLib.Instance.RegistryGetDWORD7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "KeyCode", out keyCode);


                InteropSvc.InteropLib.Instance.RegistryGetString7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "Url", currentUrl, 300);



                InteropSvc.InteropLib.Instance.RegistryGetString7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "Uri", currentUri, 300);
            }

            var curActionType = (KeyAction.ActionType)dwCurActionType;

            var list = new List<ApplicationViewModel>();
            foreach (var app in predefinedApplications)
            {
                list.Add(app);
                if (app.action.type == curActionType)
                {
                    bool setSelected = false;
                    if (curActionType == KeyAction.ActionType.ACTION_URL)
                    {
                        if (currentUrl.ToString() == app.action.uri)
                            setSelected = true;
                    }
                    else if (curActionType == KeyAction.ActionType.ACTION_RUNAPPLICATION)
                    {
                        if (currentUri.ToString() == app.action.uri)
                            setSelected = true;
                    }
                    else if (curActionType == KeyAction.ActionType.ACTION_KEYEVENT)
                    {
                        if (keyCode == app.action.KeyCode)
                            setSelected = true;
                    }
                    else
                    {
                        setSelected = true;
                    }
                    if (setSelected)
                        selectedModel = app;
                }
            }
            
            foreach (var item in appList)
            {

                var app = new ApplicationViewModel();
                app.Title = item.Title();
                app.Author = item.Author();
                app.action = new KeyAction();
                app.action.type = KeyAction.ActionType.ACTION_RUNAPPLICATION;
                string uri, pageid;
                item.GetInvocationInfo(out uri, out pageid);
                app.action.uri = uri;
                list.Add(app);
                if (uri == currentUri.ToString())
                    selectedModel = app;
            }
            list.Sort(new ApplicationViewModelComparer());
            return list;
        }

        private ApplicationViewModel _previousApplication = null;
        public ApplicationViewModel PreviousSelectedApplication
        {
            get
            {
                return _previousApplication;
            }
            set
            {
                _previousApplication = value;
                OnChange("PreviousSelectedApplication");
            }
        }

        private ApplicationViewModel _selectedApplication = null;
        public ApplicationViewModel SelectedApplication
        {
            get
            {
                return _selectedApplication;
            }
            set
            {
                PreviousSelectedApplication = _selectedApplication;
                if (_selectedApplication != value)
                {
                    _selectedApplication = value;
                    OnChange("SelectedApplication");

                    if (OnSelectedItemChanged != null)
                        OnSelectedItemChanged(this, new EventArgs());
                }
            }
        }

        private bool _isLoadingList = false;
        public bool IsLoadingList
        {
            get
            {
                return _isLoadingList;
            }
            set
            {
                _isLoadingList = value;
                OnChange("IsLoadingList");
            }
        }
    }
}