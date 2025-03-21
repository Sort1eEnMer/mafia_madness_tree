//admin.cs
//Various administration commands.

$MM::LoadedAdmin = true;

$MM::AdminToolsMidGame = true;
$MM::NotifyHostMidGame = true;

$MM::AnnounceByPrefix = true;
$MM::AnnouncePrefix = "#";

function serverCmdPassTime(%this)
{
	if(!%this.isSuperAdmin)
		return;

	if(!isObject(%mini = getMinigameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return;

	echo(%this.getSimpleName() SPC "passed to the next day/night period.");

	%mini.MM_DayCycle(!%mini.isDay);
}

function serverCmdClearInv(%this, %target)
{
	if(!%this.isAdmin)
		return;

	%cl = findClientByName(%target);
	if(!isObject(%cl))
	{
		messageClient(%this, '', "\c4Could not find client by name of '\c3" @ %target @ "\c4'");
		return;
	}

	%cl.clearInventory();
	schedule(%cl.getPing(), 0, serverCmdUnUseTool, %cl);
	if(isObject(%cl.player))
		%cl.player.schedule(%cl.getPing(), unMountImage, 0);

	messageClient(%this, '', "\c4Cleared \c3" @ %cl.getSimpleName() @ "\c4's inventory.");
}

function serverCmdGenerateRolesFromGamemode(%this, %gamemodeQuery, %numberOfPlayers) 
{
	%minigame = getMinigameFromObject(%this);

	if(!%this.isSuperAdmin)
		return;
	if(!isObject(%minigame) || !%minigame.isMM || %minigame.running)
		return;

	if(!getMinigameFromObject(%this))
		return messageClient(%this, '', "\c4You are not in a Mafia Madness minigame.");
	if(%gamemodeQuery $= "")
		return messageClient(%this, '', "\c4Please provide a gamemode name or ID as your first argument.");
	if(%numberOfPlayers $= "")
		return messageClient(%this, '', "\c4Please provide a number of players as your second argument.");

	%gamemode = %gamemodeQuery;

	if($MM::GameMode[%gamemodeQuery] $= "") {
		%gamemodeIdByName = MM_FindGameModeByName(%gamemodeQuery, true);
		if(%gamemodeIdByName == -1) 
			return messageClient(%this, '', "\c4Could not find gamemode by string or ID.");
		%gamemode = %gamemodeIdByName;
	}

	if(!$MM::GameModeIsCustom[%gamemode])
		return messageClient(%this, '', "\c4This command is is meant for MM2 gamemodes.");

	messageClient(%this, '', "\c4Found requested gamemode:\c5" SPC $MM::GameMode[%gamemode]);
	messageAll('', "\c4ALL: Attempting to test role generation for a gamemode. It is not safe to start.");
	if(!$MMDebug)
		messageClient(%this, '', "\c4You do not have \c5$MMDebug\c4 as a truthy value. If you enable \c5$MMDebug\c4, you will get a very granular breakdown of why roles are generating.");

	// This command requires modifying global variables.
	// We want to keep this around to clean up back to our previous state.
	%currentMode = $MM::CurrentMode;

	// Set the mode.
	%minigame.MM_SetGameMode(%gamemode);
	// Force it's load.
	MM_ModeReadyCustom();
	// Ready to run roles function.
	%roleList = MM_InitModeCustom(%minigame, true, %numberOfPlayers);

	messageClient(%this, '', "\c4Roles generated:\c5" SPC %roleList);

	// Get back now.
	%minigame.MM_SetGameMode(%currentMode);
	messageAll('', "\c4ALL: Test role generation over. It is now safe to start.");
}

function serverCmdIsolateChat(%this, %v0, %v1, %v2, %v3, %v4, %v5)
{
	if(!%this.isAdmin)
		return;

	%name = %this.getPlayerName();

	%target = trim(%v0 SPC %v1 SPC %v2 SPC %v3 SPC %v4 SPC %v5);

	if(%target $= "")
	{
		if($MM::IsolateChat)
		{
			$MM::IsolateChat = false;

			for(%i = 0; %i < $MM::IsolateChatMems; %i++)
			{
				%obj = $MM::IsolateChatMem[%i];

				if(isObject(%obj))
					%obj.isolated = "";

				$MM::IsolateChatMem[%i] = "";
			}

			$MM::IsolateChatMems = "";
			messageAll('', "<font:impact:24>\c3" @ %name SPC "\c0disabled isolated chat.");

			echo(%name SPC "disabled isolated chat.");
			deactivatePackage(MM_IsolatedChat);
		}

		return;
	}

	%obj = findClientByName(%target);

	if(!isObject(%obj))
	{
		messageClient(%this, '', "\c4Could not find client \c3'" @ %target @ "'");
		return;
	}

	if(!$MM::IsolateChat)
	{
		messageAll('', "<font:impact:24>\c3" @ %name SPC "\c0enabled isolated chat.");
		echo(%name SPC "enabled isolated chat.");
		activatePackage(MM_IsolatedChat);
	}

	$MM::IsolateChat = true;

	%oName = %obj.getPlayerName();

	if(!%obj.isolated)
	{
		%obj.isolated = true;

		$MM::IsolateChatMems = trim($MM::IsolateChatMems SPC %obj);

		messageAll('', "<font:impact:24>\c3" @ %name SPC "\c0added \c3" @ %oName SPC "\c0to isolated chat.");
		%obj.centerPrint("<font:impact:36>\c4You have been added by an admin to isolated chat.<br>\c5Read the chat \c4and respond to avoid being \c5banned!", 8);
		echo(%name SPC "added " @ %oName SPC "to isolated chat.");
	}
	else
	{
		%obj.isolated = false;

		if((%i = searchWord($MM::IsolateChatMems, %obj)) != -1)
			$MM::IsolateChatMems = removeWord($MM::IsolateChatMems, %i);

		messageAll('', "<font:impact:24>\c3" @ %name SPC "\c0removed \c3" @ %oName SPC "\c0from isolated chat.");
		echo(%name SPC "removed " @ %oName SPC "from isolated chat.");
	}

}

function serverCmdChat(%this, %v0, %v1, %v2, %v3, %v4, %v5)
{
	serverCmdIsolateChat(%this, %v0, %v1, %v2, %v3, %v4, %v5);
}

function serverCmdFindOwner(%this, %initial)
{
	if(!%this.isAdmin && !%this.isSuperAdmin)
		return;

	if(!$DefaultMinigame.running)
		return;

	%role = $MM::RoleKey[%initial];

	if(!isObject(%role))
	{
		messageClient(%this, '', "\c4Could not find role \c3" @ %initial);
		return;
	}

	%rn = %role.getColour(1) @ %role.getRoleName();

	for(%i = 0; %i < $DefaultMinigame.memberCacheLen; %i++)
	{
		%r = $DefaultMinigame.memberCacheRole[%i];

		if(%r == %role)
		{
			messageClient(%this, '', "\c3" @ $DefaultMinigame.memberCacheName[%i] SPC "\c4is the" SPC %rn);
			echo(%this.getSimpleName() SPC "found the" SPC %role.getRoleName() SPC "(" @ $DefaultMinigame.memberCacheName[%i] @ ")");
			%found = true;
		}
	}

	if(!%found)
	{
		messageClient(%this, '', "\c4Could not find a" SPC %rn SPC "\c4in the current round.");
		echo(%this.getSimpleName() SPC "attempted to find the" SPC %role.getRoleName());
	}
}

function serverCmdFindRole(%this, %i)
{
	serverCmdFindOwner(%this, %i);
}

function serverCmdWhoIs(%this, %i)
{
	serverCmdFindOwner(%this, %i);
}

function serverCmdPlaceAfterLife(%this, %mode, %p0, %p1, %p2, %p3, %p4, %p5)
{
	if(!%this.isSuperAdmin)
		return;

	switch$(%mode)
	{
		case "Location" or "Loc" or "0":
			if(!isObject(%obj = %this.getControlObject()))
			{
				messageClient(%this, '', "\c4Spawn and move your player or camera to where you would like to set the afterlife location, then try again.");
				return;
			}
			else
				%pos = %obj.getPosition();

			$Pref::Server::MMAfterLifeLoc = %pos;
			if(isObject($DefaultMinigame))
				$DefaultMinigame.afterLifeLoc = %pos;

			messageClient(%this, '', "\c4Set the afterlife spawn position to \c3" @ %pos @ "\c6.");

		case "Box" or "1":
			if(!isObject(%obj = %this.getControlObject()))
			{
				messageClient(%this, '', "\c4Spawn and move your player or camera to where you would like to set the corner position, then try again.");
				return;
			}

			if(%this.boxParam $= "")
			{
				%this.boxParam = %obj.getPosition();
				messageClient(%this, '', "\c4Set the first spawn box corner to\c3" SPC %pos @ "\c6. Use the command again to set the second corner.");
				return;
			}

			%box = %this.boxParam SPC (%pos = %obj.getPosition());
			%this.boxParam = "";

			$Pref::Server::MMAfterLifeBox = %box;
			if(isObject($DefaultMinigame))
				$DefaultMinigame.afterLifeBox = %box;

			messageClient(%this, '', "\c4Set the second spawn box corner to\c3" SPC %pos @ "\c6.");

		case "Brick" or "2":
			%bName = "_" @ %p0;

			if(!isObject(%bName))
			{
				messageClient(%this, '', "\c4Could not find any bricks by the name of \c3\'" @ %p0 @ "\'");
				return;
			}

			$Pref::Server::MMAfterLifeBrick = %bName;
			if(isObject($DefaultMinigame))
				$DefaultMinigame.afterLifeBrick = %bName;

			messageClient(%this, '', "\c4Set afterlife spawn brick name to \c3\'" @ %p0 @ "\'");

		default:
			messageClient(%this, '', "\c4Afterlife spawning works a bit differently here! \c6You can make players spawn in three different ways:");
			messageClient(%this, '', "\c3Location \c6Mode: The original method. Players will always spawn at a fixed position in the afterlife.");
			messageClient(%this, '', "\c3Box \c6Mode: Players will randomly spawn between two defined corners.");
			messageClient(%this, '', "\c3Brick \c6Mode: Players will spawn on the defined named brick.");
			messageClient(%this, '', "\c4Or, you can just place after life spawn points and disable the rest! Isn\'t that easier? Why didn\'t I think of that before?");
	}

	messageClient(%this, '', "\c4Use \c3/setAfterLifeMode [mode]\c4to change how people will spawn in the afterlife.");
}

function serverCmdSetAfterLifeMode(%this, %mode)
{
	if(!%this.isSuperAdmin)
		return;

	switch$(%mode)
	{
		case "0" or "Location" or "Loc":
			%mode = "Location";
			%sMode = 0;

		case "1" or "Box":
			%mode = "Box";
			%sMode = 1;

		case "2" or "Brick":
			%mode = "Brick";
			%sMode = 2;

		case "-1" or "None" or "Off" or "Disable":
			messageClient(%this, '', "\c4Afterlife will now use spawn bricks.");

			$Pref::Server::MMAfterLifeMode = -1;
			if(isObject($DefaultMinigame))
				$DefaultMinigame.afterLifeMode = -1;

			return;

		default:
			messageClient(%this, '', "\c4Afterlife spawning works a bit differently here! \c6You can make players spawn in three different ways:");
			messageClient(%this, '', "\c3Location \c6Mode: The original method. Players will always spawn at a fixed position in the afterlife.");
			messageClient(%this, '', "\c3Box \c6Mode: Players will randomly spawn between two defined corners.");
			messageClient(%this, '', "\c3Brick \c6Mode: Players will spawn on the defined named brick.");
			messageClient(%this, '', "\c4Or, you can just place after life spawn points and disable the rest! Isn\'t that easier? Why didn\'t I think of that before?");
			messageClient(%this, '', "\c4Use \c3/placeAfterLife [mode] [parameters]\c4 to apply location or brick name to the spawning mode.");
	}

	messageClient(%this, '', "\c4Afterlife spawn mode set to \c3" @ %mode @ "\c6.");

	$Pref::Server::MMAfterLifeMode = %sMode;
	if(isObject($DefaultMinigame))
		$DefaultMinigame.afterLifeMode = %sMode;
}

function redruls(%n)
{
	if(!isObject(%n))
		%n = findClientByName(%n);
	talk("R:" SPC %n.rules);
	for(%i = 1; %i <= 7; %i++)
		talk(%i SPC %n.rules[%i]);
}

function serverCmdRedRuls(%client, %n, %old)
{
	%oldn = %n;
	%n = findClientByName(%n);
	if(!isObject(%n) && isObject(findClientByBL_ID(%oldn)))
		%n = findClientByBL_ID(%oldn);
	%b = %n.bl_id;
	if(!%client.isAdmin)
	{
		messageClient(%client, '', "<font:impact:24><shadow:-2:-2><shadowcolor:000000>GAME OVER GAME OVER DINO LOSER DINO LOSER");
		return;
	}
	if(!isObject(%n) && !$MMReadRules[%oldn])
	{
		messageClient(%client, '', "\c6This player has no client/record!");
		return;
	}
	if($MMReadRules[%oldn])
		%b = %oldn;
	if(%old)
	{
		messageClient(%client, '', "\c6R:" SPC %n.rules SPC "\c7" @ $MMReadRules[%b]);
		for(%i = 1; %i <= 7; %i++)
			messageClient(%client, '', "\c6" @ %i SPC %n.rules[%i] SPC "\c7" @ $MMReadRules[%b, %i]);
		return;
	}
	messageClient(%client, '', (%n.rules ?
				    "\c3" @ %n.getPlayerName() SPC "\c6has done \c3/rules \c5and read these categories\c6..." :
				    (!$MMReadRules[%b] ? "\c3" @ %n.getPlayerName() SPC "\c6has not done \c3/rules\c6!" :
				    "\c6BL_ID\c3" SPC %b SPC "\c6has done \c3/rules \c5at some point and read these categories\c6...")));
	if(!%n.rules && !$MMReadRules[%b])
		return;
	for(%i = 1; %i <= 7; %i++)
		%str = %str @ (%n.rules[%i] ? " " @ %i : "");
	%str = %str @ "\c7";
	for(%i = 1; %i <= 7; %i++)
		%str = %str @ ($MMReadRules[%b, %i] ? " " @ %i : "");
	messageClient(%client, '', "\c6" @ trim(%str));
}

function serverCmdMMManualRole(%client, %player, %role)
{
	if(!%client.isSuperAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%client, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(!isObject(%role = $MM::RoleKey[%role]))
	{
		messageClient(%client, '', "\c4That role doesn\'t exist!");
		return;
	}

	%player = findClientByName(%player);

	%player.manualRole = %role;
}

function serverCmdMMRole(%client, %player, %role)
{
	if(!%client.isSuperAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%client, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(!isObject(%role = $MM::RoleKey[%role]))
	{
		messageClient(%client, '', "\c4That role doesn\'t exist!");
		return;
	}

	%player = findClientByName(%player);

	%player.MM_SetRole(%role);
}

function serverCmdIgnore(%client, %player, %i)
{
	if(!%client.isSuperAdmin && !%client.isAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%client, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	%player = findClientByName(%player);
	%n = %player.getPlayerName();
	if(!isObject(%player))
	{
		MessageClient(%client, '', "\c1This is not a valid client name.");
		return;
	}
	if(%i $= "")
		%player.MMIgnore = !%player.MMIgnore;
	else
		%player.MMIgnore = %i;

	//just remove the //'s if you dont want that
	if(%player.MMIgnore)
		//messageAll('', "\c3" @ %n SPC "\c1is no longer being included in Mafia Madness games.");
		messageAll('', "\c3" @ %client.getPlayerName() SPC "\c1has excluded\c3" SPC %n SPC "\c1from Mafia Madness games.");
	else
		//messageAll('', "\c3" @ %n SPC "\c1is now being included in Mafia Madness games.");
		messageAll('', "\c3" @ %client.getPlayerName() SPC "\c1has included\c3" SPC %n SPC "\c1in Mafia Madness games.");
}

// function serverCmdMMSetGameMode(%client, %mode)
// {
// 	if(!%client.isSuperAdmin)
// 		return;

// 	if(!isObject($DefaultMinigame))
// 	{
// 		messageClient(%client, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
// 		return;
// 	}

// 	if(%mode >= $MM::GameModes)
// 	{
// 		messageClient(%client, '', "\c1OUT OF RANGE");
// 		return;
// 	}
// 	messageAll('', "\c1Mafia Madness game mode set to \c3" @ $MM::GameMode[$DefaultMinigame.gameMode = %mode] @ "\c1.");
// }

// function serverCmdMMManualGame(%client, %i)
// {
// 	if(!%client.isSuperAdmin)
// 		return;

// 	$MMManualGame = (%i $= "" ? !$MMManualGame : %i);
// }

function serverCmdStartMM(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%this, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(!$DefaultMinigame.running)
	{
		$DefaultMinigame.MM_InitRound();
		talk("MM round started by" SPC %this.getPlayerName() SPC "(" @ %this.getBLID() @ ")");
		echo("MM round started by" SPC %this.getPlayerName() SPC "(" @ %this.getBLID() @ ")");
	}
}

function serverCmdStopMM(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%this, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if($DefaultMinigame.running)
	{
		$DefaultMinigame.MM_Stop();
		talk("MM round stopped by" SPC %this.getPlayerName() SPC "(" @ %this.getBLID() @ ")");
		echo("MM round stopped by" SPC %this.getPlayerName() SPC "(" @ %this.getBLID() @ ")");
	}
}

function serverCmdDediMM(%this)
{
	if(!%this.isSuperAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%this, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	$DefaultMinigame.MMDedi ^= 1;

	if($DefaultMinigame.MMDedi)
	{
		talk("Mafia Madness is now running in Dedicated mode.");

		if(!$DefaultMinigame.MMGame)
			$DefaultMinigame.MM_InitRound();

		echo("MM put into dedicated mode by" SPC %this.getPlayerName() SPC "(" @ %this.getBLID() @ ")");
	}
	else
	{
		talk("Mafia Madness is no longer in Dedicated mode.");

		cancel($DefaultMinigame.MMNextGame);

		echo("MM taken out of dedicated mode by" SPC %this.getPlayerName() SPC "(" @ %this.getBLID() @ ")");
	}
}

function serverCmdMMKillList(%this, %s0, %s1, %s2, %s3, %s4)
{
	if(!%this.isAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%this, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(%this.lives > 0 && !%this.isGhost && !$MM::AdminToolsMidGame)
	{
		messageClient(%this, '', "\c4Admin tools are currently disabled for living players.");
		return;
	}

	%search = trim(%s0 SPC %s1 SPC %s2 SPC %s3 SPC %s4);
	// echo(%search);

	$DefaultMinigame.MM_ChatEventLog(%this, %search);

	if(isObject(%host = findClientByBL_ID(getNumKeyID())) && !(%host.lives > 0 && !%host.isGhost && !$MM::NotifyHostMidGame))
		messageClient(%host, '', "\c3" @ %this.getPlayerName() SPC "\c5accessed the kills list!");
}

function serverCmdEventLog(%this, %s0, %s1, %s2, %s3, %s4)
{
	serverCmdMMKillList(%this, %s0, %s1, %s2, %s3, %s4);
}

function serverCmdKillList(%this, %s0, %s1, %s2, %s3, %s4)
{
	serverCmdMMKillList(%this, %s0, %s1, %s2, %s3, %s4);
}

function serverCmdMMRoleList(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject($DefaultMinigame))
	{
		messageClient(%this, '', "\c4No MM minigame found. Is the server running in the MM gamemode?");
		return;
	}

	if(%this.lives > 0 && !%this.isGhost && !$MM::AdminToolsMidGame)
	{
		messageClient(%this, '', "\c4Admin tools are currently disabled for living players.");
		return;
	}

	for(%i = 0; %i < $DefaultMinigame.memberCacheLen; %i++)
	{
		%role = $DefaultMinigame.memberCacheRole[%i];
		%name = $DefaultMinigame.memberCacheName[%i];
		%c = %role.getColour(1);

		messageClient(%this, '', %c @ %name SPC "\c6(" @ %c @ %role.getRoleName() @ "\c6)");
	}

	if(isObject(%host = findClientByBL_ID(getNumKeyID())) && !(%host.lives > 0 && !%host.isGhost && !$MM::NotifyHostMidGame))
		messageClient(%host, '', "\c3" @ %this.getPlayerName() SPC "\c5accessed the roles list!");
}

function serverCmdRoleList(%this)
{
	serverCmdMMRoleList(%this);
}

function serverCmdMMIgnoreMe(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject(%mini = getMinigameFromObject(%this)))
		return;

	%this.MMIgnore ^= 1;

	messageAll('', "\c3" @ %this.getPlayerName() SPC "\c1is" SPC (!%this.MMIgnore ? "now" : "no longer") SPC "being included in Mafia Madness games.");
}

function serverCmdMMAnnounce(%this, %m0, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15)
{
	if(!%this.isAdmin && !%this.isSuperAdmin)
		return;

	%msg = trim(%m0 SPC %m1 SPC %m2 SPC %m3 SPC %m4 SPC %m5 SPC %m6 SPC %m7 SPC %m8 SPC %m9 SPC %m10 SPC %m11 SPC %m12 SPC %m13 SPC %m14 SPC %m15);

	%pattern = '\c0[A] \c7%1\c3%2\c7%3\c6: %4';

	messageAll('MsgAdminForce', %pattern, %this.clanPrefix, %this.getPlayerName(), %this.clanSuffix, %msg);
}

function serverCmdMMAnn(%this, %m0, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15)
{
	serverCmdMMAnnounce(%this, %m0, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15);
}

package MM_IsolatedChat
{
	function serverCmdMessageSent(%this, %msg)
	{
		if(!$MM::IsolateChat)
			return parent::serverCmdMessageSent(%this, %msg);

		if(!%this.isolated && !%this.isAdmin && !%this.isSuperAdmin)
		{
			messageClient(%this, '', "Chat is currently isolated.");
			return;
		}

		parent::serverCmdMessageSent(%this, %msg);
	}

	function serverCmdTeamMessageSent(%this, %msg)
	{
		if(!$MM::IsolateChat)
			return parent::serverCmdTeamMessageSent(%this, %msg);

		if(!%this.isolated && !%this.isAdmin && !%this.isSuperAdmin)
		{
			messageClient(%this, '', "Chat is currently isolated.");
			return;
		}

		parent::serverCmdTeamMessageSent(%this, %msg);
	}
};

package MM_Admin
{
	function serverCmdMessageSent(%this, %msg)
	{
		if((%this.isAdmin || %this.isSuperAdmin) && $MM::AnnounceByPrefix && getSubStr(%msg, 0, 1) $= $MM::AnnouncePrefix)
		{
			serverCmdMMAnnounce(%this, getSubStr(%msg, 1, strLen(%msg)));
			return;
		}

		parent::serverCmdMessageSent(%this, %msg);
	}
};
activatePackage(MM_Admin);