//duckUtils Help File System
//Author: ottosparks
//ripped from an unfinished server utility project
$DuckUtils::Help::Version = 0;

function duckUtils_LoadHelp(%filen, %noreset)
{
	if($duckUtils::HelpDebug)
	{
		echo("duckUtils_LoadHelp : Loading help file...");
		echo("   +--file:" SPC %filen);
	}

	if(!isFile(%filen))
	{
		warn("duckUtils_LoadHelp : Could not find file at \'" SPC %filen SPC "\'");
		return false;
	}

	%file = new FileObject();
	%file.openForRead(%filen);
	%version = %file.readLine();
	if(firstWord(%version) !$= "helpv")
	{
		%file.close();
		%file.delete();
		warn("duckUtils_LoadHelp : File at \'" SPC %filen SPC "\' is not a valid help file");
		return false;
	}
	%info = restWords(%version);
	%vnum = firstWord(%info);
	%hname = restWords(%info);

	if($duckUtils::HelpDebug)
	{
		echo("   +-version:" SPC %vnum);
		echo("   +-name:" SPC %hname);
	}

	if($duckUtils::HelpDebug)
		echo("\nBeginning file read...");

	if(!%noreset)
	{
		if($duckUtils::HelpDebug)
			echo("Clearing current content...");

		deleteVariables("$duckUtils::Help::Content" @ %hname @ "*");
	}

	while(!%file.isEOF())
	{
		%line = %file.readLine();
		for(%i = 0; %i < 10; %i++)
			%line = collapseEscape(%line);

		if(%line $= "")
			continue;

		%first = firstWord(%line);
		if(strCmp(%first, "CONTENT") == 0)
		{
			if(%currContent !$= "")
			{
				if($duckUtils::HelpDebug)
					echo("Cannot nest content! Consider a more clever setup.");
				continue;
			}

			%name = restWords(%line);

			%currContent = %name;
			%currBody = "";

			if($duckUtils::HelpDebug)
				echo("+NEW CONTENT :" SPC %name);

			continue;
		}
		else if(strCmp(%first, "ALIAS") == 0)
		{

			%name = restWords(%line);

			if(%currContent $= "")
			{
				if($duckUtils::HelpDebug)
					echo("Cannot add alias \'" SPC %name SPC "\' because there is no current content.");
				continue;
			}

			%currAlias = trim(%currAlias TAB %name);

			if($duckUtils::HelpDebug)
				echo("+NEW ALIAS FOR" SPC %currContent SPC ":" SPC %name);

			continue;
		}
		else if(strCmp(%first, "END") == 0)
		{
			%wat = restWords(%line);
			if(%wat $= "" || %wat $= "CONTENT")
			{
				$duckUtils::Help::Content[%hname, %currContent] = %currBody;

				%ct = getFieldCount(%currAlias);
				for(%i = 0; %i < %ct; %i++)
				{
					%al = getField(%currAlias, %i);
					$duckUtils::Help::Content[%hname, %al] = %currBody;
					$duckUtils::Help::AliasPointsTo[%hname, %al] = %currContent;
				}

				if($duckUtils::HelpDebug)
					echo("-END CONTENT:" SPC %currContent);

				%currContent = "";
				%currBody = "";
				%currAlias = "";
			}

			continue;
		}
		else
		{
			if(%currContent $= "")
				continue;

			%currBody = ltrim(%currBody NL ltrim(%line));

			if($duckUtils::HelpDebug)
				echo(ltrim(%line));
		}
	}

	if(%currContent !$= "")
	{
		$duckUtils::Help::Content[%hname, %currContent] = %currBody;

		%ct = getFieldCount(%currAlias);
		for(%i = 0; %i < %ct; %i++)
			$duckUtils::Help::Content[%hname, getField(%currAlias, %i)] = %currBody;

		if($duckUtils::HelpDebug)
			echo("-?EOF CONTENT:" SPC %currContent);

		%currContent = "";
		%currBody = "";
		%currAlias = "";
	}

	%file.close();
	%file.delete();

	if($duckUtils::HelpDebug)
		echo("Finished.");

	warn("duckUtils : Loaded help file \'" SPC %hname SPC "\'");

	return true;
}

function duckUtils_GetHelpContent(%topic, %content)
{
	return $duckUtils::Help::Content[%topic, %content];
}

function duckUtils_HelpSubtopic(%str)
{
	return strReplace(%str, " ", ".");
}