/*
 * Application API
 * (C) ultrashot
 * 
 * See "ApplicationAPI - Readme.txt" for overview.
 * See "ApplicationAPI - Terms of usage.txt" for terms of usage.
*/
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class ApplicationApi
{

    /// <summary>
    /// MAKE SURE YOU CHANGE IT!
    /// <remarks>
    /// This is against of DLL-hell issue that still exists in WP7
    /// </remarks>
    /// </summary>
    private const string AppShortName = "SK";

    #region "Installation/Uninstallation"

    /// <summary>
    /// Class representing application installation context
    /// </summary>
    public class Application
    {

        public enum InstallationState : uint
        {
            AppInstall = 0,
            AppInstallComplete = 1,
            AppUpdate = 2,
            AppUpdateComplete = 3,
            AppRemove = 4,
            AppRemoveComplete = 5,
            AppDownload = 8,
            AppDownload2 = 9,
            AppDownloadComplete = 10,
            AppInstallProgress = 11,
            AppUpdateProgress = 12
        };

        private enum EventReceiveError : uint
        {
            Error_First = 0xFFFFFFFE,
            Error_NoData = 0xFFFFFFFE,
            Error_NoListener = 0xFFFFFFFF
        };

        public struct ApplicationEvent
        {
            public InstallationState state;
            public UInt32 progress;
            public bool error;
        };

        private UInt32 _context = 0;

        /// <summary>
        /// Create new Application object 
        /// </summary>
        /// <param name="guid"></param>
        public Application(Guid guid)
        {
            if (instance == null)
                Initialize();
            _context = instance.BeginDeploy(null, guid.ToString());
        }

        /// <summary>
        /// Create new Application object 
        /// </summary>
        /// <param name="guid"></param>
        public Application(string fileName, Guid guid)
        {
            if (instance == null)
                Initialize();
            _context = instance.BeginDeploy(fileName, guid.ToString());
        }

        /// <summary>
        /// Create new Application object 
        /// </summary>
        /// <param name="guid"></param>
        public Application(string fileName, string guid)
        {
            if (instance == null)
                Initialize();
            _context = instance.BeginDeploy(fileName, guid);
        }

        /// <summary>
        /// Create new Application object 
        /// </summary>
        /// <param name="guid"></param>
        public Application(string fileName)
        {
            if (instance == null)
                Initialize();
            _context = instance.BeginDeploy(fileName, null);
        }

        /// <summary>
        /// Checks if current context is valid.
        /// </summary>
        /// <returns>TRUE if it context is valid, FALSE otherwise</returns>
        public bool IsValidContext()
        {
            return (_context > 0) ? true : false;
        }

        /// <summary>
        /// Checks if application is already installed.
        /// </summary>
        /// <returns>TRUE if installed, FALSE otherwise</returns>
        public bool IsInstalled()
        {
            if (instance == null)
                Initialize();
            return instance.IsInstalled(_context);
        }

        /// <summary>
        /// Starts installation.
        /// </summary>
        /// <returns>TRUE on success, FALSE otherwise</returns>
        /// <remarks>Success means that you would be able to use WaitForEvent</remarks>
        public bool Install()
        {
            if (instance == null)
                Initialize();
            return instance.Install(_context, false);
        }

        /// <summary>
        /// Starts updating.
        /// </summary>
        /// <returns>TRUE on success, FALSE otherwise</returns>
        /// <remarks>Success means that you would be able to use WaitForEvent</remarks>
        public bool Update()
        {
            if (instance == null)
                Initialize();
            return instance.Install(_context, true);
        }

        /// <summary>
        /// Starts uninstallation.
        /// </summary>
        /// <returns>TRUE on success, FALSE otherwise</returns>
        /// <remarks>Success means that you would be able to use WaitForEvent</remarks>
        public bool Uninstall()
        {
            if (instance == null)
                Initialize();
            return instance.Uninstall(_context);
        }

        /// <summary>
        /// Waits for incoming notifications.
        /// </summary>
        /// <returns>Returns ApplicationEvent with current information about install/update process.
        /// Sets .error = true on failure.
        /// </returns>
        public ApplicationEvent WaitForEvent()
        {
            if (instance == null)
                Initialize();
            ApplicationEvent evt = new ApplicationEvent();
            UInt32 data = instance.WaitForEvents(_context, 0xFFFFFFFF);

            if (data >= (uint)EventReceiveError.Error_First)
            {
                evt.error = true;
                return evt;
            }
            UInt32 state = 0, progress = 0, error = 0;
            ApplicationApi.instance.DecodeState(data, out state, out progress, out error);


            //uint st = state;
            evt.state = (InstallationState)state;//Enum.ToObject(typeof(ApplicationApi.ApplicationContext.InstallationState), state);

            if (progress > 100)
                progress = 0;
            evt.progress = progress;
            evt.error = (error > 0) ? true : false;
            return evt;
        }


        ~Application()
        {
            if (instance == null)
                Initialize();

            if (_context != 0)
                instance.EndDeploy(_context);
        }
    }

    #endregion 
    #region "Application info"

    /// <summary>
    /// Application Info class
    /// </summary>
    public class ApplicationInfo
    {

        private enum GuidIndex : uint
        {
            ProductID = 0,
            InstanceID = 1,
            OfferID = 2
        };

        private enum AppInfoIntegerIndex : uint
        {
            AppId = 0,
            IsNotified = 1,
            AppInstallType = 2,
            AppState = 3,
            IsRevoked = 4,
            IsUpdateAvailable = 5,
            IsUninstallable = 6,
            IsThemable = 7,
            Rating = 8,
            AppId2 = 9
        };

        private enum AppInfoStringIndex : uint
        {
            DefaultTask = 0,
            Title = 1,
            ApplicationIcon = 2,
            InstallFolder = 3,
            DataFolder = 4,
            Genre = 5,
            Publisher = 6,
            Author = 7,
            Description = 8,
            Version = 9,
            ImagePath = 10
        };


        private UInt32 _appInfo = 0;

        /// <summary>
        /// Creates a new ApplicationInfo object and fullfills it with an actual information.
        /// </summary>
        /// <param name="guid"></param>
        public ApplicationInfo(string guid)
        {
            if (instance == null)
                Initialize();
            _appInfo = instance.GetApplicationInfo(guid);
        }

        /// <summary>
        /// Creates a new ApplicationInfo object and fullfills it with an actual information.
        /// </summary>
        /// <param name="guid"></param>
        public ApplicationInfo(Guid guid)
        {
            _checkInstance();
            _appInfo = instance.GetApplicationInfo(guid.ToString());
        }

        /// <summary>
        /// Creates a new ApplicationInfo object and fullfills it with an actual information.
        /// </summary>
        /// <param name="guid"></param>
        public ApplicationInfo(UInt32 nativeAppInfo)
        {
            _checkInstance();
            _appInfo = nativeAppInfo;
        }

        ~ApplicationInfo()
        {
            _checkInstance();
            if (_appInfo != 0)
                instance.ReleaseApplicationInfo(_appInfo);
        }

        /// <summary>
        /// Returns invoke information.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <returns>True on success, false otherwise</returns>
        public bool GetInvocationInfo(out string uri, out string parameters)
        {
            _checkInstance();
            if (_appInfo == 0)
            {
                uri = "";
                parameters = "";
                return false;
            }
            return instance.ApplicationInfoGetInvocationInfo(_appInfo, out uri, out parameters);
        }

        /// <summary>
        /// Returns main application uri
        /// </summary>
        /// <returns></returns>
        public string GetUri()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string uri = "", parameters = "";
            if (instance.ApplicationInfoGetInvocationInfo(_appInfo, out uri, out parameters) == true)
            {
                if (uri == null)
                    uri = "";
                return uri;
            }
            return "";
        }

        public string GetParameters()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string uri = "", parameters = "";
            if (instance.ApplicationInfoGetInvocationInfo(_appInfo, out uri, out parameters) == true)
            {
                if (parameters == null)
                    parameters = "";
                return parameters;
            }
            return "";
        }
        /// <summary>
        /// Returns Product ID
        /// </summary>
        /// <returns></returns>
        public Guid ProductID()
        {
            _checkInstance();
            if (_appInfo == 0)
                return Guid.Empty;
            string guidString = instance.ApplicationInfoGetGuid(_appInfo, (uint)GuidIndex.ProductID);
            if (guidString.Length == 0)
                return Guid.Empty;
            return new Guid(guidString);
        }

        /// <summary>
        /// Returns Instance ID
        /// </summary>
        /// <returns></returns>
        public Guid InstanceID()
        {
            _checkInstance();
            if (_appInfo == 0)
                return Guid.Empty;
            string guidString = instance.ApplicationInfoGetGuid(_appInfo, (uint)GuidIndex.InstanceID);
            if (guidString.Length == 0)
                return Guid.Empty;
            return new Guid(guidString);
        }

        /// <summary>
        /// Returns Offer ID
        /// </summary>
        /// <returns></returns>
        public Guid OfferID()
        {
            _checkInstance();
            if (_appInfo == 0)
                return Guid.Empty;
            string guidString = instance.ApplicationInfoGetGuid(_appInfo, (uint)GuidIndex.OfferID);
            if (guidString.Length == 0)
                return Guid.Empty;
            return new Guid(guidString);
        }

        /// <summary>
        /// Returns Application Id.
        /// </summary>
        /// <returns></returns>
        public UInt64 AppId()
        {
            _checkInstance();
            if (_appInfo == 0)
                return 0;
            UInt64 ull1 = (UInt64)instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.AppId);
            UInt64 ull2 = (UInt64)instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.AppId2);
            UInt64 result = (ull2 << 32) | ull1;
            return result;
        }

        /// <summary>
        /// Checks if application is notified
        /// </summary>
        /// <returns></returns>
        public UInt32 IsNotified()
        {
            _checkInstance();
            if (_appInfo == 0)
                return 0;
            return instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.IsNotified);
        }

        /// <summary>
        /// Application installation types.
        /// </summary>
        public enum AppInstallationType : uint
        {
            Marketplace = 0,
            System = 1,
            Oem = 2,
            External = 3,
            Last = 3,
            Unknown = 0xFFFFFFFF
        };

        /// <summary>
        /// Checks application installation type.
        /// </summary>
        /// <returns></returns>
        public AppInstallationType AppInstallType()
        {
            _checkInstance();
            if (_appInfo == 0)
                return 0;
            UInt32 type = instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.AppInstallType);
            if (type <= (uint)AppInstallationType.Last)
                return (AppInstallationType)type;
            return AppInstallationType.Unknown;
        }


        /// <summary>
        /// Checks application state.
        /// </summary>
        /// <returns></returns>
        public UInt32 AppState()
        {
            _checkInstance();
            if (_appInfo == 0)
                return 0;
            return instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.AppState);
        }

        /// <summary>
        /// Checks if application is revoked
        /// </summary>
        /// <returns></returns>
        public UInt32 IsRevoked()
        {
            _checkInstance();
            if (_appInfo == 0)
                return 0;
            return instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.IsRevoked);
        }

        /// <summary>
        /// Checks if an update is available.
        /// </summary>
        /// <returns></returns>
        public UInt32 IsUpdateAvailable()
        {
            _checkInstance();
            if (_appInfo == 0)
                return 0;
            return instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.IsUpdateAvailable);
        }

        /// <summary>
        /// Checks if application is uninstallable
        /// </summary>
        /// <returns></returns>
        public UInt32 IsUninstallable()
        {
            _checkInstance();
            if (_appInfo == 0)
                return 0;
            return instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.IsUninstallable);
        }

        /// <summary>
        /// Checks if application is themable
        /// </summary>
        /// <returns></returns>
        public UInt32 IsThemable()
        {
            _checkInstance();
            if (_appInfo == 0)
                return 0;
            return instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.IsThemable);

        }

        /// <summary>
        /// Returns application rating on marketplace.
        /// </summary>
        /// <returns></returns>
        public UInt32 Rating()
        {
            _checkInstance();
            if (_appInfo == 0)
                return 0;
            return instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.Rating);

        }

        /// <summary>
        /// Returns correctly application title [Reinterpreted]
        /// </summary>
        /// <returns></returns>
        public string Title()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Title);
            string result = instance.ReinterpreteString(str);
            return result;
        }

        /// <summary>
        /// Returns default task
        /// </summary>
        /// <returns></returns>
        public string DefaultTask()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.DefaultTask);
            return str;
        }

        /// <summary>
        /// Returns application icon path [Reinterpreted]
        /// </summary>
        public string ApplicationIcon
        {
            get
            {
                _checkInstance();
                if (_appInfo == 0)
                    return "";
                string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.ApplicationIcon);
                string result = instance.ReinterpreteString(str);
                return result;
            }
            set
            {
                _checkInstance();
                if (_appInfo == 0)
                    return;
                instance.UpdateAppIconPath(ProductID().ToString(), value);
            }
        }

        /// <summary>
        /// Returns application's install folder
        /// </summary>
        /// <returns></returns>
        public string InstallFolder()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.InstallFolder);
            return str;
        }

        /// <summary>
        /// Returns application's data folder
        /// </summary>
        /// <returns></returns>
        public string DataFolder()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.DataFolder);
            return str;
        }

        /// <summary>
        /// Returns application's genre
        /// </summary>
        /// <returns></returns>
        public string Genre()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Genre);
            return str;
        }

        /// <summary>
        /// Returns application's publisher [Reinterpreted]
        /// </summary>
        /// <returns></returns>
        public string Publisher()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Publisher);
            string result = instance.ReinterpreteString(str);
            return result;

        }

        /// <summary>
        /// Returns application's author [Reinterpreted]
        /// </summary>
        /// <returns></returns>
        public string Author()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Author);
            string result = instance.ReinterpreteString(str);
            return result;
        }

        /// <summary>
        /// Returns application's description [Reinterpreted]
        /// </summary>
        /// <returns></returns>
        public string Description()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Description);
            string result = instance.ReinterpreteString(str);
            return result;
        }

        /// <summary>
        /// Returns application version 
        /// </summary>
        /// <returns></returns>
        public string Version()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Version);
            return str;
        }

        /// <summary>
        /// Returns path to EXE that is used to run the application.
        /// </summary>
        /// <returns></returns>
        public string ImagePath()
        {
            _checkInstance();
            if (_appInfo == 0)
                return "";
            string str = instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.ImagePath);
            return str;
        }

        /// <summary>
        /// Terminates application processes
        /// </summary>
        public void Terminate()
        {
            _checkInstance();
            if (_appInfo == 0)
                return;
            instance.TerminateApplicationProcesses(ProductID().ToString());
        }
    }
    #endregion 
    #region "File associations"

    public static class FileAssocation
    {

        /* all about HKCR\.smth */

        /// <summary>
        /// Returns handler class of certain extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetClass(string extension)
        {
            _checkInstance();
            return instance.GetAssocationClass(extension, 0);
        }

        /// <summary>
        /// Returns backup handler class of certain extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetBackupClass(string extension)
        {
            _checkInstance();
            return instance.GetAssocationClass(extension, 1);
        }

        /// <summary>
        /// Sets new handler class for certain extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static bool SetClass(string extension, string className)
        {
            _checkInstance();
            return instance.SetAssocationClass(extension, className);
        }

        /// <summary>
        /// Backups handler class for certain extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static bool BackupClass(string extension)
        {
            _checkInstance();
            return instance.BackupAssocationClass(extension);
        }

        /* all about HKCR\SomeClass */

        /// <summary>
        /// Creates a new handler class
        /// </summary>
        /// <param name="className"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool CreateClass(string className, string command)
        {
            _checkInstance();
            return instance.CreateAssocationClass(className, command);
        }

        /// <summary>
        /// Removes certain handler class
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static bool RemoveClass(string className)
        {
            _checkInstance();
            return instance.RemoveAssocationClass(className);
        }
    }
    #endregion
    #region "Sessions, tasks, pages"

    /// <summary>
    /// Starts new session with specified uri
    /// </summary>
    /// <param name="uri">URI</param>
    public static void LaunchSession(string uri)
    {
        if (instance == null)
            Initialize();
        instance.LaunchSessionByUri(uri);
    }

    /// <summary>
    /// Starts new sesson with specified appId and token.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="token">Application token</param>
    public static void LaunchSession(UInt64 appId, string token)
    {
        _checkInstance();
        instance.LaunchSession(appId, token);
    }

    /// <summary>
    /// Starts specified control panel applet.
    /// </summary>
    /// <param name="cpl">Canonical applet name OR GUID OR empty string.</param>
    public static void LaunchCplApplet(string cpl)
    {
        _checkInstance();
        instance.LaunchCplApplet(cpl);
    }

    /// <summary>
    /// Starts specified uri with certain arguments
    /// </summary>
    /// <param name="uri">URI</param>
    /// <param name="args">Arguments</param>
    public static void LaunchTask(string uri, string args)
    {
        _checkInstance();
        LaunchTask(uri, args);
    }

    /// <summary>
    /// Checks application running in background
    /// </summary>
    /// <param name="uri">URI of foreground application</param>
    /// <param name="pageId">PageId of foreground application</param>
    public static void GetForegroundPageUri(out string uri, out string pageId)
    {
        _checkInstance();
        instance.GetForegroundPageUri(out uri, out pageId);
    }

    #endregion 
    #region "Application lists"

    /// <summary>
    /// Returns list of all applications
    /// </summary>
    /// <returns></returns>
    public static List<ApplicationInfo> GetAllApplications()
    {
        _checkInstance();
        UInt32 iterator = instance.GetAllApplicationsIterator();
        return GoThroughIterator(iterator);
    }

    /// <summary>
    /// Returns list of certain hub type applications
    /// </summary>
    /// <param name="hubType"></param>
    /// <returns></returns>
    public static List<ApplicationInfo> GetAllApplications(UInt32 hubType)
    {
        _checkInstance();
        UInt32 iterator = instance.GetApplicationsOfHubTypeIterator(hubType);
        return GoThroughIterator(iterator);
    }

    /// <summary>
    /// Returns list of all visible applications
    /// </summary>
    /// <returns></returns>
    public static List<ApplicationInfo> GetAllVisibleApplications()
    {
        _checkInstance();
        UInt32 iterator = instance.GetAllVisibleApplicationsIterator();
        return GoThroughIterator(iterator);
    }

