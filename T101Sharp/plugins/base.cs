using System;
using System.Collections.Generic;
using T101Sharp;

public class Base { 

	public Irc irc { get; set; }
	public dynamic setting { get; set; }

	private bool authSent;

	public void Init(){
		//do nothing
	}

	public string GetHelp(){
		return string.Format ("base plugin, you probably dont want to remove this or things wont work :)");
	}

	public void OnData(Data d){
		switch(d.type){
			case "NOTICE":
			{
				if (!authSent) {
					irc.Nick (setting.Nick);
					irc.User (setting.Nick, setting.Nick);
					authSent = true;
				}
				break;
			}
			case "PING":
			{
				irc.Pong (d.message);
				break;
			}
			case "376":
			{
				if ((bool)setting.Oper) {
					string[] od = setting.OperDetails.ToObject<string[]> ();
					irc.Oper (od [0], od [1]);
				}
				foreach (var c in setting.DefaultChans.ToObject<List<string>>()) {
					irc.Join (c);
				}
				break;
			}
		}
	}
}