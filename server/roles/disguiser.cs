//disguiser.cs
//Code for the basic Mafia role

$MM::LoadedRole_Disguiser = true;

if(!isObject(MMRole_Disguiser))
{
	new ScriptObject(MMRole_Disguiser)
	{
		class = "MMRole";

		name = "Disguiser";
		corpseName = "malicious obfuscator";
		displayName = "Disguiser";

		letter = "D";

		colour = "<color:B40450>";
		nameColour = "0.71 0.02 0.02";

		canDisguise = true;

		alignment = 1;
		isEvil = 1;

		helpText = "";

		description =  "\c4The <color:B40450>Disguiser\c4 is able to take the appearance of dead players." NL
		               "\c4This is done by holding a corpse and typing \c6/disguise \c4." NL
					   "\c4This ability can only be done once a game.";
		
	};
}

//HOOKS