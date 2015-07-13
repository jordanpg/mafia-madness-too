//fingerprintExpert.cs
//Code for the Fingerprint Expert innocent role.

$MM::LoadedRole_FingerprintExpert = true;

if(!isObject(MMRole_FingerprintExpert))
{
	new ScriptObject(MMRole_FingerprintExpert)
	{
		class = "MMRole";

		name = "Fingerprint Expert";
		corpseName = "forensics enthusiast";
		displayName = "Fingerprint Expert";

		letter = "F";

		colour = "<color:C9FFF2>";
		nameColour = "0.78823529 1 0.949019607";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = true;

		alignment = 0;

		helpText = 	"\c4You are also the <color:C9FFF2>Fingerprint Expert\c4! When you examine (click) a corpse, you will get a list of fingerprints!" NL
					"\c4Whenever someone picks up a corpse, they will leave a fingerprint on the corpse." NL
					"\c4You are the only Fingerprint Expert in the game, so if someone else claims to be one, there's a strong chance they're Mafia!";
	};
}

//SUPPORT
function GameConnection::MM_canFingerprint(%this)
{
	if(!isObject(%mini = getMiniGameFromObject(%this)) || !%mini.isMM || !%mini.running)
		return false;

	if(%this.isGhost || %this.lives < 1)
		return true;

	if(!isObject(%this.role))
		return false;

	if(%this.MM_isMaf())
		return false;

	if(!%this.role.getCanFingerprint() && !%mini.allFingerprint)
		return false;

	return true;
}

function AIPlayer::MM_InvestigateFingerprints(%this, %client)
{
	if(!%this.isCorpse || !%client.MM_canFingerprint())
		return;

	%rtod = mFloor($Sim::Time - %this.timeOfDeath);
	%rtod = %rtod - (%rtod % 30);
	%rsec = %rtod % 60;
	%rmin = (%rtod - %rsec) / 60;
	%rsec2 = (%rtod + 30) % 60;
	%rmin2 = ((%rtod + 30) - %rsec2) / 60;
	messageClient(%client, '', "\c2Died between\c3" SPC %rmin SPC "\c2min\c3" SPC %rsec SPC "\c2sec and\c3" SPC %rmin2 SPC "\c2min\c3" SPC %rsec2 SPC "\c2secs ago.");

	messageClient(%client, '', "\c2Fingerprints:");
	for(%i = 0; %i < %this.fingerprintCt; %i++)
		messageClient(%client, '', "\c2" @ %i + 1 @ ".\c3" SPC %this.fingerprints[%i]);
}

//HOOKS
package MM_FingerprintExpert
{
	function AIPlayer::MM_onCorpseSpawn(%this, %mini, %client, %killerClient)
	{
		parent::MM_onCorpseSpawn(%this, %mini, %client, %killerClient);

		%this.fingerprintCt = 0;
	}

	function AIPlayer::MM_Investigate(%this, %client)
	{
		parent::MM_Investigate(%this, %client);

		if(%client.MM_canFingerprint())
			%this.MM_InvestigateFingerprints(%client);
	}

	function AIPlayer::MM_onCorpsePickUp(%this, %obj)
	{
		parent::MM_onCorpsePickUp(%this, %obj);

		if(!isObject(%cl = %obj.getControllingClient()) || %this.fingerprints[%this.fingerprintCt - 1] $= (%n = %cl.getSimpleName()))
			return;

		%this.fingerprints[%this.fingerprintCt] = %n;
		%this.fingerprintCt++;
	}

	function Player::MM_Abduct(%this, %mini, %obj)
	{
		%cl = %this.client;

		parent::MM_Abduct(%this, %mini, %obj);

		if(isObject(%cl.corpse) || isObject(%aCl = %obj.getControllingClient()))
		{
			%cl.corpse.fingerprints[%cl.corpse.fingerprintCt] = %aCl.getSimpleName();
			%cl.corpse.fingerprintCt++;
		}
	}
};
activatePackage(MM_FingerprintExpert);