using ServiceProjectTemplate.Core;
using ServiceProjectTemplate.Core.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ServiceProjectTemplate.Service
{
    // To Install service (from Developer Command Prompt): installutil.exe ServiceProjectTemplate.Service.exe
    // To Uninstall service (from Developer Command Prompt): installutil.exe /u ServiceProjectTemplate.Service.exe

    /// <summary>
    /// The windows service. A basic admin page is accesible via a browser at 127.0.0.1:12999
    /// </summary>
    public partial class MyService : ServiceBase
    {
        private System.Timers.Timer mainTimer = new System.Timers.Timer();
        private System.Timers.Timer adminTimer = new System.Timers.Timer();

        /// <summary>
        /// The interval between the calls to MainTimer_Tick
        /// </summary>
        private double MainTimerInterval = 60000; // 60 seconds

        /// <summary>
        /// The interval between the calls to AdminTimer_Tick
        /// </summary>
        private double AdminTimerInterval = 1000; // 1 second

        //The main socket listener   
        private TcpListener RemoteAdminServer = new TcpListener(IPAddress.Any, 12999);//Dns.GetHostEntry("localhost").AddressList[0], 12999);
        //The admin client
        private TcpClient ClientConnection = new TcpClient();

        // remote admin login details
        private static string Auth_Username = "theusername"; // TODO Update these with new login details!
        private static string Auth_Password = "thepassword"; // TODO Update these with new login details!

        //A flag I've found very helpful, to keep a record of whether
        //someone is connected
        private bool ClientConnected = false;
        //Tells us if the current user has authenticated
        private bool ClientAuth = false;
        //Tells us how many time the user has tried to authenticate
        private int ClientAuthCount = 0;
        //The path of this app
        private String AppPath = AppDomain.CurrentDomain.BaseDirectory + @"\";
        //The path of the logs, relative to the app
        private String LogPath = @"Logs\";
        private String DataPath = @"Data\";
        private const String LogName_Service = "MyService.log";
        private const String LogName_Error = "Errors.log";
        private String ServiceLogPath = String.Empty;
        private String ErrorLogPath = String.Empty;

        private DateTime lastRun = DateTime.MinValue;

        public MyService()
        {
            InitializeComponent();
            ServiceLogPath = AppPath + LogPath + LogName_Service;
            ErrorLogPath = AppPath + LogPath + LogName_Error;
        }

        /// <summary>
        /// When the service is started
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            if (!System.IO.Directory.Exists(AppPath + LogPath))
            {
                System.IO.Directory.CreateDirectory(AppPath + LogPath);
            }
            if (!System.IO.Directory.Exists(AppPath + DataPath))
            {
                System.IO.Directory.CreateDirectory(AppPath + DataPath);
            }

            UpdateLog(LogType.Standard, "START - Windows Service");

            try
            {
                mainTimer = new System.Timers.Timer();
                mainTimer.Interval = MainTimerInterval;
                mainTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.MainTimer_Tick);
                mainTimer.Start();
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(ErrorLogPath, "ERROR: " + ex.ToString() + Environment.NewLine);
            }
            finally
            {
                System.IO.File.AppendAllText(ServiceLogPath, "START - Windows Service - maintimer started successfully" + Environment.NewLine);
            }

            try
            {
                adminTimer = new System.Timers.Timer();
                adminTimer.Interval = AdminTimerInterval;
                adminTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.AdminTimer_Tick);
                adminTimer.Start();
                RemoteAdminServer.ExclusiveAddressUse = true;
                RemoteAdminServer.Start();
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(ErrorLogPath, "ERROR: " + ex.ToString() + Environment.NewLine);
            }
            finally
            {
                System.IO.File.AppendAllText(ServiceLogPath, "START - Windows Service - admintimer started successfully" + Environment.NewLine);
            }
        }

        /// <summary>
        /// When the service is stopped
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                mainTimer.Stop();
                mainTimer.Dispose();
                adminTimer.Stop();
                adminTimer.Dispose();
                RemoteAdminServer.Stop();
                System.IO.File.AppendAllText(ServiceLogPath, DateTime.Now + " - STOP - Windows Service" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(ErrorLogPath, "ERROR: " + ex.ToString() + Environment.NewLine);
            }
            finally
            {
                System.IO.File.AppendAllText(ServiceLogPath, "Windows Service Stopped Successfully" + Environment.NewLine);
            }
        }

        /// <summary>
        /// Method run every MainTimer_Tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void MainTimer_Tick(object sender, System.Timers.ElapsedEventArgs args)
        {
            /*if ((DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday) && (DateTime.UtcNow.Hour == 2) && (lastRun.Day < DateTime.UtcNow.Day)) // run once a week
            {
                lastRun = DateTime.UtcNow;
                // Add work here
            }*/

            // test the service
            if ((DateTime.UtcNow.DayOfWeek == DayOfWeek.Friday) && (lastRun < DateTime.UtcNow.AddMinutes(2))) // run every 2 mins on friday
            {
                lastRun = DateTime.UtcNow;
                DoHelloWorld();
            }
        }

        private void DoHelloWorld()
        {
            // Core project services
            Services _services = new Services(UpdateLog, -1);
            _services.General.HelloWorld();
        }

        /// <summary>
        /// Method run every AdminTimer_Tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void AdminTimer_Tick(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (RemoteAdminServer.Pending())
            {
                try
                {
                    ClientConnection = RemoteAdminServer.AcceptTcpClient();
                    ClientConnected = true;
                    Thread t = new Thread(new ThreadStart(RemoteAdminGetCommands));
                    //Make sure we thread them off, then the server isn't waiting
                    //for the admin to do stuff
                    t.Name = "AdminThread";
                    t.Start();
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(ErrorLogPath, "ERROR: " + ex.ToString() + Environment.NewLine);
                }
            }
        }
        
        /// <summary>
        /// Process remote commands
        /// </summary>
        private void RemoteAdminGetCommands()
        {
            try
            {
                while (ClientConnection.Connected)
                {
                    IPEndPoint clnt = (IPEndPoint)ClientConnection.Client.RemoteEndPoint;
                    string ClientResponse = "";
                    string response = "";
                    string ExtraHeader = "";
                    int responseType = 200;
                    ClientAuth = false;

                    //Get the stream from the client; This will wait until the client
                    //decides to send something, so its essentially a while statement
                    //on one line.

                    NetworkStream strm = ClientConnection.GetStream();

                    //Put the data into a byte array
                    byte[] inputBuffer = new byte[ClientConnection.ReceiveBufferSize + 1];
                    //Get the size of it
                    int Bytes = strm.Read(inputBuffer, 0, Convert.ToInt32(ClientConnection.ReceiveBufferSize));
                    //Convert it to something useable
                    string responsetext = Encoding.ASCII.GetString(inputBuffer, 0, Bytes);
                    if (clnt.Address.ToString() != "127.0.0.1")
                    {
                        ClientAuth = doAuthentication(responsetext, ref ClientResponse, ref responseType, ref ExtraHeader);
                    }
                    else
                    {
                        ClientAuth = true;
                    }

                    //If this isn't from our office, then ignore it.
                    //We must accept the buffer first, or the connection will hang

                    if (ClientAuth == true)
                    {
                        string getText = "";
                        string postText = "";
                        string paramText = "";

                        responsetext = responsetext.Replace(@"\r", " ").Replace(@"\n", " ");

                        //Grab the get response if there is one
                        if (responsetext.Contains("GET"))
                        {
                            getText = responsetext.Substring(0, responsetext.IndexOf("HTTP") - 1);
                            if (getText.Length != (getText.IndexOf("/") + 1))
                            {
                                getText = (getText.Substring(getText.IndexOf("/") + 2, getText.Length - (getText.IndexOf("/") + 2))).Trim();
                                paramText = getText;
                            }
                            else
                            {
                                paramText = "";
                            }
                        }

                        //Grab the post response if there is one
                        if (responsetext.Contains("POST"))
                        {
                            getText = responsetext.Substring(0, responsetext.IndexOf("HTTP") - 1);
                            if (getText.Length != (getText.IndexOf("/") + 1))
                            {
                                getText = (getText.Substring(getText.IndexOf("/") + 2, getText.Length - (getText.IndexOf("/") + 2))).Trim();
                                paramText = getText;
                            }

                            postText = (responsetext.Substring(responsetext.LastIndexOf(" "), responsetext.Length - responsetext.LastIndexOf(" "))).Trim();
                            postText = WebUtility.UrlDecode(postText);
                            if (paramText.Length > 0)
                            {
                                paramText = paramText + "&" + postText;
                            }
                            else
                            {
                                paramText = postText;
                            }
                        }

                        urlParams liveParams = new urlParams(paramText);

                        //Checks the data sent by the user against the RemoteAdminCommands function
                        //to see if a response more than "OK" is needed
                        ClientResponse = RemoteAdminCommands(liveParams.ByKey("COMMAND"), liveParams);

                        if (mainTimer.Enabled == true)
                        {
                            response = "Imagine Service - Running!<BR><BR>" + Environment.NewLine;
                            response = response + "<a href=\"/?command=PAUSESERVICE\">Pause Service</a><BR><BR>" + Environment.NewLine;
                        }
                        else
                        {
                            response = "Imagine Service - Paused!<BR><BR>" + Environment.NewLine;
                            response = response + "<a href=\"/?command=RESTARTSERVICE\">ReStart Service</a><BR><BR>" + Environment.NewLine;
                        }
                    }

                    //Feed the response code back with standard HTTP headers
                    if (!ClientConnection.Connected)
                        throw new Exception("Command finished but Http connection closed in the meantime.");
                    ClientConnection.Client.Send(Encoding.ASCII.GetBytes("HTTP/1.1 " + responseType + " OK" + Environment.NewLine + "Server: FATHTTP" + Environment.NewLine + "Date: 25/08/2006" + Environment.NewLine + ExtraHeader + "Content-Type: text/html" + Environment.NewLine + "Accept-Ranges: bytes" + Environment.NewLine + "Last-Modified 02/06/2006" + Environment.NewLine + "Content-Length: " + (response + ClientResponse).Length + Environment.NewLine + "" + Environment.NewLine + response + ClientResponse));
                    ClientConnection.Close();
                }
            }
            catch (Exception ex)
            {
                ClientConnected = false;
                Thread.CurrentThread.Abort();
                System.IO.File.AppendAllText(ErrorLogPath, "ERROR WITH CLIENT CONNECTION: " + ex.ToString() + Environment.NewLine);
            }
        }



        private String RemoteAdminCommands(String Command, urlParams currentParams)
        {
            String browserresponse = String.Empty;
            browserresponse = @"<a href=""/?command=SHOWSERVICELOGS"">Show Service Logs</a><BR><BR>" + Environment.NewLine;
            browserresponse = browserresponse + @"<a href=""/?command=SHOWERRORLOGS"">Show Error Logs</a><BR><BR>" + Environment.NewLine;
            browserresponse = browserresponse + @"<a href=""/?command=RESETLOGS"">Reset/Delete Logs</a><BR><BR>" + Environment.NewLine;

            browserresponse = browserresponse + "<BR><BR>" + Environment.NewLine;

            browserresponse = browserresponse + @"<a href=""/?command=DOHELLOWWORLD"">Do Hello World Method</a><BR><BR>" + Environment.NewLine;

            browserresponse = browserresponse + "<BR><BR>" + Environment.NewLine;


            //Return the service log files inside a textarea
            if (currentParams.ByKey("Command") == "SHOWSERVICELOGS")
            {
                browserresponse = browserresponse + @"Current Service Log contents:<BR><BR><textarea cols=""100"" rows=""20"">" + Environment.NewLine;
                browserresponse = browserresponse + System.IO.File.ReadAllText(ServiceLogPath) + Environment.NewLine;
                browserresponse = browserresponse + "</textarea>" + Environment.NewLine;
            }

            //Return the error log files inside a textarea
            if (currentParams.ByKey("Command") == "SHOWERRORLOGS")
            {
                browserresponse = browserresponse + @"Current Error Log contents:<BR><BR><textarea cols=""100"" rows=""20"">" + Environment.NewLine;
                browserresponse = browserresponse + System.IO.File.ReadAllText(ErrorLogPath) + Environment.NewLine;
                browserresponse = browserresponse + "</textarea>" + Environment.NewLine;
            }

            if (currentParams.ByKey("Command") == "RESETLOGS")
            {
                System.IO.File.WriteAllText(ServiceLogPath, "");
                System.IO.File.WriteAllText(ErrorLogPath, "");
            }

            //Pause the main timer
            if (currentParams.ByKey("Command") == "PAUSESERVICE")
            {
                mainTimer.Stop();
                UpdateLog(LogType.Standard, "Pause Service");
            }

            //Restart the main timer
            if (currentParams.ByKey("Command") == "RESTARTSERVICE")
            {
                mainTimer.Enabled = true;
                mainTimer.Start();
                UpdateLog(LogType.Standard, "Restart Service");
            }

            if (currentParams.ByKey("Command") == "DOHELLOWWORLD")
            {
                DoHelloWorld();
            }

            return browserresponse;
        }

        /// <summary>
        /// Authenticate if the admin request is from a remote location i.e. not local
        /// </summary>
        /// <param name="responsetext"></param>
        /// <param name="ClientResponse"></param>
        /// <param name="responseType"></param>
        /// <param name="ExtraHeader"></param>
        /// <returns></returns>
        private bool doAuthentication(String responsetext, ref String ClientResponse, ref int responseType, ref String ExtraHeader)
        {
            //The user was not at our office, or on the localhost, so get them to authenticate
            if (responsetext.Contains("Authorization: Basic "))
            {
                string AuthString = "";
                string[] AuthResponse = null;
                //AuthString = Strings.Mid(responsetext, responsetext.IndexOf("Authorization: Basic ") + 20, (Strings.InStr((Strings.InStr(responsetext, "Authorization: Basic ") + 21), responsetext, Strings.Chr(10)) - 1) - (responsetext.IndexOf("Authorization: Basic ") + 20));

                AuthString = responsetext.Substring(responsetext.IndexOf("Authorization: Basic ") + 20, responsetext.IndexOf(@"\n", responsetext.IndexOf("Authorization: Basic ") + 21) - (responsetext.IndexOf("Authorization: Basic ") + 20));
                AuthResponse = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(AuthString)).Split(':');
                if (AuthResponse.Count() > 1)
                {
                    if (authUser(AuthResponse[0], AuthResponse[1]) == true)
                    {
                        ClientAuth = true;
                    }
                }
                if (ClientAuth == false)
                {
                    ClientAuthCount = ClientAuthCount + 1;
                    if (ClientAuthCount > 4)
                    {
                        ClientAuthCount = 0;
                        Thread.Sleep(20000);
                    }
                    else
                    {
                        ExtraHeader = "WWW-Authenticate: Basic realm=\"Speak friend and enter\"";
                    }
                    ClientAuth = false;
                    responseType = 401;
                    ClientResponse = "Computer says no" + Environment.NewLine;
                }
            }
            else
            {
                responseType = 401;
                ExtraHeader = "WWW-Authenticate: Basic realm=\"Speak friend and enter\"";
                ClientResponse = "Computer says no" + Environment.NewLine;
            }
            return ClientAuth;
        }

        /// <summary>
        /// Check username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool authUser(String username, String password)
        {
            if ((username == Auth_Username) && (password == Auth_Password))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// The log method used by the LogDelegate
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        public void UpdateLog(LogType type, String msg)
        {
            msg = DateTime.Now.ToString() + " - " + msg + Environment.NewLine;
            switch (type)
            {
                case LogType.Minor:
                    System.IO.File.AppendAllText(ServiceLogPath, "MINOR: " + msg);
                    break;
                case LogType.Error:
                    System.IO.File.AppendAllText(ServiceLogPath, "ERROR: " + msg);
                    System.IO.File.AppendAllText(ErrorLogPath, "ERROR: " + msg);
                    break;
                default:
                    System.IO.File.AppendAllText(ServiceLogPath, msg);
                    break;
            }
        }

        /// <summary>
        /// Parse the querystring parameters
        /// </summary>
        public class urlParams
        {
            public List<urlParam> theparams { get; set; }
            public urlParams(String loadingParams)
            {
                theparams = new List<urlParam>();
                if (loadingParams.Contains("="))
                {
                    var tempParams = loadingParams.Replace("/?", "").Split('&');
                    foreach (var newURL in tempParams)
                    {
                        var loadingURL = newURL.Split('=');
                        theparams.Add(new urlParam() { name = loadingURL[0].Trim(), value = WebUtility.UrlDecode(loadingURL[1]) });
                    }
                }
            }

            public String ByKey(String key)
            {
                String result = theparams.Where(x => x.name.ToLowerCase() == key.ToLowerCase()).Select(x => x.value).FirstOrDefault();
                return (!String.IsNullOrEmpty(result)) ? result : String.Empty;
            }
        }

        public class urlParam
        {
            public String name { get; set; }
            public String value { get; set; }
        }
    }
}
