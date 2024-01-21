using TechTalk.SpecFlow;
using Xunit.Abstractions;

namespace Bindings
{
    public class TestContext
    {        
        public ScenarioContext scenarioContext;
        public FeatureContext featureContext;
        public ITestOutputHelper output;

        public const string ScenarioContextStart = "SV:";
        public const string FeatureContextStart = "FV:";
        public const int ContextStartLength = 3;

        public TestContext(FeatureContext f, ScenarioContext s, ITestOutputHelper o)
        {
            scenarioContext = s;
            featureContext = f;
            output = o;
        }
        
        public void SetToContext<T>(T value, string varName)
        {
            switch (varName.Substring(0, ContextStartLength))
            {
                case FeatureContextStart:
                    featureContext.Set<T>(value, varName.Substring(ContextStartLength));
                    break;
                case ScenarioContextStart:
                    scenarioContext.Set<T>(value, varName.Substring(ContextStartLength));
                    break;
                default:
                    output.WriteLine("Context was not choosen!");
                    break;
            }
        }

        public T GetFromContext<T>(string varName)
        {
            if (varName.Length < ContextStartLength)
                return (T)(object)varName;
            switch (varName.Substring(0, ContextStartLength))
            {
                case FeatureContextStart:
                    if (IsContextContainKey(varName))
                        return featureContext.Get<T>(varName.Substring(ContextStartLength));
                    output.WriteLine($"Variable {varName} is not presented in Feature context.");
                    return (T)(object)varName;
                case ScenarioContextStart:
                    if (IsContextContainKey(varName))
                        return scenarioContext.Get<T>(varName.Substring(ContextStartLength));
                    output.WriteLine($"Variable {varName} is not presented in Scenario context.");
                    return (T)(object)varName;
                default:
                    return (T)(object)varName;
            }
        }

        public bool IsContextContainKey(string varName)
        {
            if (varName.Length < ContextStartLength)
                return false;
            switch (varName.Substring(0, ContextStartLength))
            {
                case FeatureContextStart:
                    return featureContext.ContainsKey(varName.Substring(ContextStartLength));
                case ScenarioContextStart:
                    return scenarioContext.ContainsKey(varName.Substring(ContextStartLength));
                default:
                    return false;
            }
        }

        public void RemoveFromContext(string varName)
        {
            if (varName.Length < ContextStartLength)
                return;
            switch (varName.Substring(0, ContextStartLength))
            {
                case FeatureContextStart:
                    featureContext.Remove(varName.Substring(ContextStartLength));
                    break;
                case ScenarioContextStart:
                    scenarioContext.Remove(varName.Substring(ContextStartLength));
                    break;
                default:
                    break;
            }
        }
    }
}