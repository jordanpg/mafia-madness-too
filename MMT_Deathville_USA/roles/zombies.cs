//zombies.cs
//Code for the zombie roles in the Deathville USA gamemode.

$MM::LoadedRole_Zombies = true;

$MM::GPZombieInfectRole = "IN";
$MM::GPZombieRaiseRole = "Z";
$MM::GPZombieInfectNotifySpectators = true;
$MM::GPZombieRevivePlayDead = true;
$MM::GPZombieInfectRange = 8;
$MM::GPZombieWeakInfectChance = 0.2;
$MM::GPZombieWeakRaiseChance = 0.4;
$MM::GPZombieAdditionalAttackWeight = 0.25; //each subsequent attack increases odds of infection by this amount
$MM::GPZombieAttackTimeout = 0.5;
$MM::GPZombieAttackDist = 8;
$MM::GPZombieAttackDamage = 14;
$MM::GPZombiePushForce = 5;
$MM::GPZombiePushAirForce = 5;

$MM::ZombieAlignment = 70;
$MM::Alignment[$MM::ZombieAlignment] = "Zombie";
$MM::AlignmentColour[$MM::ZombieAlignment] = "<color:80A046>";

$MM::InvStatus[$MM::ZombieAlignment] = '\c3%1 \c4seems to be somewhat <color:80A046>unwell\c4.';

if(!isObject(MMRole_Infected))
{
	new ScriptObject(MMRole_Infected)
	{
		class = "MMRole_Zombie";
		superClass = "MMRole";

		name = "Infected";
		corpseName = "unhealthy friend";
		displayName = "Infected";

		letter = "IN";

		colour = "<color:A0B060>";
		nameColour = "0.627 0.690 0.376";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		zombieInfect = true;
		zombieAttack = false;

		alignment = $MM::ZombieAlignment;
		isEvil = 1;

		forceInvAlignment = 0;

		helpText =	"\c4At night, the <color:A0B060>Infected \c4may infect another player by \c3right-clicking them\c4." NL
					"\c4The selected player will become <color:A0B060>Infected \c4in \c3one day \c4if they stay alive." NL
					"\c4If the selected player dies, they will instead revive as a <color:80A046>Zombie \c4at that time." NL
					"\c4It is best if the <color:A0B060>Infected \c4first looks to infect and expand their team, rather than focusing on elimination.";

		description = 	"\c4The <color:A0B060>Infected \c4is a <color:80A046>Zombie \c4role whose goal is to infect other players." NL
						"\c4At night, the <color:A0B060>Infected \c4may infect another player by \c3right-clicking them\c4." NL
						"\c4The selected player will become <color:A0B060>Infected \c4in \c3one day \c4if they stay alive." NL
						"\c4If the selected player dies, they will instead revive as a <color:80A046>Zombie \c4at that time." NL
						"\c4It is best if the <color:A0B060>Infected \c4first looks to infect and expand their team, rather than focusing on elimination.";
	};
}

if(!isObject(MMRole_Zombie))
{
	new ScriptObject(MMRole_Zombie)
	{
		class = "MMRole";

		name = "Zombie";
		corpseName = "sickly corpse";
		displayName = "Zombie";

		letter = "Z"; //cult member??

		colour = "<color:80A046>";
		nameColour = "0.5 0.627 0.275";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		zombieInfect = false; //handled separately
		zombieAttack = true;

		alignment = $MM::ZombieAlignment;
		isEvil = 1;

		helpText =	"\c4The <color:80A046>Zombie may attack other players by clicking on them." NL
					"\c4Players that are attacked have a chance to become <color:A0B060>Infected \c4in \c3one day\c4." NL
					"\c4If the attacked player dies, they will have an increased chance of reviving as a <color:80A046>Zombie \c4upon dead rising." NL
					"\c4If the attacked player survives turning once, they are immune from future infections.";

		description = 	"\c4The <color:80A046>Zombie \c4is a basic role whose goal is to infect and eliminate other players." NL
						"\c4The <color:80A046>Zombie may attack other players by clicking on them." NL
						"\c4Players that are attacked have a chance to become <color:A0B060>Infected \c4in \c3one day\c4." NL
						"\c4If the attacked player dies, they will have an increased chance of reviving as a <color:80A046>Zombie \c4upon dead rising." NL
						"\c4If the attacked player survives turning once, they are immune from future infections.";

		gun = -1;
	};
}

//ZUPPORT
function MM_PickZombieRole()
{
	return $MM::GPZombieRaiseRole;
}

function GameConnection::MM_isZombie(%this)
{
	if(!isObject(%this.role))
		return false;

	return %this.role.getAlignment() == $MM::ZombieAlignment;
}

