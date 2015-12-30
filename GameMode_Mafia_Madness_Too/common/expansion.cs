$MM::LoadedCommon_Expansion = true;

$MM::ExpansionPattern = "Add-Ons/MMT_*/description.txt";
$MM::ExpansionConfig = "config/server/MMTExpansions.cs";

function MM_FindExpansions()
{
	echo("Finding Mafia Madness (too) Expansions...");

	deleteVariables("$MM::Expansions*");
	$MM::ExpansionsCt = 0;

	for(%i = findFirstFile($MM::ExpansionPattern); isFile(%i); %i = findNextFile($MM::ExpansionPattern))
	{
		%path = strReplace(%i, "/description.txt", "");
		%name = fileBase(%path);

		MM_AddExpansion(%path);
		echo("   +--Found expansion" SPC %name);
	}

	echo("Found" SPC $MM::ExpansionsCt SPC "expansions.");

	return $MM::ExpansionsCt;
}

function MM_AddExpansion(%path)
{
	%i = $MM::ExpansionsCt;

	$MM::Expansions[%i] = %path;

	if(isFile(%path @ "/server.cs"))
		$MM::ExpansionsHasCode[%i] = (isValidAddOn(getSubStr(%path, 8, strLen(%path))) ? 1 : -1);
	else
		$MM::ExpansionsHasCode[%i] = 0;

	if(isFile(%desc = (%path @ "/description.txt")))
	{
		%file = new FileObject();
		%file.openForRead(%desc);
		while(!%file.isEOF())
			$MM::ExpansionsDesc[%i] = $MM::ExpansionDesc[%i] NL %file.readLine();
		%file.close();
		%file.delete();
	}

	$MM::ExpansionsCt++;
}

