//doctor.cs
//Code for the zombie doctor role.

$MM::LoadedRole_ZombieDoctor = true;

$MM::GPZombieDocRange = 8;

$MM::ZombieInvStatus[0] = '\c3%1 \c4is \c2perfectly healthy\c4 and requires no treatment.';
$MM::ZombieInvStatus[1] = '\c3%1 \c4requires <color:A0B060>immediate medical attention\c4!';
$MM::ZombieInvStatus[2] = '\c3%1 \c4carries some <color:A0B060>unusual pathogens\c4.';
$MM::ZombieInvStatus[3] = '\c3%1 <color:A0B060>cannot be saved\c4.';

$MM::ZombieDocComment[0] = "\c4Cross your fingers and hope for the best.";
$MM::ZombieDocComment[1] = "\c4Hope they get well soon.";
$MM::ZombieDocComment[2] = "\c4Best of luck to them.";
$MM::ZombieDocComments = 3;

if(!isObject(MMRole_ZombieDoctor))
{
	new ScriptObject(MMRole_ZombieDoctor)
	{
		class = "MMRole";

		name = "Zombie Doctor";
		corpseName = "talented pathologist";
		displayName = "Doctor";

		letter = "ZD";

		colour = "<color:A0B0EC>";
		nameColour = "0.627 0.690 0.925";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		zombieDoc = true;

		alignment = 0;

		helpText =	"\c4At night, the <color:A0B0EC>Zombie Doctor \c4may either diagnose or treat another player." NL
					"\c4A diagnosis will determine whether a player is <color:80A046>turning \c4and if they are treatable." NL
					"\c4Diagnosis is done by using the \c3/diagnose \c4command and then right-clicking the target player." NL
					"\c4If a player has been infected, but has not yet converted to an <color:A0B060>Infected\c4, they may be treated." NL
					"\c4Treatment is done by using the \c3/treat \c4command and then right-clicking the target player." NL
					"\c4Players that have been successfully treated before turning are granted immunity to further infection.";

		description = 	"\c4The <color:A0B0EC>Zombie Doctor \c4is an innocent role is to protect others from infection." NL
						"\c4At night, the <color:A0B0EC>Zombie Doctor \c4may either diagnose or treat another player." NL
						"\c4A diagnosis will determine whether a player is <color:80A046>turning \c4and if they are treatable." NL
						"\c4Diagnosis is done by using the \c3/diagnose \c4command and then right-clicking the target player." NL
						"\c4If a player has been infected, but has not yet converted to an <color:A0B060>Infected\c4, they may be treated." NL
						"\c4Treatment is done by using the \c3/treat \c4command and then right-clicking the target player." NL
						"\c4Players that have been successfully treated before turning are granted immunity to further infection.";
	};
}

//ZUPPORT
function MM_getZombieInvResult(%doc, %target)
{
	if($MM::ZombieInvStatus[%doc.role.forceZombieInvResult] !$= "")
		return %doc.role.forceZombieInvResult;

	if(!isObject(%target.role))
		return 0;

	if($MM::ZombieInvStatus[%target.role.forceZombieInvAlignment] !$= "")
		return %target.role.forceZombieInvAlignment;

	return %target.infected | (%target.MM_isZombie() << 1);
}

function GameConnection::MM_isZombieDoc(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(%mini.isDay)
		return false;

	if(!isObject(%this.role))
		return false;

	if(%this.isGhost || %this.lives < 1)
		return false;

	if(%this.role.getAlignment() != 0)
		return false;

	if(!%this.role.zombieDoc && !%mini.allZombieDoc)
		return false;

	if(%this.doctor[%mini.day])
		return false;

	return true;
}

