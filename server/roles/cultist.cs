//cultist.cs
//Code for the basic Cultist role.

$MM::LoadedRole_Cultist = true;

$MM::GPCultistOnlyCommAtNight = false; //cultists will only have access to cultist chat at night
$MM::GPCultistFakeMafRecruit = true; //cultist leader will appear to attempt to recruit maf members (to avoid cultists being able to know who the maf is instantly)
$MM::GPCultistFakeDeadRecruit = true; //similar to above, but for dead players.
$MM::GPCultistRecruitUnaligned = false; //cultist leader can recruit unaligned roles
$MM::GPCultistRecruitRole = "CM"; //role that the cultist leader recruits people as
$MM::GPCultistRecruitNotifySpectators = true; //spectators will receive notifications of people joining the cult
$MM::GPCultistTouchRecruit = true; //cult leaders recruit with right-click rather than /recruit
$MM::GPCultistRecruitmentRange = 8;

$MM::Alignment[4] = "Cultist";
$MM::AlignmentColour[4] = "<color:400040>";

$MM::InvStatus[4] = '\c3%1 \c4has a bit of a <color:400040>frightening disposition\c4.';

if(!isObject(MMRole_Cultist))
{
	new ScriptObject(MMRole_Cultist)
	{
		class = "MMRole";

		name = "Cultist";
		corpseName = "devout murderer";
		displayName = "Cultist";

		letter = "CM"; //cult member??

		colour = "<color:400040>";
		nameColour = "0.376 0 0.376";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 4;
		isEvil = 1;

		description = 	"\c4The <color:400040>Cultist \c4is basic role whose goal is to eliminate both the \c2Innocents \c4and the \c0Mafia\c4.";
	};
}

if(!isObject(MMRole_CultLeader))
{
	new ScriptObject(MMRole_CultLeader)
	{
		class = "MMRole_Cultist";
		superClass = "MMRole";

		name = "Cult Leader";
		corpseName = "devout gospeler";
		displayName = "Cult Leader";

		letter = "CL";

		colour = "<color:9500B3>";
		nameColour = "0.584 0 0.702";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 4;

		helpText = 	"\c4At night, the <color:9500B3>Cult Leader \c4can choose any \c2innocent \c4to recruit into the cult." NL
					"\c4The selected player will become a cultist at dawn, losing their former role and all its powers." NL
					"\c4To select a player for recruitment, right-click them at night." NL
					"\c4You may, in some gamemodes, have to instead use the \c3/recruit \c7[PLAYER NAME] \c4command at night for recruitment.";

		description = 	"\c4The <color:9500B3>Cult Leader \c4is <color:400040>Cultist whose goal is to expand their team." NL
						"\c4At night, the <color:9500B3>Cult Leader \c4can choose any \c2innocent \c4to recruit into the cult." NL
						"\c4The selected player will become a cultist at dawn, losing their former role and all its powers." NL
						"\c4To select a player for recruitment, right-click them at night." NL
						"\c4You may, in some gamemodes, have to instead use the \c3/recruit \c7[PLAYER NAME] \c4command at night for recruitment.";

		cultRecruit = true;
	};
}

//SUPPORT
function GameConnection::MM_isCultist(%this)
{
	if(!isObject(%this.role))
		return false;

	return %this.role.getAlignment() == 4;
}

function MinigameSO::MM_GetCultList(%this)
{
	%list = "";
	for(%i = 0; %i < %this.memberCacheLen; %i++)
	{
		%mem = %this.memberCache[%i];

		%r = %this.memberCacheRole[%i];

		if(%has[%mem])
			continue;

		%has[%mem] = true;

		if(!isObject(%mem) && %r.getAlignment() == 4)
			%list = %list SPC %mem;
		else if(isObject(%mem) && %mem.MM_isCultist())
			%list = %list SPC %mem;
	}

	return trim(%list);
}

