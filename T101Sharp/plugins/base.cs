using System;
using System.Collections.Generic;
using T101Sharp;

public class Base { 

	public Irc irc { get; set; }
	public dynamic setting { get; set; }
	
	private bool authSent;
	
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
					foreach (var c in setting.DefaultChans.ToObject<List<string>>()) {
						irc.Join (c);
					}
					break;
				}
			
		}
	}
}