function MinigameSO::MM_GetZombieList(%this)
{
	%list = "";
	for(%i = 0; %i < %this.memberCacheLen; %i++)
	{
		%mem = %this.memberCache[%i];

		%r = %this.memberCacheRole[%i];

		if(%has[%mem])
			continue;

		%has[%mem] = true;

		if(!isObject(%mem) && %r.getAlignment() == $MM::ZombieAlignment)
			%list = %list SPC %mem;
		else if(isObject(%mem) && %mem.MM_isZombie())
			%list = %list SPC %mem;
	}

	return trim(%list);
}

function GameConnection::MM_DisplayZombieList(%this, %centrePrint)
{
	%mini = getMiniGameFromObject(%this);
	if(!isObject(%mini) || !%mini.running)
		return;

	if(%centrePrint $= "")
		%centrePrint = $MM::MafListSetting | 0;

	if(%centrePrint != 2)
		messageClient(%this, '', "<color:400040>--");

	%cStr = "";

	%list = %mini.MM_GetZombieList();
	%ct = getWordCount(%list);
	for(%i = 0; %i < %ct; %i++)
	{
		%cl = getWord(%list, %i);

		if(!isObject(%r = %mini.role[%cl]))
			continue;

		%str = (isObject(%cl) ? %cl.MM_GetName(false, true) : %mini.memberCacheName[%mini.memberCacheKey[%cl]]) SPC "(" @ %r.getLetter() @ ")";

		if(%centrePrint != 2)
			messageClient(%this, '', %str);

		if(%centrePrint)
			%cStr = %cStr NL %str @ " ";
	}

	if(%centrePrint != 2)
		messageClient(%this, '', "<color:400040>--");

	if(%centrePrint)
		%this.centerPrint("<just:right><font:verdana:18><color:80A046>Zombies\c6:\n<font:verdana:16>" @ trim(%cStr) @ " ");
	else
		%this.centerPrint("");
}

function serverCmdZombieList(%this, %cp)
{
	if(!%this.MM_isZombie() && !((%this.isGhost || %this.lives < 1) && $MM::SpectatorMafList))
		return;

	%this.MM_DisplayZombieList(%cp);
}

function GameConnection::MM_canZombieInfect(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(%mini.isDay)
		return false;

	if(!isObject(%this.role))
		return false;

	if(%this.isGhost || %this.lives < 1)
		return false;

	if(!%this.MM_isZombie())
		return false;

	if(!%this.role.zombieInfect && !%mini.allInfect)
		return false;

	if(%this.infected[%mini.day])
		return false;

	return true;
}

function GameConnection::MM_ZombieInfectPlayer(%cl, %src)
{
	if(%cl.isGhost || %cl.lives < 1)
		return;

	if(!isObject(%mini = getMiniGameFromObject(%cl)))
		return;

	%role = $MM::RoleKey[$MM::GPZombieInfectRole];
	if(!isObject(%role))
		%role = MMRole_Zombie;

	%roleStr = %role.getColour(1) @ %role.getRoleName();

	if(isObject(%src))
		%mini.MM_LogEvent(%src.MM_getName(1) @ "\c6\'s infection turned" SPC %cl.MM_getName(1) SPC "\c6into the" SPC %roleStr);
	else
		%mini.MM_LogEvent(%cl.MM_getName(1) @ "\c6\'s infection turned them into the" SPC %roleStr);

	%cl.lives = 1 + %role.additionalLives;
	%cl.MM_setRole(%role);

	%cl.MM_UpdateUI();
	%cl.MM_DisplayStartText();
	%cl.clearInventory();
	%cl.MM_GiveEquipment();

	%cl.infector = "";
	%cl.infectDay = "";
	%cl.weakInfect = "";
	%cl.weakInfectWeight = "";
	%cl.turned = true;

	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%mem = %mini.member[%i];

		if((%isc = (%mem.MM_isZombie() && %mem.lives > 0)) || (%mem.lives < 1 && $MM::GPZombieInfectNotifySpectators))
		{
			messageClient(%mem, '', "<font:impact:24pt>\c3" @ %cl.getSimpleName() SPC "\c4has joined the zombies as the" SPC %roleStr @ "\c4!");

			if(%isc)
				%mem.MM_DisplayZombieList(2);
		}
	}
}

