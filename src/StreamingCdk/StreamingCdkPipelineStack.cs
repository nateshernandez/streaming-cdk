using Amazon.CDK;
using Amazon.CDK.Pipelines;
using Amazon.CDK.AWS.CodeBuild;
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
                Input = CodePipelineSource.Connection("nateshernandez/streaming-cdk", "main", new ConnectionSourceOptions
                {
                    ConnectionArn = "arn:aws:codeconnections:us-east-2:975050288287:connection/0c61784b-964f-4b34-a2f5-4221e21dc11c"
                }),
                Commands = new string[] { "npm install -g aws-cdk", "cdk synth" },
                InstallCommands = new string[]
                {
                    "npm install -g aws-cdk"
                },
                PrimaryOutputDirectory = "cdk.out"
            }),
            CodeBuildDefaults = new CodeBuildOptions
            {
                BuildEnvironment = new BuildEnvironment
                {
                    BuildImage = LinuxBuildImage.STANDARD_7_0,
                    ComputeType = ComputeType.SMALL
                }
            }
        });

        pipeline.AddStage(new StreamingCdkStage(this, nameof(StreamingCdkStage), new StageProps
        {
            Env = props.Env
        }));
    }
}
