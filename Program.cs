using Jyuno;
using Jyuno.Language;
using System.Text;

class Program
{
    public static void Main(string[] args)
    {
        string? filename = null;
        Runtime.AddJyunoCommandType option = Runtime.AddJyunoCommandType.Default;
        foreach(var arg in args)
        {
            if (File.Exists(arg))
            {
                filename = arg;
                break;
            }
            else if (arg[0] is '-')
            {
                switch (args[0].Substring(1).ToLower())
                {
                    case "console":
                    case "default":
                    case "math":
                        break;
                    case "file":
                        option |= Runtime.AddJyunoCommandType.File;
                        break;
                    default:
                        Console.WriteLine("'{0}' is unknown option." , arg);
                        return;
                }
            } else
            {
                Console.WriteLine("'{0' is not exist file path." , arg);
                return;
            }
        }

        if (filename is not null)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("'{0' is not exist file path." , filename);
                return;
            }
            Runtime runtime = new(option);
            runtime.Create(File.ReadAllLines(filename)).Run();
            return;
        }

        Console.Title = "Jyuno Prompt";
        Console.WriteLine("Jyuno Prompt\n[Notice] Jyuno keywords do not work in prompt.");
        JyunoPrompt prompt = new();
        prompt.Run();
    }
}

class JyunoPrompt
{
    public JyunoPrompt()
    {
        this.Runtime = new(Runtime.AddJyunoCommandType.All);
        this.Interpreter = Runtime.Create();

        Runtime.AddFunction("exit" , _ => {
            Running = false;
            return null;
        });
        Runtime.AddFunction("copyright" , _ => {
            return "All rights reserved jyunni.";
        });
        Runtime.AddFunction("help" , _ => {
            return HelpContent;
        });
        Runtime.AddVariable(".input" , () => CommandLineText , v => CommandLineText = v as string);
        Runtime.AddVariable(".return" , () => ReturnText , v => ReturnText = v as string);
        Runtime.AddVariable(".error" , () => ErrorText , v => ErrorText = v as string);
        Runtime.AddVariable(".remove" , () => Interpreter.EnableRemoveVariable , args => {
            Interpreter.EnableRemoveVariable = (bool)args;
        });
        Runtime.AddVariable(".substitute" , () => !Interpreter.BlockSubstitute , args => {
            Interpreter.BlockSubstitute = !(bool)args;
        });
        Runtime.AddFunction("execute" , args => {
            using(Interpreter newint = new(Runtime,File.ReadAllLines(args.Length is 0 ? "jyuno" : (string)(args[0] ?? "jyuno"))))
            {
                return newint.Run();
            }
        });
        Runtime.AddFunction("history" , args => {
            StringBuilder sb = new();
            foreach(var s in Interpreter.scripts)
            {
                sb.AppendLine(s.script);
            }
            return sb.ToString();
        });
        Runtime.AddFunction("eval" , args => {
            return this.Interpreter.ExecuteLine(args[0] as string);
        });
    }

    public Runtime Runtime { get; init; }
    public Interpreter Interpreter { get; init; }
    public bool Running { get; private set; } = true;
    public string CommandLineText = ">>>";
    public string ReturnText = "\nReturn: {0}\n";
    public string ErrorText = "\nError: {0}\n";
    public string ExceptionText = "\nC# Exception: {0}\n";

    public void Run()
    {

        while (Running)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(CommandLineText);
            Console.ForegroundColor = ConsoleColor.White;
            string? input = Console.ReadLine();
            if (input is null)
                continue;
            Console.ForegroundColor = ConsoleColor.Gray;
            try
            {
                var ret = Interpreter.ExecuteLine(input) ?? "null";
                if (ret is IEnumerable<string> arr)
                    ret = string.Join('\n' , ret);
                Console.WriteLine(ReturnText , ret);
            }
            catch (JyunoException ex)
            {
                Console.WriteLine(ErrorText , ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ExceptionText , ex.Message);
            }
        }
    }
    public void Stop()
    {
        Running = false;
    }

    public const string HelpContent = @"[Jyuno Helper]
Data types: int, double, string, bool

Console:
- console.write (string) (object...)
- (string) : console.readline
- console.clear

Convert:
- (int) : int (string)
- (double) : double (string)
- (string) : string (any)

Math:
- (double) : math.abs (int|double)
- (double) : math.sin (double)
- (double) : math.cos (double)
- (double) : math.tan (double)
- (double) : math.pow (double) (double)
- (double) : math.log (double)
- (double) : math.log2 (double)
- (double) : math.pi

File:
- (bool) : file.exist (string)
- (bool) : directory.exist (string)
- (string) : file.read (string)
- file.write (string) (string)
- (string) : directory.now
- directory.move (string) (string)

Github: https://github.com/jyunrcaea/Jyuno";
}