if(!$MM::LoadedRole)
	exec($MM::Server @ "MM_Role.cs");

exec("./innocent.cs");
exec("./mafia.cs");

exec("./abductor.cs");
exec("./cop.cs");
exec("./cop_variants.cs");
exec("./crazy.cs");
exec("./devil.cs");
exec("./fingerprintExpert.cs");
exec("./godfather.cs");
exec("./miller.cs");
exec("./theLaw.cs");
exec("./ventriloquist.cs");