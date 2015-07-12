if(!$MM::LoadedRole)
	exec($MM::Server @ "MM_Role.cs");

exec("./innocent.cs");
exec("./mafia.cs");