//miller.cs
//Code for the Miller inno role.

if(!$MM::LoadedRole_Innocent)
	exec("./innocent.cs");

$MM::LoadedRole_Miller = true;

if(!isObject(MMRole_Miller))
{
	new ScriptObject(MMRole_Miller : MMRole_Innocent)
	{
		name = "Miller";
		corpseName = "hoodie-wearing skittles-carrying african citizen"; //oo brakets u topical....
		displayName = "Innocent";

		letter = "L";

		colour = "<color:667C00>";
		nameColour = "0.4 0.486 0";

		displayColour = "<color:00FF00>";

		description = "\c4The <color:667C00>Miller \c4is a role that believes they are \c2Innocent\c4, but appears to be \c0suspicious \c4when investigated.";

		forceInvAlignment = 1;
	};
}