using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.IO;

using TestGen;

namespace TestGeneratorConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string DestinationPath = @"D:\Works C#\спп лаба 4\MPPTestGenerator\Samples\GeneratedTests";
            string SourcePath = @"D:\Works C#\спп лаба 4\MPPTestGenerator\Samples\SourceClasses";

            if (!Directory.Exists(DestinationPath))
                Directory.CreateDirectory(DestinationPath);

            await new Pipeline().Generate(DestinationPath, SourcePath);
        }
    }

    public class Pipeline
    {
        private TestGenerator generator = new TestGenerator();

        public Task Generate(string DestinantionPath, string SourcePath)
        {
            var ExecutionOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 3 };

            TransformBlock<string, string> OpenFile = new TransformBlock<string, string>
                (
                    async path => await File.ReadAllTextAsync(path),
                    ExecutionOptions              
                );

            TransformManyBlock<string, TestInfo> GenerateTests = new TransformManyBlock<string, TestInfo>
                (
                    async code => await Task.Run(() => generator.Generate(code)),
                    ExecutionOptions
                ); 
            ActionBlock<TestInfo> WritebackFile = new ActionBlock<TestInfo>
                (
                    async info =>
                    {
                        await File.WriteAllTextAsync(Path.Combine(DestinantionPath, info.FileName), info.TestCode);
                    }, 
                    ExecutionOptions
                );

            var LinkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            OpenFile.LinkTo(GenerateTests, LinkOptions);
            GenerateTests.LinkTo(WritebackFile, LinkOptions);

            foreach (string FileName in Directory.GetFiles(SourcePath)) {
                string FilePath = Path.Combine(SourcePath, FileName);
                if (Path.GetExtension(FilePath) == ".cs")
                    OpenFile.Post(FilePath);
            }

            OpenFile.Complete();
            WritebackFile.Completion.Wait();

            return WritebackFile.Completion;
        }
    }
}
