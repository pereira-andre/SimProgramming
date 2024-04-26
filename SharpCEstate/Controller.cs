// Controller.cs
using System;
using Microsoft.ML;

namespace SharpCEstate
{
    public class ApplicationController
    {
        private MLContext mlContext = new MLContext(seed: 0);
        private ITransformer? model;
        private string modelPath = "./realEstateModel.zip";
        private string dataPath = "./dados_limpos.csv";

        public ApplicationController()
        {
            // Registar o evento de predição concluída
            RealEstateDataProcessor.OnPredictionCompleted += UpdateViewAfterPrediction;
        }

        // Método para atualizar a view após a predição
        private void UpdateViewAfterPrediction(RealEstatePrediction prediction)
        {
            ViewUpdater.ShowForecast(prediction.PredictedPrice);
        }

        public void StartApplication()
        {
            // Carregar e preparar os dados
            var dataView = RealEstateDataProcessor.LoadAndPrepareData(mlContext, dataPath);
            var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var trainingData = splitData.TrainSet;

            // Treinar o modelo
            model = RealEstateDataProcessor.TrainModel(mlContext, trainingData);

            // Salvar o modelo
            RealEstateDataProcessor.SaveModel(mlContext, model, modelPath);

            // Atualizar a view após inicialização completa
            ViewUpdater.PrepareInterface();
        }

        public void RequestPriceForecast(float area, string location, string nome)
        {
            // Criar dados de entrada para a predição
            var inputData = new RealEstateData { Area = area, Localizacao = location, Nome = nome };

            // Fazer a predição
            RealEstateDataProcessor.PredictPrice(mlContext, modelPath, inputData);
        }

        public void UpdateForecastResult()
        {
            // Chamar interação do usuário para atualizações
            UserInteractionView.Interact();
        }
    }
}
