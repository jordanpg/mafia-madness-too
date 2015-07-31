//reviverCultist.cs
//Code for the Reviver Cultist and Zombie Cultist roles.

$MM::LoadedRole_ReviverCultitst = true;

$MM::Alignment[4] = "Cultist";
$MM::AlignmentColour[4] = "<color:400040>";

$MM::InvStatus[4] = '\c3%1 \c4has a bit of a <color:400040>strange disposition\c4.';

$MM::CultReviveTime = 3000;

if(!isObject(MMRole_Cultist))
{
	new ScriptObject(MMRole_Cultist)
	{
		class = "MMRole";

		name = "Zombie Cultist";
		corpseName = "devout undead lunatic";
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

		description = "\c4The <color:400040>Zombie Cultist \c4is basic role whose goal is to eliminate both the \c2Innocents \c4and the \c0Mafia\c4.";
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

function GameConnection::MM_isCultist(%this)
{
	if(!isObject(%this.role))
		return false;

	return %this.role.getAlignment() == 4;
}

function MinigameSO::MM_GetCultList(%this)
{
	%list = "";
	for(%i = 0; %i < %this.memberCacheLen; %i++)
	{
		%mem = %this.memberCache[%i];

		%r = %this.memberCacheRole[%i];

		if(%has[%mem])
			continue;

		%has[%mem] = true;

		if(!isObject(%mem) && %r.getAlignment() == 4)
			%list = %list SPC %mem;
		else if(isObject(%mem) && %mem.MM_isCultist())
			%list = %list SPC %mem;
	}

	return trim(%list);
}

function GameConnection::MM_DisplayCultList(%this, %centrePrint)
{
	%mini = getMiniGameFromObject(%this);
	if(!isObject(%mini) || !%mini.running)
		return;

	if(%centrePrint $= "")
		%centrePrint = $MM::MafListSetting | 0;

	if(%centrePrint != 2)
		messageClient(%this, '', "<color:400040>--");

	%cStr = "";

	%list = %mini.MM_GetCultList();
	%ct = getWordCount(%list);
	for(%i = 0; %i < %ct; %i++)
	{
		%cl = getWord(%list, %i);

		if(!isObject(%r = %mini.role[%cl]))
			continue;

		%str = (isObject(%cl) ? %cl.MM_GetName() : %mini.memberCacheName[%mini.memberCacheKey[%cl]]) SPC "(" @ %r.getRoleName() @ ")";

		if(%centrePrint != 2)
			messageClient(%this, '', %str);

		if(%centrePrint)
			%cStr = %cStr NL %str @ " ";
	}

	if(%centrePrint != 2)
		messageClient(%this, '', "<color:400040>--");

	if(%centrePrint)
		%this.centerPrint("<just:right><font:verdana:18><color:400040>Cultists\c6:\n<font:verdana:16>" @ trim(%cStr) @ " ");
	else
		%this.centerPrint("");
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

	%roleStr = MMRole_Cultist.getColour(1) @ MMRole_Cultist.getRoleName();

	if(isObject(%reviver))
		%mini.MM_LogEvent(%reviver.MM_getName(1) SPC "\c6revived" SPC %cl.MM_getName(1) SPC "\c6into the" SPC %roleStr);
	else
		%mini.MM_LogEvent(%cl.MM_getName(1) SPC "\c6revived into the" SPC %roleStr);

	%cl.lives = 1 + MMRole_Cultist.additionalLives;
	%cl.isGhost = false;
	%cl.MM_setRole(MMRole_Cultist);

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

function serverCmdCultList(%this, %cp)
{
	if(!%this.MM_isCultist() && !(%this.lives < 1 && $MM::SpectatorMafList))
		return;

	%this.MM_DisplayCultList(%cp);
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

	messageClient(%this, '', "\c4Reviving \c3" @ %this.player.heldCorpse.originalClient.getSimpleName() SPC "\c4as a" SPC MMRole_Cultist.getColour(1) @ MMRole_Cultist.getRoleName() @ "\c4. Drop the corpse and they will wake up in three seconds.");
}

//HOOKS
function MMRole_Cultist::SpecialWinCheck(%this, %mini, %client, %killed, %killer)
{
	%r = parent::SpecialWinCheck(%this, %mini, %client, %killed, %killer);

	if(%client.lives < 1)
		return 3;

	%foundCultist = false;
	%foundOther = false;
	for(%i = 0; %i < %mini.numMembers; %i++)
	{
		%mem = %mini.member[%i];

		if(%mem.lives > 0 && isObject(%mem.role))
		{
			if(%mem.MM_isCultist())
				%foundCultist = true;
			else if(%mem.role.getAlignment() == 0 || %mem.role.getAlignment() == 1)
				%foundOther = true;

			if(%foundCultist && %foundOther)
				return 4; //disallow any win if both cultists and inno/maf are present
		}
	}

	if(!%foundCultist && %foundOther)
		return 3;

	talk("All innocent and mafia are dead! The cultists win!");
	MMDebug("Cultist win", %mini, %killed, %killer, %client);

	%mini.resolved = 1;
	%mini.schedule(3000, MM_Stop);

	return 4;
}

package MM_Cultist
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

	function GameConnection::MM_DisplayAlignmentDetails(%this, %alignment)
	{
		%r = parent::MM_DisplayAlignmentDetails(%this, %alignment);

		if(%r >= 0)
			return %r;

		if(%alignment == 4)
		{
			messageClient(%this, '', "\c4You are a <color:400040>Cultist\c4! Your goal is to eliminate both the \c2Innocents \c4and the \c0Mafia\c4!");
			messageClient(%this, '', "\c4Type \c3/cultList \c4to see the list of cult members again.");

			%this.schedule(0, MM_DisplayCultList);

			return 4;
		}

		return %r;
	}

	function MMRole::onCleanup(%this, %mini, %client)
	{
		parent::onCleanup(%this, %mini, %client);

		for(%i = 0; %i <= %mini.day; %i++)
			%client.revived[%i] = "";
	}
};
activatePackage(MM_Cultist);