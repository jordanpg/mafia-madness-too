$MM::LoadedClient_StartGame = true;

function MM_generateExtensionsConfig()
{
	%ct = MMTGameGui_ExtensionsBox.getCount();
	for(%i = 0; %i < %ct; %i++)
	{
		%obj = MMTGameGui_ExtensionsBox.getObject(%i);

		$MMExpansion_[%obj.varName] = ($MMTGameGui::Enabled[%obj.varName] ? 1 : -1);
	}

	export("$MMExpansion_*", $MM::ExpansionConfig);
}

function MMTGameGui::onWake(%this)
{
	if(isFile($MM::ExpansionConfig))
		exec($MM::ExpansionConfig);

	MMTGameGui_ExtensionsBox.deleteAll();

	for(%i = 0; %i < $MM::ExpansionsCt; %i++)
	{
		%ext = $MM::Expansions[%i];
		%n = fileBase(%ext);
		%fn = fixFileName(%n);

		// echo(%n);

		%new = new GuiCheckBoxCtrl()
		{
			profile = "ImpactCheckProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "10 0";
			extent = "449 27";
			minExtent = "449 27";
			enabled = "1";
			visible = "1";
			clipToParent = "1";
			variable = "$MMTGameGui::Enabled" @ %fn;
			//command = "CustomGameGui.clickAddOnItem();";
			text = %n;
			groupNum = "-1";
			buttonType = "ToggleButton";
			varName = %fn;
		};

		%new.resize(10, %i * ImpactCheckProfile.fontSize, 898, ImpactCheckProfile.fontSize);
		%new.setValue($MMExpansion_[%fn] == 1);

		MMTGameGui_ExtensionsBox.add(%new);
	}

	MMTGameGui_ExtensionsBox.resize(0, 0, 918, ImpactCheckProfile.fontSize * %i);
}

function MMTGameGui::clickBack(%this)
{
	MM_generateExtensionsConfig();

	Canvas.popDialog(MMTGameGui);
	Canvas.pushDialog(GameModeGui);
}

function MMTGameGui::clickSelect(%this)
{
	MM_generateExtensionsConfig();

	Canvas.popDialog(MMTGameGui);
	Canvas.pushDialog(ServerSettingsGui);
}

package MMClient_StartGame
{
	function GameModeGui::clickSelect(%this)
	{
		%gm = $MM::Root @ "gamemode.txt";
		if($GameModeGui::GameMode[$GameModeGui::CurrGameModeIdx] !$= %gm)
			return parent::clickSelect(%this);

		Canvas.popDialog(GameModeGui);
		Canvas.pushDialog(MMTGameGui);

		$GameModeArg = %gm;
	}

	function ServerSettingsGui::clickBack(%this)
	{
		%r = parent::clickBack(%this);

		if($GameModeArg !$= ($MM::Root @ "gamemode.txt"))
			return %r;

		Canvas.popDialog(%r);
		Canvas.pushDialog(MMTGameGui);
	}
};
activatePackage(MMClient_StartGame);