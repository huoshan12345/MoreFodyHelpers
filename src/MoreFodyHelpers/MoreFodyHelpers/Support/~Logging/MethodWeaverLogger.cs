namespace MoreFodyHelpers.Support;

public class MethodWeaverLogger : IWeaverLogger
{
    private readonly IWeaverLogger _log;
    private readonly MethodDefinition _method;

    public MethodWeaverLogger(IWeaverLogger log, MethodDefinition method)
    {
        _log = log;
        _method = method;
    }

    public void Debug(string message)
        => _log.Debug(QualifyMessage(message));

    public void Info(string message)
        => _log.Info(QualifyMessage(message));

    public void Warning(string message, SequencePoint? sequencePoint)
        => _log.Warning(QualifyMessage(message), sequencePoint);

    public void Error(string message, SequencePoint? sequencePoint)
        => _log.Error(QualifyMessage(message), sequencePoint);

    public string QualifyMessage(string message, Instruction? instruction = null)
    {
        if (message.Contains(_method.FullName))
            return message;

        return instruction != null
            ? $"{message} (in {_method.FullName} at instruction {instruction})"
            : $"{message} (in {_method.FullName})";
    }
}
