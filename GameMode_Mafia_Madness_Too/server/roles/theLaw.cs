//theLaw.cs
//I Am The Law

if(!$MM::LoadedRole_Crazy)
	exec("./crazy.cs");

$MM::LoadedRole_TheLaw = true;

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

		equipment[1] = nameToID(TAssaultRifleItem);
	};
}

TAssaultRifleImage.projectile = gunProjectile;

function MMRole_TheLaw::onAssign(%this, %mini, %client)
{
	parent::onAssign(%this, %mini, %client);

	messageAll('', "<font:impact:36pt>\c3The Law\c0 has come");
}