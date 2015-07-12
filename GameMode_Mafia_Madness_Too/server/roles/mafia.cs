//mafia.cs
//Code for the basic Mafia role

$MM::LoadedRole_Mafia = true;

if(!isObject(MMRole_Mafia))
{
	new ScriptObject(MMRole_Mafia)
	{
		class = "MMRole";

		name = "Mafia";
		corpseName = "mafia scumbag";
		displayName = "Mafia";

		letter = "M";

		colour = "<color:FF0040>";
		nameColour = "1 0 0";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 1;

		helpText = "";
	};
}

//HOOKS