// See https://aka.ms/new-console-template for more information
using RobotSimulator;

var testCases = await GetTestCases();

if (testCases == null)
    return;

var result = testCases.ConvertAll(c => c.Run());

Console.WriteLine($"=== Total: {testCases.Count}");
Console.WriteLine($"=== Passed: {result.Count(c => c)}");
Console.WriteLine($"=== Failed: {result.Count(c => !c)}");

async Task<List<TestCase>?> GetTestCases()
{
    try
    {
        if (!File.Exists("commands.txt"))
        {
            Console.WriteLine("Please provide a correct commands.txt file");
        }

        var allText = await File.ReadAllTextAsync("commands.txt");
        return allText
            .Split("===")
            .Select(GetTestCase)
            .Where(c => c != null)
            .Select(c => c!)
            .ToList();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"There are some errors when processing commands.txt file ex: {ex} ");
        return null;
    }
}

TestCase? GetTestCase(string text)
{
    try
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var testCase = text.Trim().Split("=");

        return new TestCase
        {
            InputText = testCase[0],
            CommandInputs = GetCommands(testCase[0]),
            ExpectedText = testCase[1],
            ExpectedOutput = GetOutputs(testCase[1])
        };
    }
    catch (Exception ex)
    {
        Console.WriteLine($"GetTestCase error text: {text}, ex: {ex}");
    }

    return null;
}

List<Robot> GetOutputs(string text)
{
    if (string.IsNullOrWhiteSpace(text))
        return new List<Robot>();

    return text.Trim()
        .Split(Environment.NewLine)
        .Select(GetOutput)
        .Where(c => c != null)
        .Select(c => c!)
        .ToList();
}

Robot? GetOutput(string text)
{
    if (string.IsNullOrWhiteSpace(text))
        return null;

    var parameters = text.Trim().Split(",");
    return new Robot
    {
        X = int.Parse(parameters[0]),
        Y = int.Parse(parameters[1]),
        F = Enum.Parse<EFace>(parameters[2])
    };
}

List<CommandInputBase> GetCommands(string text)
{
    if (string.IsNullOrWhiteSpace(text))
        return new List<CommandInputBase>();

    return text.Trim()
        .Split(Environment.NewLine)
        .Select(GetCommand)
        .Where(c => c != null)
        .Select(c => c!)
        .ToList();
}

CommandInputBase? GetCommand(string text)
{
    if (string.IsNullOrWhiteSpace(text))
        return null;
    var input = text.Trim();
    if (input.StartsWith(nameof(ECommand.PLACE)))
    {
        var parameters = input.Split(" ")[1].Split(",");
        return new PlaceCommandInput
        {
            Params = new Robot
            {
                X = int.Parse(parameters[0]),
                Y = int.Parse(parameters[1]),
                F = Enum.Parse<EFace>(parameters[2])
            },
        };
    }

    if (input.Equals(nameof(ECommand.MOVE)))
        return new MoveCommandInput();

    if (input.Equals(nameof(ECommand.LEFT)))
        return new LeftCommandInput();

    if (input.Equals(nameof(ECommand.RIGHT)))
        return new RightCommandInput();

    if (input.Equals(nameof(ECommand.REPORT)))
        return new ReportCommandInput();

    return null;
}

namespace RobotSimulator
{
    public enum EFace
    {
        NORTH,
        SOUTH,
        EAST,
        WEST
    }

    public enum ECommand
    {
        PLACE,
        MOVE,
        LEFT,
        RIGHT,
        REPORT
    }

    public class Robot
    {
        public int X { get; set; }
        public int Y { get; set; }
        public EFace F { get; set; }
        public bool IsValid => X >= 0 && X <= 4 && Y >= 0 && Y <= 4;

        public Robot Clone() =>
            new()
            {
                X = X,
                Y = Y,
                F = F
            };

        public bool Equals(Robot robot) => X == robot.X && Y == robot.Y && F == robot.F;
    }

    public abstract class CommandInputBase
    {
        public abstract Robot? Execute(Robot? robot);

