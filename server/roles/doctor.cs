//doctor.cs
//Code for the Doctor role

$MM::LoadedRole_Doctor = true;

$MM::DocHealRange = 12;

if(!isObject(MMRole_Doctor))
{
    new ScriptObject(MMRole_Doctor)
    {
        class = "MMRole";
        
        name = "Doctor";
        corpseName = "altruistic clinician";
        displayName = "Doctor";
        
        letter = "DC";
        
        colour = "<color:55ffcc>";
        nameColour = "0.33 1.0 0.8";
        
        canAbduct = false;
        canInvestigate = false;
        canImpersonate = false;
        canCommunicate = false;
        canHeal = true;
        
        alignment = 0;
      
        helpText =         "\c4You are also the <color:55ffcc>Doctor\c4! You can right click someone once per night to make them immune to one bullet or abduction." NL 
                           "\c4Your special ability is best used on Innocents with special powers, like the Cop or Fingerprint Expert." NL 
                           "\c4Your ability requires you to get close to your target, so be careful as people may mistake you for an Abductor!" NL 
                           "\c4You will get a message when someone tries to abduct the target. Good luck!";
        
        description =      "\c4The <color:66ffcc>Doctor\c4 can right click someone once per night to make them immune to one bullet and an abduction attempt." NL 
                           "\c4Its special ability is best used on Innocents with special powers, like the Cop or Fingerprint Expert." NL 
                           "\c4The ability requires you to get close to your target, so be careful as people may mistake you for an Abductor!" NL 
                           "\c4The Doctor will get a message when someone tries to abduct the target. Good luck!";
    };
}

// FUNCTIONALITY

//### Return whether the role can heal ###//
function MMRole::getCanHeal(%this) 
{
    return %this.canHeal ? true : false;
}

//### Conditions that allow player to heal ###//
function Gameconnection::MM_canHeal(%this)
{
    if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running) //Gamemode running?
        return false;

    if(%mini.isDay) //is it day?
        return false;

    if(!isObject(%this.role)) //does player have a role?
        return false;

    if(!%this.role.getCanHeal()) //can that role heal?
        return false;

    if(%this.healed) //has the doc already healed?
        return false;

    return true;
}


// ROLE SPECIFIC CODE
// For this section, there's three object values:
//            BOOL Player.protected[i]            - is the player protected on day i?
//            STRING Player.healer[i]             - name of the healer for day i
//            BOOL Client.healed[i]               - has the client used healing ability on day i?