function GameConnection::MM_ZombieTreat(%doc, %this)
{
	if(!isObject(%this))
		return;

	if((%this.isGhost || %this.lives < 1))
		return;

	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return;

	if(!isObject(%doc) || !%doc.MM_isZombieDoc())
		return;

	%doc.doctor[%mini.day] = true;
	%doc.docAction = 0;

	%role = %this.role;
	if(%role.MM_isZombie())
	{
		%mini.MM_LogEvent(%doc.MM_getName(1) @ "\c6's treatment could not bring back" SPC %this.MM_getName(1));
		return;
	}

	if(!%this.infected)
	{
		%mini.MM_LogEvent(%doc.MM_getName(1) @ "\c6's treatment had no benefit to" SPC %this.MM_getName(1));
		return;
	}

	%this.infected = false;
	%this.infector = "";
	%this.infectDay = "";
	%this.weakInfect = "";
	%this.weakInfectWeight = "";

	%this.infectImmune = true;

	%mini.MM_LogEvent(%doc.MM_getName(1) SPC "\c6successfully treated" SPC %this.MM_getName(1));
}

function GameConnection::MM_ZombieInvPlayer(%this, %target)
{
	if(!isObject(%target) || !isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !$DefaultMiniGame.running)
		return;

	if(%target.isGhost || %target.lives < 1)
		return;

	%this.docAction = 0;

	if(!%this.MM_isZombieDoc())
	{
		messageClient(%this, '', "\c4You may not diagnose players.");
		return;
	}

	%r = MM_getZombieInvResult(%this, %target);

	messageClient(%this, '', $MM::ZombieInvStatus[%r], %target.getSimpleName());

	%mini.MM_LogEvent(%this.MM_getName(1) SPC "\c6diagnosed" SPC %target.MM_getName(1));

	%this.doctor[%mini.day] = true;
}

function serverCmdDiagnose(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return;

	if(%this.doctor[%mini.day])
	{
		messageClient(%this, '', "\c4You have already used an action tonight.");
		return;
	}

	if(!%this.MM_isZombieDoc())
		return;

	%this.docAction = 1;

	messageClient(%this, '', "\c4Right-click a player to \c3diagnose \c4them.");
}

function serverCmdTreat(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return;

	if(%this.doctor[%mini.day])
	{
		messageClient(%this, '', "\c4You have already used an action tonight.");
		return;
	}

	if(!%this.MM_isZombieDoc())
		return;

	%this.docAction = 2;

	messageClient(%this, '', "\c4Right-click a player to \c3treat \c4them.");
}

package MMRole_ZombieDoctor
{
	function MMRole::onTrigger(%this, %mini, %client, %obj, %slot, %val)
	{
		parent::onTrigger(%this, %mini, %client, %obj, %slot, %val); //Call base MMRole functionality (most likely nothing)

		if(!isObject(%client.player) || %client.getControlObject() != %client.player || !%client.MM_isZombieDoc())
			return;

		// talk(%slot SPC %val);
		if(%slot != 4 || !%val)
			return;

		%start = %obj.getEyePoint();
		%vec = %obj.getEyeVector();
		%end = VectorAdd(%start, VectorScale(%vec, $MM::GPZombieDocRange));

		%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
		%aObj = firstWord(%ray);
		if(!isObject(%aObj) || %aObj.getClassName() !$= "Player" || !isObject(%cl = %aObj.getControllingClient()))
			return;

		switch(%client.docAction)
		{
			case 0:
				messageClient(%client, '', "\c4Use the \c3/diagnose \c4or \c3/treat \c4commands to select an action for\c3" SPC %cl.getSimpleName());
				return;

			case 1:
				%client.MM_ZombieInvPlayer(%cl);
				return;

			case 2:
				messageClient(%client, '', "\c4Treating\c3" SPC %cl.getSimpleName() @ "\c4..." SPC $MM::ZombieDocComment[getRandom($MM::ZombieDocComments)]);

				%client.MM_ZombieTreat(%cl);
				return;

			default:
				messageClient(%client, '', "\c4You try your hardest to enact ancient voodoo magic upon the patient, but to no avail.");
				return;
		}
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		for(%i = 0; %i <= %mini.day; %i++)
			%client.doctor[%i] = "";
	}
};
activatePackage(MMRole_ZombieDoctor);