using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace HBYWCZP2024.Utils
{
    public sealed class BoundedRichTextBoxSink : ILogEventSink, IDisposable
    {
        private const int DrainBatchSize = 250;
        private static readonly Brush TextBrush = CreateBrush("#ffffff");
        private static readonly Brush ValueBrush = CreateBrush("#00ffff");
        private static readonly Brush WarningBrush = CreateBrush("#ffff00");
        private static readonly Brush ErrorBrush = CreateBrush("#ff4d4d");
        private readonly RichTextBox _richTextBox;
        private readonly MessageTemplateTextFormatter _formatter;
        private readonly ConcurrentQueue<RenderedLogLine> _pending = new();
        private readonly int _maxLines;
        private readonly int _maxPendingLines;
        private int _flushScheduled;
        private bool _disposed;

        public BoundedRichTextBoxSink(
            RichTextBox richTextBox,
            int maxLines = 300,
            string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        {
            _richTextBox = richTextBox;
            _maxLines = Math.Max(1, maxLines);
            _maxPendingLines = Math.Max(_maxLines * 2, DrainBatchSize);
            _formatter = new MessageTemplateTextFormatter(outputTemplate, CultureInfo.InvariantCulture);
        }

        public void Emit(LogEvent logEvent)
        {
            if (_disposed)
            {
                return;
            }

            using StringWriter writer = new(CultureInfo.InvariantCulture);
            _formatter.Format(logEvent, writer);
            _pending.Enqueue(new RenderedLogLine(writer.ToString(), logEvent.Level));

            while (_pending.Count > _maxPendingLines && _pending.TryDequeue(out _))
            {
            }

            ScheduleFlush();
        }

        public void Dispose()
        {
            _disposed = true;
            while (_pending.TryDequeue(out _))
            {
            }
        }

        private void ScheduleFlush()
        {
            if (_disposed || Interlocked.Exchange(ref _flushScheduled, 1) == 1)
            {
                return;
            }

            Dispatcher dispatcher = _richTextBox.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                Flush();
            }
            else
            {
                dispatcher.BeginInvoke(Flush, DispatcherPriority.Background);
            }
        }

        private void Flush()
        {
            Interlocked.Exchange(ref _flushScheduled, 0);
            if (_disposed)
            {
                return;
            }

            int drained = 0;
            _richTextBox.BeginChange();
            try
            {
                while (drained < DrainBatchSize && _pending.TryDequeue(out var line))
                {
                    AppendLine(line);
                    drained++;
                }

                TrimBlocks();
                _richTextBox.CaretPosition = _richTextBox.Document.ContentEnd;
                _richTextBox.ScrollToEnd();
            }
            finally
            {
                _richTextBox.EndChange();
            }

            if (!_pending.IsEmpty)
            {
                ScheduleFlush();
            }
        }

        private void AppendLine(RenderedLogLine logLine)
        {
            string line = logLine.Text.TrimEnd('\r', '\n');
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0),
                Foreground = TextBrush
            };

            int levelStart = line.IndexOf(' ');
            int headerEnd = line.IndexOf("] ", StringComparison.Ordinal);
            if (line.StartsWith("[", StringComparison.Ordinal) && levelStart > 0 && headerEnd > levelStart)
            {
                paragraph.Inlines.Add(new Run(line[..(headerEnd + 2)]) { Foreground = TextBrush });
                AddMessageInlines(paragraph, line[(headerEnd + 2)..], logLine.Level);
            }
            else
            {
                AddMessageInlines(paragraph, line, logLine.Level);
            }

            _richTextBox.Document.Blocks.Add(paragraph);
        }

        private void TrimBlocks()
        {
            while (_richTextBox.Document.Blocks.Count > _maxLines)
            {
                _richTextBox.Document.Blocks.Remove(_richTextBox.Document.Blocks.FirstBlock);
            }
        }

        private static void AddMessageInlines(Paragraph paragraph, string message, LogEventLevel level)
        {
            int separatorIndex = FindDataSeparator(message);
            if (separatorIndex >= 0 && separatorIndex < message.Length - 1)
            {
                paragraph.Inlines.Add(new Run(message[..(separatorIndex + 1)])
                {
                    Foreground = GetLevelForeground(level)
                });
                paragraph.Inlines.Add(new Run(message[(separatorIndex + 1)..])
                {
                    Foreground = ValueBrush
                });
                return;
            }

            paragraph.Inlines.Add(new Run(message)
            {
                Foreground = GetLevelForeground(level)
            });
        }

        private static int FindDataSeparator(string message)
        {
            int englishColon = message.IndexOf(':');
            int chineseColon = message.IndexOf('：');

            if (englishColon < 0)
            {
                return chineseColon;
            }

            if (chineseColon < 0)
            {
                return englishColon;
            }

            return Math.Min(englishColon, chineseColon);
        }

        private static Brush GetLevelForeground(LogEventLevel level)
        {
            return level switch
            {
                LogEventLevel.Warning => WarningBrush,
                LogEventLevel.Error => ErrorBrush,
                LogEventLevel.Fatal => ErrorBrush,
                _ => TextBrush
            };
        }

        private static Brush CreateBrush(string color)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;
            brush.Freeze();
            return brush;
        }

        private readonly struct RenderedLogLine
        {
            public RenderedLogLine(string text, LogEventLevel level)
            {
                Text = text;
                Level = level;
            }

            public string Text { get; }

            public LogEventLevel Level { get; }
        }
    }

    public static class BoundedRichTextBoxSinkExtensions
    {
        public static LoggerConfiguration BoundedRichTextBox(
            this LoggerSinkConfiguration sinkConfiguration,
            RichTextBox richTextBox,
            int maxLines = 300,
            string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        {
            return sinkConfiguration.Sink(new BoundedRichTextBoxSink(richTextBox, maxLines, outputTemplate));
        }
    }
}
