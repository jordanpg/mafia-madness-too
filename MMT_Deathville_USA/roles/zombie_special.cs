//zombie_special.cs
//Code for special zombie roles.

if(!$MM::LoadedRole_Zombies)
	exec("./zombies.cs");

$MM::LoadedRole_ZombieSpecial = true;

$MM::GPZombieRunnerSpeedMod = 1.5;
$MM::GPZombieRunnerScale = "0.875 0.875 0.875";

$MM::GPSpecialZombies = true;
$MM::GPSpecialZombieTransformations = "ZR 0.2";

if(!isObject(MMRole_ZombieRunner))
{
	new ScriptObject(MMRole_ZombieRunner : MMRole_Zombie)
	{
		class = "MMRole_Zombie";
		superClass = "MMRole";

		name = "Zombie Runner";
		corpseName = "athletic cadaver";
		displayName = "Zombie Runner";

		letter = "ZR";

		helpText =	"\c4The <color:80A046>Zombie may attack other players by clicking on them." NL
					"\c4Players that are attacked have a chance to become <color:A0B060>Infected \c4in \c3one day\c4." NL
					"\c4If the attacked player dies, they will have an increased chance of reviving as a <color:80A046>Zombie \c4upon dead rising." NL
					"\c4If the attacked player survives turning once, they are immune from future infections." NL
					"\c4As a <color:80A046>Zombie Runner\c4, you have enhanced speed and reduced size, enhancing your combat abilities!";

		description = 	"\c4The <color:80A046>Zombie Runner\c4is an upgraded <color:80A046>Zombie \c4with enhanced run speed and reduced size.";
	};
}

//ZUPPORT
function MMRole_ZombieRunner::onSpawn(%this, %mini, %client)
{
	parent::onSpawn(%this, %mini, %client);

	%client.player.setScale($MM::GPZombieRunnerScale);

	if($MM::GPZombieRunnerSpeedMod > 0)
		%client.player.setSpeedMod($MM::GPZombieRunnerSpeedMod);
}

package MMRole_Zombie_Special
{
	function MM_PickZombieRole()
	{
		if(!$MM::GPSpecialZombies)
			return parent::MM_PickZombieRole();

		%list = $MM::GPSpecialZombieTransformations;

		while((%ct = getFieldCount(%list)) > 0)
		{
			%rand = getRandom(%ct-1);
			%field = getField(%list, %rand);
			%list = removeField(%list, %rand);

			%r = firstWord(%field);
			%chance = getWord(%field, 1);

			if(!isObject($MM::RoleKey[%r]))
				continue;

			if(getRandom() < %chance)
				return %r;
		}

		return $MM::GPZombieRaiseRole;
	}
};
activatePackage(MMRole_Zombie_Special);