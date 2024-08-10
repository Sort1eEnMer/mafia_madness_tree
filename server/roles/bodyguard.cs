

$MM::LoadedRole_Bodyguard = true;

if(isObject(MMRole_BodyGuard))
	MMRole_BodyGuard.delete();

if(!isObject(MMRole_Bodyguard))
{
	new ScriptObject(MMRole_Bodyguard)
	{
		class = "MMRole";

		name = "Bodyguard";
		corpseName = "steadfast conservator";
		displayName = "Bodyguard";

		letter = "BG";

		colour = "<color:FFFF33>";
		nameColour = "1.0 1.0 0.15";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 2;
		isEvil = 1;

		helpText = 	"\c4You are the <color:FFFF33>Bodyguard\c4! Starting out, \c3you are neither innocent nor mafia!" NL
					"\c4You will start off the game with a pre-decided \c3Client\c4. You must protect this person at all costs! If they die, you die too..." NL
					"\c4You can only win if your \c3Client\c4 stays alive by the end of the game!" NL
					"\c4When you are \c3close to your Client, you are invincible to all attacks!" NL
					"\c4Stay close to your \c3Client\c4! Keep them \c3safe\c4! Shoot anyone who tries to kill them!" NL
					"\c0<font:impact:35>BEWARE! YOU ARE NOT ALLOWED TO RDM AS BODYGUARD! ONLY SHOOT ANYONE ATTACKING YOUR CLIENT!";

		description = 	"\c4The <color:FFFF33>Bodyguard\c4 starts out \c3neither innocent nor mafia!" NL
					"\c4You will start off the game with a pre-decided \c3Client\c4. You must protect this person at all costs! If they die, you die too..." NL
					"\c4You can only win if your \c3Client\c4 stays alive by the end of the game!" NL
					"\c4When you are \c3close to your Client, you are invincible to all attacks!" NL
					"\c4Stay close to your \c3Client\c4! Keep them \c3safe\c4! Shoot anyone who tries to kill them!" NL
					"\c0<font:impact:35>BEWARE! YOU ARE NOT ALLOWED TO RDM AS BODYGUARD! ONLY SHOOT ANYONE ATTACKING YOUR CLIENT!";

		bodyGuard = true;
	};
}

//SUPPORT
function GameConnection::MM_isBodyguard(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(%this.isGhost || %this.lives < 1)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.role.bodyguard)
		return false;

	return true;
}

package MM_Bodyguard {
	function Player::MM_BodyguardLoop(%this) {
		%c = %this.client;
		if(!isObject(%this))
			return cancel(%this.bodyguardLoop);

		if(%c.bodyGuardClient.isGhost || !isObject(%c.bodyguardClient) || %c.bodyGuardClient == -1) {
			cancel(%this.bodyguardLoop);
			%this.kill();
			return;
		}

		%dist = VectorDist(%this.getTransform(),%c.bodyGuardClient.player.getTransform());
		if(%dist <= 6) {
			%c.bodyguardClose = 1;
		}
		else {
			%c.bodyguardClose = 0;
		}
		%this.bodyGuardLoop = %this.schedule(100,MM_BodyGuardLoop);
	}

	function GameConnection::onDeath( %this, %source, %killer, %type, %location) {
		parent::onDeath( %this, %source, %killer, %type, %location);
		if(!%this.bodyGuard.isGhost && %this.bodyguard.bodyguardClient == %this && %this.bodyguard.MM_isBodyguard()) {
			%mini = getMiniGameFromObject(%this);
			%mini.MM_LogEvent(%this.bodyGuard.MM_GetName(1) SPC "\c6died because their client," SPC %this.MM_GetName(1) @ "\c6, was killed!");
			messageClient(%this.bodyGuard,'',"\c4Your client, \c3" @ %this.getSimpleName() @ "\c4, has died! You are overcome with guilt and break your own neck.");
			%this.bodyguard.player.kill();
			%this.bodyguard.player.schedule(500,kill);
			//announce("client died. killing bodyguard" SPC %this.bodyGuard.name SPC %this.bodyGuard.bodyguardclient);
			
		}
	}

	function MMRole::onSpawn(%this, %mini, %client) {
		parent::onSpawn(%this, %mini, %client);
		%j = 0;
		%this = %client;
		%this.bodyguardClient = -1;
		cancel(%this.player.bodyguardLoop);
		if(%this.MM_isBodyGuard()) {
			%client.player.setScale("1.3 1 1");
			%mini = getMiniGameFromObject(%this);
			for(%i=0;%i<%mini.numMembers;%i++) {
				%mem = %mini.member[%i];
				if(!%mem.isGhost && !%mem.MM_isBodyGuard() && %mem.role.name !$= "Pacifist") {
					%list[%j] = %mem;
					%j++;
				}
			}
			%chosen = %list[getRandom(0,%j-1)];
			//talk(%chosen.name @ " is the bodyguard's chosen client");
			
			%mini.MM_LogEvent(%this.MM_GetName(1) SPC "\c6became a \c3Bodyguard\c6 for" SPC %chosen.MM_GetName(1));
			%this.bodyguardClient = %chosen;
			%chosen.bodyGuard = %this;
			%this.player.MM_BodyGuardLoop();
			schedule(100,0,messageClient,%this,'',"\c3" @ %chosen.getSimpleName() @ "\c4 is your client for this round! \c3Protect them with your life\c4!");
		}
	}

	function Player::damage(%this, %obj, %pos, %amt, %type)
    	{
        	%mini = getMiniGameFromObject(%obj);
		%c = %this.client;
        	if(!%c.isGhost && %c.MM_isBodyGuard() && isObject(%c.bodyguardclient) && %c.bodyguardClose && !%c.bodyGuardClient.isGhost)
		{
          	    %amt = 0;
        	}
        	parent::damage(%this, %obj, %pos, %amt, %type);
   	 }

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);
		%client.bodyguard = -1;
		%client.bodyguardClient = -1;
		%client.bodyguardClose = -1;
		cancel(%client.player.bodyguardloop);
		cancel(%client.player.bodyguardloop);
	}
};
deactivatepackage(MM_BodyGuard);
activatepackage(MM_BodyGuard);