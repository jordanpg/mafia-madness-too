//reviverCultist.cs
//Code for the Reviver Cultist and Zombie Cultist roles.

if(!$MM::LoadedRole_Cultist)
	exec("./cultist.cs");

$MM::LoadedRole_ReviverCultitst = true;

$MM::CultReviveTime = 3000;

if(!isObject(MMRole_ZombieCultist))
{
	new ScriptObject(MMRole_ZombieCultist)
	{
		class = "MMRole_Cultist";
		superClass = "MMRole";

		name = "Zombie Cultist";
		corpseName = "devout undead";
		displayName = "Zombie Cultist";

		letter = "ZC";

		colour = "<color:400040>";
		nameColour = "0.376 0 0.376";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 4;

		helpText = "";

		description = 	"\c4The <color:400040>Zombie Cultist \c4is basic role whose goal is to eliminate both the \c2Innocents \c4and the \c0Mafia\c4." NL
						"\c4It is like the regular <color:400040>Cultist\c4, however it cannot recruit members at night.";
	};
}

if(!isObject(MMRole_ReviverCultist))
{
	new ScriptObject(MMRole_ReviverCultist)
	{
		class = "MMRole_Cultist";
		superClass = "MMRole";

		name = "Reviver Cultist";
		corpseName = "devout necromancer";
		displayName = "Reviver Cultist";

		letter = "RC";

		colour = "<color:408040>";
		nameColour = "0.376 0.5 0.376";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 4;

		helpText = 	"\c4You are also the <color:408040>Reviver Cultist\c4! \c4You have the ability to \c3raise the dead \c4into your alignment!" NL
					"\c4To revive a corpse, pick it up and type \c3/reviveCorpse\c4. Three seconds after you drop the corpse, the player will rise as a <color:400040>Zombie Cultist\c4!";

		description =	"\c4The <color:408040>Reviver Cultist \c4is a <color:400040>Cultist \c4that may \c3raise the dead \c4into their alignment once every day." NL
						"\c4To revive a corpse, pick it up and type \c3/reviveCorpse\c4. Three seconds after you drop the corpse, the player will rise as a <color:400040>Zombie Cultist\c4!";

		cultRevive = true;
	};
}

//SUPPORT
function GameConnection::MM_canCultRevive(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(!isObject(%this.role))
		return false;

	if(!%this.MM_isCultist())
		return false;

	if(!%this.role.cultRevive && !%mini.allRevive)
		return false;

	if(%this.revived[%mini.day])
		return false;

	return true;
}

function AIPlayer::MM_CultRevive(%this, %reviver)
{
	cancel(%this.revSched);
	
	if(!isObject(%cl = %this.originalClient) || !%cl.isGhost || %cl.lives > 0)
	{
		if(isObject(%reviver))
			%reviver.revived[getMiniGameFromObject(%reviver).day] = false;

		return;
	}

	if(!isObject(%mini = getMiniGameFromObject(%cl)))
	{
		if(isObject(%reviver))
			%reviver.revived[getMiniGameFromObject(%reviver).day] = false;

		return;
	}

	%roleStr = MMRole_ZombieCultist.getColour(1) @ MMRole_ZombieCultist.getRoleName();

	if(isObject(%reviver))
		%mini.MM_LogEvent(%reviver.MM_getName(1) SPC "\c6revived" SPC %cl.MM_getName(1) SPC "\c6into the" SPC %roleStr);
	else
		%mini.MM_LogEvent(%cl.MM_getName(1) SPC "\c6revived into the" SPC %roleStr);

	%cl.lives = 1 + MMRole_ZombieCultist.additionalLives;
	%cl.isGhost = false;
	%cl.MM_setRole(MMRole_ZombieCultist);

	if(isObject(%cl.player) && %cl.player.isGhost)
		%cl.player.delete();

	%cl.player = %this;
	%this.client = %cl;
	%cl.setControlObject(%this);

	%this.unMountImage(0);
	%this.isCorpse = false;
	%this.playThread(3, "root");
	%this.revive = "";
	%this.isRisenCorpse = true;

	%cl.MM_UpdateUI();
	%cl.MM_DisplayStartText();
	%cl.clearInventory();
	%cl.MM_GiveEquipment();

	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%mem = %mini.member[%i];

		if(%mem.MM_isCultist())
		{
			messageClient(%mem, '', "<font:impact:24pt>\c3" @ %cl.getSimpleName() SPC "\c4has joined the cult as the" SPC %roleStr @ "\c4!");
			%mem.MM_DisplayCultList(2);
		}
	}
}

function serverCmdReviveCorpse(%this)
{
	if(!isObject(%this.player) || !isObject(%mini = getMiniGameFromObject(%this)))
		return;

	if(!%this.MM_canCultRevive())
	{
		if(%this.revived[%mini.day])
			messageClient(%this, '', "\c4You can only revive once per day!");
		else
			messageClient(%this, '', "\c4You cannot revive!");

		return;
	}

	if(!isObject(%this.player.heldCorpse))
	{
		messageClient(%this, '', "\c4You cannot revive a player because you aren't holding a corpse! Pick up the corpse of the player you would like to revive.");
		return;
	}

	if(!isObject(%ccl = %this.player.heldCorpse.originalClient))
	{
		messageClient(%this, '', "\c4This player has left the game.");
		return;
	}

	if(%ccl.lives > 0 || !%ccl.isGhost)
	{
		messageClient(%this, '', "\c4This player cannot currently be revived! Either they are still alive (multiple lives) or they aren't in this round.");
		return;
	}

	if(%this.player.heldCorpse.disfigured || !isObject(%this.player.heldCorpse.role))
	{
		messageClient(%this, '', "\c4This corpse cannot be revived.");
		return;
	}

	%this.player.heldCorpse.revive = %this;
	%this.revived[%mini.day] = true;

	messageClient(%this, '', "\c4Reviving \c3" @ %this.player.heldCorpse.originalClient.getSimpleName() SPC "\c4as a" SPC MMRole_ZombieCultist.getColour(1) @ MMRole_ZombieCultist.getRoleName() @ "\c4. Drop the corpse and they will wake up in three seconds.");
}

//HOOKS
package MM_ReviverCultist
{
	function AIPlayer::MM_onCorpsePickUp(%this, %obj)
	{
		parent::MM_onCorpsePickUp(%this, %obj);

		cancel(%this.revSched);
	}

	function AIPlayer::MM_onCorpseThrow(%this, %obj)
	{
		parent::MM_onCorpseThrow(%this, %obj);

		if(isObject(%this.revive))
			%this.revSched = %this.schedule($MM::CultReviveTime, MM_CultRevive, %this.revive);
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		for(%i = 0; %i <= %mini.day; %i++)
			%client.revived[%i] = "";
	}
};
activatePackage(MM_ReviverCultist);