using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using SmartX.Core; // Ensures access to your TelemetryPacket<T>

// Corrected Part 2 namespaces:
using SmartX.Core.Processing;
using SmartX.Core.Management;
using SmartX.Core.Intelligence;

namespace SmartX_Ecosystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();
            builder.Services.AddOpenApi();

            // Register Part 2 Data Structures as Singletons to persist memory during execution
            builder.Services.AddSingleton<TelemetryProcessor>();
            builder.Services.AddSingleton<DeviceStateManager>();
            builder.Services.AddSingleton<RecommendationEngine>();

            var app = builder.Build();

            // Note: The hardcoded mock data seeding was removed here so the user can dynamically add devices via the ingestion form[cite: 22].

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // ==========================================
            // PART 1: Smart-X API Integration Layer
            // ==========================================
            app.MapPost("/api/telemetry/ingest", (TelemetryPacket<string> packet, DeviceStateManager stateManager) =>
            {
                // Fulfills Technical Requirement: Advanced Arrays and Lists
                // Utilizes jagged arrays to manage raw telemetry before transferring to optimized Collections
                string[][] rawBatch = new string[1][];
                rawBatch[0] = new string[] { packet.MacAddress, packet.Location, packet.Payload };

                List<string> optimizedCollection = rawBatch[0].ToList();

                // Part 2: Dynamically add the user's device to the Dictionary
                var newDevice = new SmartX.Core.Management.DeviceProfile
                {
                    MacAddress = packet.MacAddress,
                    Category = packet.Payload
                };
                stateManager.RegisterDevice(newDevice);

                // Returns a success response to the Windows Forms dashboard
                return Results.Ok(new
                {
                    Message = "Payload ingested and device registered successfully.",
                    Data = optimizedCollection
                });
            })
            .WithName("IngestTelemetry");


            // ==========================================
            // PART 2: Real-Time Processing & Management
            // ==========================================

            // --- 1. Telemetry Queues ---
            app.MapPost("/api/telemetry/standard", (TelemetryPacket<string> packet, TelemetryProcessor processor) =>
            {
                processor.EnqueueStandardTelemetry(new TelemetryPacket<string> { MacAddress = packet.MacAddress, Payload = packet.Payload });
                return Results.Ok(new { Status = "Standard Telemetry Queued" });
            });

            app.MapPost("/api/telemetry/critical", (TelemetryPacket<string> packet, int severity, TelemetryProcessor processor) =>
            {
                processor.EnqueueCriticalAlert(new TelemetryPacket<string> { MacAddress = packet.MacAddress, Payload = packet.Payload }, severity);
                return Results.Ok(new { Status = "Critical Alert Queued for Immediate Processing" });
            });

            // --- 2. Command Stack (Undo History) ---
            app.MapPost("/api/commands/issue", (DeviceCommand command, TelemetryProcessor processor) =>
            {
                processor.IssueCommand(command);
                return Results.Ok(new { Status = $"Command '{command.Action}' Issued" });
            });

            app.MapPost("/api/commands/undo", (TelemetryProcessor processor) =>
            {
                var reverted = processor.UndoLastCommand();
                return reverted != null
                    ? Results.Ok(new { Status = "Command Reverted", RevertedCommand = reverted })
                    : Results.BadRequest("No commands to undo.");
            });

            // --- 3. Dictionary Device Search ---
            app.MapGet("/api/devices/{macAddress}", (string macAddress, DeviceStateManager stateManager) =>
            {
                var device = stateManager.GetDevice(macAddress);
                return device != null
                    ? Results.Ok(device)
                    : Results.NotFound(new { Message = $"Device with MAC {macAddress} not found in the live registry." });
            });

            // --- 4. Predictive Insights Engine ---
            app.MapGet("/api/insights", (RecommendationEngine engine) =>
            {
                var insights = engine.GenerateRecommendations();
                return Results.Ok(insights);
            });

            app.Run();
        }
    }
}