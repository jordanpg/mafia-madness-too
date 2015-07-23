//guiHook.cs
//Server-side GUI functionality

$MM::AllowGui = false;
$MM::ServerGUIVersion = "0";

$MM::HudMode::HUD = 1;
$MM::HudMode::BottomPrint = 2;

function serverCmdMMHandshake(%this, %version)
{
	%this.MMGuiVersion = %version;
	%this.MMHasGui = true;

	%this.MM_setDefaultGUISettings();
}

function GameConnection::MM_setDefaultGUISettings(%this)
{
	%this.guiPref["HUDMode"] = 1;
	%this.guiPref["BottomPrintLayout"] = 2;
}

package MM_GuiHook
{
	function GameConnection::MM_UpdateUI(%this)
	{
		if(!$MM::AllowGui || !%this.MMHasGui)
			return parent::MM_UpdateUI(%this);

		%mini = getMinigameFromObject(%this);
		%role = %this.role;
		if(!isObject(%mini) || !%mini.isMM || !%mini.running || !isObject(%role) || %this.isGhost || %this.lives < 1)
		{
			commandToClient(%this, 'MMSetHUD', false);
			bottomPrint(%this, "", 0);
			return;
		}

		%pref = %this.guiPref["HUDMode"];

		%roleList = %mini.MM_GetRolesList();

		if(%pref & $MM::HudMode::HUD)
		{
			commandToClient(%this, 'MMSetDisplayName', (%this.knowsFullRole ? %role.getRoleName() : %role.getDisplayName()));
			commandToClient(%this, 'MMSetDisplayNameColour', %role.getColour(%this.knowsFullRole));
			commandToClient(%this, 'MMSetRolesList', %roleList);

			commandToClient(%this, 'MMSetHUD', true);
		}
		else
			commandToClient(%this, 'MMSetHud', false);

		if(%pref & $MM::HudMode::BottomPrint)
		{
			%roleStr = %role.getColour(%this.knowsFullRole) @ (%this.knowsFullRole ? %role.getRoleName() : %role.getDisplayName());

			switch(%this.guiPref["BottomPrintLayout"])
			{
				case 1:
					%this.bottomPrint("\c5You are:" SPC %roleStr SPC "  \c5ROLES\c6:" SPC %roleList);
				default:
					%this.bottomPrint("\c5You are:" SPC %roleStr SPC "<just:right>\c5ROLES\c6:" SPC %roleList @ " ");
			}
		}
		else
			bottomPrint(%this, "", 0);
	}
};
activatePackage(MM_GuiHook);