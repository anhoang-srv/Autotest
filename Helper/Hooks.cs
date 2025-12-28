using TechTalk.SpecFlow;

namespace SamsungCloudTest.Helper 
{
    [Binding]
    public class Hook
    {
        private readonly Sessions _sessions;

        
        public Hook(Sessions sessions)
        {
            _sessions = sessions;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _sessions.InitializeSession();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _sessions.StopSession();
        }
    }
}