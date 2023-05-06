using ExtremeVoiceEngine.Command;

namespace ExtremeVoiceEngine.Interface;

public interface IParametableEngine<T>
    : ISpeakEngine
    where T : IEngineParameter
{
    public void CreateCommand();

    public void Parse(Result? result);

    public void SetParameter(T param);
}
