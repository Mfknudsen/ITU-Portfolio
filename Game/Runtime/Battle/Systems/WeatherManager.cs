#region Packages

using System.Collections;
using System.Collections.Generic;
using Runtime.Weathers;

#endregion

namespace Runtime.Battle.Systems
{
    public class WeatherManager
    {
        #region Values

        private BattleSystem battleSystem;
        private ClimateWeather climateWeather;
        private IrritantWeather irritantWeather;
        private EnergyWeather energyWeather;
        private WeatherTerrain weatherTerrain;

        private bool amplifyNext;

        #endregion

        #region Getters

        public Weather[] GetAll()
        {
            return new Weather[] { this.climateWeather, this.irritantWeather, this.energyWeather };
        }

        public List<T> GetWeatherWithInterface<T>()
        {
            List<T> result = new List<T>();

            foreach (Weather weather in this.GetAll())
            {
                if (weather is T t)
                    result.Add(t);
            }

            return result;
        }

        public bool GetAmplified()
        {
            if (!this.amplifyNext) return false;

            this.amplifyNext = false;
            return true;
        }

        #endregion

        #region Setters

        public void SetAmplified(bool set)
        {
            this.amplifyNext = set;
        }

        #endregion

        #region In

        public void ApplyClimate(ClimateWeather set)
        {
            this.ShiftWeather(this.climateWeather, set);
            this.climateWeather = set;
        }

        public void ApplyIrritant(IrritantWeather set)
        {
            this.ShiftWeather(this.irritantWeather, set);
            this.irritantWeather = set;
        }

        public void ApplyEnergy(EnergyWeather set)
        {
            this.ShiftWeather(this.energyWeather, set);
            this.energyWeather = set;
        }

        public void ApplyTerrain(WeatherTerrain set)
        {
            this.weatherTerrain = set;
            this.weatherTerrain.Start();
        }

        #endregion

        #region Internal

        private void ShiftWeather(Weather current, Weather next)
        {
            //battleManager.GetHolderObject().StartCoroutine(Shift());
        }

        #endregion

        #region IEnumerator

        private IEnumerator Shift()
        {
            yield return 0;
        }

        #endregion
    }
}