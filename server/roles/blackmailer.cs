
$MM::LoadedRole_Blackmailer = true;


if(!isObject(MMRole_Blackmailer))

{

	new ScriptObject(MMRole_Blackmailer)

	{

		class = "MMRole";


		name = "Blackmailer";

		corpseName = "bloodthirsty extortionist";

		displayName = "Blackmailer";


		letter = "BM";


		colour = "<color:B7777C>";

		nameColour = "0.8 0.4 0.4";


		canAbduct = false;

		canInvestigate = false;

		canImpersonate = false;

		canCommunicate = false;

		canFingerprint = false;

		canBlackmail = true;

        
	alignment = 1;


		helpText = 	"\c4You are also the <color:B7777C>Blackmailer\c4! Once a night you can do /blackmail (playername) to silence someone for that night." NL
					"\c4This ability is best used on roles which provide vital information to the innocents." NL
					"\c4You cannot use this ability on your fellow mafia.";

		description = 	"\c4The <color:B7777C>Blackmailer\c4 is a role that can silence one innocent player a night with /blackmail (playername)." NL
		                "\c4It's primary targets should compose of roles that can give the innocents important information.";

	};

}



function MinigameSO::MM_ClearBlackMails(%this)
{
	for(%i = 0; %i < %this.numMembers; %i++) {
		%this.member[%i].player.BMgagged = 0;
		%this.member[%i].player.gagged = 0;
	}
}

package MM_Blackmailer

{

	// Controls blackmail gagging

	function serverCmdMessageSent(%this, %msg)
	{
		if(isObject(%this.player) && !%this.player.isGhost)
		{
			if(%this.player.BMgagged != 1) {
				parent::serverCmdMessageSent(%this,%msg); 
				return;
			}

			//talk("gagged" SPC %this.getPlayerName());
			messageClient(%this, '',"\c4You try to vocalize, but someone has you <color:cc5555>blackmailed\c4!");
			return;	
		}
		parent::serverCmdMessageSent(%this,%msg);
		return;
	}


	function serverCMDblackmail(%c, %v)
	{
		if(!isObject(%mini = getMiniGameFromObject(%c)))
			return;
		%v = findClientbyName(%v);
		if(!%v.MM_isMaf())
		{
			if(%c.blackmailed[%mini.day])
			{
				messageClient(%c, '', "\c4You can only blackmail one person per night!");
				return;
			}
			if(%mini.isDay)
			{
				messageClient(%c, '', "\c4You can only blackmail a person at night!");
				return;
			}
			if(%v.player.BMgagged)
			{
				messageClient(%c, '', "\c4This player is already blackmailed!");
				return;
			}
			%mini.MM_LogEvent(%c.MM_GetName(1) SPC "\c6attempted to blackmail" SPC %v.MM_GetName(1));
			for(%i = 0; %i < %mini.numMembers; %i++)
				if(%mini.member[%i].MM_isMaf() && %mini.member[%i] != %c)
					messageClient(%mini.member[%i], '', "\c3" @ %c.getSimpleName() SPC "\c4is attempting to blackmail\c3" SPC %v.getSimpleName() @ "\c4...");
			%v.player.BMgagged = 1;
			%v.player.gagged = 1;
			%c.blackmailed[%mini.day] = 1;
			messageClient(%c, '', "\c4Blackmailed\c3" SPC %v.getSimpleName() @ "\c4...");
		}
		else
		{
			messageClient(%c, '', "\c3" SPC %v.getSimpleName() SPC "\c4is a fellow mafia! You can't blackmail them!");
			return;
		}
	}

	function MinigameSO::MM_onDay(%this)
	{
		parent::MM_onDay(%this);
		%this.schedule(0, MM_ClearBlackMails);
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		Parent::onCleanUp(%this, %mini, %client);
		%client.player.BMgagged = 0;
		for(%i = 0; %i < %mini.day; %i++) {
			%client.blackmailed[%i] = 0;
			%client.player.gagged = 0;
			%client.player.BMgagged = 0;
		}
	}
};

deactivatepackage(MM_Blackmailer);

activatepackage(MM_Blackmailer);