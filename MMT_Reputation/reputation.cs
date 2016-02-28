//reputation.cs
//Creates the basic functionality needed for the reputation system.
$MM::GPLoadedReputation = true;

$MM::GPUseReputation = true;
$MM::GPReputationUnit = "rad points"; //arbitrary unit of measurement for how cool someone is
$MM::GPMaxReputation = 1000;
$MM::GPInitialReputation = 1000;
$MM::GPReputationRatio = 0.002;
$MM::GPKillPenalty = 30;
$MM::GPEvilRepRatio = 0.0003;
$MM::GPEvilKillBonus = 40;
$MM::GPRoundRepHeal = 5;
$MM::GPCleanRepBonus = 15;
$MM::GPRepAutokick = true;
$MM::GPRepKicklevel = 450;
$MM::GPRepAutoBan = false;
$MM::GPRepAutoBanTime = 60;
$MM::GPRepKickReason = "You have been kicked for having a low reputation score.";
$MM::GPRepBanReason = "You have been banned for %1 minutes because of your low reputation score.";
$MM::GPLogRepChanges = true;

$MM::GPEvilAlignments = "1 3 4"; //Alignments that will be evil regardless (unless overridden by isEvil being -1)

//straight from TTT LoL
$MM::GPRepNames = "0 \c6a <color:FF8200>Liability\t500 <color:FFB400>Dangerous\t650 <color:DC3C00>Trigger-happy\t800 <color:FFF087>Crude\t890 <color:FFFFFF>Reputable";

$MM::GPRepSave = "config/server/mmReputation.cs";

if(isFile($MM::GPRepSave) && !$MM::GPRepLoaded)
{
	exec($MM::GPRepSave);
	$MM::GPRepLoaded = true;
}

function serverCmdGiveRep(%this, %amt, %t0, %t1, %t2, %t3, %t4)
{
	if(!%this.isAdmin || !%this.isSuperAdmin)
		return;

	%amt |= 0;
	if(%amt == 0)
		return;

	%target = trim(%t0 SPC %t1 SPC %t2 SPC %t3 SPC %t4);
	%cl = findClientByName(%target);

	if(!isObject(%cl))
	{
		messageClient(%this, '', "\c6Could not find an active player the name of \'\c3" @ %target @ "\c6\.' Use /giveRepBLID to give reputation by BL_ID.");
		return;
	}

	$MMRep_[%cl.getBLID()] = %cl.repScore = (%cl.currRepScore += %amt);

	%name = %cl.getPlayerName();
	if(%amt > 0)
		messageClient(%this, '', "\c6Gave \c3" @ %amt SPC "\c6" @ $MM::GPReputationUnit SPC "to\c3" SPC %name @ "\c6.");
	else
		messageClient(%this, '', "\c6Took \c3" @ -%amt SPC "\c6" @ $MM::GPReputationUnit SPC "from\c3" SPC %name @ "\c6.");

	messageClient(%this, '', "\c3" @ %name SPC "\c6is now" SPC MM_GetRepString(%cl.repScore) @ "\c6, having a reputation score of \c3" @ %cl.repScore @ "\c6/" @ trim($MM::GPMaxReputation SPC $MM::GPReputationUnit) @ ".");

	echo(%this.getPlayerName() SPC "modified" SPC %name @ "\'s reputation by" SPC %amt);

	export("$MMRep*", $MM::GPRepSave);
}

function serverCmdTakeRep(%this, %amt, %t0, %t1, %t2, %t3, %t4)
{
	serverCmdGiveRep(%this, -%amt, %t0, %t1, %t2, %t3, %t4);
}

function serverCmdCheckRep(%this, %t0, %t1, %t2, %t3, %t4)
{
	%target = trim(%t0 SPC %t1 SPC %t2 SPC %t3 SPC %t4);
	if(%target $= "")
		%cl = %this;
	else
		%cl = findClientByName(%target);

	if(!isObject(%cl))
	{
		messageClient(%this, '', "\c6Could not find an active player the name of \'\c3" @ %target @ "\c6\.' Use /checkRepBLID to check reputation by BL_ID.");
		return;
	}

	messageClient(%this, '', "\c3" @ %cl.getPlayerName() SPC "\c6is" SPC MM_GetRepString(%cl.repScore) @ "\c6, having a reputation score of \c3" @ %cl.repScore @ "\c6/" @ trim($MM::GPMaxReputation SPC $MM::GPReputationUnit) @ ".");
}

function serverCmdRep(%this, %t0, %t1, %t2, %t3, %t4)
{
	serverCmdCheckRep(%this, %t0, %t1, %t2, %t3, %t4);
}

function serverCmdCheckRepBLID(%this, %blid)
{
	%rep = $MMRep_[%blid];

	if(%rep $= "")
	{
		messageClient(%this, '', "\c6No data found for BL_ID\c3" SPC %blid);
		return;
	}

	messageClient(%this, '', "\c3BL_ID" SPC %blid SPC "\c6is" SPC MM_GetRepString(%rep) @ "\c6, having a reputation score of \c3" @ %rep @ "\c6/" @ trim($MM::GPMaxReputation SPC $MM::GPReputationUnit) @ ".");
}

function serverCmdRepBLID(%this, %blid)
{
	serverCmdCheckRepBLID(%this, %blid);
}