function AIPlayer::MM_ZombieInfectCorpse(%this, %reviver, %tCl)
{
	%cl = %tCl;
	if(!isObject(%cl))
		%cl = %this.originalClient;

	if(!isObject(%cl) || !%cl.isGhost || %cl.lives > 0 || %ccl.MMIgnore)
		return;

	if(!isObject(%mini = getMiniGameFromObject(%cl)))
		return;

	%role = $MM::RoleKey[MM_PickZombieRole()];
	if(!isObject(%role))
		%role = MMRole_Zombie;

	%roleStr = %role.getColour(1) @ %role.getRoleName();

	if(isObject(%reviver))
		%mini.MM_LogEvent(%reviver.MM_getName(1) @ "\c6\'s infection revived" SPC %cl.MM_getName(1) SPC "\c6into the" SPC %roleStr);
	else
		%mini.MM_LogEvent(%cl.MM_getName(1) SPC "\c6revived into the" SPC %roleStr);

	%cl.lives = 1 + %role.additionalLives;
	%cl.isGhost = false;
	%cl.MM_setRole(%role);

	if(isObject(%cl.player) && %cl.player.isGhost)
		%cl.player.delete();

	%cl.player = %this;
	%this.client = %cl;
	%cl.setControlObject(%this);

	%this.unMountImage(0);
	%this.isCorpse = false;
	if(!$MM::GPZombieRevivePlayDead)
		%this.playThread(3, "root");

	%this.isRisenCorpse = true;

	%cl.infector = "";
	%cl.infectDay = "";
	%cl.weakInfect = "";
	%cl.weakInfectWeight = "";
	%cl.turned = true;

	%cl.MM_UpdateUI();
	%cl.MM_DisplayStartText();
	%cl.clearInventory();
	%cl.MM_GiveEquipment();

	%role.onSpawn(%mini, %cl);

	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%mem = %mini.member[%i];

		if((%isc = (%mem.MM_isZombie() && %mem.lives > 0)) || (%mem.lives < 1 && $MM::GPZombieInfectNotifySpectators))
		{
			messageClient(%mem, '', "<font:impact:24pt>\c3" @ %cl.getSimpleName() SPC "\c4has joined the zombies as the" SPC %roleStr @ "\c4!");

			if(%isc)
				%mem.MM_DisplayZombieList(2);
		}
	}
}

//HOOKZ
function MMRole_Zombie::SpecialWinCheck(%this, %mini, %client, %killed, %killer)
{
	%r = parent::SpecialWinCheck(%this, %mini, %client, %killed, %killer);

	if(%client.lives < 1)
		return 3;

	%foundZombie = false;
	%foundOther = false;
	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%mem = %mini.member[%i];

		if(%mem.lives > 0 && isObject(%mem.role))
		{
			if(%mem.MM_isZombie())
				%foundZombie = true;
			else
				%foundOther = true;

			if(%foundZombie && %foundOther)
				return 4; //disallow anyone other than zombies is still present
		}
	}

	if(!%foundZombie && %foundOther)
		return 3;

	talk("All players are dead or turned. The zombies win!");
	MMDebug("Zombies win", %mini, %killed, %killer, %client);

	%mini.resolved = 1;
	%mini.schedule(3000, MM_Stop);

	return 4;
}

