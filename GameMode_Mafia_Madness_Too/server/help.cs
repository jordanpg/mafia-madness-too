//help.cs
//Mafia Madness revised rules/help

$MM::LoadedHelp = true;

function serverCmdWhatIs(%this, %letter, %format)
{
	%letter = strUpr(%letter);

	%role = $MM::RoleKey[%letter];

	if(!isObject(%role))
	{
		messageClient(%this, '', "\c4Could not find a role with '\c3" @ %letter @ "\c4'");
		return;
	}

	%a = %role.getAlignment();

	messageClient(%this, '', "\c3" @ %letter @ (%format ? "\c6:" : " \c4is the") SPC %role.getColour(1) @ %role.getRoleName() SPC $MM::AlignmentColour[%a] @ "(" @ $MM::Alignment[%a] @ ")");
}

function serverCmdDescribe(%this, %letter)
{
	%letter = strUpr(%letter);

	%role = $MM::RoleKey[%letter];

	if(!isObject(%role))
	{
		messageClient(%this, '', "\c4Could not find a role with '\c3" @ %letter @ "\c4'");
		return;
	}

	%a = %role.getAlignment();

	messageClient(%this, '', "\c3" @ %letter SPC "\c4is the" SPC %role.getColour(1) @ %role.getRoleName() SPC $MM::AlignmentColour[%a] @ "(" @ $MM::Alignment[%a] @ ")");

	%this.messageLines(%role.description);
}

function serverCmdListRoles(%this)
{
	if(!isObject(MMRoles))
		return;

	%ct = MMRoles.getCount();
	for(%i = 0; %i < %ct; %i++)
		serverCmdWhatIs(%this, MMRoles.getObject(%i).getLetter(), 1);

	messageClient(%this, '', "\c4Use \c3/describe \c6[initial] \c4to get a description of a role. Use Page Up and Page Down to scroll through the list.");
}

//uuuuugh i'll come back to this later i realised how massive this is going to end up being
function serverCmdNewRules(%this, %cat, %subcat)
{
	switch$(%cat)
	{
		case "roles" or "special" or 3:
			if(%subcat $= "roles" || %subcat $= "")
			{

			}
	}
}