//Return the name corresponding to the given reputation score.
function MM_GetRepString(%rep)
{
	%last = "-1?";

	%ct = getFieldCount($MM::GPRepNames);
	for(%i = 0; %i < %ct; %i++)
	{
		%f = getField($MM::GPRepNames, %i);
		%base = firstWord(%f);
		%name = restWords(%f);

		if(%rep < %base)
		{
			if(%last $= "-1?")
				return "\c0???";

			return %last;
		}

		%last = %name;
	}

	return %last;
}

//Calculates the reward for killing an evil role
function MM_GetReputationReward()
{
	return $MM::GPMaxReputation * mClampF($MM::GPEvilKillBonus * $MM::GPEvilRepRatio, 0, 1);
}

//Determines if a role is considered evil
function MMRole::getRepEvil(%this)
{
	if(%this.isEvil)
		return true;

	if(isInList($MM::GPEvilAlignments, %this.alignment) && %this.isEvil != -1)
		return true;

	return false;
}

//Returns the amount of reputation lost for killing this player (if they aren't evil)
function GameConnection::getReputationMod(%this)
{
	return %this.currRepScore * mClampF($MM::GPKillPenalty * $MM::GPReputationRatio, 0, 1);
}

//Update a player's reputation score at the end of the round
function GameConnection::UpdateReputation(%client)
{
	%bonus = $MM::GPRoundRepHeal + (%client.cleanRound | $MM::GPCleanRepBonus);
	%client.currRepScore += %bonus;

	%blid = %client.getBLID();

	%client.cleanRound = true;

	%client.currRepScore = mClamp(%client.currRepScore, 0, $MM::GPMaxReputation);

	return $MMRep_[%blid] = %client.repScore = %client.currRepScore;
}

function GameConnection::CheckRepKick(%this)
{
	if(%this.isAdmin || %this.isSuperAdmin)
		return;

	if(%this.repScore < $MM::GPRepKicklevel)
	{
		%name = %this.getPlayerName();
		echo("Kicking" SPC %name SPC "for having low reputation.");

		%blid = %this.getBLID();
		$MMRep_[%blid] = mClamp($MM::GPInitialReputation * 0.8, $MM::GPRepKicklevel * 1.1, $MM::GPMaxReputation);

		export("$MMRep*", $MM::GPRepSave);

		if($MM::GPRepAutoBan)
			serverCmdBan(0, %this, %blid, $MM::GPRepAutoBanTime, strReplace($MM::GPRepBanReason, "%1", $MM::GPRepAutoBanTime));
		else
		{
			%this.delete($MM::GPRepKickReason);
			messageAll('MsgAdminForce', "\c3" @ %name SPC "\c2was kicked for having a low reputation score.");
		}
	}
}

package MM_Reputation
{
	function GameConnection::autoAdminCheck(%this)
	{
		%r = parent::autoAdminCheck(%this);

		%blid = %this.getBLID();

		if($MMRep_[%blid] $= "")
		{
			$MMRep_[%blid] = $MM::GPInitialReputation;
			export("$MMRep*", $MM::GPRepSave);
		}

		%this.repScore = %this.currRepScore = $MMRep_[%blid];

		return %r;
	}

	function MMRole::onDeath(%this, %mini, %client, %srcObj, %srcClient, %damageType, %loc)
	{
		parent::onDeath(%this, %mini, %client, %srcObj, %srcClient, %damageType, %loc);

		if(!$MM::GPUseReputation)
			return;

		if(!isObject(%client) || !isObject(%srcClient))
			return;

		if(%client.getID() == %srcClient.getID())
			return;

		%arole = %srcClient.role;

		%evil1 = %arole.getRepEvil();
		%evil2 = %this.getRepEvil();

		if(%evil1 == %evil2) //Team kill
		{
			if(%evil1 && %evil2 && %this.alignment != %arole.alignment)
				return;

			%penalty = %client.getReputationMod();

			%srcClient.currRepScore -= %penalty;
			%srcClient.cleanRound = false;

			if($MM::GPLogRepChanges)
			{
				%add = "\c6(\c0-" @ %penalty SPC "\c6" @ $MM::GPReputationUnit @ ")";

				%logInd = %mini.eventLogLen - 1;
				%mini.eventLog[%logInd] = %mini.eventLog[%logInd] SPC %add;
			}
		}
		else if(!%arole.getRepEvil() && %this.getRepEvil())
		{
			%reward = MM_GetReputationReward();

			%srcClient.currRepScore += %reward;

			if($MM::GPLogRepChanges)
			{
				%add = "\c6(\c2+" @ %reward SPC "\c6" @ $MM::GPReputationUnit @ ")";

				%logInd = %mini.eventLogLen - 1;
				%mini.eventLog[%logInd] = %mini.eventLog[%logInd] SPC %add;
			}
		}
	}

	function MinigameSO::MM_Stop(%mini)
	{
		parent::MM_Stop(%mini);

		if(!$MM::GPUseReputation)
			return;

		for(%i = 0; %i < %mini.numMembers; %i++)
		{
			%this = %mini.member[%i];

			%this.UpdateReputation();

			if($MM::GPRepAutokick)
				%this.CheckRepKick();
		}

		export("$MMRep*", $MM::GPRepSave);
	}
};
activatePackage(MM_Reputation);