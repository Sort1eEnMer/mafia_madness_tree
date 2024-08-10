// by phanto
$MM::LoadedRole_Forger = true;

if(!isObject(MMRole_Forger))
{
	new ScriptObject(MMRole_Forger)
	{
		class = "MMRole";

		name = "Forger";
		corpseName = "iniquitous coroner";
		displayName = "Forger";

		letter = "FG";

		colour = "<color:DDBB77>";
		nameColour = "0.86 0.73 0.46";

		canForge = true;

		alignment = 1;
		isEvil = 1;

		helpText = 	"\c4You are also the <color:DDBB77>Forger\c4!  Change a corpses' role by typing /forgeCorpse [role letter] while holding a body!" NL
					"\c4Your special ability is best used to throw off the Role Count and pin guilt on Innocent corpses." NL
					"\c4The ability can only be used once per day, so use it wisely!" NL
					"\c4The forge ability can also feign the death of your teammates, causing the Innocents to let their guard down!" NL
					"\c4You can also \c3fake one corpse\c4 by typing /fakebody [name] [role letter]! This corpse can be used to fake deaths!";

		description = 	"\c4The <color:DDBB77>Forger\c4 can a corpses' role by typing /forgeCorpse [role letter] while holding a body!" NL
						"\c4Your special ability is best used to throw off the Role Count and pin guilt on Innocent corpses." NL
						"\c4The ability can only be used once per day, so use it wisely!" NL
						"\c4The forge ability can also feign the death of your teammates, causing the Innocents to let their guard down!" NL
						"\c4You can also \c3fake one corpse\c4 by typing /fakebody [name] [role letter]! This corpse can be used to fake deaths!";

	};
}

//SUPPORT
function MM_getAbductionPos()
{
	if(!isObject(MMAbductionSpawns) || (%ct = MMAbductionSpawns.getCount()) == 0)
	{
		if($Pref::Server::MMDumpsterLoc !$= "")
			return $Pref::Server::MMDumpsterLoc;

		return;
	}

	return MMAbductionSpawns.getObject(getRandom(%ct - 1)).getTransform();
}

function MMRole::getCanForge(%this)
{
	return %this.canForge ? true : false;
}

function GameConnection::MM_canForge(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.MM_isMaf())
		return false;

	if(!%this.role.getCanForge())
		return false;

	return true;
}
function servercmdaa(%this) { announce(%this); }

function servercmdfakebody(%this, %n, %r)
{
	//announce("fake corpse");
	
	if(%this.role.getName() !$= "MMRole_Forger"){
		return; }
	if(%this.hasfakecorpse)
		return;

	if(!isObject(%name = findclientbyname(%n)) || !isObject(%role = $MM::RoleKey[%r])){
		messageClient(%this, '', "\c4You must enter a valid \c3name\c4 or\c3 role\c4 to place a fake corpse!"); return;
	}

	%corpse = new AIPlayer(botCorpse)
	{
		datablock = PlayerNoJet;

		originalClient = %this;
		name = %name.name;
		role = %role;

		timeOfDeath = $Sim::Time;
		causeOfDeath = 0;

		isCorpse = true;
		isFakeCorpse = true;
	};


		%corpse.setTransform(%this.player.getTransform());

		%corpse.setNodeColor("ALL", "1 0 0 1");
		%corpse.setCrouching(true);
		%corpse.playThread(3, "death1");
		messageClient(%this, '', "\c4You have created a \c3fake body\c4 of \c3" @ %name.name @ "\c4 as the role\c3 " @ %r @ "\c4!");
		%this.minigame.MM_LogEvent(%this.MM_GetName(1) SPC "\c6created a fake corpse of" SPC %name.MM_GetName(1) @" as role" SPC %role.colour @ %role.name);
	%this.hasFakecorpse = true;
}

function servercmdforgeCorpse(%this, %role)
{
	%mini = getMinigameFromObject(%this);
	if(!%this.MM_canForge()){
		return;}

	if(%this.forged[%mini.day]) {
		messageClient(%this, '', "\c4You have already forged a corpse today! Wait until the next morning to try again!"); return;}

	if(!isObject(%role = $MM::RoleKey[%role])) {
		messageClient(%this, '', "\c4You haven't entered a valid role! Please enter a valid role letter!"); return;}

	if(!isObject(%this.player.heldcorpse)){
		messageClient(%this, '', "\c4You must be holding a corpse in order to use this ability!"); return;}

	%this.player.heldcorpse.role = %role;
	messageClient(%this, '', "\c4You have forged this corpse as" SPC %role.colour @ %role.name @"\c4! You can now drop it!");
	%this.forged[%mini.day] = true;
	%mini.MM_LogEvent(%this.MM_GetName(1) SPC "\c6forged" SPC %this.player.heldcorpse.originalClient.MM_GetName(1) @"'s corpse as" SPC %role.colour @ %role.name);
}

//HOOKS
package MM_Forger
{
	function serverCmdTakeRole(%this)
	{
	if(%this.role.getName() !$= "MMRole_Amnesiac")
		return;
	if(%this.player.heldcorpse.isfakecorpse){
		messageClient(%this, '', "\c4You tried to remember the role of this corpse but it's fake!"); return; }

		parent::servercmdTakeRole(%this);
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		for(%i = 0; %i <= %mini.day; %i++)
			%client.forged[%i] = "";
			%client.hasfakecorpse = false;
	}
};deactivatePackage(MM_Forger);
activatePackage(MM_Forger);