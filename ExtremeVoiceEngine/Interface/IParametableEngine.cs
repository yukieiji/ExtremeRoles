namespace ExtremeVoiceEngine.Interface;

public interface IParametableEngine<T>
    : ISpeakEngine
    where T : IEngineParameter
{
    public void SetParameter(T param);
}
