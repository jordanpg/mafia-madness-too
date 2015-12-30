$MM::LoadedServer_Expansion = true;

function MM_LoadExpansion(%path)
{
	echo("Loading MMT Expansion" SPC %path @ "...");

	//Add-Ons

	if(isValidAddOn(fileBase(%path)) != 1)
		warn("   +--Invalid add-on, only loading gamemodes...");
	else if(isFile(%cs = (%path @ "/server.cs")))
		exec(%cs);

	echo("   +--Loading gamemodes...");

	%amt = MM_RegisterAllModeFiles(%path @ "/*.mmgm");

	echo("   +--Found" SPC %amt SPC "custom modes");
}

function MM_LoadExpansions()
{
	if(isFile($MM::ExpansionConfig))
		exec($MM::ExpansionConfig);

	if($MM::ExpansionsCt == 0)
		MM_FindExpansions();

	echo("Loading active MMT Expansions...");

	for(%i = 0; %i < $MM::ExpansionsCt; %i++)
	{
		if($MMExpansion_[fixFileName(fileBase($MM::Expansions[%i]))] != 1)
			continue;

		MM_LoadExpansion($MM::Expansions[%i]);
	}
}