deactivatePackage(MM_Doctor); //I usually do this to re-activate the package in case something is changed...
package MM_Doctor
{
    function MMRole::onTrigger(%this, %mini, %client, %obj, %slot, %val)
    {
        parent::onTrigger(%this, %mini, %client, %obj, %slot, %val);
        
        if(!isObject(%client.player) || %client.getControlObject() != %client.player || !%client.MM_canHeal())
        {
            return;
        }
        
        if(%slot != 4 || !%val) // If any button besides RCLICK is pressed
            return;
        
        //### SETUP RAYCASTING ###//
        %start = %obj.getEyePoint();
        %vec = %obj.getEyeVector();
        %end = VectorAdd(%start, VectorScale(%vec, $MM::DocHealRange));

        %ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
        %aObj = firstWord(%ray);
      
        //### Whatever the raycast hit was NOT a player ###//
        if(!isObject(%aObj) || %aObj.getClassName() !$= "Player") {
            return;
        }
      
        //### The doctor has already used the ability that night ###//
        if(%client.healed == true || %client.healed == 1)
        {
            MMDebug("Doctor" SPC %client.getSimpleName() SPC "tried to heal but has already used his ability for the night");
            return;
        }
      
        //### The target is already protected by a doctor ###//
        if(isObject(%aObj.client) && %aObj.protected)
        {
            messageClient(%client, '', "\c3" @ %aObj.client.getSimpleName() SPC "\c4is already under the protection of a doctor!");
            MMDebug("Doctor" SPC %client.getSimpleName() SPC "tried to heal" SPC %aObj.client.getSimpleName() SPC "who was already protected by" SPC %aObj.healer);
            return;
        }
        
        //### The heal was SUCCESSFUL ###//
        %aObj.protected = true;
        %aObj.healer = %client.getSimpleName();
        %client.healed = true;
        MessageClient(%client, '', "\c4You successfully healed\c3" SPC %aobj.client.getSimpleName() @ "\c4!");
      
        MMDebug("Doctor" SPC %client.getSimpleName() SPC "successfully healed" SPC %aObj.client.getSimpleName());
        %mini.MM_LogEvent(%client.MM_GetName(1) SPC "\c6healed" SPC %aObj.client.MM_GetName(1));
        %aObj.gagged = false;
    }
    
    //### DAMAGE NEGATING FUNCTION ###//
    function Player::damage(%this, %obj, %pos, %amt, %type)
    {
        %mini = getMiniGameFromObject(%obj);
        if(%this.protected) //is the player being protected by a doc?
        {
            %amt = 0;
            MMDebug(%this.client.getSimpleName() SPC "took damage but was protected by" SPC %this.healer);
            %mini.MM_LogEvent(findclientbyname(%this.healer).MM_GetName(1) SPC "\c6protected" SPC %this.client.MM_GetName(1) SPC "\c6from a gunshot wound delivered by" SPC %obj.client.MM_GetName(1));
            %this.protected = false;
            %this.healer = "";
        }
        echo(%this.protected);
        parent::damage(%this, %obj, %pos, %amt, %type);
    }
    
    //### CLEANING THE ROLE WHEN IT IS REMOVED ###/
    function MMRole::onCleanup(%this, %mini, %client)
    {
        parent::onCleanup(%this, %mini, %client);
        
        for(%i = 0; %i <= %mini.day; %i++)
            %client.player.protected = ""; 
            %client.healed = "";
            %client.player.healer = "";
    }
  
    function MinigameSO::MM_onDay(%this) //Resets all healing statuses and shit
    {
        parent::MM_onDay(%this);

        for(%i = 0; %i < clientGroup.getCount(); %i++)
        {
          for(%a = 0; %a < 30; %a++) //This is super shitty, i'm sorry to whoever has to read this :O
          {
              %client = clientGroup.getObject(%i);
              %player = %client.player;
          
              %player.protected = false;
              %player.healer = "";
              %client.healed = false;
          }
        }
    }
  
    function Player::MM_Abduct(%this, %mini, %obj) //Overwrites the abduction process to not kill 
    {
        mmdebug(%this.client.getSimpleName() SPC "is attempting to be abducted by" SPC %obj.client.getSimpleName());
       %pos = MM_getAbductionPos();
       %this.setTransform(%pos);
       if(%this.protected == 1 || %this.protected == true)
       {
         messageClient(%this.client,'',"\c4You were abducted! However, a friendly \c3Doctor\c4 protected you from a premature grave!");
         MMDebug(%this.client.getSimpleName() SPC "was abducted by" SPC %obj.client.getSimpleName() SPC "but was protected by" SPC %this.healer);
         %this.protected = false;
         %mini.MM_LogEvent(findclientbyname(%this.healer).MM_GetName(1) SPC "\c6protected" SPC %this.client.MM_GetName(1) SPC "\c6from abduction by" SPC %obj.client.MM_GetName(1));
         return;
       }
        if(%this.protected == 0 || %this.protected $= "" || !%obj.protected)
        {
            %this.damage(%obj, %obj.getPosition(), %this.getDatablock().maxDamage, $DamageType::Direct);
            %this.protected = false;
            %this.healer = "";
            %this.client.healed = false;
	    parent::MM_Abduct(%this,%mini,%obj);
        }
    }
};
activatePackage(MM_Doctor);

