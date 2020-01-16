using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static System.Math;

namespace TeleprompterConsole
{
  internal class TeleprompterConfig
  {
    public int DelayInMilliseconds { get; private set; } = 200;

    public void UpdateDelay(int increment) // negative to speed up
    {
      var newDelay = Min(DelayInMilliseconds + increment, 1000);
      newDelay = Max(newDelay, 20);
      DelayInMilliseconds = newDelay;
    }

    public bool Done { get; private set; }

    public void SetDone()
    {
      Done = true;
    }

  }

  class Program
  {
    static void Main(string[] args)
    {
      RunTeleprompter().Wait();
    }

    private static async Task RunTeleprompter()
    {
      var config = new TeleprompterConfig();
      var displayTask = ShowTeleprompter(config);

      var speedTask = GetInput(config);
      await Task.WhenAny(displayTask, speedTask);
    }

    private static async Task ShowTeleprompter(TeleprompterConfig config)
    {
      var words = ReadFrom("sampleQuotes.txt");
      foreach (var word in words)
      {
        Console.Write(word);
        if (!string.IsNullOrWhiteSpace(word))
        {
          await Task.Delay(200);
        }
      }
      config.SetDone();
    }

    public static async Task GetInput(TeleprompterConfig config)
    {
      Action work = () =>
      {
        do
        {
          var key = Console.ReadKey(true);
          if (key.KeyChar == '<')
          {
            config.UpdateDelay(10);
          }
          else if (key.KeyChar == '>')
          {
            config.UpdateDelay(-10);
          }
          else if (key.KeyChar == 'X' || key.KeyChar == 'x')
          {
            config.SetDone();
          }
        } while (!config.Done);
      };
      await Task.Run(work);
    }

    static IEnumerable<string> ReadFrom(string file)
    {
      string line;
      using (var reader = File.OpenText(file))
      {
        while ((line = reader.ReadLine()) != null)
        {
          var words = line.Split(' ');
          int lineLength = 0;
          foreach (var word in words)
          {
            yield return word + " ";
            lineLength += word.Length + 1;
            if (lineLength > 70)
            {
              yield return Environment.NewLine;
              lineLength = 0;
            }
          }
          yield return Environment.NewLine;
        }
      }
    }
  }
}
