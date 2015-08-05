//clown.cs
//Code for the unaligned Clown joke role.

$MM::InvStatus[2] = '\c3%1 \c4is quite a <color:808080>strange individual\c4.';

$MM::GPClownHornTimeout = 0.3;
$MM::GPClownForceCostume = true;
$MM::GPClownPush = true;
$MM::GPClownPushRange = 2;
$MM::GPClownPushForce = 5;
$MM::GPClownPushAirForce = 5;
$MM::GPClownPushTimeout = 2;
$MM::GPClownNoMafTrigger = false;

if(!isObject(MMRole_Clown))
{
	new ScriptObject(MMRole_Clown)
	{
		class = "MMRole";

		name = "The Clown";
		corpseName = "annoying comedian";
		displayName = "The Clown";

		letter = "CLOWN";

		colour = "<color:80FF80>";
		nameColour = "0.5 1 0.5";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 2;

		helpText = 	"\c4You are <color:80FF80>The Clown\c4! You aren't part of the mafia or innocent." NL
					"\c4If you get killed by any other player, you will win the round." NL
					"\c4But you don't have a gun! Instead, you have a horn! Click to use it." NL
					"\c4Suicide and falling won't count, though! The only death that counts is a kill, and you have two lives.";

		description = 	"\c4<color:80FF80>The Clown\c4 isn't part of the mafia or the innocents." NL
						"\c4If they get killed by any other player, they will win the round." NL
						"\c4But they don't have a gun! Instead, they have a horn! Click to use it." NL
						"\c4Suicide and falling won't count, though! The only death that counts is a kill, and they have two lives.";

		gun = -1;
		additionalLives = 1;
	};
}

//SUPPORT
function Player::MM_applyClownOutfit(%this)
{
	if(%this.client.isGhost || %this.client.lives < 1 || %this.isCorpse)
		return false;

	if(fileName(%this.getDatablock().shapeFile) !$= "m.dts")
		return false;

	%this.hideNode("ALL");

	if(isObject((%o = %this.getControlObject())) && %o.getDatablock().getName() $= "SkiVehicle")
	{
		%this.unHideNode("lski");
		%this.unHideNode("rski");
	}

	%this.unHideNode("headSkin");
	%this.unHideNode("chest");
	%this.unHideNode("pants");
	%this.unHideNode("LShoe");
	%this.unHideNode("RShoe");
	%this.unHideNode("LArm");
	%this.unHideNode("RArm");
	%this.unHideNode("LHand");
	%this.unHideNode("RHand");
	%this.unHideNode("epaulets");
	%this.unHideNode("pointyHelmet");
	%this.setFaceName("memeHappy");
	%this.setDecalName("Chef");

	%this.setNodeColor("headSkin", "1 1 1 1");
	%this.setNodeColor("chest", "0.9 0.9 0 1");
	%this.setNodeColor("pants", "0.9 0 0 1");
	%this.setNodeColor("LShoe", "0.2 0 0.8 1");
	%this.setNodeColor("RShoe", "0.2 0 0.8 1");
	%this.setNodeColor("LHand", "1 1 1 1");
	%this.setNodeColor("RHand", "1 1 1 1");
	%this.setNodeColor("LArm", "0.9 0.9 0 1");
	%this.setNodeColor("RArm", "0.9 0.9 0 1");
	%this.setNodeColor("epaulets", "0.9 0 0 1");
	%this.setNodeColor("pointyHelmet", "0.9 0 0 1");

	return true;
}

//HOOKS
function MMRole_Clown::onTrigger(%this, %mini, %client, %obj, %slot, %val)
{
	parent::onTrigger(%this, %mini, %client, %obj, %slot, %val);

	// echo("honk");

	if(!%val)
		return;

	if(%slot == 0 && ($Sim::Time - %obj.lastHorn) >= $MM::GPClownHornTimeout)
	{
		ServerPlay3D(ClownHornSound, %obj.getPosition());
		%obj.lastHorn = $Sim::Time;
	}
}

function MMRole_Clown::SpecialWinCheck(%this, %mini, %client, %killed, %killer)
{
	%r = parent::SpecialWinCheck(%this, %mini, %client, %killed, %killer);

	// talk("check" SPC %client SPC %killed SPC %killer);

	if(%killed == %killer || !isObject(%killer))
		return %r;

	if(%client == %killed && %client.lives < 1 && !(%killer.MM_isMaf() && $MM::GPClownNoMafTrigger))
	{
		// talk(%client.lives);
		%mini.MM_LogEvent("<color:80FF80>HONK HONK");
		talk("Honk!");
		MMDebug("Clown win", %mini, %killed, %killer, %client);

		%mini.resolved = 1;
		%mini.schedule(3000, MM_Stop);

		return 4;
	}

	return %r;
}

function MMRole_Clown::applyOutfit(%this, %mini, %client, %day)
{
	%r = parent::applyOutfit(%this, %mini, %client, %day);

	if(!$MM::GPClownForceCostume)
		return %r;

	if(!isObject(%client.player))
		return %r;

	if(%day)
		return %client.player.MM_applyClownOutfit();

	return %r;
}

function MMRole_Clown::onAssign(%this, %mini, %client)
{
	parent::onAssign(%this, %mini, %client);

	schedule(100, 0, messageAll, '', "<font:impact:36pt><color:80FF80>The Clown \c0has come! If you kill them, they will win!");
}

function MMRole_Clown::onDeath(%this, %mini, %client, %srcObj, %srcClient, %damageType, %loc)
{
	parent::onDeath(%this, %mini, %client, %srcObj, %srcClient, %damageType, %loc);

	if(%client == %srcClient || !isObject(%srcClient))
		%client.lives = 0;
}

package MM_Clown
{
	function Player::activateStuff(%this)
	{
		%r = parent::activateStuff(%this);

		if(!isObject(%cl = %this.client))
			return %r;

		if(!isObject(%cl.role) || nameToID(%cl.role) != nameToID(MMRole_Clown))
			return %r;

		if(!$MM::GPClownPush || $Sim::Time - %this.lastPush < $MM::GPClownPushTimeout)
			return %r;

		%start = %this.getEyePoint();
		%eye = %this.getEyeVector();
		%add = VectorScale(%eye, $MM::GPClownPushRange);
		%ray = containerRayCast(%start, VectorAdd(%start, %add), $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %this);
		%obj = firstWord(%ray);

		// echo("a" SPC %obj.getType() & $TypeMasks::PlayerObjectType);

		if(!isObject(%obj) || !(%obj.getType() & $TypeMasks::PlayerObjectType) || %obj.isCorpse)
			return %r;

		%vel = VectorAdd(VectorScale(getWords(%eye, 0, 1), $MM::GPClownPushForce), "0 0" SPC $MM::GPClownPushAirForce);

		%obj.addVelocity(%vel);

		%this.lastPush = $Sim::Time;

		// echo("b");

		return %r;
	}
};
activatePackage(MM_Clown);