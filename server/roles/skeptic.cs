//cultSpy.cs
//Code for the "cult spy" mafia role. (name WIP)

if(!$MM::LoadedRole_Spy)
	exec("./spy.cs");

$MM::LoadedRole_Skeptic = true;

if(!isObject(MMRole_Skeptic))
{
	new ScriptObject(MMRole_Skeptic)
	{
		class = "MMRole";

		name = "Skeptic";
		corpseName = "expert complainer";
		displayName = "Skeptic";

		letter = "K";

		colour = "<color:806080>";
		nameColour = "0.5 0.376 0.5";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;
		canCultSleuth = true;

		alignment = 0;

		helpText = 	"\c4You are also the <color:806080>Skeptic\c4! You are able to listen in on the cult." NL
					"\c4You will be able to monitor all activity in the cultist chat, but will not know the names of those speaking through it." NL
					"\c4Use this ability to help stop the cult from becoming too powerful! But be careful, you may be targeted by the cult!";

		description = 	"\c4The <color:806080>Skeptic\c4 is are able to listen in on the cult." NL
						"\c4You will be able to monitor all activity in the cultist chat, but will not know the names of those speaking through it." NL
						"\c4Use this ability to help stop the cult from becoming too powerful!";
	};
}

//SUPPORT
function GameConnection::MM_canCultSleuth(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(%this.isGhost || %this.lives < 1)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.role.canCultSleuth)
		return false;

	return true;
}

//HOOKS
package MM_Skeptic
{
	function GameConnection::MM_CultistChat(%this, %msg, %pre2)
	{
		parent::MM_CultistChat(%this, %msg, %pre2);


		%pre = "\c7[<color:400040>Cult\c7]";

		%name = %this.garboName;

		%format = '%1\c5%2\c6: %3';

		%mini = getMiniGameFromObject(%this);
		for(%i = 0; %i < %mini.numMembers; %i++)
		{
			%mem = %mini.member[%i];
			if(%mem.MM_canCultSleuth())
				commandToClient(%mem, 'chatMessage', %this, '', '', %format, %pre, %name, %msg);
		}

		return %r;
	}
};
deactivatePackage(MM_Skeptic);
activatePackage(MM_Skeptic);