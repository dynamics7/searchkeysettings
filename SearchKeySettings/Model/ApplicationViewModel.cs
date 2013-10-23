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
using SearchKeySettings;
using System.Collections.Generic;
namespace ObjectModel
{
    public class ApplicationViewModel : BaseViewModel
    {
        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnChange("Title");
            }
        }

        private string _author;
        public string Author
        {
            get
            {
                if (_author.ToLower() == "customurltext")
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder(300);
                    try
                    {
                        InteropSvc.InteropLib.Instance.RegistryGetString7(InteropSvc.InteropLib.HKEY_LOCAL_MACHINE, "Software\\OEM\\WebSearchOverride", "Url", sb, 300);
                    }
                    catch (Exception ex)
                    {
                    }
                    if (sb.ToString().Contains("http"))
                        return sb.ToString();
                    return LocalizedResources.CustomUrlText;
                }
                return _author;
            }
            set
            {
                _author = value;
                OnChange("Author");
            }
        }

        public KeyAction action;

        private Visibility _visible = Visibility.Visible;
        public Visibility Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                OnChange("Visible");
            }

        }

        public bool Contains(string s)
        {
            if (_title.ToLower().Contains(s) || _author.ToLower().Contains(s))
                return true;
            return false;
        }

        private bool _isSpecial = false;
        public bool IsSpecial
        {
            get
            {
                return _isSpecial;
            }
            set
            {
                _isSpecial = value;
                OnChange("IsSpecial");
                OnChange("SpecialVisibility");
            }
        }

        public Visibility SpecialVisibility
        {
            get
            {
                if (_isSpecial)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public int SpecialItemIndex { get; set; }
    }

    public class ApplicationViewModelComparer : IComparer<ApplicationViewModel>
    {

        public int Compare(ApplicationViewModel x, ApplicationViewModel y)
        {
            if (x.IsSpecial && !y.IsSpecial)
                return -1;
            else if (!x.IsSpecial && y.IsSpecial)
                return 1;
            else if (x.IsSpecial && y.IsSpecial)
                return x.SpecialItemIndex - y.SpecialItemIndex;
            return x.Title.CompareTo(y.Title);
        }
    }
}