using Amazon.CDK;
using Amazon.CDK.Pipelines;
using Constructs;

namespace StreamingCdk;

public class StreamingCdkPipelineStack : Stack
{
    internal StreamingCdkPipelineStack(
        Construct scope,
        string id,
        IStackProps props = null) : base(scope, id, props)
    {
        var pipelineId = "streaming-pipeline";

        var pipeline = new CodePipeline(this, pipelineId, new CodePipelineProps
        {
            PipelineName = pipelineId,
            Synth = new ShellStep("Synth", new ShellStepProps
            {
                Input = CodePipelineSource.GitHub("OWNER/REPO", "main"),
                Commands = new string[] { "npm install -g aws-cdk", "cdk synth" }
            })
        });
    }
}
