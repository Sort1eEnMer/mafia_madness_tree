exec("./dependency.cs");
exec("./Support_DuckHelp.cs");
// exec("./support/main.cs");

// exec("./data/MMgun.cs");
// exec("./data/Silent Combat Knife.cs");
// exec("./data/Snubnose.cs");
// exec("./data/Magnum Research.cs");
// exec("./data/Colt Python.cs");
// exec("./data/Tanaka Works.cs");
// exec("./data/misc.cs");

// exec("./bricks.cs");

exec("./data/main.cs");

exec("./MM_Core.cs");
exec("./MM_Role.cs");
exec($MM::Roles @ "main.cs");
exec("./MM_GameModes.cs");

schedule(0, 0, MM_InitGameModes);

// exec($MM::GMDir @ "main.cs"); //executed in MM_InitGameModes

exec("./afterlife.cs");
exec("./ahole.cs");
exec("./chat.cs");
exec("./cmd.cs");
exec("./admin.cs");
exec("./help.cs");
exec("./corpses.cs");
exec("./voting.cs");

exec("./guiHook.cs");

exec("./expansion.cs");

schedule(0, 0, MM_LoadExpansions);