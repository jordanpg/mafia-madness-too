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
		nameColour = "1 0 0.376";

		canAbduct = false;
		canInvestigate = false;
		canImpersonate = false;
		canCommunicate = false;
		canFingerprint = false;

		alignment = 1;

		helpText = "";

		description = "\c4The \c0Mafia \c4is basic role whose goal is to eliminate the \c2Innocents\c4.";
	};
}

//HOOKS