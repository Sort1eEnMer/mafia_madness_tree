//phanto using horrible whitespacing

$MM::LoadedRole_Enchanter = true;

if(!isObject(MMRole_Enchanter))
{
	new ScriptObject(MMRole_Enchanter)
	{
		class = "MMRole";

		name = "Enchanter";
		corpseName = "bewitched foreigner";
		displayName = "Enchanter";

		letter = "EN";

		colour = "<color:BB66FF>";
		nameColour = "0.75 0.6 1";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		canEnchant = true;

		alignment = 1;
		isEvil = 1;

		helpText = 	"\c4You are also the <color:BB66FF>Enchanter\c4!  You are responsible for making sure certain 'sensitive' information remains unspoken." NL
					"\c4Type \c6/enchant (person) (word)\c4 to enchant a specific keyword!" NL
					"\c4Anyone who says the keyword will mysteriously pass away...  Good luck!" NL
					"<font:impact:32pt>\c4You are still a member of the \c0Mafia\c4 though, so don't forget it!";

		description = 	"\c4The <color:BB66FF>Enchanter\c4 is responsible for making sure certain 'sensitive' information remains unspoken." NL
					"\c4Type \c6/enchant (person) (word)\c4 to enchant a specific keyword!" NL
					"\c4Anyone who says the keyword will mysteriously pass away... appearing to have suicided." NL
					"<font:impact:32pt>\c4You are still a member of the \c0Mafia\c4 though, so don't forget it!";

	};
}

//SUPPORT
function MMRole::getCanEnchant(%this)
{
	return %this.canEnchant ? true : false;
}

function GameConnection::MM_canEnchant(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.MM_isMaf())
		return false;

	if(!%this.role.getCanEnchant())
		return false;

	if(%this.isGhost || %this.lives < 1)
		return false;

	return true;
}


function ServerCmdEnchant(%this, %name, %keyword)
{

	if(!%this.MM_canEnchant())

		return;


	%target = findclientbyname(%name);

	if(!isObject(%target))

		return messageClient(%this,'',"\c4That person does not exist!");


	

if(%target.role.alignment == 1)

		return messageClient(%this,'',"\c4That person is on your team! What are you doing?!");


	if(%this.hasEnchanted)

		return messageClient(%this,'',"\c4You have already enchanted a person! Please wait until the next day!");


	if(strLen(%keyword) < 5)

		return messageClient(%this,'',"\c4That word is too short! Choose a longer word!");


	%mini = getMiniGameFromObject(%this);

	%mini.enchantKeyword = %keyword;

	%target.enchanted = 1;

	%target.enchanter = %this;

	%this.hasEnchanted = 1;

	messageClient(%this,'',"\c4You have enchanted \c6" @ %target.name @ "\c4 from speaking the word \c6" @ %keyword @ "\c4! If they say it, they will die!");
	%mini.MM_LogEvent(%this.MM_GetName(1) SPC "\c6enchanted" SPC %target.MM_GetName(1) SPC "\c6from speaking the word\c4" SPC %mini.enchantKeyword);

	for(%i = 0; %i < clientGroup.getCount(); %i++)
 
	{

		%e = clientGroup.getObject(%i);

		if(!%e.isGhost && %e.MM_isMaf())
			messageClient(%e, '', "\c6" @ %this.name @ "\c4 has enchanted \c6" @ %target.name @ "\c4 from speaking the word \c6" @ %keyword @ "\c4!");

	}


}

package MM_Enchanter
{

	function MinigameSO::MM_InitRound(%this)
	{

		parent::MM_InitRound(%this);

		%this.enchantKeyword = "219751257120743082147381206408231807142";

	}

	function MiniGameSO::MM_onDay(%this)
	{
		parent::MM_onDay(%this);

		%this.enchantKeyword = "219751257120743082147381206408231807142";

		for(%i = 0; %i < clientGroup.getCount(); %i++)
		{

			clientGroup.getObject(%i).hasEnchanted = 0;

			clientGroup.getObject(%i).enchanted = 0;

		}

	}
	function MMRole::onChat(%role, %mini, %this, %msg, %type)
	{
		if(%this.enchanted == 1 && %this.role.alignment != 1)
		{
	
		%mini = getMiniGameFromObject(%this);

			for(%i = 0; %i < getWordCount(%msg); %i++)
			{

				%w = getWord(%msg,%i);

				//announce(%this.name SPC %msg SPC %i SPC getWord(%msg,%i));

				if(%w $= %mini.enchantKeyword)
				{
	
				%mini.MM_LogEvent(%this.MM_GetName(1) SPC "\c6spoke the enchanted word put by" SPC %this.enchanter.MM_GetName(1));

					%this.player.kill();

					messageClient(%this,'',"\c4A mysterious force grips your arteries as you utter the fateful word........\c6" SPC %mini.enchantKeyword @ "\c4!!!!!");

					break;

				}

			}

		}

		parent::onChat(%role, %mini, %this, %msg, %type);
	}

	
function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);
		%mini.enchantKeyword = "219751257120743082147381206408231807142";
		%client.hasEnchanted = 0;

		%client.enchanted = 0;
	}
};
deactivatePackage(MM_Enchanter);
activatePackage(MM_Enchanter);