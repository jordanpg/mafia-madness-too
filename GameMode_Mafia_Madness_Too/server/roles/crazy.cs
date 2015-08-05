//crazy.cs
//Code for the Crazy mafia role.

$MM::LoadedRole_Crazy = true;

$MM::GPCrazyReplaceName = "disfigured corpse";
$MM::GPCrazyReplaceRole = "permanently retired";

$MM::GPDisfigureDecapitates = true;

if(!isObject(MMRole_Crazy))
{
	new ScriptObject(MMRole_Crazy)
	{
		class = "MMRole";

		name = "Crazy";
		corpseName = "disgruntled lunatic";
		displayName = "Crazy";

		letter = "C";

		colour = "<color:FF00FF>";
		nameColour = "1 0 1";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 1;

		helpText = 	"\c4You are also the \c0Crazy\c4!  That means you get an extra weapon - a knife!" NL
					"\c4Your knife is not very useful compared to a gun, but by hitting a corpse with it, you can make a corpse unrecognizable." NL
					"\c4Anyone who inspects the corpse will not know their name or their role.  You can also charge up the knife to do it silently." NL
					"\c4No one but you has the knife, though, so be careful who you show it to!  Good luck!";

		description = 	"\c4The \c0Crazy\c4 gets an extra weapon - a knife!" NL
						"\c4The knife is not very useful compared to a gun, but by hitting a corpse with it, you can make a corpse unrecognizable." NL
						"\c4Anyone who inspects the corpse will not know their name or their role.  You can also charge up the knife to do it silently." NL
						"\c4No one but the crazy has the knife, though, so be careful who you show it to!";

		equipment[0] = nameToID(TrenchKnifeItem);
	};
}

//SUPPORT
function AIPlayer::MM_DisfigureCorpse(%this, %obj, %ech)
{
	if(!%this.isCorpse)
		return;

	if(!isObject(%cl = %obj.getControllingClient()) || !isObject(%mini = getMiniGameFromObject(%cl)) || !%mini.isMM || !$DefaultMinigame.running)
		return;

	if(%this.disfigured)
	{
		if(!%ech)
			messageClient(%cl, '', "\c4That corpse is already unrecognizable!");
		return;
	}

	%this.disfigured = true;
	%this.disfiguringClient = %cl;

	if($MM::GPDisfigureDecapitates)
		%this.hideNode("headSkin");
}

package MM_Crazy
{
	function AIPlayer::MM_onCorpseSpawn(%this, %mini, %client, %killerClient, %damageType)
	{
		parent::MM_onCorpseSpawn(%this, %mini, %client, %killerClient, %damageType);

		// if(%damageType == $DamageType::CombatKnife && !%this.disfigured)
		// {
		// 	if(isObject(%killerClient.player))
		// 		%this.MM_DisfigureCorpse(%killerClient.player, 1);
		// 	else
		// 	{
		// 		%this.disfigured = true;
		// 		%this.disfiguringClient = %killerClient;
		// 	}
		// }
	}

	function AIPlayer::MM_onCorpseReSpawn(%this, %mini, %client, %killerClient, %oldCorpse, %damageType)
	{
		parent::MM_onCorpseReSpawn(%this, %mini, %client, %killerClient, %oldCorpse, %damageType);

		if(%oldCorpse.disfigured)
		{
			%this.disfigured = true;
			%this.disfiguringClient = %oldCorpse.disfiguringClient;

			if($MM::GPDisfigureDecapitates)
				%this.hideNode("headSkin");
		}
	}

	function AIPlayer::MM_getCorpseName(%this)
	{
		if(%this.disfigured)
			return $MM::GPCrazyReplaceName;

		return parent::MM_getCorpseName(%this);
	}

	function AIPlayer::MM_getRoleName(%this)
	{
		if(%this.disfigured)
			return $MM::GPCrazyReplaceRole;

		return parent::MM_getRoleName(%this);
	}

	function AIPlayer::MM_InvestigateFingerprints(%this, %client)
	{
		if(%this.disfigured)
		{
			messageClient(%client, '', "\c2You cannot gather any information from the corpse.");
			return;
		}

		parent::MM_InvestigateFingerprints(%this, %client);
	}

	function TrenchKnifeImage::onFire(%this, %obj, %slot)
	{
		parent::onFire(%this, %obj, %slot);

		if(!$DefaultMinigame.running || !isObject(%cl = %obj.client) || !getMiniGameFromObject(%cl).isMM)
			return;

		%start = %obj.getEyePoint();
		%vec = %obj.getEyeVector();
		%end = VectorAdd(%start, VectorScale(%vec, %this.raycastWeaponRange));

		%ray = containerRayCast(%start, %end, %this.raycastWeaponTargets, %obj);
		if(!isObject(%hObj = firstWord(%ray)) || !%hObj.isCorpse)
			return;

		%hObj.MM_DisfigureCorpse(%obj);
	}

	function TrenchKnifeImage::onStabFire(%this, %obj, %slot)
	{
		parent::onStabFire(%this, %obj, %slot);

		if(!$DefaultMinigame.running || !isObject(%cl = %obj.client) || !getMiniGameFromObject(%cl).isMM)
			return;

		%start = %obj.getEyePoint();
		%vec = %obj.getEyeVector();
		%end = VectorAdd(%start, VectorScale(%vec, %this.raycastWeaponRange));

		%ray = containerRayCast(%start, %end, %this.raycastWeaponTargets, %obj);
		if(!isObject(%hObj = firstWord(%ray)) || !%hObj.isCorpse)
			return;

		%hObj.MM_DisfigureCorpse(%obj);
	}

	function GameConnection::applyMMSilhouette(%this)
	{
		parent::applyMMSilhouette(%this);

		if(%this.player.isCorpse && %this.player.disfigured && $MM::GPDisfigureDecapitates)
			%this.hideNode("headSkin");
	}
};
activatePackage(MM_Crazy);