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

namespace ObjectModel
{
    public class KeyAction
    {
        public enum ActionType
        {
            ACTION_NO = 0,
            ACTION_BING = 1,
            ACTION_SEARCH = 2,
            ACTION_RUNAPPLICATION = 3,
            ACTION_URL = 4,
            ACTION_EXE = 5,
            ACTION_STOCK = 6,
            ACTION_KEYEVENT = 7
        }

        public ActionType type;
        public string uri;
        public int KeyCode;
    }
}