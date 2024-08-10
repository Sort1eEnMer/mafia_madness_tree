
if(!$MM::LoadedRole_Crazy)
	exec("./crazy.cs");

$MM::LoadedRole_FredDurst = true;

if(!isObject(MMRole_FredDurst))
{
	new ScriptObject(MMRole_FredDurst : MMRole_Crazy)
	{
		name = "Fred Durst";
		corpseName = "forgotten nu metal rapper";
		displayName = "Fred Durst";

		letter = "DURST";

		colour = "<color:400040>";
		nameColour = "0.376 0 0.376";

		helpText = MMRole_Crazy.helpText NL "<font:impact:28pt>\c3YOU DID IT ALL \c5FOR THE NOOKIE.";

		description = "<color:400040>The Law \c4is a rarely occurring upgraded <color:FF00FF>Crazy\c4 who also has an \c3assault rifle!";

		equipment[1] = nameToID(ChainsawItem);
	};
}


function MMRole_TheLaw::onAssign(%this, %mini, %client)
{
	parent::onAssign(%this, %mini, %client);

	schedule(100, 0, messageAll, '', "<font:impact:36pt>\c3FRED DURST\c0 HAS ENTERED THE STAGE");
}