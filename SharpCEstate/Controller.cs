using System;
using Microsoft.ML;

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
        private MLContext mLContext = new MLContext(seed: 0); // Cria um contexto ML com uma semente para reprodutibilidade
        public void InitializeSystem()
        {
            // Carregar configurações e inicializar modelo
            Configurations.Load();
            var model = ModelInitializer.InitializeModel(mLContext);  // Assumindo que isso retorna algo que precisamos

            // Depois de inicializar o sistema, preparar a interface
            ViewUpdater.PrepareInterface();
        }
    }

    public class PriceForecastController
    {
        private MLContext mlContext = new MLContext(seed: 0); // Cria um contexto ML com uma semente para reprodutibilidade
        private ITransformer? model;
        public PriceForecastController()
        {
            // Inscrever-se no evento de previsão de preços completada
            MachineLearningModel.PredictionsReady += OnPricePredicted;
        }

        public void InitiatePriceForecast()
        {
            var dataView = RealEstateDataProcessor.LoadAndPrepareData(mlContext, "./imovirtual_casas.csv"); // Isto tem de ser corrigido
            // Processar dados e executar ML.NET
            model = RealEstateDataProcessor.TrainModel(mlContext, dataView); // Supõe que existe um método TrainModel que retorna ITransformer
            MachineLearningModel.ExecuteMLNET(mlContext, model, dataView);
        }

        private void OnPricePredicted(object? sender, RealEstatePredictionEventArgs e)
        {
            // Atualizar a previsão na interface de usuário
            Console.WriteLine($"Previsão recebida: {e.PredictedPrice}");
            ViewUpdater.ShowForecast(e.PredictedPrice);
        }

        public void UpdateForecastResult()
        {
            // Interagir com o usuário para atualizar a previsão
            UserInteractionView.Interact();
        }
    }
}
