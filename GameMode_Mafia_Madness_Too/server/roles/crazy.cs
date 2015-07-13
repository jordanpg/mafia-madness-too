//crazy.cs
//Code for the Crazy mafia role.

$MM::LoadedRole_Crazy = true;

$MM::CrazyReplaceName = "disfigured corpse";
$MM::CrazyReplaceRole = "permanently retired";

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
					"\c4Noone but you has the knife, though, so be careful who you show it to!  Good luck!";

		equipment[0] = nameToID(SilentCombatKnifeItem);
	};
}

//SUPPORT
function AIPlayer::MM_DisfigureCorpse(%this, %obj)
{
	if(!%this.isCorpse)
		return;

	if(!isObject(%cl = %obj.getControllingClient()) || !isObject(%mini = getMiniGameFromObject(%cl)) || !%mini.isMM || !$DefaultMinigame.running)
		return;

	if(%this.disfigured)
	{
		messageClient(%cl, '', "\c4That corpse is already unrecognizable!");
		return;
	}

	%this.disfigured = true;
	%this.disfiguringClient = %cl;
}

package MM_Crazy
{
	function AIPlayer::MM_getCorpseName(%this)
	{
		if(%this.disfigured)
			return $MM::CrazyReplaceName;

		return parent::MM_getCorpseName(%this);
	}

	function AIPlayer::MM_getRoleName(%this)
	{
		if(%this.disfigured)
			return $MM::CrazyReplaceRole;

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

	function SilentCombatKnifeImage::onFire(%this, %obj, %slot)
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

	function SilentCombatKnifeImage::onStabFire(%this, %obj, %slot)
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
};
activatePackage(MM_Crazy);