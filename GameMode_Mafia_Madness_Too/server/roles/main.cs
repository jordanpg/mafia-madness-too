if(!$MM::LoadedRole)
	exec($MM::Server @ "MM_Role.cs");

exec("./innocent.cs");
exec("./mafia.cs");

exec("./abductor.cs");
exec("./crazy.cs");
exec("./fingerprintExpert.cs");