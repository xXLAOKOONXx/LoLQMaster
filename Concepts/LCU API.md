# LCU
## Connection Information
To connect to the LCU you need to get a port and a token/passphrase from the League Client.
In https://github.com/PixelHir/lolav there is a nice implementation for this step.
```C#
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


                    foreach (Match m in Regex.Matches(o["CommandLine"].ToString(), authRegexPattern, authRegexOptions))
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
```

## Get Requests
To send a Get request you need to send a GET request towards 127.0.0.1:PORT/API with a basic authentication (username: "riot", password: "TOKEN")
PORT and TOKEN therefore need to be recieved as mentioned in Connection Information.
To find out the API there are a couple of methods. Using 127.0.0.1:PORT/help you will get a json response listing all connections. The HTTP requests start with their method (eg `GET`), the and the API listeners start with `OnJsonApiEvent`. The uri path that u need is not explicit mentioned there, but you can build it up yourself (uppercase => lowercase and / or - before).
A much more convenient method is using https://github.com/Pupix/rift-explorer .
Some APIs might need additional information eather in the body, as parameter or in the url itself.
Trial and error works here pretty good to figure that out.

## Post Requests
POST requests work in the same schema as GET requests work.

## Socket Connection
To recieve information about live events such as Queue Start you either need to send GET requests on a regular basis or you go a more convenient way in using a socket to subscribe to specific events and await and handle the socket traffic.

To set up a socket you need a listener method, taking in the arguments `object` and `MessageEventArgs`.
Example:
```C#
private static async void OnWebsocketMessage(object sender, MessageEventArgs e)
```
This is the method taking on all the websocket messages. Inside this method is the real beauty of your application.
To enable this method as such, you need to open a socket.
WebSocketSharp is one library helping in that regards, it is used in this application and the following examples.
Code to open a connection:
```C#
            // get port and token for the current league session
            string port;
            string token;
            GetAuth(out port, out token);

            var wb = new WebSocket("wss://127.0.0.1:" + port + "/", "wamp");

            wb.SetCredentials("riot", token, true);
            // LCU uses SSL
            wb.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            // add method to be called on message received
            wb.OnMessage += OnWebsocketMessage;

            wb.Connect();

```
Once the connection is established you can add endpoints to listen to.
To get a full List of those events using 127.0.0.1:PORT/help is really helpful, as you can identify the events by the prefix `OnJsonApiEvent` and use can use the second half of the method as event path.
For example the method `OnJsonApiEvent_lol-lobby_v1_lobby` is an event you can listen to on the partial uri `lol-lobby_v1_lobby`.
To subscribe to such an event you  need to send a request to LCU as the following:
```C#
wb.Send("[5,\"lol-lobby_v1_lobby\"]");
```
Remember the websocket is as long active as you do not close him. So if you do no longer need the websocket close him.
```C#
            wb.Close();
```