        public Robot? Output { get; set; }
    }

    public class PlaceCommandInput : CommandInputBase
    {
        public Robot Params { get; set; } = null!;

        public override Robot? Execute(Robot? robot)
        {
            return Params.IsValid ? Params.Clone() : robot;
        }
    }

    public class MoveCommandInput : CommandInputBase
    {
        public override Robot? Execute(Robot? robot)
        {
            if (robot == null)
                return null;

            var newRobot = robot.Clone();
            switch (newRobot.F)
            {
                case EFace.EAST:
                    newRobot.X++;
                    break;
                case EFace.WEST:
                    newRobot.X--;
                    break;
                case EFace.NORTH:
                    newRobot.Y++;
                    break;
                case EFace.SOUTH:
                    newRobot.Y--;
                    break;
            }

            return newRobot.IsValid ? newRobot : robot;
        }
    }

    public class LeftCommandInput : CommandInputBase
    {
        public override Robot? Execute(Robot? robot)
        {
            if (robot == null)
                return null;

            var newRobot = robot.Clone();
            switch (newRobot.F)
            {
                case EFace.EAST:
                    newRobot.F = EFace.NORTH;
                    break;
                case EFace.WEST:
                    newRobot.F = EFace.SOUTH;
                    break;
                case EFace.NORTH:
                    newRobot.F = EFace.WEST;
                    break;
                case EFace.SOUTH:
                    newRobot.F = EFace.EAST;
                    break;
            }

            return newRobot.IsValid ? newRobot : robot;
        }
    }

    public class RightCommandInput : CommandInputBase
    {
        public override Robot? Execute(Robot? robot)
        {
            if (robot == null)
                return null;

            var newRobot = robot.Clone();
            switch (newRobot.F)
            {
                case EFace.EAST:
                    newRobot.F = EFace.SOUTH;
                    break;
                case EFace.WEST:
                    newRobot.F = EFace.NORTH;
                    break;
                case EFace.NORTH:
                    newRobot.F = EFace.EAST;
                    break;
                case EFace.SOUTH:
                    newRobot.F = EFace.WEST;
                    break;
            }

            return newRobot.IsValid ? newRobot : robot;
        }
    }

    public class ReportCommandInput : CommandInputBase
    {
        public override Robot? Execute(Robot? robot)
        {
            if (robot == null)
                return null;

            Output = robot.Clone();

            return robot;
        }
    }

    public class TestCase
    {
        public string InputText { get; set; } = null!;
        public List<CommandInputBase> CommandInputs { get; set; } = null!;
        public List<Robot> ExpectedOutput { get; set; } = null!;
        public string ExpectedText { get; set; } = null!;

        public bool Run()
        {
            Console.WriteLine("Run Test:");
            Console.WriteLine("===");
            Console.WriteLine($"{InputText}");
            if (!CommandInputs.Any())
                return true;

            Robot? robot = null;
            foreach (var commandInput in CommandInputs)
            {
                robot = commandInput.Execute(robot);
            }

            var actualOutputs = CommandInputs
                .Where(c => c.Output != null)
                .Select(c => c.Output!)
                .ToList();

            var isPassed = CheckResult(actualOutputs);
            if (isPassed)
            {
                Console.WriteLine("\t => Passed");
                return true;
            }

            Console.WriteLine("\t => Failed");
            Console.WriteLine($"\tExpected: {ExpectedText}", ExpectedText);

            var actualText = string.Join(
                Environment.NewLine,
                actualOutputs.Select(c => $"{c.X},{c.Y},{c.F}")
            );
            Console.WriteLine("\tActual: ");
            Console.WriteLine($"{actualText}");

            return false;
        }

        private bool CheckResult(List<Robot> actualOutputs)
        {
            if (actualOutputs.Count != ExpectedOutput.Count)
                return false;

            for (int i = 0; i < actualOutputs.Count; i++)
            {
                if (!actualOutputs[i].Equals(ExpectedOutput[i]))
                    return false;
            }

            return true;
        }
    }
}
