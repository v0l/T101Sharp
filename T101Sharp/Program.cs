using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace T101Sharp
{
	class MainClass
	{
		public static ServerSettings s ;
		public static Irc i;

		public static void Main (string[] args)
		{
			LoadSettings();

			if (s != null) {
				i = new Irc (s.Server, s.Port, s.useTLS);
		
				i.OnData += (Data d) => {
					switch (d.type) {
					case "NOTICE":
						{
							if (d.target == "Auth" && d.message.Contains ("*** Looking")) {
								i.Nick (s.Nick);
								i.User (s.Nick, s.Nick);
							}
							break;
						}
					case "PING":
						{
							i.Pong (d.message);
							break;
						}
					case "376":
						{
							foreach (var c in s.DefaultChans) {
								i.Join (c);
							}
							break;
						}
					}
				};
				i.Connect ();
				i.ReadLoop ();
			} else {
				Console.WriteLine ("Config file missing");
				Console.ReadKey ();
			}
		}

		public static void LoadSettings(){
			if (File.Exists ("options.conf")) {
				using(FileStream fs = new FileStream("options.conf", FileMode.Open)){
					using(StreamReader r = new StreamReader(fs)){
						s = JsonConvert.DeserializeObject<ServerSettings> (r.ReadToEnd ());
					}
				}
			}
		}
	}
}