function GameConnection::MM_DisplayCultList(%this, %centrePrint)
{
	%mini = getMiniGameFromObject(%this);
	if(!isObject(%mini) || !%mini.running)
		return;

	if(%centrePrint $= "")
		%centrePrint = $MM::MafListSetting | 0;

	if(%centrePrint != 2)
		messageClient(%this, '', "<color:400040>--");

	%cStr = "";

	%list = %mini.MM_GetCultList();
	%ct = getWordCount(%list);
	for(%i = 0; %i < %ct; %i++)
	{
		%cl = getWord(%list, %i);

		if(!isObject(%r = %mini.role[%cl]))
			continue;

		%str = (isObject(%cl) ? %cl.MM_GetName(false, true) : %mini.memberCacheName[%mini.memberCacheKey[%cl]]) SPC "(" @ %r.getLetter() @ ")";

		if(%centrePrint != 2)
			messageClient(%this, '', %str);

		if(%centrePrint)
			%cStr = %cStr NL %str @ " ";
	}

	if(%centrePrint != 2)
		messageClient(%this, '', "<color:400040>--");

	if(%centrePrint)
		%this.centerPrint("<just:right><font:verdana:18><color:400040>Cultists\c6:\n<font:verdana:16>" @ trim(%cStr) @ " ");
	else
		%this.centerPrint("");
}

function serverCmdCultList(%this, %cp)
{
	if(!%this.MM_isCultist() && !(%this.lives < 1 && $MM::SpectatorMafList))
		return;

	%this.MM_DisplayCultList(%cp);
}

function GameConnection::MM_canCultComm(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.MM_isCultist())
		return false;

	if(%mini.isDay && $MM::GPCultistOnlyCommAtNight)
		return false;

	return true;
}

function MM_CultCheck(%this, %target)
{
	if(!%target.MM_isCultist() && !(%target.isGhost || %target.lives < 1))
		return 2;

	return 1;
}

function GameConnection::MM_CultistChat(%this, %msg, %pre2)
{
	if(!(%c = %this.MM_canCultComm()))
	{
		// messageClient(%this, '', "\c5You cannot use Godfather Chat because you are not the Godfather!  (^ is Godfather chat.)"); //gona just keep this message the same
		
		return 1;
	}

	if(%c == 2)
		return 1;

	%pre2 = %pre2 @ "\c7[<color:400040>Cult\c7]";

	%this.MM_Chat(%this.player, -1, %msg, "", %pre2, MM_CultCheck);

	return 1;
}

function GameConnection::MM_canCultRecruit(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(%mini.isDay)
		return false;

	if(!isObject(%this.role))
		return false;

	if(%this.isGhost || %this.lives < 1)
		return false;

	if(!%this.MM_isCultist())
		return false;

	if(!%this.role.cultRecruit && !%mini.allRecruit)
		return false;

	if(%this.recruited[%mini.day])
		return false;

	return true;
}

function GameConnection::MM_CultRecruit(%cl, %src)
{
	if(!isObject(%src))
		return;

	if(%cl.isGhost || %cl.lives < 1)
		return;

	if(%src.isGhost || %src.lives < 1)
		return;

	if(!isObject(%mini = getMiniGameFromObject(%cl)))
		return;

	%role = $MM::RoleKey[$MM::GPCultistRecruitRole];
	if(!isObject(%role))
		%role = MMRole_Cultist;

	%roleStr = %role.getColour(1) @ %role.getRoleName();

	%mini.MM_LogEvent(%src.MM_getName(1) SPC "\c6recruited" SPC %cl.MM_getName(1) SPC "\c6into the" SPC %roleStr);

	%cl.lives = 1 + %role.additionalLives;
	%cl.MM_setRole(%role);

	%cl.MM_UpdateUI();
	%cl.MM_DisplayStartText();
	%cl.clearInventory();
	%cl.MM_GiveEquipment();

	%cl.recruiter = "";
	%cl.isRecruit = true;

	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%mem = %mini.member[%i];

		if((%isc = (%mem.MM_isCultist() && %mem.lives > 0)) || (%mem.lives < 1 && $MM::GPCultistRecruitNotifySpectators))
		{
			messageClient(%mem, '', "<font:impact:24pt>\c3" @ %cl.getSimpleName() SPC "\c4has joined the cult as the" SPC %roleStr @ "\c4!");

			if(%isc)
				%mem.MM_DisplayCultList(2);
		}
	}
}

