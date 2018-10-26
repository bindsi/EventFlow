using System.IO;
using System.Linq;
using Microsoft.Diagnostics.EventFlow;
using Microsoft.Diagnostics.EventFlow.Inputs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventFlow
{
	class Program
	{
		private const string WorkspaceId = "<Add your OMS workspaceId here>";
		private const string WorkspaceKey = "<Add your OMS workspaceKey here>";

		static void Main(string[] args)
		{
			using (var pipeline = CreatePipelineWithStaticOverrides(WorkspaceKey, WorkspaceId))
			{
				var factory = new LoggerFactory()
					.AddEventFlow(pipeline);

				var logger = new Logger<Program>(factory);
				using (logger.BeginScope("myState"))
				{
					logger.LogDebug("Hello from {friend} for {family}!", "LoggerInput", "EventFlow");
				}

				System.Diagnostics.Trace.TraceWarning("Warning - EventFlow is working!");
				System.Diagnostics.Trace.TraceError("Error - EventFlow is working!");
			}
		}

		/// <summary>
		/// Overwrites OMS configuration for local development with OMS Workspace Id/Key from Dev01
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		private static DiagnosticPipeline CreatePipelineWithStaticOverrides(string workspaceKey, string workspaceId)
		{
			string configFilePath = Path.Combine(".\\eventFlowConfig.json");

			if (!File.Exists(configFilePath))
				throw new FileNotFoundException("Cannot find eventFlowConfig.json", configFilePath);

			IConfiguration config = new ConfigurationBuilder().AddJsonFile(configFilePath).Build();

			IConfiguration omsOutput = config.GetSection("outputs").GetChildren().FirstOrDefault(c => c["type"] == "OmsOutput");
			if (omsOutput != null)
			{
				omsOutput["workspaceKey"] = workspaceKey;
				omsOutput["workspaceId"] = workspaceId;
			}

			return DiagnosticPipelineFactory.CreatePipeline(config);
		}
	}
}