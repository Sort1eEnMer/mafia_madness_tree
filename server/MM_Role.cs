//MM_Role.cs
//Basic functionality for roles.
//Rather than hard-coding roles in with global tables, we're taking a more dynamic, object-oriented approach.

$MM::LoadedRole = true;

$MM::Alignment[0] = "Innocent";
$MM::Alignment[1] = "Mafia";
$MM::Alignment[2] = "Unaligned";

$MM::AlignmentColour[0] = "<color:00FF00>";
$MM::AlignmentColour[1] = "<color:FF0040>";
$MM::AlignmentColour[2] = "<color:808080>";

function MMRole::getRoleName(%this)
{
	return %this.name;
}

function MMRole::getColour(%this, %forceNorm)
{
	if(!%forceNorm && %this.displayColour !$= "")
		return %this.displayColour;

	return %this.colour;
}

function MMRole::getColor(%this, %forceNorm)
{
	return %this.getColour(%forceNorm);
}

function MMRole::getCorpseName(%this)
{
	return %this.corpseName;
}

function MMRole::getLetter(%this)
{
	return %this.letter;
}

function MMRole::getDisplayName(%this)
{
	return %this.displayName;
}

function MMRole::getNameColour(%this)
{
	return %this.nameColour;
}

function MMRole::getNameColor(%this)
{
	return %this.getNameColour();
}

function MMRole::getAlignment(%this)
{
	return %this.alignment;
}

function MMRole::getHelpText(%this)
{
	return %this.helpText;
}

function MMRole::onAdd(%this)
{
	if(!isObject(MMRoles))
		new SimGroup(MMRoles);

	MMDebug("Added MMRole" SPC %this.name SPC "(" @ %this @ ")");

	MMRoles.add(%this);

	$MM::RoleKey[%this.getLetter()] = %this;
}

function MMRole::onAssign(%this, %mini, %client)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "assigned to" SPC %client.getSimpleName() SPC "(" @ %client @ ")", %this, %mini, %client);
}

function MMRole::onCleanup(%this, %mini, %client)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "cleaned up on" SPC %client.getSimpleName() SPC "(" @ %client @ ")", %this, %mini, %client);
}

function MMRole::onTrigger(%this, %mini, %client, %obj, %slot, %val)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "received trigger from" SPC %client.getSimpleName() SPC "(" @ %client @ "," SPC %slot @ "," SPC %val @ ")", %this, %mini, %client);
}

function MMRole::onDay(%this, %mini, %client)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "received day time event for" SPC %client.getSimpleName() SPC "(" @ %client @ ")", %this, %mini, %client);
}

function MMRole::onNight(%this, %mini, %client)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "received night time event for" SPC %client.getSimpleName() SPC "(" @ %client @ ")", %this, %mini, %client);
}

function MMRole::onSpawn(%this, %mini, %client)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "received spawn event for" SPC %client.getSimpleName() SPC "(" @ %client @ ")", %this, %mini, %client);
}

function MMRole::onChat(%this, %mini, %client, %msg, %type)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "received chat event for" SPC %client.getSimpleName() SPC "(" @ %client @ "," @ %type @ ")", %this, %mini, %client);
	MMDebug("   +Msg:" SPC %msg, %this, %mini, %client);

	return 0;
}

function MMRole::onTeamChat(%this, %mini, %client, %msg, %type)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "received team chat event for" SPC %client.getSimpleName() SPC "(" @ %client @ "," @ %type @ ")", %this, %mini, %client);
	MMDebug("   +Msg:" SPC %msg, %this, %mini, %client);

	return 0;
}

function MMRole::onDeath(%this, %mini, %client, %srcObj, %srcClient, %damageType, %loc)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "received death event for" SPC %client.getSimpleName() SPC "(" @ %client SPC %srcObj SPC %srcClient SPC %damageType SPC %loc @ ")", %this, %mini, %client);
}

function MMRole::SpecialWinCheck(%this, %mini, %client, %killed, %killer)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "received win check for" SPC %client.getSimpleName() SPC "(" @ %client SPC %killed SPC %killer @ ")", %this, %mini, %client);

	return 0;
}

function MMRole::applyOutfit(%this, %mini, %client, %day)
{
	MMDebug("Role" SPC %this.name SPC "(" @ %this @ ")" SPC "received outfit event for" SPC %client.getSimpleName() SPC "(" @ %client SPC %day @ ")", %this, %mini, %client);

	return false;
}