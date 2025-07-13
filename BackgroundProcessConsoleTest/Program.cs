// See https://aka.ms/new-console-template for more information
using System.Text.RegularExpressions;
using BackgroundProcessConsoleTest;

var bgIntervalTestList = new List<BgIntervalTest>();
for (int i = 0; i < 3; i++)
{
    var bgIntervalTest = new BgIntervalTest("INSTANCE_" + i.ToString());
    bgIntervalTestList.Add(bgIntervalTest);
}

while (true)
{
    var command = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(command))
    {
        continue;
    }

    if (command == "exit")
    {
        foreach (var bgIntervalTest in bgIntervalTestList)
        {
            bgIntervalTest.Dispose();
        }
        break;
    }
    else if (command.StartsWith("start"))
    {
        var regex = new Regex(@"\(\d+\)");
        if (regex.IsMatch(command))
        {
            var match = regex.Match(command);
            if (match.Success && int.TryParse(match.Value.Trim('(', ')'), out int instanceIndex) && instanceIndex < bgIntervalTestList.Count)
            {
                var bgIntervalTest = bgIntervalTestList[instanceIndex];
                bgIntervalTest.StartActivity();
            }
            else
            {
                Console.WriteLine("Invalid instance index. Use 'start(instanceIndex)' where instanceIndex is 0, 1, or 2.");
            }
        }
        else
        {
            Console.WriteLine("Invalid command format. Use 'start(instanceIndex)' to start a specific instance.");
        }
    }
    else if (command.StartsWith("stop"))
    {
        var regex = new Regex(@"\(\d+\)");
        if (regex.IsMatch(command))
        {
            var match = regex.Match(command);
            if (match.Success && int.TryParse(match.Value.Trim('(', ')'), out int instanceIndex) && instanceIndex < bgIntervalTestList.Count)
            {
                var bgIntervalTest = bgIntervalTestList[instanceIndex];
                bgIntervalTest.StopActivity();
            }
            else
            {
                Console.WriteLine("Invalid instance index. Use 'stop(instanceIndex)' where instanceIndex is 0, 1, or 2.");
            }
        }
        else
        {
            Console.WriteLine("Invalid command format. Use 'stop(instanceIndex)' to stop a specific instance.");
        }
    }
    else
    {
        Console.WriteLine("Unknown command. Use 'start', 'stop', or 'exit'.");
    }
}

Console.WriteLine("Exit the application by pressing any key...");
Console.ReadLine();
