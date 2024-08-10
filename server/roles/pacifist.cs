$MM::LoadedRole_Pacifist = true;

if(!isObject(MMRole_Pacifist))
{
	new ScriptObject(MMRole_Pacifist)
	{
		class = "MMRole";

		name = "Pacifist";
		corpseName = "hoplophobic treehugger";
		displayName = "Pacifist";

		letter = "IF";

		colour = "<color:A2A0FF>";
		nameColour = "0.5 1 0.5";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 0;

        helpText =         "\c4You are also the <color:A2A0FF>Pacifist\c4! Your gun starts empty, but you have 2 lives." NL
		                   "\c4As the <color:A2A0FF>Pacifist\c4 you should not reveal yourself. Instead attempt to be killed to confirm roles.";

		description = "\c4The <color:A2A0FF>Pacifist \c4is an innocent bystander role that cannot fire their gun, but has 2 lives.";
		
		additionalLives = 1;
	};
}

function MMRole_Pacifist::onSpawn(%this, %mini, %client)
{
	parent::onSpawn(%this, %mini, %client);

	if(isObject(%client.player))
	{
		%client.player.forceEmptyGun = true;
		%client.player.toolAmmo[0] = 0; //shouldn't be necessary but i'll have the line here just in case
	}
}