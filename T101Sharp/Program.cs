using System;
using System.Dynamic;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace T101Sharp
{
	class MainClass
	{
		public static DynamicDictionary _s ;
		public static Irc i;
		public static PluginManager pl;

		public static void Main (string[] args)
		{
			
			LoadSettings();

			dynamic s = _s;

			if (s != null && !string.IsNullOrEmpty(s.Server)) {
				pl = new PluginManager ();
				pl.LoadPlugins ();

				i = new Irc (s.Server, (int)s.Port, s.useTLS);
		
				i.OnData += (Data d) => {
					pl.OnData(d);
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
				using (FileStream fs = new FileStream ("options.conf", FileMode.Open)) {
					using (StreamReader r = new StreamReader (fs)) {
						_s = JsonConvert.DeserializeObject<DynamicDictionary> (r.ReadToEnd ());
					}
				}
			} else {
				using (FileStream fs = new FileStream ("options.conf",FileMode.Create, FileAccess.ReadWrite)) {
					using (StreamWriter r = new StreamWriter (fs)) {
						_s = new DynamicDictionary ();
						r.Write (JsonConvert.SerializeObject (_s,Formatting.Indented));
					}
				}
			}
		}
	}
}
