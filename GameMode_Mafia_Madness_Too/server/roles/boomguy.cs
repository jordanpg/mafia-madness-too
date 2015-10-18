//boomguy.cs
//boom

$MM::LoadedRole_BoomGuy = true;

$MM::GPBoomGuyExplosion = VehicleFinalExplosionProjectile;
$MM::GPBoomGuyExplosionScale = 2;

if(!isObject(MMRole_BoomGuy))
{
	new ScriptObject(MMRole_BoomGuy)
	{
		class = "MMRole";

		name = "Boom Guy";
		corpseName = "explosive customer";
		displayName = "Boom Guy";

		letter = "BOOM";

		colour = "<color:804000>";
		nameColour = "0.5 0.376 0";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 0;

		helpText = "";

		description = "";
	};
}

function MMRole_BoomGuy::onTrigger(%this, %mini, %client, %obj, %slot, %val)
{
	parent::onTrigger(%this, %mini, %client, %obj, %slot, %val);

	if(%slot == 0 && %val)
		%obj.spawnExplosion($MM::GPBoomGuyExplosion, $MM::GPBoomGuyExplosionScale);
}