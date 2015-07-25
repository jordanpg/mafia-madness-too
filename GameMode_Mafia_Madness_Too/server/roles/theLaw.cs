//theLaw.cs
//I Am The Law

if(!$MM::LoadedRole_Crazy)
	exec("./crazy.cs");

$MM::LoadedRole_TheLaw = true;

$Pref::Server:TTAmmo = 2;

if(!isObject(MMRole_TheLaw))
{
	new ScriptObject(MMRole_TheLaw : MMRole_Crazy)
	{
		name = "The Law";
		corpseName = "crazed machine gun enthusiast";
		displayName = "The Law";

		letter = "LAW";

		colour = "<color:400040>";
		nameColour = "0.376 0 0.376";

		helpText = MMRole_Crazy.helpText NL "<font:impact:28pt>\c3The Law \c5is in your hands.";

		description = "<color:400040>The Law \c4is a rarely occurring upgraded <color:FF00FF>Crazy\c4 who also has an \c3assault rifle!";

		equipment[1] = nameToID(TommyGunItem);
	};
}

TAssaultRifleProjectile1.directDamage = 100;
TAssaultRifleProjectile2.directDamage = 100;

function MMRole_TheLaw::onAssign(%this, %mini, %client)
{
	parent::onAssign(%this, %mini, %client);

	schedule(100, 0, messageAll, '', "<font:impact:36pt>\c3The Law\c0 has come");
}