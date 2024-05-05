// Controller.cs
using System;
using Microsoft.ML;
using System.Threading.Tasks;

namespace SharpCEstate
{
    public class ApplicationController
    {
        private static ApplicationController? instance;
        private MLContext mlContext = new MLContext(seed: 0);
        private ITransformer? model;
        private string modelPath = "./realEstateModel.zip";
        private string dataPath = "./dados_limpos.csv";

        public ApplicationController()
        {
            RealEstateDataProcessor.OnPredictionCompleted += UpdateViewAfterPrediction;
        }

        // Propriedade Singleton
        public static ApplicationController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ApplicationController();
                }
                return instance;
            }
        }

        // Método para atualizar a view após a predição
        private void UpdateViewAfterPrediction(RealEstatePrediction prediction)
        {
            ViewUpdater.ShowForecast(prediction.PredictedPrice);
        }

        public async Task StartApplicationAsync()
        {
            try
            {
                // Carregar e preparar os dados
                var dataView = await RealEstateDataProcessor.LoadAndPrepareDataAsync(mlContext, dataPath);
                var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                var trainingData = splitData.TrainSet;

                // Treinar o modelo
                model = RealEstateDataProcessor.TrainModel(mlContext, trainingData);

                // Salvar o modelo
                RealEstateDataProcessor.SaveModel(mlContext, model, modelPath);

                // Atualizar a view após inicialização completa
                ViewUpdater.PrepareInterface();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante a inicialização da aplicação: {ex.Message}");
            }
            
        }

        public void RequestPriceForecast(float area, string location, string nome)
        {
            try
            {
                // Criar dados de entrada para a predição
                var inputData = new RealEstateData { Area = area, Localizacao = location, Nome = nome };

                // Fazer a predição
                RealEstateDataProcessor.PredictPrice(mlContext, modelPath, inputData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante a previsão de preço: {ex.Message}");
            }
        }

        public void UpdateForecastResult()
        {
            // Chamar interação do usuário para atualizações
            UserInteractionView.Interact();
        }
    }
}
