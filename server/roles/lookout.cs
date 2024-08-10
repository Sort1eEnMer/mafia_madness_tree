
$MM::LoadedRole_Lookout = true;

if(isObject(MMRole_Lookout)){
MMRole_Lookout.delete();
}

if(!isObject(MMRole_Lookout)){
	
	new ScriptObject(MMRole_Lookout){
		class = "MMRole";

		name = "Lookout";
		corpseName = "intuitional sentinel";
		displayName = "Lookout";

		letter = "LK";

		colour = "<color:99FF00>";
		nameColour = "0.6 1.0 0.0";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;
		canLookout = true;

		alignment = 0;

		helpText = 	"\c4You are also the \c6Lookout\c4! You are responsible for keeping an eye on suspicious individuals." NL
					"\c6Right Click\c4 on someone to plant a camera on them.  Then type \c6/lookout\c4 to watch them!" NL
					"\c4You can only have one \c6camera\c4 active at a time!  In order to retrieve the camera, you must right click on the person you gave it to.";

		description = 	"\c4Tthe \c6Lookout\c4 is responsible for keeping an eye on suspicious individuals." NL
					"\c6Right Click\c4 on someone to plant a camera on them.  Then type \c6/lookout\c4 to watch them!" NL
					"\c4You can only have one \c6camera\c4 active at a time!  In order to retrieve the camera, you must right click on the person you gave it to.";};}

//SUPPORT
function MMRole::getCanLookout(%this){
	return %this.canLookout ? true : false;}

function GameConnection::MM_canLookout(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(!isObject(%this.role))
		return false;

	if(%this.MM_isMaf())
		return false;

	if(!%this.role.getCanLookout())
		return false;

	return true;
}

function ServerCmdLookout(%this) {
	if(!%this.MM_canLookout())
		return;

	if(%this.buggedClient == -1) {
		return messageClient(%this,'',"\c4You must camera bug someone before you can watch them!");
	}

	if(!isObject(%this.buggedClient) || %this.buggedClient.isGhost || %this.buggedClient.lives < 1) {
		return messageClient(%this,'',"\c4The person you camera bugged isn't alive anymore!");
	}

	if(%this.isBugWatching == 1) {
		%this.setControlObject(%this.player);
		%this.isBugWatching = 0;
		return;
	}

	messageClient(%this,'',"\c4You are now watching\c3" SPC %this.buggedClient.getSimpleName() @ "\c4! Press any button to stop.");
	%this.camera.setSpecPlayer(%this,%this.buggedClient);
	%this.isBugWatching = 1;
}

package MM_Lookout {
	function AIPlayer::MM_Investigate(%this, %client)
	{
		parent::MM_Investigate(%this, %client);

		talk(%this.originalClient);
		talk(%client.buggedClient);
		talk(%this.disfigured);
		if(
			%client.MM_canLookout() && 
			%this.originalClient == %client.buggedClient && 
			!%this.disfigured
		) {
			messageClient(%client, '', "\c2This corpse has your camera. Pick it up to retrieve it.");
		}
	}

	function GameConnection::MM_Chat(%this, %obj, %type, %msg, %excludeList, %pre2, %condition, %a0, %a1, %a2, %a3, %a4) {
		%mini = getMiniGameFromObject(%this);
		for(%i=0;%i<%mini.numMembers;%i++) {
			%mem = %mini.member[%i];
			if(%mem.MM_canLookout() && isObject(%mem.buggedClient) && !%mem.buggedClient.isGhost && %mem.isBugWatching) {
				messageClient(%mem,'',"\c4[\c6<color:99FF00>LK\c4]\c3" @ getRandom(10000,99999) @ "\c6: " @ %msg);
				%excludeList = %excludeList SPC %mem;
			}
		}
		parent::MM_Chat(%this, %obj, %type, %msg, %excludeList, %pre2, %condition, %a0, %a1, %a2, %a3, %a4);

	}


	function MMRole::onTrigger(%this, %mini, %client, %obj, %slot, %val)
	{
		%client.MM_UpdateUI();
		parent::onTrigger(%this, %mini, %client, %obj, %slot, %val); 
		if(!isObject(%client.player) || %client.getControlObject() != %client.player || !%client.MM_canLookout())
			return;

		%client.isBugWatching = 0;
		if(%slot != 4 || !%val)
			return;

		

		%start = %obj.getEyePoint();
		%vec = %obj.getEyeVector();
		%end = VectorAdd(%start, VectorScale(%vec, $MM::GPAbductionRange));

		%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
		%aObj = firstWord(%ray);
		if(!isObject(%aObj.client) || %aObj.getClassName() !$= "Player" || !isObject(%cl = %aObj.getControllingClient()))
			return;

		if(%client.buggedClient == -1 || !isObject(%client.buggedClient)) {
			%client.buggedClient = %aObj.client;
			messageClient(%client, '', "\c3" @ %aObj.client.getSimpleName() SPC "\c4is now bugged with your camera! Type \c3/lookout\c4 at any time to spectate them!");
			return;
		}
		if(%client.buggedClient != %aObj.client) {
			messageClient(%client, '', "\c4You already have a camera bug on \c3" SPC %client.buggedClient.getSimpleName() @ "\c4! Retrieve your camera from them before placing a new one on someone else!");
			return;
		}
		
		if(%client.buggedClient == %aObj.client)
		{
			%client.buggedClient = -1;
			messageClient(%client, '', "\c4You have removed your camera bug from\c3" SPC %aObj.client.getSimpleName() @ "\c4!");
			return;
		}
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		Parent::onCleanUp(%this, %mini, %client);
		%client.buggedClient = -1;
		%client.isBugWatching = 0;
	}
};
deactivatepackage(MM_Lookout);
activatepackage(MM_Lookout);