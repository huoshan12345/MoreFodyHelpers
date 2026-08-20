namespace MoreFodyHelpers.Support;

public class WeaverLogger(BaseModuleWeaver moduleWeaver) : IWeaverLogger
{
    public void Debug(string message)
        => moduleWeaver.WriteDebug(message);

    public void Info(string message)
        => moduleWeaver.WriteInfo(message);

    public void Warning(string message, SequencePoint? sequencePoint)
        => moduleWeaver.WriteWarning(message, sequencePoint);

    public void Error(string message, SequencePoint? sequencePoint)
        => moduleWeaver.WriteError(message, sequencePoint);
}
