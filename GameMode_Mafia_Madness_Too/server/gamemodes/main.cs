if(!$MM::LoadedGameModes)
	exec($MM::Server @ "MM_GameModes.cs");

exec("./oldGameModes.cs");

exec("./mmtoo.cs");