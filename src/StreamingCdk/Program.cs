using Amazon.CDK;

namespace StreamingCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            
            new StreamingCdkStack(app, "StreamingCdkStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = "975050288287",
                    Region = "us-east-2"
                }
            });

            app.Synth();
        }
    }
}