function serverCmdRecruit(%this, %v0, %v1, %v2, %v3, %v4, %v5)
{
	if(!isObject(%this.player) || !isObject(%mini = getMiniGameFromObject(%this)))
		return;

	if($MM::GPCultistTouchRecruit)
	{
		messageClient(%this, '', "\c4Right-click a player to select them for recruitment!");
		return;
	}

	if(!%this.MM_canCultRecruit())
	{
		if(%mini.isDay)
			messageClient(%this, '', "\c4You can only recruit at night!");
		else if(%this.recruited[%mini.day])
			messageClient(%this, '', "\c4You can only recruit once per night!");
		else if(%this.isRecruit && !$MM::GPCultistRecruitRecruitment)
			messageClient(%this, '', "\c4Recruited cultists cannot recruit others to the cult!");
		else
			messageClient(%this, '', "\c4You cannot recruit!");

		return;
	}

	%tname = trim(%v0 SPC %v1 SPC %v2 SPC %v3 SPC %v4 SPC %v5);
	%ccl = findClientByName(%tname);

	if(!isObject(%ccl))
	{
		%ccl = findClientByBL_ID($Pref::Server::MMNicknames[%tname]);

		if(!isObject(%ccl))
		{
			messageClient(%this, '', "\c4Could not find a player by the name of '\c3" @ %tname @ "\c4.'");
			return;
		}
	}

	if(!isObject(%ccl.role))
	{
		messageClient(%this, '', "\c4This player cannot be recruited because they are not in the current game.");
		return;
	}

	if(%ccl.lives < 1 || %ccl.isGhost)
	{
		if(!$MM::GPCultistFakeDeadRecruit)
		{
			messageClient(%this, '', "\c4This player cannot be recruited because they are dead.");
			return;
		}
		else
			%fakeRecruit = true;
	}

	if(%ccl.MM_isCultist())
	{
		messageClient(%this, '', "\c4This player cannot be recruited because they are a fellow cultist.");
		return;
	}

	if(%ccl.role.getAlignment() != 0 && !(%ccl.role.getAlignment() != 1 && $MM::GPCultistRecruitUnaligned))
	{
		if(!$MM::GPCultistFakeMafRecruit)
		{
			messageClient(%this, '', "\c4This player cannot be recruited because they are not a member of the innocents.");
			return;
		}
		else
			%fakeRecruit = true;
	}

	%mini.MM_LogEvent(%this.MM_GetName(1) SPC "\c6attempted to recruit" SPC %ccl.MM_GetName(1));

	%this.recruited[%mini.day] = true;

	if(!%fakeRecruit)
		%ccl.recruiter = %this;

	for(%i = 0; %i < %mini.numMembers; %i++)
		if(%mini.member[%i].MM_isCultist() && %mini.member[%i] != %this)
			messageClient(%mini.member[%i], '', "\c3" @ %this.getSimpleName() SPC "\c4is attempting to recruit\c3" SPC %ccl.getSimpleName() @ "\c4...");

	messageClient(%this, '', "\c4Attempting to recruit\c3" SPC %ccl.getSimpleName() SPC "\c4into the cult. They will join at dawn.");
}

//HOOKS
function MMRole_Cultist::SpecialWinCheck(%this, %mini, %client, %killed, %killer)
{
	%r = parent::SpecialWinCheck(%this, %mini, %client, %killed, %killer);

	if(%client.lives < 1)
		return 3;

	%foundCultist = false;
	%foundOther = false;
	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%mem = %mini.member[%i];

		if(%mem.lives > 0 && isObject(%mem.role))
		{
			if(%mem.MM_isCultist())
				%foundCultist = true;
			else if(%mem.role.getAlignment() == 0 || %mem.role.getAlignment() == 1)
				%foundOther = true;

			if(%foundCultist && %foundOther)
				return 4; //disallow any win if both cultists and inno/maf are present
		}
	}

	if(!%foundCultist && %foundOther)
		return 3;

	talk("All innocent and mafia are dead! The cultists win!");
	MMDebug("Cultist win", %mini, %killed, %killer, %client);

	%mini.resolved = 1;
	%mini.schedule(3000, MM_Stop);

	return 4;
}

