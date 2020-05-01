using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Text.RegularExpressions;
using WebSocketSharp;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RestSharp.Authenticators;

namespace LolQMaster.Services
{
    public class LCUConnection : INotifyPropertyChanged
    {
        private int _currentSummonerIconId;
        private string _currentSummonerName;
        private string _currentSummonerId;
        private string _currentSummonerClubTag;

        private string _apiDomain;
        private AuthenticationHeaderValue _authHeader;
        private string _token;
        private string _port;

        private WebSocket _webSocket;
        private IconManager _iconManager;
        private const string SummonerIconChangedEvent = "OnJsonApiEvent_lol-summoner_v1_current-summoner";
        private const string LoggedInEvent = "OnJsonApiEvent_lol-login_v1_login-data-packet";
        private const string QueueUpEvent = "OnJsonApiEvent_lol-lobby-team-builder_v1_lobby";
        private const string LobbyChangedEvent = "OnJsonApiEvent_lol-lobby_v2_lobby";
        private const string GameEvent = "OnJsonApiEvent_lol-gameflow_v1_session";

        static readonly string _authRegexPattern = @"""--remoting-auth-token=(?'token'.*?)"" | ""--app-port=(?'port'|.*?)""";
        static readonly RegexOptions _authRegexOptions = RegexOptions.Multiline;

        public EventHandler<MessageEventArgs> WebsocketMessageEventHandler { get; private set; }
        public EventHandler<JArray> SummonerIconChangedEventHandler { get; private set; }
        public EventHandler<JArray> LoggedInEventHandler { get; private set; }
        public EventHandler<JArray> QueueUpEventHandler { get; private set; }
        public EventHandler<JArray> LobbyChangedEventHandler { get; private set; }
        public EventHandler<JArray> GameFlowSessionEventHandler { get; private set; }
        public int CurrentSummonerIconId
        {
            get => _currentSummonerIconId; internal set
            {
                _currentSummonerIconId = value;
                NotifyPropertyChanged();
            }
        }
        public string CurrentSummonerName
        {
            get => _currentSummonerName; internal set
            {
                _currentSummonerName = value;
                NotifyPropertyChanged();
            }
        }
        public string CurrentSummonerId
        {
            get => _currentSummonerId; internal set
            {
                _currentSummonerId = value;
                NotifyPropertyChanged();
            }
        }
        public string CurrentSummonerClubTag
        {
            get => _currentSummonerClubTag; internal set
            {
                _currentSummonerClubTag = value;
                NotifyPropertyChanged();
            }
        }

        public LCUConnection(IconManager iconManager)
        {
            Init(iconManager);
            SetUpConnection();

            UpdateSummonerInformation();
        }

        private void Init(IconManager iconManager)
        {
            _iconManager = iconManager;
            WebsocketMessageEventHandler += OnWebSocketMessage;
            SummonerIconChangedEventHandler += OnSummonerIconChanged;
            LoggedInEventHandler += OnLoggedIn;
            QueueUpEventHandler += OnQueueUp;
            LobbyChangedEventHandler += OnLobbyChanged;
            GameFlowSessionEventHandler += OnGameFlowSessionChanged;
        }

        private void OnGameFlowSessionChanged(object sender, JArray e)
        {
            try
            {
                var qid = int.Parse(e[2]["data"]["gameData"]["queue"]["id"].ToString());
                ChangeToQueueIcon(qid);
            }catch(Exception ex)
            {

            }
        }

        private void OnLobbyChanged(object sender, JArray e)
        {
            DoLobbyStuff();
        }

        private void OnQueueUp(object sender, JArray e)
        {
            DoLobbyStuff();
        }

        private void OnLoggedIn(object sender, JArray e)
        {
            UpdateSummonerInformation();
        }

        private void OnSummonerIconChanged(object sender, JArray e)
        {
            UpdateSummonerInformation(e[2]["data"]);
        }

        private void ChangeToQueueIcon(int queue)
        {
            var iconId = this._iconManager.GetQueueValue(queue);

            ChangeSummonerIcon(iconId);
        }

        private void ChangeSummonerIcon(int iconId)
        {
            if (iconId == -1)
            {
                return;
            }

            string URL = _apiDomain + "/lol-summoner/v1/current-summoner/icon";
            string body = "{\"profileIconId\": " + iconId + "}";

            HttpClient Client = new HttpClient();
            StringContent Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // automatically trust the certificate
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.Timeout = new TimeSpan(0, 0, 15);
            Client.DefaultRequestHeaders.Authorization = _authHeader;

            using (Client)
            {
                HttpResponseMessage Response = Client.PutAsync(URL, Content).Result;
                Response.EnsureSuccessStatusCode();

                var response = Response.Content.ReadAsStringAsync().Result;
            }
        }

        public void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            var Messages = JArray.Parse(e.Data);

            int MessageType = 0;
            if (!int.TryParse(Messages[0].ToString(), out MessageType) || MessageType != 8)
                return;

            var EventName = Messages[1].ToString();

            Console.WriteLine("Event: " + EventName + " uri " + Messages[2]["uri"]);

