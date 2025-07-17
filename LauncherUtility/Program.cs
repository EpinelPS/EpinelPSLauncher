using System.Diagnostics;
using System.Text;
using LauncherUtility;

// check if required dll is present
if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sail_api_impl64.dll")))
    Environment.Exit(-3);

if (args.Length != 3)
{
    Console.WriteLine("Incorrect arguments. Usage: LauncherUtility <Game EXE> <Resource Directory> <Login session data in JSON base64>");
    Environment.Exit(-4);
}

Game game = new();
try
{
    // Usage: LauncherUtility <Game EXE> <Resource Directory> <Login session data in JSON base64>
    game.ExePath = args[0];
    game.ResourcePath = args[1];
    game.LoginData = Encoding.ASCII.GetString(Convert.FromBase64String(args[2]));

    Process? process = game.Launch();

    // terminate this process when game exists
    if (process != null)
    {
        process.WaitForExit();
        Environment.Exit(0);
    }
    else
    {
        Environment.Exit(-1);
    }
}
catch(Exception ex)
{
    Console.WriteLine(ex);
    Environment.Exit(-2);
}
