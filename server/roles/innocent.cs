$MM::LoadedRole_Innocent = true;

if(!isObject(MMRole_Innocent))
{
	new ScriptObject(MMRole_Innocent)
	{
		class = "MMRole";

		name = "Innocent";
		corpseName = "upstanding citizen";
		displayName = "Innocent";

		letter = "I";

		colour = "<color:00FF00>";
		nameColour = "0 1 0";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 0;

		helpText = "";

		description = "\c4The \c2Innocent \c4is basic role whose goal is to eliminate the \c0Mafia\c4.";
	};
}

//HOOKS