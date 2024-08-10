
$MM::LoadedRole_RandomTown = true;
$MM::GPRandomPot["?T"] = "I I I I F F LK IF L S";
$MM::GPRandomPercentage["?T"] = getRolePercent($MM::GPRandomPot["?T"]);

if(!isObject(MMRole_RandomTown)){
	
	new ScriptObject(MMRole_RandomTown){
		class = "MMRole";

		name = "Random Town";
		corpseName = "Random Town";
		displayName = "Random Town";

		letter = "?T";

		colour = "<color:00FF00>";
		nameColour = "0.0 1.0 0.0";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;
		isRandom = true;

		alignment = 0;


	};
	MMRole_RandomTown.description = "\c6Here are the role percentages for \c2Random Town\c6:" NL "\c6" @ getRolePercent($MM::GPRandomPot["?T"]);
}

$MM::LoadedRole_RandomProtect = true;
$MM::GPRandomPot["?P"]  = "BB DC";
$MM::GPRandomPercentage["?P"]  = getRolePercent($MM::GPRandomPot["?P"]);

if(isObject(MMRole_RandomProtect)){
MMRole_RandomProtect.delete();
}

if(!isObject(MMRole_RandomProtect) && isObject(MMRole_RandomTown)){
	
	new ScriptObject(MMRole_RandomProtect){
		class = "MMRole";

		name = "Random Protective";
		corpseName = "Random Protective";
		displayName = "Random Protective";

		letter = "?P";

		colour = "<color:00FF00>";
		nameColour = "0.0 1.0 0.0";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;
		isRandom = true;

		alignment = 0;


	};
	MMRole_RandomProtect.description = "\c6Here are the role percentages for \c2Random Protective\c6:" NL "\c6" @ getRolePercent($MM::GPRandomPot["?P"]);
}



$MM::LoadedRole_RandomDetective = true;
$MM::GPRandomPot["?C"]  = "O O O O P N IC CC";
$MM::GPRandomPercentage["?C"]  = getRolePercent($MM::GPRandomPot["?C"]);

if(isObject(MMRole_RandomDetective)){
MMRole_RandomDetective.delete();
}

if(!isObject(MMRole_RandomDetective) && isObject(MMRole_RandomTown)){
	
	new ScriptObject(MMRole_RandomDetective){
		class = "MMRole";

		name = "Random Cop";
		corpseName = "Random Cop";
		displayName = "Random Cop";

		letter = "?C";

		colour = "<color:00FF00>";
		nameColour = "0.0 1.0 0.0";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;
		isRandom = true;

		alignment = 0;

	};
	MMRole_RandomDetective.description = "\c6Here are the role percentages for \c2Random Cop\c6:" NL "\c6" @ getRolePercent($MM::GPRandomPot["?C"]);
}



$MM::LoadedRole_RandomAntiCult = true;
$MM::GPRandomPot["?A"]  = "AL K";
$MM::GPRandomPercentage["?A"]  = getRolePercent($MM::GPRandomPot["?A"]);

if(!isObject(MMRole_RandomAntiCult) && isObject(MMRole_RandomTown)){
	
	new ScriptObject(MMRole_RandomAntiCult){
		class = "MMRole";

		name = "Random Anti-Cult";
		corpseName = "Random Anti-Cult";
		displayName = "Random Anti-Cult";

		letter = "?A";

		colour = "<color:00FF00>";
		nameColour = "0.0 1.0 0.0";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;
		isRandom = true;

		alignment = 0;

	};
	MMRole_RandomAntiCult.description = "\c6Here are the role percentages for \c2Random Anti-Cult\c6:" NL "\c6" @ getRolePercent($MM::GPRandomPot["?A"]);
}

//SUPPORT
function getRolePercent(%e) {
	%tot = 0;
	%elast = "";
	for(%i=0;%i<getWordCount(%e);%i++) {
		%w = getWord(%e,%i);
		if(!isInList(%elast,%w))
			%elast = %elast SPC %w;
		%r[%w]++;
		%tot++;
	}
	echo(%tot);
	%lines = "";
	for(%i=0;%i<getWordCount(%elast);%i++) {
		%w = getWord(%elast,%i);
		if(%w $= "" || %r[%w] <= 0)
			continue;
		%eo = "\c2" @ %w @ "\c6: " @ mCeil((%r[%w] / %tot)*100) @ "%";
		%lines = %lines SPC %eo;
	}
	return %lines;
}


function MM_GetRandomRole(%pot) {
	%count = 0;
	for(%i=0;%i<getWordCount(%pot);%i++) {
		%letter = getWord(%pot,%i);
		%r = $MM::RoleKey[%letter];
		if(isObject(%r) == 1 || isObject(%r)) {
			%list[%count] = %r;
			%egg = %list[%count];
			%count++;
			//talk(%egg.name);
		}
	}
	return %list[getRandom(0,%count-1)];
}

function GameConnection::MM_RandomSpawn(%this) {
	//announce("the egg has spawned");
	%rRole = %this.role;
	%this.role = MM_GetRandomRole($MM::GPRandomPot[%rRole.letter]);
	%role = %this.role;
	
	%mini = getMiniGameFromObject(%this);

	%roleStr = %role.getColour(1) @ %role.getRoleName();

	%mini.MM_LogEvent(%this.MM_getName(1) SPC "\c6became the" SPC %roleStr);

	//%this.MM_SetRole(%role);

	//%this.knowsFullRole = true;

	%this.MM_UpdateUI();

	%this.MM_DisplayStartText();

	%this.clearInventory();

	%this.MM_GiveEquipment();

	%this.applyBodyParts();
	%this.applyBodyColors();
}

package MM_RandomRoles {
	function serverCmdClaim(%this, %claim) {
		%role = $MM::RoleKey[%claim];
		if(%role.isRandom) {
			messageClient(%this, '', "\c4That role cannot be claimed.");
			return;
		}
		parent::serverCmdClaim(%this, %claim);
	}

	function MMRole::onSpawn(%this, %mini, %client) {
		parent::onSpawn(%this, %mini, %client);
		//announce("onspawn");
		if(%this.isRandom) {
			%client.MM_RandomSpawn();
		}
	}
	function MiniGameSO::MM_SetRole(%this,%client,%role) {
		parent::MM_SetRole(%this,%client,%role);
		//announce("setrole");
		if(%role.isRandom) {
			%client.MM_RandomSpawn();
		}
	}
};
deactivatepackage(MM_RandomRoles);
activatepackage(MM_RandomRoles);