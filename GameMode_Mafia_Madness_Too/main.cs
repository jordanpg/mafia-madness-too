$MM::Root = "Add-Ons/GameMode_Mafia_Madness_Too/";
$MM::Server = $MM::Root @ "server/";
$MM::Client = $MM::Root @ "client/";
$MM::Common = $MM::Root @ "common/";

$MM::Roles = $MM::Server @ "roles/";

function MM_LoadCommon()
{
	exec("./common/main.cs");
}

function MM_LoadClient()
{
	MM_LoadCommon();
	exec("./client/main.cs");
}

function MM_LoadServer()
{
	MM_LoadCommon();
	exec($MM::Server @ "main.cs");
}

function MMDebug(%msg, %o0, %o1, %o2)
{
	%debug = $MMDebug;

	for(%i = 0; %i < 3; %i++)
	{
		if(isObject(%o[%i]))
		{
			if(%o[%i].MMDebug < 0 || ((%o[%i].class $= "MMRole" || %o[%i].superClass $= "MMRole") && !$MMDebugRoles))
				return;

			%debug |= %o[%i].MMDebug;
		}
	}

	switch(%debug)
	{
		case 1:
			echo(%msg);
		case 2:
			talk(%msg);
		case 3:
			echo(%msg);
			talk(%msg);
	}
}