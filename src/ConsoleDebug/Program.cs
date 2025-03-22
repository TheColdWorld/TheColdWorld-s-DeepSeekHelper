using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheColdWorld.DeepSeekHelper;
using TheColdWorld.DeepSeekHelper.codec;

namespace ConsoleDebug
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /* lang res test
             ResourceManager resourceManager = new("TheColdWorld.DeepSeekHelper.Properties.Resources",typeof(DeepSeekHelper).Assembly);
            Console.WriteLine(resourceManager.GetString("AuthenticationFails"));
            Console.WriteLine(resourceManager.GetString("InsufficientBalance"));
            Console.WriteLine(resourceManager.GetString("RateLimitReached"));
            Console.WriteLine(resourceManager.GetString("ServerError"));
            Console.WriteLine(resourceManager.GetString("ServerOverloaded"));
            Console.ReadKey(true);
            return;
             */
            DeepSeekHelper deepSeekHelper = new();
            
            Dialogue dialogue = new(Encoding.ASCII.GetBytes(""),()=>RandomNumberGenerator.GetBytes(32), TheColdWorld.DeepSeekHelper.arguments.request.Model.deepseek_reasoner)
            {
                Temperature = 2,
                Max_Token=1024
            };
            dialogue.AddSystemContent("你是一个很会讲笑话的助手，你要在尽量满足用户的要求下尽量加入笑话");
            Console.OutputEncoding = Encoding.UTF8;
            StreamWriter writer = new(Console.OpenStandardOutput())
            {
                AutoFlush = true
            };
            Console.SetOut(writer);
            /*
            var r = deepSeekHelper.Complete("帮我睿评一下进行DDOS攻击的人",dialogue,false);
            Console.WriteLine(r);
            
             if (r.Choices[0].Message.reasoning_content is null)
                Console.WriteLine($"[Assistant]{r.Choices[0].Message.Content}");
            else {
                Console.WriteLine($"[Assistant Reasoning]{r.Choices[0].Message.reasoning_content}");
                Console.WriteLine($"[Assistant]{r.Choices[0].Message.Content}"); }
             */
            deepSeekHelper.Complete("帮我睿评一下进行DDOS攻击的人", dialogue, true, (e) =>  Console.WriteLine($"Unhandeled Exception:{e.GetType().Name}\n{e.Message}{e.StackTrace}"),
                () => Console.WriteLine("[Assistant Reasoning]"),
                Console.Write,
                ()=>Console.WriteLine("\n[Assistant]"),
                Console.Write,
                Console.WriteLine,
                (t)=> Console.WriteLine(t.ToString()));
            Console.ReadKey(true);
        }
    }
}