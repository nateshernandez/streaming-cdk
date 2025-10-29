using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Constructs;
using System.Collections.Generic;

namespace StreamingCdk;

public class StreamingCdkStage : Stage
{
    internal StreamingCdkStage(Construct scope, string id, StageProps props) : base(scope, id, props)
    {
        var stack = new StreamingCdkStack(this, id);
    }
}

public class StreamingCdkStack : Stack
{
    internal StreamingCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        // Create Lambda function from Docker image
        var streamingFunction = new DockerImageFunction(this, "StreamingFunction", new DockerImageFunctionProps
        {
            Code = DockerImageCode.FromImageAsset("."),
            Architecture = Architecture.X86_64,
            Timeout = Duration.Minutes(1),
            MemorySize = 512,
            Environment = new Dictionary<string, string>
            {
                { "AWS_LA_PORT", "8080"},
                { "AWS_LWA_INVOKE_MODE", "response_stream" }
            }
        });

        // Add Function URL with streaming enabled
        var functionUrl = streamingFunction.AddFunctionUrl(new FunctionUrlOptions
        {
            AuthType = FunctionUrlAuthType.NONE,
            InvokeMode = InvokeMode.RESPONSE_STREAM
        });

        // Output the Function URL
        new CfnOutput(this, "FunctionUrl", new CfnOutputProps
        {
            Value = functionUrl.Url,
            Description = "Lambda Function URL for streaming API"
        });
    }
}