package MMRole_Zombie
{
	function GameConnection::MM_DisplayAlignmentDetails(%this, %alignment)
	{
		%r = parent::MM_DisplayAlignmentDetails(%this, %alignment);

		if(%r >= 0)
			return %r;

		if(%alignment == $MM::ZombieAlignment)
		{
			messageClient(%this, '', "\c4You are a member of the <color:80A046>Zombies\c4! Your goal is to eliminate or turn all other players.");
			messageClient(%this, '', "\c4Type \c3/zombieList \c4to see the list of zombie members again.");

			%this.schedule(0, MM_DisplayZombieList);
			%this.schedule(10000, MM_DisplayZombieList, 2);

			return 4;
		}

		return %r;
	}

	function MinigameSO::MM_onDay(%this)
	{
		parent::MM_onDay(%this);

		for(%i = 0; %i < %this.numMembers; %i++)
		{
			%mem = %this.member[%i];

			if(!%mem.infected)
				continue;

			if(%this.day >= %mem.infectDay && !%mem.isGhost && %mem.lives > 0)
			{
				if(%mem.MM_isCultist() || %mem.infectImmune || %mem.weakInfect && getRandom() > ($MM::GPZombieWeakInfectChance * (1 + %mem.weakInfectWeight * $MM::GPZombieAdditionalAttackWeight)))
				{
					%mem.infected = false;
					%mem.infectDay = "";
					%mem.infector = "";
					%mem.weakInfect = false;
					%mem.weakInfectWeight = "";

					%mem.infectImmune = true;

					%this.MM_LogEvent(%mem.MM_GetName(1) SPC "\c6resisted infection.");

					continue;
				}

				%mem.MM_ZombieInfectPlayer(%mem.infector);
			}
			else if(%mem.isGhost || %mem.lives < 1)
			{
				if(%mem.weakInfect && getRandom() > ($MM::GPZombieWeakRaiseChance * (1 + %mem.weakInfectWeight * $MM::GPZombieAdditionalAttackWeight)))
					continue;

				%mem.corpse.MM_ZombieInfectCorpse(%mem.infector);
			}
		}
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		%client.infector = "";
		%client.infected = "";
		%client.infectDay = "";
		%client.infectImmune = "";
		%client.weakInfect = "";
		%client.weakInfectWeight = "";
		%client.turned = false;

		for(%i = 0; %i < %mini.day; %i++)
			%client.infected[%i] = false;
	}

	function MMRole::onTrigger(%this, %mini, %client, %obj, %slot, %val)
	{
		parent::onTrigger(%this, %mini, %client, %obj, %slot, %val); //Call base MMRole functionality (most likely nothing)

		if(!isObject(%client.player) || %client.getControlObject() != %client.player || !%client.MM_canZombieInfect())
			return;

		// talk(%slot SPC %val);
		if(%slot != 4 || !%val)
			return;

		%start = %obj.getEyePoint();
		%vec = %obj.getEyeVector();
		%end = VectorAdd(%start, VectorScale(%vec, $MM::GPZombieInfectRange));

		%ray = containerRayCast(%start, %end, $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %obj);
		%aObj = firstWord(%ray);
		if(!isObject(%aObj) || %aObj.getClassName() !$= "Player" || !isObject(%cl = %aObj.getControllingClient()))
			return;

		if(isObject(%cl) && (%cl.MM_isZombie() || %cl.infected))
		{
			messageClient(%client, '', "\c3" SPC %cl.getSimpleName() SPC "\c4is already infected!");
			return;
		}

		%mini.MM_LogEvent(%client.MM_GetName(1) SPC "\c6attempted to infect" SPC %cl.MM_GetName(1));

		%client.infected[%mini.day] = true;
		%cl.infected = true;
		%cl.infector = %client;
		%cl.infectDay = %mini.day + 2;

		for(%i = 0; %i < %mini.numMembers; %i++)
			if(%mini.member[%i].MM_isZombie() && %mini.member[%i] != %client)
				messageClient(%mini.member[%i], '', "\c3" @ %client.getSimpleName() SPC "\c4is attempting to infect\c3" SPC %cl.getSimpleName() @ "\c4...");

		messageClient(%client, '', "\c4Attempting to infect\c3" SPC %cl.getSimpleName() @ "\c4. The infection will take effect after one day.");
	}

	function Player::activateStuff(%this)
	{
		%r = parent::activateStuff(%this);

		if(!isObject(%cl = %this.client))
			return %r;

		if(!isObject(%mini = getMiniGameFromObject(%this.client)) || !%mini.isMM || !%mini.running)
			return %r;

		if(!isObject(%cl.role) || !%cl.role.zombieAttack)
			return %r;

		if($Sim::Time - %this.lastAttack < $MM::GPZombieAttackTimeout)
			return %r;

		%start = %this.getEyePoint();
		%eye = %this.getEyeVector();
		%add = VectorScale(%eye, $MM::GPZombieAttackDist);
		%ray = containerRayCast(%start, VectorAdd(%start, %add), $Typemasks::PlayerObjectType | $Typemasks::FXbrickObjectType | $Typemasks::TerrainObjectType | $Typemasks::InteriorObjectType | $TypeMasks::VehicleObjectType, %this);
		%obj = firstWord(%ray);

		// echo("a" SPC %obj.getType() & $TypeMasks::PlayerObjectType);

		if(!isObject(%obj) || !(%obj.getType() & $TypeMasks::PlayerObjectType) || %obj.isCorpse)
			return %r;

		if(!isObject(%ocl = %obj.getControllingClient()) || %ocl.MM_isZombie())
			return %r;

		%vel = VectorAdd(VectorScale(getWords(%eye, 0, 1), $MM::GPZombiePushForce), "0 0" SPC $MM::GPZombiePushAirForce);

		%obj.addVelocity(%vel);

		%this.lastAttack = $Sim::Time;

		%obj.damage(%this, %this.getPosition(), $MM::GPZombieAttackDamage, $DamageType::Direct);

		%cl.MM_GunLog(%cl.MM_GetName(1) SPC "\c6attacked" SPC %ocl.MM_GetName(1));

		if(!%ocl.infected)
		{
			%ocl.infected = true;
			%ocl.weakInfect = true;
			%ocl.infectDay = %mini.day + 2;
			%ocl.infector = %cl;
			%ocl.weakInfectWeight = 0;

			messageClient(%cl, '', "\c3" SPC %ocl.getSimpleName() SPC "\c4may infect after one day. Subsequent attacks will increase odds of infection.");

			%mini.MM_LogEvent(%cl.MM_GetName(1) SPC "\c6attempted to weakly infect" SPC %ocl.MM_GetName(1));
		}
		else if(%ocl.weakInfect)
			%ocl.weakInfectWeight++;

		// echo("b");

		return %r;
	}
};
activatePackage(MMRole_Zombie);