            switch (EventName)
            {
                case SummonerIconChangedEvent:
                    SummonerIconChangedEventHandler.Invoke(sender, Messages);
                    break;
                case LoggedInEvent:
                    //LoggedInEventHandler.Invoke(sender, Messages);
                    break;
                case QueueUpEvent:
                    //QueueUpEventHandler.Invoke(sender, Messages);
                    break;
                case LobbyChangedEvent:
                    //LobbyChangedEventHandler.Invoke(sender, Messages);
                    break;
                case GameEvent:
                    GameFlowSessionEventHandler.Invoke(sender, Messages);
                    break;
                default:
                    break;
            }
        }

        private void DoLobbyStuff()
        {
            int queueId = -1;

            var resp = SendRequest("/lol-lobby/v2/lobby");

            try
            {
                var jArray = JObject.Parse(resp);
                queueId = int.Parse(jArray["gameConfig"]["queueId"].ToString());
            }
            catch (Exception ex)
            {

            }
            this.ChangeToQueueIcon(queueId);
        }

        public void SetUpConnection()
        {
            string port;
            string token;
            GetAuth(out port, out token);


            var BaseString = string.Format("{0}:{1}", "riot", token);
            var Base64Data = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(BaseString));
            _authHeader = new AuthenticationHeaderValue("Basic", Base64Data);

            var wb = new WebSocket("wss://127.0.0.1:" + port + "/", "wamp");


            wb.SetCredentials("riot", token, true);
            wb.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            wb.OnMessage += WebsocketMessageEventHandler;

            wb.Connect();

            wb.Send("[5,\"" + SummonerIconChangedEvent + "\"]");
           // wb.Send("[5,\"" + QueueUpEvent + "\"]");
          //  wb.Send("[5,\"" + LobbyChangedEvent + "\"]");
            wb.Send("[5,\"" + GameEvent + "\"]");
          //  wb.Send("[5,\"" + LoggedInEvent + "\"]");

            _webSocket = wb;

            _token = token;
            _port = port;
            _apiDomain = String.Format("https://127.0.0.1:{0}", port);

        }

        private static void GetAuth(out string Port, out string Token)
        {
            String token = "";
            String port = "";
            var mngmt = new ManagementClass("Win32_Process");
            foreach (ManagementObject o in mngmt.GetInstances())
            {
                if (o["Name"].Equals("LeagueClientUx.exe"))
                {
                    //Console.WriteLine(o["CommandLine"]);


                    foreach (Match m in Regex.Matches(o["CommandLine"].ToString(), _authRegexPattern, _authRegexOptions))
                    {
                        if (!String.IsNullOrEmpty(m.Groups["port"].ToString()))
                        {
                            port = m.Groups["port"].ToString();
                        }
                        else if (!String.IsNullOrEmpty(m.Groups["token"].ToString()))
                        {
                            token = m.Groups["token"].ToString();
                        }
                    }
                    //return o["CommandLine"].GetType().ToString();
                }
            }
            if (String.IsNullOrEmpty(token) || String.IsNullOrEmpty(port))
            {
                throw new Exception("No League client found");
            }

            Token = token;
            Port = port;

        }

        public IEnumerable<int> OwnedIcons()
        {
            List<int> icons = new List<int>();

            string URL = String.Format("/lol-collections/v2/inventories/{0}/summoner-icons", CurrentSummonerId);

            string response = SendRequest(URL);

            var jArray = JObject.Parse(response);

            var iconArr = jArray["icons"];

            yield return -1; // for no change;

            foreach (var icon in iconArr)
            {
                yield return int.Parse(icon.ToString());
            }

            // /lol-summoner/v1/current-summoner

            // /lol-collections/v2/inventories/{summonerId}/summoner-icons

        }


        private string SendRequest(string partialUrl, object jsonBody = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => { return true; };
            RestClient client = new RestClient(_apiDomain)
            {
                Authenticator = new HttpBasicAuthenticator("riot", _token)
            };
            var req = new RestRequest(partialUrl, Method.GET);
            if (jsonBody != null)
            {
                req.AddJsonBody(new { jsonBody });
            }
            var res = client.Execute(req);
            return res.Content;
        }

        public void UpdateSummonerInformation(JToken jToken)
        {
            this.CurrentSummonerIconId = int.Parse(jToken["profileIconId"].ToString());
            this.CurrentSummonerId = jToken["summonerId"].ToString();
            this.CurrentSummonerName = jToken["displayName"].ToString();
        }
        public void UpdateSummonerInformation()
        {
            string partialUrl = "/lol-summoner/v1/current-summoner";
            string body = "";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => { return true; };


            RestClient restClient = new RestClient(_apiDomain);
            restClient.Authenticator = new HttpBasicAuthenticator("riot", _token);
            RestRequest request = new RestRequest(partialUrl, Method.GET);

            var response = restClient.Execute(request).Content.ToString();

            var jArray = JObject.Parse(response);

            this.CurrentSummonerIconId = int.Parse(jArray["profileIconId"].ToString());
            this.CurrentSummonerId = jArray["summonerId"].ToString();
            this.CurrentSummonerName = jArray["displayName"].ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