#endregion
    #region "Resource code"
    /// <summary>
    /// Replaces string written in "@AppResLib.dll,-123"-like form with correct information.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ReinterpreteString(string str)
    {
        _checkInstance();
        string result = instance.ReinterpreteString(str);
        if (result == null)
            result = "";
        return result;
    }
    #endregion
    #region "EXE/EXE7"

    public class NativeExe
    {
        private string _fileName;
        private UInt32 _handle;

        private bool _wasCopied = false;


        /// <summary>
        /// Returns true if file is EXE7
        /// </summary>
        private bool IsExe7
        {
            get
            {
                if (_fileName == null)
                    return false;
                if (_fileName.Contains("."))
                {
                    string ext = _fileName.Substring(_fileName.LastIndexOf(".")).ToLower();
                    if (ext == ".exe7")
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// File name
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public NativeExe()
        {
        }

        public NativeExe(string newFileName)
        {
            FileName = newFileName;
        }

        ~NativeExe()
        {
            Cleanup();
        }

        /// <summary>
        /// Run application
        /// </summary>
        /// <param name="arguments">Arguments that will be passed to main()</param>
        /// <param name="accountName">Name of account in which an app will run</param>
        /// <param name="getHandle">Save handle or not (required for Wait())</param>
        /// <returns>true on success, false otherwise</returns>
        public bool Run(string arguments, string accountName = "", bool getHandle = false)
        {
            if (_fileName == null)
                return false;

            _checkInstance();

            string realFileName = FileName;
            if (IsExe7 == true)
            {
                realFileName = FileName.Substring(0, FileName.Length - 1);
                CopyFile(FileName, realFileName);
                _wasCopied = true;
            }

            UInt32 handle;
            if (instance.CreateProcess(realFileName, arguments, accountName, out handle) == true)
            {
                if (getHandle == false)
                {
                    instance.CloseHandle7(handle);
                    _handle = 0;
                }
                else
                {
                    _handle = handle;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes EXE7->EXE intermediate file if it was created.
        /// </summary>
        public void Cleanup()
        {
            if (IsExe7 && _wasCopied && FileName != null)
            {
                string realFileName = FileName;
                realFileName = FileName.Substring(0, FileName.Length - 1);
                RemoveFile(realFileName);
                _wasCopied = false;
            }
        }

        public const UInt32 WaitingFailed = 0xFFFFFFFF;

        /// <summary>
        /// Waits for EXE to close.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>Returns 0 on success, WaitingFailed on failure</returns>
        public UInt32 Wait(UInt32 timeout = 0xFFFFFFFF)
        {
            _checkInstance();
            if (_handle > 0)
            {
                UInt32 result = instance.WaitForSingleObject7(_handle, timeout);
                return result;
            }
            return WaitingFailed;
        }
    }
    #endregion
    #region "Important WINAPI functions"

    /// <summary>
    /// Gets current locale code.
    /// </summary>
    /// <returns></returns>
    public static UInt32 GetLocaleId()
    {
        _checkInstance();
        return instance.GetLocale();
    }

    /// <summary>
    /// Copy certain file to a new location. Replaces any file that would be there.
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    public static bool CopyFile(string src, string dest)
    {
        _checkInstance();
        return instance.CopyFile(src, dest);
    }

    /// <summary>
    /// Remove certain file.
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static bool RemoveFile(string src)
    {
        _checkInstance();
        return instance.RemoveFile(src);
    }

    #endregion 
    #region "Internal code"

    private static List<ApplicationInfo> GoThroughIterator(UInt32 iterator)
    {
        List<ApplicationInfo> list = new List<ApplicationInfo>();
        if (iterator != 0)
        {
            while (true)
            {
                UInt32 appInfo = instance.GetNextApplication(iterator);
                if (appInfo == 0)
                    break;
                ApplicationInfo info = new ApplicationInfo(appInfo);
                list.Add(info);
            }
            instance.ReleaseIterator(iterator);
        }
        return list;

    }
    /// <summary>
    /// Checking if instance is up and running, creating new instance if it isn't.
    /// </summary>
    private static void _checkInstance()
    {
        if (instance == null)
        {
            if (Initialize() == false)
            {
                //MessageBox.Show("Cannot instanciate Application API", "Application API", MessageBoxButton.OK);
                throw new Exception("Cannot instanciate Application API");
            }
        }
    }

    public static IXapHandler instance = null;

    /// <summary>
    /// Initializes Application API instance
    /// </summary>
    /// <returns></returns>
    public static bool Initialize()
    {
        if (instance == null)
        {
            uint retval = 0;
            try
            {
                retval = Microsoft.Phone.InteropServices.ComBridge.RegisterComDll("ComXapHandler" + AppShortName + ".dll", new Guid("7E6418C7-C93F-4B82-947E-83FEA7A757CC"));
            }
            catch (Exception ex)
            {
                return false;
            }
            instance = (IXapHandler)new CXapHandler();
            return (instance != null) ? true : false;
        }
        return true;
    }

   

    [ComImport, Guid("55D492CE-1269-4102-8079-5FC729F93FA3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IXapHandler
    {
        UInt32 BeginDeploy([MarshalAs(UnmanagedType.BStr)]string path, [MarshalAs(UnmanagedType.BStr)]string productID);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsInstalled(UInt32 context);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool Install(UInt32 context, [MarshalAs(UnmanagedType.Bool)]bool update);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool Uninstall(UInt32 context);

        UInt32 WaitForEvents(UInt32 context, UInt32 timeout);

        void DecodeState(UInt32 data, out UInt32 state, out UInt32 progress, out UInt32 error);

        UInt32 EncodeState(UInt32 state, UInt32 progress, UInt32 error);

        void EndDeploy(UInt32 context);

        void LaunchSessionByUri([MarshalAs(UnmanagedType.BStr)]string uri);
        void LaunchSession([MarshalAs(UnmanagedType.U8)] UInt64 ulAppId, [MarshalAs(UnmanagedType.BStr)]string pszTokenId);
        void LaunchCplApplet([MarshalAs(UnmanagedType.BStr)]string pszCpl);
        void LaunchTask([MarshalAs(UnmanagedType.BStr)]string pszTaskUri, [MarshalAs(UnmanagedType.BStr)]string pszCmdLineArguments);
        void GetForegroundPageUri([MarshalAs(UnmanagedType.BStr)] out string szTaskUri, [MarshalAs(UnmanagedType.BStr)] out  string szPageId);

        UInt32 GetApplicationInfo([MarshalAs(UnmanagedType.BStr)]string guidString);
        void ReleaseApplicationInfo(UInt32 appInfo);

        [return: MarshalAs(UnmanagedType.BStr)]
        string ApplicationInfoGetGuid(UInt32 appInfo, UInt32 dwIndex);

        [return: MarshalAs(UnmanagedType.BStr)]
        string ApplicationInfoGetString(UInt32 appInfo, UInt32 dwIndex);

        UInt32 ApplicationInfoGetInteger(UInt32 appInfo, UInt32 dwIndex);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool ApplicationInfoGetInvocationInfo(UInt32 appInfo, [MarshalAs(UnmanagedType.BStr)] out string pszUri, [MarshalAs(UnmanagedType.BStr)] out string pszParameters);

        UInt32 GetAllApplicationsIterator();
        UInt32 GetAllVisibleApplicationsIterator();
        UInt32 GetApplicationsOfHubTypeIterator(UInt32 hubType);
        UInt32 GetNextApplication(UInt32 iterator);

        void ReleaseIterator(UInt32 iterator);

        [return: MarshalAs(UnmanagedType.BStr)]
        string ReinterpreteString([MarshalAs(UnmanagedType.BStr)] string oldString);

        void UpdateAppIconPath(string guid, [MarshalAs(UnmanagedType.BStr)]string path);
        void TerminateApplicationProcesses([MarshalAs(UnmanagedType.BStr)]string guid);

        UInt32 GetLocale();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetAssocationClass([MarshalAs(UnmanagedType.BStr)]string extension, int type);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool SetAssocationClass([MarshalAs(UnmanagedType.BStr)]string extension, [MarshalAs(UnmanagedType.BStr)]string className);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool BackupAssocationClass([MarshalAs(UnmanagedType.BStr)]string extension);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool CreateAssocationClass([MarshalAs(UnmanagedType.BStr)]string className, [MarshalAs(UnmanagedType.BStr)]string openCommand);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool RemoveAssocationClass([MarshalAs(UnmanagedType.BStr)]string className);
        
        [return: MarshalAs(UnmanagedType.Bool)]
        bool CopyFile([MarshalAs(UnmanagedType.BStr)] string sourceFile, [MarshalAs(UnmanagedType.BStr)] string destFile);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool RemoveFile([MarshalAs(UnmanagedType.BStr)] string sourceFile);


        [return: MarshalAs(UnmanagedType.Bool)]
        bool CreateProcess([MarshalAs(UnmanagedType.BStr)] string path, [MarshalAs(UnmanagedType.BStr)] string arguments, [MarshalAs(UnmanagedType.BStr)] string accountName, out UInt32 handle);
        
        UInt32 WaitForSingleObject7(UInt32 handle, UInt32 dwMilliseconds);
        
        [return: MarshalAs(UnmanagedType.Bool)]
        bool CloseHandle7(UInt32 handle);

    }


    [ComImport, ClassInterface(ClassInterfaceType.None), Guid("7E6418C7-C93F-4B82-947E-83FEA7A757CC")]
    public class CXapHandler
    {

    }

    #endregion 

}

