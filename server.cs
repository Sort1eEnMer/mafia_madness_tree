exec("./main.cs");

MM_LoadServer();

exec("Add-Ons/Server_SMMSounds/server.cs");

function serverCmdQExec(%client)
{
	if(%client.isSuperAdmin)
	{
		MM_LoadServer();
		talk("Gamemode executed.");
	}
}