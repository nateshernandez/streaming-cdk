using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3.Assets;
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
        // Lambda Web Adapter Layer ARN - version 2.9.1 (latest as of 2024)
        // Format: arn:aws:lambda:{region}:753240598075:layer:LambdaAdapterLayerX86:25
        var lambdaWebAdapterLayerArn = $"arn:aws:lambda:{this.Region}:753240598075:layer:LambdaAdapterLayerX86:25";
        
        // Create Lambda function from zip package with bundling
        var streamingFunction = new Function(this, "StreamingFunction", new FunctionProps
        {
            Runtime = Runtime.DOTNET_8,
            Handler = "run.sh",  // Shell script that starts the .NET application
            Architecture = Architecture.X86_64,
            Timeout = Duration.Minutes(1),
            MemorySize = 512,
            Code = Code.FromAsset("src/StreamingApi", new Amazon.CDK.AWS.S3.Assets.AssetOptions
            {
                Bundling = new BundlingOptions
                {
                    Image = Runtime.DOTNET_8.BundlingImage,
                    User = "root",
                    OutputType = BundlingOutput.ARCHIVED,
                    Command = new[]
                    {
                        "/bin/sh",
                        "-c",
                        string.Join(" && ", new[]
                        {
                            "cd /asset-input",
                            "export DOTNET_CLI_HOME=\"/tmp/DOTNET_CLI_HOME\"",
                            "export PATH=\"$PATH:/tmp/DOTNET_CLI_HOME/.dotnet/tools\"",
                            "dotnet tool install -g Amazon.Lambda.Tools",
                            "dotnet publish -c Release -r linux-x64 --self-contained",
                            "cd bin/Release/net8.0/linux-x64/publish",
                            "echo '#!/bin/bash' > run.sh",
                            "echo 'exec ./StreamingApi' >> run.sh",
                            "chmod +x run.sh",
                            "chmod +x StreamingApi",
                            "zip -r /asset-output/function.zip ."
                        })
                    }
                }
            }),
            Layers = new[] { LayerVersion.FromLayerVersionArn(this, "LambdaWebAdapter", lambdaWebAdapterLayerArn) },
            Environment = new Dictionary<string, string>
            {
                { "AWS_LAMBDA_EXEC_WRAPPER", "/opt/bootstrap" },
                { "PORT", "8080" },
                { "ASPNETCORE_URLS", "http://localhost:8080" },
                { "AWS_LWA_INVOKE_MODE", "response_stream" },
                { "RUST_LOG", "info" }
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
