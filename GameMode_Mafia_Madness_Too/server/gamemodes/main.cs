//Under normal conditions, none of these should be loaded.

if(!$MM::LoadedGameModes)
	exec($MM::Server @ "MM_GameModes.cs");

if($MM::LoadOldGameModes)
	exec("./oldGameModes.cs");

if(!$MM::AllowGameModeExec)
	exec("./mmtoo.cs");