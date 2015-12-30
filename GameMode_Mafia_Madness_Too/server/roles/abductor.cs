//abductor.cs
//Code for the Abductor mafia role

$MM::LoadedRole_Abductor = true;

$MM::GPAbductionRange = 8;
$MM::GPAbductionDelay = 3000;
$MM::GPAbductionGags = true;

if(!isObject(MMRole_Abductor))
{
	new ScriptObject(MMRole_Abductor)
	{
		class = "MMRole";

		name = "Abductor";
		corpseName = "incorrigible kidnapper";
		displayName = "Abductor";

		letter = "A";

		colour = "<color:2D1A4A>";
		nameColour = "0.176470588 0.1019607843 0.29019607";

		canAbduct = true;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;

		alignment = 1;

		helpText = 	"\c4You are also the <color:2d1a4a>Abductor\c4!  Right click someone once per night to make them disappear... permanently." NL
					"\c4Your special ability is best used on Innocents with special powers, like the Cop or Fingerprint Expert." NL
					"\c4The ability can only be used at close-range, and requires a lot of subtlety to use." NL
					"\c4The bodies of people you kill in this way should go to somewhere in the basement of the pyramid.  Good luck!";

		description = 	"\c4The <color:2d1a4a>Abductor\c4 can right click someone once per night to make them disappear... permanently." NL
						"\c4Its special ability is best used on Innocents with special powers, like the Cop or Fingerprint Expert." NL
						"\c4The ability can only be used at close-range, and requires a lot of subtlety to use." NL
						"\c4The bodies of people it kills in this way should go to somewhere in the basement of the pyramid.  Good luck!";
	};
}

//SUPPORT
function MM_getAbductionPos()
{
	if(!isObject(MMAbductionSpawns) || (%ct = MMAbductionSpawns.getCount()) == 0)
	{
		if($Pref::Server::MMDumpsterLoc !$= "")
			return $Pref::Server::MMDumpsterLoc;

		return;
	}

	return MMAbductionSpawns.getObject(getRandom(%ct - 1)).getTransform();
}

function MMRole::getCanAbduct(%this)
{
	return %this.canAbduct ? true : false;
}

function GameConnection::MM_canAbduct(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(%mini.isDay)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.MM_isMaf())
		return false;

	if(!%this.role.getCanAbduct() && !%mini.allAbduct)
		return false;

	if(%this.abducted[%mini.day])
		return false;

	return true;
}

function Player::MM_Abduct(%this, %mini, %obj)
{
	%pos = MM_getAbductionPos();
	%this.setTransform(%pos);
	%this.damage(%obj, %obj.getPosition(), %this.getDatablock().maxDamage, $DamageType::Direct);
}

//HOOKS
package MM_Abductor
{
	function MMRole::onTrigger(%this, %mini, %client, %obj, %slot, %val) //ok we're not just making a class thing since we need to also account for allAbduct
	{
		parent::onTrigger(%this, %mini, %client, %obj, %slot, %val); //Call base MMRole functionality (most likely nothing)

		if(!isObject(%client.player) || %client.getControlObject() != %client.player || !%client.MM_canAbduct())
			return;

		// talk(%slot SPC %val);
		if(%slot != 4 || !%val)
			return;

		%start = %obj.getEyePoint();
		%vec = %obj.getEyeVector();
		%end = VectorAdd(%start, VectorScale(%vec, $MM::GPAbductionRange));

		%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
		%aObj = firstWord(%ray);
		if(!isObject(%aObj) || %aObj.getClassName() !$= "Player" || !isObject(%cl = %aObj.getControllingClient()))
			return;

		if(isObject(%aObj.client) && %aObj.client.MM_isMaf())
		{
			messageClient(%client, '', "\c3" SPC %aObj.client.getSimpleName() SPC "\c4is a fellow mafia! You can't abduct them!");
			return;
		}

		%aObj.gagged = $MM::GPAbductionGags ? true : false;
		%aObj.schedule($MM::GPAbductionDelay, MM_Abduct, %mini, %obj);

		%client.abducted[%mini.day] = true;

		for(%i = 0; %i < %mini.numMembers; %i++)
			if(%mini.member[%i].MM_isMaf() && %mini.member[%i] != %client)
				messageClient(%mini.member[%i], '', "\c3" @ %client.getSimpleName() SPC "\c4is abducting\c3" SPC %aObj.client.getSimpleName() @ "\c4...");

		%mini.MM_LogEvent(%client.MM_GetName(1) SPC "\c6abducted" SPC %cl.MM_GetName(1));
		messageClient(%client, '', "\c2Abducted\c3" SPC %cl.getSimpleName() @ "\c2.");
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		for(%i = 0; %i <= %mini.day; %i++)
			%client.abducted[%i] = "";
	}
};
activatePackage(MM_Abductor);