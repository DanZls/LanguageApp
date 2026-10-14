using System.Diagnostics;
using System.Text;

public static class ExceptionService {
  public static string GetExceptionDetails(Exception ex) {
    var exceptionInfo = new StringBuilder();
    exceptionInfo.Append($"{ex.Message}\n");
    exceptionInfo.AppendLine("[Details]:");
    exceptionInfo.AppendLine($"{ex.GetType().FullName}");
    var stackTrace = new StackTrace(ex, fNeedFileInfo: true);
    for (var i = 0; i < stackTrace?.FrameCount; i++) {
      var frame = stackTrace?.GetFrame(i);
      var method = frame?.GetMethod();
      var fileName = frame?.GetFileName();
      var line = frame?.GetFileLineNumber();
      var column = frame?.GetFileColumnNumber();
      exceptionInfo.AppendLine($"Call stack, {i}:");
      exceptionInfo.AppendLine($"  Method: {method?.DeclaringType?.FullName}.{method?.Name}");
      exceptionInfo.AppendLine($"  File: {fileName}");
      exceptionInfo.AppendLine($"  Line: {line}, Column: {column}");
    }
    return exceptionInfo.ToString();
  }
}
