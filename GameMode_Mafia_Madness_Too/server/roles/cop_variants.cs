//cop_variants.cs
//Code for the Cop inno role variants.

if(!$MM::LoadedRole_Cop)
	exec("./cop.cs");

$MM::LoadedRole_Cop_Variants = true;

if(!isObject(MMRole_Cop_Paranoid))
{
	new ScriptObject(MMRole_Cop_Paranoid : MMRole_Cop)
	{
		name = "Paranoid Cop";
		corpseName = "panphobic officer of the law";

		letter = "P";

		colour = "<color:CC4444>";
		nameColour = "0.8 0.2666667 0.2666667";

		displayColour = "<color:1122CC>";

		forceInvResult = 1;
	};
}

if(!isObject(MMRole_Cop_Naive))
{
	new ScriptObject(MMRole_Cop_Naive : MMRole_Cop)
	{
		name = "Naive Cop";
		corpseName = "credulous officer of the law";

		letter = "N";

		colour = "<color:58C2FF>";
		nameColour = "0.345 0.761 1";

		displayColour = "<color:1122CC>";

		forceInvResult = 0;
	};
}

if(!isObject(MMRole_Cop_Insane))
{
	new ScriptObject(MMRole_Cop_Insane : MMRole_Cop)
	{
		name = "Insane Cop";
		corpseName = "schizophrenic officer of the law";

		letter = "IC";

		colour = "<color:FF40FF>";
		nameColour = "1 0.376 1";

		displayColour = "<color:1122CC>";

		insane = true;
	};
}

package MM_Cop_Variants
{
	function MM_getInvResult(%cop, %target)
	{
		%r = parent::MM_getInvResult(%cop, %target);

		if(%cop.role.insane)
			return getRandom(1);

		return %r;
	}
};
activatePackage(MM_Cop_Variants);