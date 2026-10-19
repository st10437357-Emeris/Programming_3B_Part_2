using System;
using System.Collections.Generic;

namespace SmartX.Core.Processing
{
    // Command structure for the Undo stack
    public class DeviceCommand
    {
        public string CommandId { get; set; }
        public string Action { get; set; }
    }

    public class TelemetryProcessor
    {
        // 1. Message Queues (Queue<T>) for standard FIFO processing[cite: 4]
        private readonly Queue<TelemetryPacket<string>> _standardTelemetryQueue;

        // 2. Priority Queues for critical alerts bypassing the standard queue[cite: 4]
        private readonly PriorityQueue<TelemetryPacket<string>, int> _criticalAlertQueue;

        // 3. Stacks (Stack<T>) for the manual override "Undo" feature[cite: 4]
        private readonly Stack<DeviceCommand> _commandHistory;

        public TelemetryProcessor()
        {
            _standardTelemetryQueue = new Queue<TelemetryPacket<string>>();
            _criticalAlertQueue = new PriorityQueue<TelemetryPacket<string>, int>();
            _commandHistory = new Stack<DeviceCommand>();
        }

        public void EnqueueStandardTelemetry(TelemetryPacket<string> packet)
        {
            _standardTelemetryQueue.Enqueue(packet);
            Console.WriteLine($"Standard packet queued for device {packet.MacAddress}");
        }

        public void EnqueueCriticalAlert(TelemetryPacket<string> packet, int severity)
        {
            _criticalAlertQueue.Enqueue(packet, severity);
            Console.WriteLine($"CRITICAL ALERT queued for device {packet.MacAddress} with severity {severity}");
        }

        public void IssueCommand(DeviceCommand command)
        {
            _commandHistory.Push(command);
            Console.WriteLine($"Command '{command.Action}' issued and added to history stack.");
        }

        public DeviceCommand UndoLastCommand()
        {
            if (_commandHistory.Count > 0)
            {
                DeviceCommand lastCommand = _commandHistory.Pop();
                Console.WriteLine($"Reverting last command: {lastCommand.Action}");
                return lastCommand;
            }

            Console.WriteLine("No commands in history to undo.");
            return null;
        }
    }
}