package MM_Cultist
{
	function GameConnection::MM_DisplayAlignmentDetails(%this, %alignment)
	{
		%r = parent::MM_DisplayAlignmentDetails(%this, %alignment);

		if(%r >= 0)
			return %r;

		if(%alignment == 4)
		{
			messageClient(%this, '', "\c4You are a <color:400040>Cultist\c4! Your goal is to eliminate both the \c2Innocents \c4and the \c0Mafia\c4!");
			messageClient(%this, '', "\c4Cultists can also communicate with each other by starting their message with \c3^\c4.");
			if($MM::GPCultistOnlyCommAtNight)
				messageClient(%this, '', "\c4You can only use cultist chat at night, so make good use of it!");
			messageClient(%this, '', "\c4Type \c3/cultList \c4to see the list of cult members again.");

			%this.schedule(0, MM_DisplayCultList);
			%this.schedule(10000, MM_DisplayCultList, 2);

			return 4;
		}

		return %r;
	}

	function MMRole::onChat(%role, %mini, %this, %msg, %type)
	{
		%r = parent::onChat(%role, %mini, %this, %msg, %type);

		%mark = getSubStr(%msg, 0, 1);
		if(%mark !$= "^")
			return %r;

		if(%type < 1)
			return 1;

		%rMsg = getSubStr(%msg, 1, strLen(%msg) - 1);

		return %this.MM_CultistChat(%rMsg);
	}

	function MMRole::onTeamChat(%role, %mini, %this, %msg, %type)
	{
		%r = parent::onTeamChat(%role, %mini, %this, %msg, %type);

		%mark = getSubStr(%msg, 0, 1);
		if(%mark !$= "^")
			return %r;

		if(%type < 1)
			return 1;

		%rMsg = getSubStr(%msg, 1, strLen(%msg) - 1);

		return %this.MM_CultistChat(%rMsg);
	}

	function MinigameSO::MM_onDay(%this)
	{
		parent::MM_onDay(%this);

		for(%i = 0; %i < %this.numMembers; %i++)
		{
			%mem = %this.member[%i];

			if(isObject(%mem.recruiter))
				%mem.MM_CultRecruit(%mem.recruiter);
		}
	}

	function GameConnection::MM_canComm(%this)
	{
		if(%this.role.getAlignment() == 4 && !($MM::GPCultistOnlyCommAtNight && getMiniGameFromObject(%this).isDay))
			return 2;

		return parent::MM_canComm(%this);
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		%client.isRecruit = false;
		%client.recruiter = "";

		for(%i = 0; %i < %mini.day; %i++)
			%client.recruited[%i] = false;
	}

	function MMRole::onTrigger(%this, %mini, %client, %obj, %slot, %val) //ok we're not just making a class thing since we need to also account for allAbduct
	{
		parent::onTrigger(%this, %mini, %client, %obj, %slot, %val); //Call base MMRole functionality (most likely nothing)

		if(!$MM::GPCultistTouchRecruit || !isObject(%client.player) || %client.getControlObject() != %client.player || !%client.MM_canCultRecruit())
			return;

		// talk(%slot SPC %val);
		if(%slot != 4 || !%val)
			return;

		%start = %obj.getEyePoint();
		%vec = %obj.getEyeVector();
		%end = VectorAdd(%start, VectorScale(%vec, $MM::GPCultistRecruitmentRange));

		%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
		%aObj = firstWord(%ray);
		if(!isObject(%aObj) || %aObj.getClassName() !$= "Player" || !isObject(%cl = %aObj.getControllingClient()))
			return;

		if(isObject(%aObj.client) && %aObj.client.MM_isCultist())
		{
			messageClient(%client, '', "\c3" SPC %aObj.client.getSimpleName() SPC "\c4is a fellow cultist! You can't recruit them!");
			return;
		}

		%mini.MM_LogEvent(%client.MM_GetName(1) SPC "\c6attempted to recruit" SPC %aObj.client.MM_GetName(1));

		%client.recruited[%mini.day] = true;
		%aObj.client.recruiter = %client;

		for(%i = 0; %i < %mini.numMembers; %i++)
			if(%mini.member[%i].MM_isCultist() && %mini.member[%i] != %client)
				messageClient(%mini.member[%i], '', "\c3" @ %client.getSimpleName() SPC "\c4is attempting to recruit\c3" SPC %aObj.client.getSimpleName() @ "\c4...");

		messageClient(%client, '', "\c4Attempting to recruit\c3" SPC %aObj.client.getSimpleName() SPC "\c4into the cult. They will join at dawn.");
	}
};
activatePackage(MM_Cultist);