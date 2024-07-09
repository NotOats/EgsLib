using EgsLib.ConfigFiles;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

namespace EgsLib.Tests.ConfigFiles
{
    public class DialogueFixture : BaseFileFixture<Dialogue>
    {
        public DialogueFixture() : base(@"Resources\Configuration\Dialogues.ecf", Dialogue.ReadFile)
        { }
    }

    public class DialogueTests : IClassFixture<DialogueFixture>
    {
        private DialogueFixture Fixture { get; }

        public DialogueTests(DialogueFixture fixture) => Fixture = fixture;

        [Fact]
        public void HasRequiredFields()
        {
            foreach (var dialogue in Fixture.Objects)
            {
                Assert.NotNull(dialogue.Name);
                Assert.NotEqual(string.Empty, dialogue.Name);
            }
        }

        [Fact]
        public void CheckNextIfSyntax()
        {
            foreach (var dialogue in Fixture.Objects)
            {
                foreach (var next in dialogue.Next)
                {
                    var line = CleanEcfCode(next.Conditional);
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var simulate = $"if ({line}) {{ return true; }} else {{ return false; }}";

                    var tree = CSharpSyntaxTree.ParseText(simulate);
                    var diagnostics = tree.GetDiagnostics();

                    Assert.Empty(diagnostics);
                }
            }
        }

        [Fact]
        public void CheckOptionSyntax()
        {
            foreach (var dialogue in Fixture.Objects)
            {
                // loop through each object
                foreach (var option in dialogue.Options)
                {
                    // Option.Text
                    {
                        var line = CleanEcfCode(option.Text);
                        if (string.IsNullOrEmpty(line))
                            continue;

                        var simulate = "$\"" + line + "\";";
                        var tree = CSharpSyntaxTree.ParseText(simulate);
                        var diagnostics = tree.GetDiagnostics();

                        Assert.Empty(diagnostics);
                    }

                    // Option.Conditional
                    {
                        var line = CleanEcfCode(option.Conditional);
                        if (string.IsNullOrEmpty(line))
                            continue;

                        var simulate = $"if ({line}) {{ return true; }} else {{ return false; }}";
                        var tree = CSharpSyntaxTree.ParseText(simulate);
                        var diagnostics = tree.GetDiagnostics();

                        if (diagnostics.Any())
                            Debugger.Break();

                        Assert.Empty(diagnostics);
                    }

                    // Option.Execute
                    {
                        var line = CleanEcfCode(option.Conditional);
                        if (string.IsNullOrEmpty(line))
                            continue;

                        var simulate = $"{line};";
                        var tree = CSharpSyntaxTree.ParseText(simulate);
                        var diagnostics = tree.GetDiagnostics();

                        Assert.Empty(diagnostics);
                    }
                }
            }
        }

        [Fact]
        public void CheckExecuteSyntax()
        {
            foreach (var execute in Fixture.Objects.SelectMany(x => x.Execute))
            {
                var line = CleanEcfCode(execute);
                if (string.IsNullOrEmpty(line))
                    continue;

                var simulate = $"{line};";
                var tree = CSharpSyntaxTree.ParseText(simulate);
                var diagnostics = tree.GetDiagnostics();

                if (diagnostics.Any())
                    Debugger.Break();

                Assert.Empty(diagnostics);
            }
        }

        private static string? CleanEcfCode(string? text)
        {
            return text?.Trim('"').Replace('\'', '"').Replace("\\\"", "\"").Replace("<nl>", "\r\n");
        }
    }
}
