using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace T101Sharp
{
	public class User {
		public bool isServer {get;set;}
		public string nick {get;set;}
		public string realname { get; set; }
		public string hostname {get;set;}
	}

	public class Data{
		public string RawMsg { get; set; }
		public User user { get; set; }
		public string type { get; set; }
		public string target { get; set; }
		public string message { get; set; }
	}

	public class Irc
	{
		private bool _useSSL;
		private EndPoint _rip;
		private string _hostname;
		private Socket _s;
		private NetworkStream _ns;
		private StreamReader _sr;
		private StreamWriter _sw;
		private SslStream _ss;
		private Thread _t;
		private bool _running;

		//Events
		public delegate void OnConnectComplete (Irc s);
		public event OnConnectComplete OnConnect;
		public delegate void OnAuthComplete (SslStream s);
		public event OnAuthComplete OnAuth;
		public delegate void OnDataRecv (Data d);
		public event OnDataRecv OnData;

		public Irc (string server, int port, bool useSSL = false)
		{
			var entry = Dns.GetHostEntry (server);
			if (entry.AddressList.Length > 0) {
				_hostname = server;
				_rip = new IPEndPoint (entry.AddressList[0], port);
				_useSSL = useSSL;
				_running = true;
			} else {
				throw new InvalidDataException ("Ip address was not recognised");
			}
		}

		public void Connect(){
			if (_s == null) {
				_s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}

			if (!_s.Connected) {
				Console.WriteLine (string.Format ("\nConnecting to: {0}\n", _hostname));
				_s.BeginConnect (_rip, new AsyncCallback (_OnConnect), _s);
			}
		}

		public void Disonnect(){
			if (_s != null && _s.Connected) {
				_s.Close ();
			}
		}

		private bool ValidateCert(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		private void _OnConnect(IAsyncResult ar){
			try{
				_s.EndConnect(ar);


				_ns = new NetworkStream(_s);
				if(_useSSL){
					_ss = new SslStream(_ns, false, new RemoteCertificateValidationCallback(ValidateCert), null);
					_ss.BeginAuthenticateAsClient(_hostname, new AsyncCallback(_OnAuth), _ss);
				}else{
					_sr = new StreamReader(_ns);
					_sw = new StreamWriter(_ns);
				}



				if(OnConnect != null){
					OnConnect(this);
				}
			}catch{

			}
		}

		public void ReadLoop(){
			_t = new Thread(new ThreadStart(RecvLoop));
			_t.Start();
			_t.Join ();
		}

		private void ParseData(string d){
			if (!string.IsNullOrEmpty (d)) {
				Console.WriteLine(d);

				var ret = new Data (){
					RawMsg = d
				};

				string[] prefix = d.Split (new char[]{' '},4);
				if (prefix.Length == 4) {
					if (!string.IsNullOrEmpty (prefix [0])) {
						if (prefix [0].Contains ("@")) {
							string[] ud = prefix [0].StartsWith (":") ? prefix [0].Substring (1).Split ('@') : prefix [0].Split ('@');
							string[] nud = ud [0].Split ('!');
							ret.user = new T101Sharp.User {
								nick = nud [0],
								realname = nud [1],
								hostname = ud [1]
							};
						} else {
							ret.user = new T101Sharp.User {
								isServer = true,
								hostname = prefix [0].Contains (":") ? prefix [0].Substring (1) : prefix [0]
							};
						}
					}

					if (!string.IsNullOrEmpty (prefix [1])) {
						ret.type = prefix [1];
					}

					if (!string.IsNullOrEmpty (prefix [2])) {
						ret.target = prefix [2];
					}

					ret.message = prefix.Length > 3 ? (prefix [3].StartsWith (":") ? prefix [3].Substring (1) : prefix [3]) : null;
				} else if (prefix.Length == 2 && prefix [0] == "PING") {
					ret.type = "PING";
					ret.message = prefix[1].StartsWith(":") ? prefix [1].Substring (1) : prefix[1];
				}

				if (OnData != null) {
					OnData (ret);
				}
			}
		}

		private void RecvLoop()
		{
			while (_running) {
				if (_s != null && _s.Connected && (_useSSL ? (_ss != null && _ss.IsAuthenticated) : _ns != null) && _sr != null) {
					string data = _sr.ReadLine ();
					if (!string.IsNullOrEmpty (data)) {
						ParseData (data);
					}
				} else {
					Thread.Sleep (20);
				}
			}
		}

		private void _OnAuth(IAsyncResult ar){
			_ss.EndAuthenticateAsClient (ar);

			_sr = new StreamReader(_ss);
			_sw = new StreamWriter(_ss);

			if (OnAuth != null) {
				OnAuth (_ss);
			}
		}

		public async Task<bool> WriteAsync(byte[] data, int offset, int len)
		{
			bool ret = false;
			try{
				if (_s != null && _s.Connected) {
					if (_useSSL) {
						await _ss.WriteAsync (data, offset, len);
					} else {
						await _ns.WriteAsync (data, offset, len);
					}
					ret = true;
				}
			}catch{
			}

			return ret;
		}

		public bool Write(string msg)
		{
			bool ret = false;
			try{
				if (_s != null && _s.Connected) {
					_sw.Write (msg);
					_sw.Flush();
					ret = true;
				}
			}catch(Exception ex){
				#if DEBUG
				Console.WriteLine(string.Format("Write error: {0}", ex.Message));
				#endif
			}

			return ret;
		}

		public void Nick(string nick){
			Write (string.Format ("NICK {0}\n", nick));
		}

		public void User(string user, string realname, int mode = 0){
			Write (string.Format("USER {0} {1} * :{2}\n", user, mode, realname));
		}

		public void Join(string chan){
			Write (string.Format ("JOIN {0}\n", chan));
		}

		public void Pong(string m){
			Write (string.Format ("PONG {0}\n", m));
		}

		public void Privmsg(string t, string m){
			Write (string.Format ("PRIVMSG {0} :{1}\n",t, m));
		}

		public void Notice(string t, string m){
			Write (string.Format ("NOTICE {0} :{1}\n",t, m));
		}

		public void Oper(string u, string p){
			Write (string.Format ("OPER {0} {1}\n",u, p));
		}
	}
}

