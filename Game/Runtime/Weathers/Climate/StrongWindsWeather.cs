using Runtime.Battle.Systems;

namespace Runtime.Weathers.Climate
{
    public class StrongWindsWeather : ClimateWeather
    {
        public override void Setup()
        {
            WeatherManager weatherManager = BattleSystem.instance.GetWeatherManager();
            
            weatherManager.ApplyIrritant(null);
            weatherManager.ApplyEnergy(null);
            
            weatherManager.SetAmplified(true);
        }
    }
}