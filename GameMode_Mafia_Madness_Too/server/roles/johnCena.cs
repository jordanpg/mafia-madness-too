//johnCena.cs
//Code for the John Cena joke role

$MM::LoadedRole_JohnCena = true;

$MM::GPJCThrowVel = 100;
$MM::GPJCPunchVel = 40;
$MM::GPJCPunchAirForce = 5;
$MM::GPJCArmDist = 8;
$MM::GPJCPunchTimeout = 0.5;
$MM::GPJCPunchDamage = 50;
$MM::GPJCThrowTick = 500;
$MM::GPJCScale = "1.125 1.125 1.125";
$MM::GPJCPlayMusic = true;
$MM::GPJCSpeedMod = 1.5;

if(!isObject(MMRole_JohnCena))
{
	new ScriptObject(MMRole_JohnCena)
	{
		class = "MMRole";

		name = "JOHN CENA";
		corpseName = "JOHN CENA AKA J.C. AKA CRACKER BARREL AKA POTTAWATAMIE MASSACRE";
		displayName = "JOHN CENA";

		letter = "JOHNCENA";

		colour = "<color:800080>";
		nameColour = "0.5 0 0.5";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 1;
		isEvil = 1;

		helpText = 	"\c4YOU ARE <color:800080>JOHN CENAAAAAAAAAAAAAAAAAAAAAAAAAAA" NL
					"\c4You can pick people up and throw them! You also have three lives and can punch people!";

		description = "<color:800080>JOHN CENA \c4can pick people up and throw them, punch, and also has three lives.";

		additionalLives = 2;

		gun = -1;
	};
}

//SUPPORT
function Player::MM_JCThrow(%obj)
{
	// echo("dsds");

	if(!$DefaultMinigame.running)
		return;

	%obj.throwSched = %obj.schedule($MM::GPJCThrowTick, MM_JCThrow);

	if(!isObject(%obj.heldCorpse))
	{
		%start = %obj.getEyePoint();
		%add = VectorScale(%obj.getEyeVector(), $MM::GPJCArmDist);
		%ray = containerRayCast(%start, VectorAdd(%start, %add), $TypeMasks::PlayerObjectType | $TypeMasks::FxBrickObjectType, %obj);
		// echo(%ray);
		if(!isObject(%hObj = firstWord(%ray)) || %hObj.getClassName() !$= "Player")
		{
			// echo(%hObj SPC %hObj.getClassName());
			return;
		}

		if(!isObject(%cl = %hObj.getControllingClient()))
			return;

		if(%cl.MM_isMaf())
			return;

		%obj.heldCorpse = %hObj;
		%obj.mountObject(%hObj, 0);
		%hObj.canDismount = false;
		%hObj.holder = %obj;

		%obj.playThread(3, "armReadyBoth");
	}
	else
	{
		%obj.heldCorpse.dismount();
		%obj.heldCorpse.setVelocity(VectorScale(%obj.getEyeVector(), $MM::GPJCThrowVel));
		%obj.heldCorpse.canDismount = true;
		%obj.heldCorpse.holder = "";
		%obj.heldCorpse = "";

		%obj.playThread(3, "root");
	}
}

//HOOKS
function MMRole_JohnCena::onTrigger(%this, %mini, %client, %obj, %slot, %val)
{
	parent::onTrigger(%this, %mini, %client, %obj, %slot, %val);

	if(!%val)
	{
		if(%slot == 4)
			cancel(%obj.throwSched);
		return;
	}

	if(%slot == 4)
		%obj.throwSched = %obj.schedule(0, MM_JCThrow);
}

function MMRole_JohnCena::onAssign(%this, %mini, %client)
{
	parent::onAssign(%this, %mini, %client);

	schedule(100, 0, messageAll, '', "<font:impact:36><shadow:2:2><shadowcolor:000000><color:800080>JOHN CENAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
}

function MMRole_JohnCena::onSpawn(%this, %mini, %client)
{
	parent::onSpawn(%this, %mini, %client);

	%client.player.setScale($MM::GPJCScale);
	%client.player.jc = true;

	if(isObject(MusicData_JOHN_CENA) && $MM::GPJCPlayMusic)
		%client.player.playAudio(0, MusicData_JOHN_CENA);

	if($MM::GPJCSpeedMod > 0)
		%client.player.setSpeedMod($MM::GPJCSpeedMod);
}

package MM_JohnCena
{
	function Player::activateStuff(%this)
	{
		%r = parent::activateStuff(%this);

		if(!$DefaultMinigame.running)
			return %r;

		if(!isObject(%cl = %this.client))
			return %r;

		if(!isObject(%cl.role) || nameToID(%cl.role) != nameToID(MMRole_JohnCena))
			return %r;

		if($Sim::Time - %this.lastPush < $MM::GPJCPunchTimeout)
			return %r;

		%start = %this.getEyePoint();
		%eye = %this.getEyeVector();
		%add = VectorScale(%eye, $MM::GPJCArmDist);
		%ray = containerRayCast(%start, VectorAdd(%start, %add), $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %this);
		%obj = firstWord(%ray);

		// echo("a" SPC %obj.getType() & $TypeMasks::PlayerObjectType);

		if(!isObject(%obj) || !(%obj.getType() & $TypeMasks::PlayerObjectType) || %obj.isCorpse)
			return %r;

		if(!isObject(%cl = %obj.getControllingClient()) || %cl.MM_isMaf())
			return %r;

		%vel = VectorAdd(VectorScale(getWords(%eye, 0, 1), $MM::GPJCPunchVel), "0 0" SPC $MM::GPJCPunchAirForce);

		%obj.addVelocity(%vel);

		%this.lastPush = $Sim::Time;

		%obj.damage(%this, %this.getPosition(), $MM::GPJCPunchDamage, $DamageType::Direct);

		// echo("b");

		return %r;
	}

	function Player::MM_ThrowCorpse(%this)
	{
		if(!%this.jc)
			return parent::MM_ThrowCorpse(%this);

		if(!isObject(%obj = %this.heldCorpse))
			return;

		%this.mountObject(%obj, 8);
		%obj.dismount();

		%vel = VectorAdd(%this.getVelocity(), VectorScale(%this.getEyeVector(), $MM::GPJCThrowVel));
		// echo(%vel);
		%obj.schedule(1, setVelocity, %vel);
		%obj.holder = "";

		%this.heldCorpse = 0;

		%obj.MM_onCorpseThrow(%this);
	}
};
activatePackage(MM_JohnCena);