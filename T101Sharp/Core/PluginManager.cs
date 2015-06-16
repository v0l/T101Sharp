using System;
using System.Collections.Generic;
using System.IO;
using CSScriptLibrary;

namespace T101Sharp
{
	public class PluginManager
	{
		private object _pl_lock = new object ();
		private List<IPlugin> _plugins { get; set; }

		public PluginManager ()
		{
			
		}

		public void LoadPlugins(){
			lock (_pl_lock) {
				_plugins = new List<IPlugin> ();
				if (Directory.Exists ("plugins")) {
					Console.WriteLine ("Loading plugins\n===============");
					foreach (var f in Directory.EnumerateFiles("plugins/","*.cs")) {
						try {
							IPlugin n = CSScript.Evaluator.LoadFile<IPlugin> (f);

							n.irc = MainClass.i;
							n.setting = MainClass._s;

							_plugins.Add (n);
							n.Init();
							Console.WriteLine (string.Format ("Plugin loaded: {0}", f));
						} catch (Exception ex) {
							#if DEBUG
							Console.WriteLine (string.Format ("Plugin load failed: {0}", ex));
							#endif
						}
					}
					Console.WriteLine ("===============");
				} else {
					Directory.CreateDirectory ("plugins");
				}
			}
		}

		public void OnData(Data d){
			lock (_pl_lock) {
				foreach (var pl in _plugins) {
					try {
						pl.irc = MainClass.i;
						pl.setting = MainClass._s;
						pl.OnData (d);
					} catch (Exception ex) {
						Console.WriteLine (string.Format ("Exception on plugin: {0}", ex));
					}
				}
			}
		}
	}

	public interface IPlugin
	{
		Irc irc { get; set; }
		dynamic setting { get; set; }

		void Init();
		string GetHelp();
		void OnData(Data d);
	}
}
