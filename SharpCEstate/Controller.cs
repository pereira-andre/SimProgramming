using System;

namespace SharpCEstate
{
    public class ApplicationController
    {
        private SystemInitializer systemInitializer = new SystemInitializer();
        private PriceForecastController priceForecastController = new PriceForecastController();

        public void StartApplication()
        {
            systemInitializer.InitializeSystem();
        }

        public void RequestPriceForecast()
        {
            priceForecastController.InitiatePriceForecast();
        }

        public void UpdateForecastResult()
        {
            priceForecastController.UpdateForecastResult();
        }
    }

    public class SystemInitializer
    {
        public void InitializeSystem()
        {
            // Carregar configurações e inicializar modelo
            Configurations.Load();
            ModelInitializer.InitializeModel();

            // Depois de inicializar o sistema, preparar a interface
            ViewUpdater.PrepareInterface();
        }
    }

    public class PriceForecastController
    {
        public void InitiatePriceForecast()
        {
            // Processar dados, executar ML.NET e mostrar previsão
            RealEstateDataProcessor.ProcessData();
            MachineLearningModel.ExecuteMLNET();

            // Depois de executar o modelo, mostrar a previsão
            ViewUpdater.ShowForecast();
        }

        public void UpdateForecastResult()
        {
            // Interagir com o usuário para atualizar a previsão
            UserInteractionView.Interact();
        }
    }
}
