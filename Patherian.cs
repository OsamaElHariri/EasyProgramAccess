using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace EasyProgramAccess
{
    // This class handles getting the paths of the various open applications
    // For example, it gets the url's of open tabs in all browser, the path in all Windows Explorers, and the paths of other open applications
    class Patherian
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWind, int nCmdShow);
        [DllImport("user32")]
        private static extern
        IntPtr SendMessage(IntPtr handle, int Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_CLOSE = 0x0010;

        // Opens all paths in the given array
        // Expects an array of strings as paths (eg. {"C:\\Program Files\\SomePath\\app.exe", "https://www.google.com"}).
        public static void OpenProcesses(string[] paths)
        {
            foreach (string path in paths)
            {
                try
                {
                    Process.Start(path);
                }
                catch (Exception)
                {
                    // Skip the faulty path and move on to the next one
                    continue;
                }
            }
        }

        // Opens all paths in the given array
        // Expects a list of strings as paths.
        public static void OpenProcesses(List<string> paths)
        {
            foreach (string path in paths)
            {
                try
                {
                    Process.Start(path);
                }
                catch (Exception)
                {
                    // Skip the faulty path and move on to the next one
                    continue;
                }
            }
        }



        // Return the url contained in the passed AutomationElement
        public static string GetUrlInUrlBar(AutomationElement elmUrlBar)
        {
            try
            {
                // Since the elmUrlBar is the URL bar in some browser, reading the value will give the url of the open tab
                string url = ((ValuePattern)elmUrlBar.GetCurrentPattern(ValuePattern.Pattern)).Current.Value;
                if ("".Equals(url))
                {
                    return "";
                }

                if (!url.StartsWith("http"))
                {
                    url = "http://" + url;
                }
                return url;
            }
            catch (Exception)
            {
                return "";
            }
        }



        // Returns the path of a Windows Explorer window
        public static string GetWindowsExplorerUrl(IntPtr handle)
        {
            foreach (SHDocVw.InternetExplorer wndw in new SHDocVw.ShellWindows())
            {
                if ((int)handle == wndw.HWND)
                {
                    return new Uri(wndw.LocationURL).LocalPath;
                }
            }
            return null;
        }


        // Returns the name and url of all open chrome tabs in a window
        public static IDictionary<string, string> GetChromeNameAndUrl(IntPtr handle, string title)
        {
            // Bring the browser forward
            ShowWindow(handle, 3);
            SetForegroundWindow(handle);

            Dictionary<string, string> chromeTab = new Dictionary<string, string>();
            // The title is trimmed because it contains the string " - Google Chrome" at the end
            var trimmedTitle = title.Substring(0, title.Length - 16);

            // to find the tabs, we first find the tab that is currently open and has the title that is the same as the title of the prowser handle 
            AutomationElement root = AutomationElement.FromHandle(handle);
            Condition condNewTab = new PropertyCondition(AutomationElement.NameProperty, trimmedTitle + "");
            AutomationElement elmNewTab = root.FindFirst(TreeScope.Descendants, condNewTab);
            // get the tabstrip by getting the parent of the tab we found 
            TreeWalker treewalker = TreeWalker.ControlViewWalker;
            AutomationElement elmTabStrip = treewalker.GetParent(elmNewTab);
            // loop through all the tabs and get the names which is the page title 
            Condition condTabItem = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem);

            // Store the UI element that holds the Url
            AutomationElement elmUrlBar = GetChromeUrlBar(handle);
            
            // For each tab, read the Url, and press ctrl + TAB to move on to the next tab
            // This is done because reading the URL bar only gives the URL of the open tab
            foreach (AutomationElement tabitem in elmTabStrip.FindAll(TreeScope.Children, condTabItem))
            {
                SendKeys.SendWait("^{TAB}");
                var url = GetUrlInUrlBar(elmUrlBar);
                if ("".Equals(url))
                {
                    continue;
                }
                chromeTab[tabitem.Current.Name] = url;


            }

            return chromeTab;
        }


        // Returns the name and url of all open firefox tabs in a window
        public static IDictionary<string, string> GetFirefoxNameAndUrl(IntPtr handle, string title)
        {
            // Bring the browser forward
            ShowWindow(handle, 3);
            SetForegroundWindow(handle);

            Dictionary<string, string> firefoxTab = new Dictionary<string, string>();
            // The title is trimmed because it contains the string " - Mozilla Firefox" at the end
            var trimmedTitle = title.Substring(0, title.Length - 18);

            // to find the tabs, we first find the tab that is currently open and has the title that is the same as the title of the prowser handle
            AutomationElement root = AutomationElement.FromHandle(handle);
            Condition condNewTab = new PropertyCondition(AutomationElement.NameProperty, trimmedTitle + "");
            AutomationElement elmNewTab = root.FindFirst(TreeScope.Descendants, condNewTab);
            // get the tabstrip by getting the parent of the tab we found 
            TreeWalker treewalker = TreeWalker.ControlViewWalker;
            AutomationElement elmTabStrip = treewalker.GetParent(elmNewTab);
            // loop through all the tabs and get the names which is the page title 
            Condition condTabItem = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem);

            // Store the UI element that holds the Url
            AutomationElement elmUrlBar = GetFirefoxUrlBar(handle);
            
            // For each tab, read the Url, and press ctrl + TAB to move on to the next tab
            // This is done because reading the URL bar only gives the URL of the open tab
            foreach (AutomationElement tabitem in elmTabStrip.FindAll(TreeScope.Children, condTabItem))
            {
                var url = GetUrlInUrlBar(elmUrlBar);
                if ("".Equals(url))
                {
                    continue;
                }
                firefoxTab[tabitem.Current.Name] = url;
                SendKeys.SendWait("^{TAB}");


            }

            return firefoxTab;
        }


        // Find Google Chrome's UrlBar and returns it
        public static AutomationElement GetChromeUrlBar(IntPtr handle)
        {
            // find the automation element
            AutomationElement elm = AutomationElement.FromHandle(handle);

            AutomationElement elm1 = elm.FindFirst(TreeScope.Children,
              new PropertyCondition(AutomationElement.NameProperty, "Google Chrome"));
            AutomationElement elm2 = TreeWalker.RawViewWalker.GetLastChild(elm1);
            // "Address and search bar" is the name of the URL bar UI Element in Google Chrome
            AutomationElement elmUrlBar = elm2.FindFirst(TreeScope.Descendants,
              new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"));

            return elmUrlBar;
        }


        // Find Google Firefox's UrlBar and returns it
        public static AutomationElement GetFirefoxUrlBar(IntPtr handle)
        {
            // find the automation element
            AutomationElement element = AutomationElement.FromHandle(handle);
            // "Search or enter address" is the name of the URL bar UI Element in Firefox, as well as another element
            var nameCondition = new PropertyCondition(AutomationElement.NameProperty, "Search or enter address");
            // We need the UI element with name "Search or enter address" AND type Edit to get the URL bar
            var controlCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
            var condition = new AndCondition(nameCondition, controlCondition);
            AutomationElement editBox = element.FindFirst(TreeScope.Subtree, condition);

            return editBox;
        }



        // Get names and paths of all open processes
        // This is the central method of this class
        public static Dictionary<string, string> GetAllPaths()
        {
            Dictionary<string, string> windowInfo = new Dictionary<string, string>();
            // firstHandle and storedFirst will be used to return the foremost window to the foreground, since browsers might have jumped to the front
            IntPtr firstHandle = new IntPtr();
            bool storedFirst = false;

            foreach (KeyValuePair<IntPtr, string> window in OpenWindowGetter.GetOpenWindows())
            {
                IntPtr handle = window.Key;
                string title = window.Value;
                string path = OpenWindowGetter.GetProcessPath(handle);

                // Leave the System processes alone, as well as this application
                if ("WINDOWS".Equals(title) || "Photos".Equals(title) || "Calculator".Equals(title) || "Settings".Equals(title) || "Easy Program Access".Equals(title))
                {
                    continue;
                }

                // Call the method that gets the Windows Explorer paths
                if (path.ToLower().Contains("C:\\WINDOWS\\Explorer.EXE".ToLower()))
                {
                    try
                    {
                        windowInfo[title] = GetWindowsExplorerUrl(handle);
                    }
                    catch (Exception)
                    {

                        continue;
                    }
                    continue;
                }

                // Call the method that gets the Chrome paths
                if (path.ToLower().Contains("Google\\Chrome\\Application\\chrome.exe".ToLower()))
                {
                    try
                    {
                        GetChromeNameAndUrl(handle, title).ToList().ForEach(
                                        x => windowInfo[x.Key] = x.Value);
                    }
                    catch (Exception)
                    {

                        continue;
                    }
                    continue;


                }

                // Call the method that gets the Firefox paths
                if (path.ToLower().Contains("Mozilla Firefox\\firefox.exe".ToLower()))
                {
                    try
                    {
                        GetFirefoxNameAndUrl(handle, title).ToList().ForEach(
                                       x => windowInfo[x.Key] = x.Value);
                    }
                    catch (Exception)
                    {

                        continue;
                    }
                    continue;
                }

                // If we reached here, then the path is stored as is
                windowInfo[title] = path;
                
                if (storedFirst) continue;
                storedFirst = true;
                firstHandle = handle;


            }

            SetForegroundWindow(firstHandle);

            return windowInfo;

        }


        // Closes all the open process windows
        public static void CloseAllProcesses()
        {
            foreach (KeyValuePair<IntPtr, string> window in OpenWindowGetter.GetOpenWindows())
            {
                IntPtr handle = window.Key;
                string title = window.Value;
                string path = OpenWindowGetter.GetProcessPath(handle);

                // Leave the System process alone
                if ("WINDOWS".Equals(title) || "Photos".Equals(title) || "Calculator".Equals(title) ||
                    "Settings".Equals(title) || "Easy Program Access".Equals(title))
                {
                    continue;
                }
                SendMessage(handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }

        }


        // Shut down the computer
        public static void ShutDown()
        {
            CloseAllProcesses();
            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }


        /// <summary>Contains functionality to get all the open windows.</summary>
        public static class OpenWindowGetter
        {

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);

            [DllImport("USER32.DLL")]
            private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

            [DllImport("USER32.DLL")]
            private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("USER32.DLL")]
            private static extern int GetWindowTextLength(IntPtr hWnd);

            [DllImport("USER32.DLL")]
            private static extern bool IsWindowVisible(IntPtr hWnd);

            [DllImport("USER32.DLL")]
            private static extern IntPtr GetShellWindow();



            /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
            /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
            public static IDictionary<IntPtr, string> GetOpenWindows()
            {
                IntPtr shellWindow = GetShellWindow();

                Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

                EnumWindows(delegate (IntPtr hWnd, int lParam)
                {
                    if (hWnd == shellWindow) return true;
                    if (!IsWindowVisible(hWnd)) return true;

                    int length = GetWindowTextLength(hWnd);
                    if (length == 0) return true;

                    StringBuilder builder = new StringBuilder(length);
                    GetWindowText(hWnd, builder, length + 1);

                    windows[hWnd] = builder.ToString();
                    return true;

                }, 0);

                return windows;
            }

            /// <summary>
            /// Retrieves the Path of a running process. 
            /// </summary>
            /// <param name="hwnd"></param>
            /// <returns></returns>
            public static string GetProcessPath(IntPtr hwnd)
            {
                try
                {
                    uint pid = 0;
                    GetWindowThreadProcessId(hwnd, out pid);
                    Process proc = Process.GetProcessById((int)pid); //Gets the process by ID. 
                    return proc.MainModule.FileName;    //Returns the path. 
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

            public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);


        }
    }
}

